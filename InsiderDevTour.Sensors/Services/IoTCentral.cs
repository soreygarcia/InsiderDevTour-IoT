using InsiderDevTour.Sensors.Constants;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InsiderDevTour.Sensors.Services
{
    //dps-keygen -di:1df0b2a0-2ec6-438a-85fd-760c0b29271c -dk:3WAvFonMjbONVsSImb7uaDTvNuPl7BKEZEWYggni4d4= -si:0ne0005CB82
    class IoTCentral
    {
        static DeviceClient Client = null;
        static TwinCollection reportedProperties = new TwinCollection();

        public static void InitClient()
        {
            try
            {
                Console.WriteLine("Connecting to hub");
                Client = DeviceClient.CreateFromConnectionString(AzureCredentials.centralConnectionString, TransportType.Mqtt);
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error in sample: {0}", ex.Message);
            }
        }

        public static async void SendDeviceProperties()
        {
            try
            {
                Console.WriteLine("Sending device properties:");
                Random random = new Random();
                TwinCollection telemetryConfig = new TwinCollection();
                reportedProperties["dieNumber"] = random.Next(1, 6);
                Console.WriteLine(JsonConvert.SerializeObject(reportedProperties));

                await Client.UpdateReportedPropertiesAsync(reportedProperties);
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error in sample: {0}", ex.Message);
            }
        }

        public static async Task SendTelemetryAsync(double temperature, string color)
        {
            try
            {
                var telemetryDataPoint = new
                {
                    time = DateTime.Now.ToString(),
                    deviceId = AzureCredentials.hubDeviceId,
                    temperature = temperature,
                    color = color
                };

                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));

                await Client.SendEventAsync(message);

                Console.WriteLine("{0} > Sending telemetry: {1}", DateTime.Now, messageString);
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Intentional shutdown: {0}", ex.Message);
            }
        }

        private static async Task HandleSettingChanged(TwinCollection desiredProperties, object userContext)
        {
            try
            {
                Console.WriteLine("Received settings change...");
                Console.WriteLine(JsonConvert.SerializeObject(desiredProperties));

                string setting = "fanSpeed";
                if (desiredProperties.Contains(setting))
                {
                    // Act on setting change, then
                    AcknowledgeSettingChange(desiredProperties, setting);
                }
                setting = "setVoltage";
                if (desiredProperties.Contains(setting))
                {
                    // Act on setting change, then
                    AcknowledgeSettingChange(desiredProperties, setting);
                }
                setting = "setCurrent";
                if (desiredProperties.Contains(setting))
                {
                    // Act on setting change, then
                    AcknowledgeSettingChange(desiredProperties, setting);
                }
                setting = "activateIR";
                if (desiredProperties.Contains(setting))
                {
                    // Act on setting change, then
                    AcknowledgeSettingChange(desiredProperties, setting);
                }
                await Client.UpdateReportedPropertiesAsync(reportedProperties);
            }

            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error in sample: {0}", ex.Message);
            }
        }

        private static void AcknowledgeSettingChange(TwinCollection desiredProperties, string setting)
        {
            reportedProperties[setting] = new
            {
                value = desiredProperties[setting]["value"],
                status = "completed",
                desiredVersion = desiredProperties["$version"],
                message = "Processed"
            };
        }
    }
}
