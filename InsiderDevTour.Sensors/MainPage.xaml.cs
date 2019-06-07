using InsiderDevTour.Sensors.Drivers;
using InsiderDevTour.Sensors.Drivers.BME280;
using InsiderDevTour.Sensors.Drivers.TCS34725;
using InsiderDevTour.Sensors.Drivers.VNCL4010;
using InsiderDevTour.Sensors.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace InsiderDevTour.Sensors
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // 16 is the pin number that the proximity interrupt is connected to on the Raspberry Pi side
        private const int intPinNumber = 6;

        // interrupt pin from the proximity sensor
        private GpioPin interruptPin;

        private DispatcherTimer measureTimer;

        // temperature sensor
        private BME280Sensor bme280Sensor = new BME280Sensor();

        // proximity sensor
        private VNCL4010Sensor vncl4010Sensor = new VNCL4010Sensor();

        //color sensor
        private TCS34725Sensor colorSensor = new TCS34725Sensor();

        public MainPage()
        {
            this.InitializeComponent();
            // set up timer to sample proximity and temperature values every 1000 milliseconds
            measureTimer = new DispatcherTimer();
            measureTimer.Interval = TimeSpan.FromMilliseconds(1000);
            // attach event handler on each 1000ms tick
            measureTimer.Tick += MeasureTimerTick;

            // initialize GPIO devices
            InitializeDevices();
        }

        #region Handlers
        private async void MeasureTimerTick(object sender, object e)
        {
            // read Temperature
            double temperature = await bme280Sensor.ReadTemperature();
            // convert to Fahrenheit
            double fahrenheitTemperature = temperature * 1.8 + 32.0;

            // read Proximity
            int proximity = vncl4010Sensor.ReadProximity();

            TemperatureStatus.Text = "The temperature is currently " + fahrenheitTemperature.ToString("n1") + "°F";
            var colorRead = await colorSensor.GetClosestColor();

            SolidColorBrush brush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, colorRead.ColorValue.R, colorRead.ColorValue.G, colorRead.ColorValue.B));
            this.Background = brush;

            await IoTHub.SendDeviceToCloudMessage(fahrenheitTemperature, colorRead.ColorName);
            await IoTCentral.SendTelemetryAsync(fahrenheitTemperature, colorRead.ColorName);
        }

        private async void OnInterrupt(GpioPin sender, GpioPinValueChangedEventArgs e)
        {
            // only act on initial pull-down event from the interrupt; avoid rising edge trigger upon reset
            if (e.Edge == GpioPinEdge.FallingEdge)
            {
                // try to recognize viewer and greet them by name
                await RecognizeAndGreetViewer();
            }
        }
        #endregion

        #region Methods
        private async void InitializeDevices()
        {
            var gpio = GpioController.GetDefault();

            // show an error if there is no GPIO controller
            if (gpio is null)
            {
                interruptPin = null;
                GpioStatus.Text = "There is no GPIO controller on this device.";
                return;
            }

            // set up interrupt pin for proximity sensor
            interruptPin = gpio.OpenPin(intPinNumber);

            // pull up interrupt pin as sensor will pull down to notify
            interruptPin.SetDriveMode(GpioPinDriveMode.InputPullUp);

            // initialize BME280 Sensor + Intitialize VNCL4010
            await Task.WhenAll(bme280Sensor.Initialize(), vncl4010Sensor.Initialize(), colorSensor.Initialize());

            // listen to interrupt pin changes
            interruptPin.ValueChanged += OnInterrupt;

            GpioStatus.Text = "Connecting to IoT Hub...";

            await IoTHub.ConnectToIoTHub();

            GpioStatus.Text = "Connecting to IoT Central...";

            IoTCentral.InitClient();

            GpioStatus.Text = "";

            // start measuring temperature and proximity 
            measureTimer.Start();
        }

        private async Task RecognizeAndGreetViewer()
        {
            GpioStatus.Text = "";

            // let viewer know we're taking picture
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => InterruptText.Text = "Taking picture...");

            // take their photo
            StorageFile photoFile = await Webcam.TakePhoto();

            // let viewer know we're recognizing them
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => InterruptText.Text = "Recognizing...");

            // use Cognitive Services to identify them
            string name = await FaceAPI.GetViewerName(photoFile);

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // clear waiting text
                InterruptText.Text = "";
                // greet viewer on screen
                GpioStatus.Text = "Greetings, " + name;
            });

            // clear the interrupt flag so that interrupt can occur again
            vncl4010Sensor.ClearInterruptFlag();

            // delete photo taken of viewer
            await photoFile.DeleteAsync();
        }

        private async Task RecognizeAndGreetViewerUsingBlogStorage()
        {
            GpioStatus.Text = "";

            // let viewer know we're taking picture
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => InterruptText.Text = "Taking picture...");

            // take their photo
            var photoFile = await Webcam.TakePhotoAndUploadToTeBlobStorage();

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => InterruptText.Text = "Recognizing...");

            // use Cognitive Services to identify them
            string name = await FaceAPI.GetViewerName(photoFile);

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // clear waiting text
                InterruptText.Text = "";
                // greet viewer on screen
                GpioStatus.Text = "Greetings, " + name;
            });

            // clear the interrupt flag so that interrupt can occur again
            vncl4010Sensor.ClearInterruptFlag();
        }
        #endregion
    }
}