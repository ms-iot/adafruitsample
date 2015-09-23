using System;
using System.Diagnostics;
using System.Net.Http;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Lesson_203
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //A class which wraps the barometric sensor
        
        public MainPage()
        {
            this.InitializeComponent();
        }

        //This method will be called by the application framework when the page is first loaded
        protected override async void OnNavigatedTo(NavigationEventArgs navArgs)
        {
            Debug.WriteLine("MainPage::OnNavigatedTo");

            MakePinWebAPICall();
            try
            {
                //Create a new object for our barometric sensor class
                
                //Initialize the sensor
                              

                //Create variables to store the sensor data: temperature, pressure and altitude. 
                //Initialize them to 0.
               

                //Create a constant for pressure at sea level. 
                //This is based on your local sea level pressure (Unit: Hectopascal)              

                //Read 10 samples of the data
                
                //Write the values to your debug console                            

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
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
            HttpClient client = new HttpClient();

            // Comment this line to opt out of the pin map.
            client.GetStringAsync("http://adafruitsample.azurewebsites.net/api?Lesson=203");
        }
    }
}
