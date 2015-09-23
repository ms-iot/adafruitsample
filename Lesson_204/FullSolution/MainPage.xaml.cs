using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Lesson_204
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

        // Our ADC Chip class
        MCP3008 mcp3008 = new MCP3008(ReferenceVoltage);

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
            mcp3008.Initialize();
        }

        protected override void OnNavigatedTo(NavigationEventArgs navArgs)
        {
            Debug.WriteLine("MainPage::OnNavigatedTo");
            
            // We will check for light level changes once per second (1000 milliseconds)
            timer = new Timer(timerCallback, this, 0, 1000);
        }

        private async void timerCallback(object state)
        {
            Debug.WriteLine("\nMainPage::timerCallback");
            if (mcp3008 == null)
            {
                Debug.WriteLine("MainPage::timerCallback not ready");
                return;
            }

            // The new light state, assume it's just right to start.
            eState newState = eState.JustRight;

            // Read from the ADC chip the current values of the two pots and the photo cell.
            int lowPotReadVal = mcp3008.ReadADC(LowPotentiometerADCChannel);
            int highPotReadVal = mcp3008.ReadADC(HighPotentiometerADCChannel);
            int cdsReadVal = mcp3008.ReadADC(CDSADCChannel);

            // convert the ADC readings to voltages to make them more friendly.
            float lowPotVoltage = mcp3008.ADCToVoltage(lowPotReadVal);
            float highPotVoltage = mcp3008.ADCToVoltage(highPotReadVal);
            float cdsVoltage = mcp3008.ADCToVoltage(cdsReadVal);

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
    }
}
