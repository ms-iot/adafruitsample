using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        const float ReferenceVoltage = 3.3F;
        const byte LowPotentiometerADCChannel = 0;
        const byte HighPotentiometerADCChannel = 1;
        const byte CDSADCChannel = 2;
        const string JustRightLightString = "Ah, just right";
        const string LowLightString = "I need a light";
        const string HighLightString = "I need to wear shades";

        enum eState { unknown, JustRight, TooBright, TooDark};
        eState CurrentState = eState.unknown;

        MCP3008 mcp3008 = new MCP3008(ReferenceVoltage);


        private SpeechSynthesizer synthesizer;


        public Timer timer;

        public MainPage()
        {
            this.InitializeComponent();

            synthesizer = new SpeechSynthesizer();

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
            int lowPotReadVal;
            int highPotReadVal;
            int cdsReadVal;
            float lowPotVoltage;
            float highPotVoltage;
            float cdsVoltage;

            eState newState = eState.JustRight;

            lowPotReadVal = await mcp3008.ReadADC(LowPotentiometerADCChannel);
            highPotReadVal = await mcp3008.ReadADC(HighPotentiometerADCChannel);
            cdsReadVal = await mcp3008.ReadADC(CDSADCChannel);

            lowPotVoltage = mcp3008.ADCToVoltage(lowPotReadVal);
            highPotVoltage = mcp3008.ADCToVoltage(highPotReadVal);
            cdsVoltage = mcp3008.ADCToVoltage(cdsReadVal);

            Debug.WriteLine(String.Format("Read values {0}, {1}, {2} ", lowPotReadVal, highPotReadVal, cdsReadVal));
            Debug.WriteLine(String.Format("Voltages {0}, {1}, {2} ", lowPotVoltage, highPotVoltage, cdsVoltage));

            if (cdsVoltage < lowPotVoltage)
            {
                newState = eState.TooDark;
            }

            if (cdsVoltage > highPotVoltage)
            {
                newState = eState.TooBright;
            }

            await CheckForStateChange(newState);
        }

        private async Task CheckForStateChange(eState newState)
        {
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

                await TextToSpeech(whatToSay);
                CurrentState = newState;
            }
        }

        private async Task TextToSpeech(String textToSpeak)
        {
            Debug.WriteLine(String.Format("MainPage::TextToSpeech {0}", textToSpeak));
            // Because we are running somewhere other than the UI thread and we need to talk to a UI element (the media control)
            // we need to use the dispatcher to move the calls to the right thread.
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High,
            async () =>
            {
                SpeechSynthesisStream synthesisStream;
                try
                {
                    //creating a stream from the text which can be played using media element. This new API converts text input into a stream.
                    synthesisStream = await synthesizer.SynthesizeTextToStreamAsync(textToSpeak);
                }
                catch (Exception)
                {
                    synthesisStream = null;
                }
                // if the SSML stream is not in the correct format throw an error message to the user
                if (synthesisStream == null)
                {
                    Debug.WriteLine("unable to synthesize text");
                    return;
                }

                // start this audio stream playing
                media.AutoPlay = true;
                media.SetSource(synthesisStream, synthesisStream.ContentType);
                media.Play();
            }
            );

        }
    }
}
