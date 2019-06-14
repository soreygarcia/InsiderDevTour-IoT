using InsiderDevTour.Sensors.Constants;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace InsiderDevTour.Sensors.Services
{
    public static class IoTHub
    {
        // IoT Hub Device client
        static DeviceClient deviceClient;

        public static async Task ConnectToIoTHub()
        {
            // Azure IoT Hub connection
            deviceClient = DeviceClient.CreateFromConnectionString(AzureCredentials.hubConnectionString, TransportType.Mqtt);
            await deviceClient.SetMethodHandlerAsync("UploadPhoto", OnUploadPhoto, null);
        }

        public static async Task SendDeviceToCloudMessage(double temperature, string color)
        {
            try
            {
                // create new telemetry message
                var telemetryDataPoint = new
                {
                    time = DateTime.Now.ToString(),
                    deviceId = AzureCredentials.hubDeviceId,
                    temperature = temperature,
                    color = color
                };

                // serialise message to a JSON string
                string messageString = JsonConvert.SerializeObject(telemetryDataPoint);

                // format JSON string into IoT Hub message
                Message message = new Message(Encoding.ASCII.GetBytes(messageString));

                // push message to IoT Hub
                await deviceClient.SendEventAsync(message);
            }
            catch (Exception)
            {

            }
        }


        public static async Task<MethodResponse> OnUploadPhoto(MethodRequest methodRequest, object userContext)
        {

            // take a photo
            StorageFile photoFile = await Webcam.TakePhoto();

            using (FileStream photoStream = new FileStream(photoFile.Path, FileMode.Open))
            {
                // upload photo via IoT Hub
                await deviceClient.UploadToBlobAsync(photoFile.Name, photoStream);

            }
            // delete photo taken of viewer
            await photoFile.DeleteAsync();

            return new MethodResponse(200);
        }
    }
}