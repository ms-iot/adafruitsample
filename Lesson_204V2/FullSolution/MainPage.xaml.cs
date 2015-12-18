using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Adc;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

using Microsoft.IoT.AdcMcp3008;

namespace Lesson_204V2

{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // Use for configuration of the MCP3008 class voltage formula
        const float ReferenceVoltage = 5.0F;

        // Values for which channels we will be using from the ADC chip
        const byte LowPotentiometerADCChannel = 0;
        const byte HighPotentiometerADCChannel = 1;
        const byte CDSADCChannel = 2;

        // Some strings to let us know the current state.
        const string JustRightLightString = "Ah, just right";
        const string LowLightString = "I need a light";
        const string HighLightString = "I need to wear shades";

        // Some internal state information
        enum eState { unknown, JustRight, TooBright, TooDark};
        eState CurrentState = eState.unknown;

        private AdcController adcController;
        private AdcChannel LowPotAdcChannel;
        private AdcChannel HighPotAdcChannel;
        private AdcChannel CdsAdcChannel;
        
        // The Windows Speech API interface
        private SpeechSynthesizer synthesizer;

        // A timer to control how often we check the ADC values.
        public Timer timer;

        public MainPage()
        {
            this.InitializeComponent();

            // Create a new SpeechSynthesizer instance for later use.
            synthesizer = new SpeechSynthesizer();

            // Initialize the ADC chip for use
            adcController = (await AdcController.GetControllersAsync(AdcMcp3008Provider.GetAdcProvider()))[0];
            LowPotAdcChannel = adcController.OpenChannel(LowPotentiometerADCChannel);
            HighPotAdcChannel = adcController.OpenChannel(HighPotentiometerADCChannel);
            CdsAdcChannel = adcController.OpenChannel(CDSADCChannel);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs navArgs)
        {
            Debug.WriteLine("MainPage::OnNavigatedTo");

            // This will get our pin on the world map showing everyone we are running the sample.
            MakeWebAPICall();

            // We will check for light level changes once per second (1000 milliseconds)
            timer = new Timer(timerCallback, this, 0, 1000);
        }

        public float ADCToVoltage(int adc)
        {
            return (float)adc * ReferenceVoltage / (float)adcController.MaxValue;
        }


        private async void timerCallback(object state)
        {
            Debug.WriteLine("\nMainPage::timerCallback");
            if (adcController == null)
            {
                Debug.WriteLine("MainPage::timerCallback not ready");
                return;
            }

            // The new light state, assume it's just right to start.
            eState newState = eState.JustRight;

            // Read from the ADC chip the current values of the two pots and the photo cell.
            int lowPotReadVal = LowPotAdcChannel.ReadValue();
            int highPotReadVal = HighPotAdcChannel.ReadValue();
            int cdsReadVal = CdsAdcChannel.ReadValue();

            // convert the ADC readings to voltages to make them more friendly.
            float lowPotVoltage = ADCToVoltage(lowPotReadVal);
            float highPotVoltage = ADCToVoltage(highPotReadVal);
            float cdsVoltage = ADCToVoltage(cdsReadVal);

            // Let us know what was read in.
            Debug.WriteLine(String.Format("Read values {0}, {1}, {2} ", lowPotReadVal, highPotReadVal, cdsReadVal));
            Debug.WriteLine(String.Format("Voltages {0}, {1}, {2} ", lowPotVoltage, highPotVoltage, cdsVoltage));

            // Compute the new state by first checking if the light level is too low
            if (cdsVoltage < lowPotVoltage)
            {
                newState = eState.TooDark;
            }

            // And now check if it too high.
            if (cdsVoltage > highPotVoltage)
            {
                newState = eState.TooBright;
            }

            // Use another method to determine what to do with the state.
            await CheckForStateChange(newState);
        }

        private async Task CheckForStateChange(eState newState)
        {
            // Checks for state changes and does something when one is detected.
            if (newState != CurrentState)
            {
                String whatToSay;

                switch (newState)
                {
                    case eState.JustRight:
                        {
                            whatToSay = JustRightLightString;
                        }
                        break;

                    case eState.TooBright:
                        {
                            whatToSay = HighLightString;
                        }
                        break;

                    case eState.TooDark:
                        {
                            whatToSay = LowLightString;
                        }
                        break;

                    default:
                        {
                            whatToSay = "unexpected value";
                        }
                        break;
                }

                // Use another method to wrap the speech synthesis functionality.
                await TextToSpeech(whatToSay);

                // Update the current state for next time.
                CurrentState = newState;
            }
        }

        private async Task TextToSpeech(String textToSpeak)
        {
            Debug.WriteLine(String.Format("MainPage::TextToSpeech {0}", textToSpeak));
            // Because we are running somewhere other than the UI thread and we need to talk to a UI element (the media control)
            // we need to use the dispatcher to move the calls to the right thread.
            await Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.High,
                async () =>
                {
                    SpeechSynthesisStream synthesisStream;

                    //creating a stream from the text which can be played using media element. This API converts text input into a stream.
                    synthesisStream = await synthesizer.SynthesizeTextToStreamAsync(textToSpeak);

                    // start this audio stream playing
                    media.AutoPlay = true;
                    media.SetSource(synthesisStream, synthesisStream.ContentType);
                    media.Play();
                }
            );
        }

        // This will put our pin on the world map of makers
        // Go to http://ms-iot.github.io/content/en-US/win10/samples/BrightOrNot.htm to view your pin
        public void MakeWebAPICall()
        {
            HttpClient client = new HttpClient();
            client.GetStringAsync("http://adafruitsample.azurewebsites.net/api?Lesson=204");
        }

    }
}
