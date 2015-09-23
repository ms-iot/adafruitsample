using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Net.Http;
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
        //A class which wraps the color sensor
        
        //A SpeechSynthesizer class for text to speech operations
        
        //A MediaElement class for playing the audio
        
        //A GPIO pin for the pushbutton
        
        //The GPIO pin number we want to use to control the pushbutton
       
        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs navArgs)
        {
            MakePinWebAPICall();

            try
            {
                //Create a new object for the color sensor class
                
                //Initialize the sensor
             

                //Create a new SpeechSynthesizer
               

                //Create a new MediaElement
                
                
                //Initialize the GPIO pin for the pushbutton
              
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        //This method is used to initialize a GPIO pin
        private void InitializeGpio()
        {
            //Create a default GPIO controller
            
            //Use the controller to open the gpio pin of given number
          
            //Debounce the pin to prevent unwanted button pressed events
     
            //Set the pin for input
   
            //Set a function callback in the event of a value change

        }

        //This method will be called everytime there is a change in the GPIO pin value
        private async void buttonPin_ValueChanged(object sender, GpioPinValueChangedEventArgs e)
        {
            //Only read the sensor value when the button is released
            if (e.Edge == GpioPinEdge.RisingEdge)
            {
                //Read the approximate color from the sensor
               
                //Output the colr name to the speaker
               
            }
        }

        //This method is used to output a string to the speaker
        private async Task SpeakColor(string colorRead)
        {
            //Create a SpeechSynthesisStream using a string
          
            //Use a dispatcher to play the audio
            var ignored = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                //Set the souce of the MediaElement to the SpeechSynthesisStream
               
                //Play the stream
               
            });
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
                client.GetStringAsync("http://adafruitsample.azurewebsites.net/api?Lesson=205");
            }
            catch (Exception e)
            {
                Debug.WriteLine("Web call failed: " + e.Message);
            }
        }
    }
}

