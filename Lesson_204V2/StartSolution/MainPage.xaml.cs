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
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Lesson_204V2

{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // Use for configuration of the MCP3008 class voltage formula

        public MainPage()
        {
            this.InitializeComponent();

        }

        protected override async void OnNavigatedTo(NavigationEventArgs navArgs)
        {
            Debug.WriteLine("MainPage::OnNavigatedTo");

        }

        public float ADCToVoltage(int adc)
        {
            return (float)adc * ReferenceVoltage / (float)adcController.MaxValue;
        }


        private async void timerCallback(object state)
        {
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
                }
            );
        }

        // This will put our pin on the world map of makers
        // Go to ENTER FINAL LINK HERE to view your pin
        public void MakeWebAPICall()
        {
            HttpClient client = new HttpClient();
            client.GetStringAsync("http://adafruitsample.azurewebsites.net/api?Lesson=204");
        }

    }
}
