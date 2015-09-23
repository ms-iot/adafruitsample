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

        public MainPage()
        {
            this.InitializeComponent();

        }

        protected override void OnNavigatedTo(NavigationEventArgs navArgs)
        {
            Debug.WriteLine("MainPage::OnNavigatedTo");

            MakePinWebAPICall();

        }

        private async void timerCallback(object state)
        {
            Debug.WriteLine("\nMainPage::timerCallback");
            if (mcp3008 == null)
            {
                Debug.WriteLine("MainPage::timerCallback not ready");
                return;
            }

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
                    // Insert code here
                }
            );
        }

        /// <summary>
        // This will put your pin on the world map of makers using this lesson.
        // Microsoft will receive the IP address of your Raspberry Pi2
        // this will be used to determine the rough geographic location of the device, in 
        // latitude and longitude.  This information will be stored for use in generating the
        // pin map showing the location of people who have also run this sample.
        // This data will not be shared with any outside party.
        /// </summary>
        public void MakePinWebAPICall()
        {
            try
            {
                HttpClient client = new HttpClient();

                // Comment this line to opt out of the pin map.
                client.GetStringAsync("http://adafruitsample.azurewebsites.net/api?Lesson=204");
            }
            catch (Exception e)
            {
                Debug.WriteLine("Web call failed: " + e.Message);
            }
        }

    }
}
