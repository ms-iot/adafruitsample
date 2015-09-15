using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Gpio;
using Windows.UI.Core;
using Windows.Media.SpeechSynthesis;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Lesson_205
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        TCS34725 colorSensor = new TCS34725();
        SpeechSynthesizer synthesizer = new SpeechSynthesizer();
        MediaElement audio = new MediaElement();
        GpioPin buttonPin;
        int gpioPin = 4;
        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs navArgs)
        {
            try
            {
                await colorSensor.Initialize();
                InitializeGpio();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        private void InitializeGpio()
        {
            var gpioController = GpioController.GetDefault();
            buttonPin = gpioController.OpenPin(gpioPin);
            buttonPin.DebounceTimeout  = new TimeSpan(1000);
            buttonPin.SetDriveMode(GpioPinDriveMode.Input);
            buttonPin.ValueChanged += buttonPin_ValueChanged;
        }

        private async void buttonPin_ValueChanged(object sender, GpioPinValueChangedEventArgs e)
        {
            if (e.Edge == GpioPinEdge.RisingEdge)
            {
                string colorRead = await colorSensor.getClosestColor();
                await SpeakColor(colorRead);
            }
        }

        private async Task SpeakColor(string colorRead)
        {
            SpeechSynthesisStream stream = await synthesizer.SynthesizeTextToStreamAsync("I think the color is " + colorRead);
            var ignored = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                audio.SetSource(stream, stream.ContentType);
                audio.Play();
            });
        }
    }
}

