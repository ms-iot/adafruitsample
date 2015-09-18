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
        BMP280 BMP280;
        //A web client for making web API calls
        HttpClient httpClient;

        public MainPage()
        {
            this.InitializeComponent();
        }

        //This method will be called by the application framework when the page is first loaded
        protected override async void OnNavigatedTo(NavigationEventArgs navArgs)
        {
            Debug.WriteLine("MainPage::OnNavigatedTo");
            try
            {
                //Create a new object for our barometric sensor class
                BMP280 = new BMP280();
                //Initialize the sensor
                await BMP280.Initialize();

                //Create a new web client
                httpClient = new HttpClient();

                //Create variables to store the sensor data: temperature, pressure and altitude. 
                //Initialize them to 0.
                float temp = 0;
                float pressure = 0;
                float altitude = 0;

                //Create a constant for pressure at sea level. 
                //This is based on your local sea level pressure (Unit: Hectopascal)
                const float seaLevelPressure = 1013.25f;

                //Read 10 samples of the data
                for(int i = 0; i < 10; i++)
                {
                    temp += await BMP280.ReadTemperature();
                    pressure += await BMP280.ReadPreasure(); 
                    altitude += await BMP280.ReadAltitude(seaLevelPressure);
                }
                
                //Write the average values to your debug console
                Debug.WriteLine("Temperature: " + (temp/10).ToString() + " deg C");
                Debug.WriteLine("Pressure: " + (pressure/10).ToString() + " Pa");
                Debug.WriteLine("Altitude: " + (altitude/10).ToString() + " m");

                //Create the grouped content to send to the web API
                FormUrlEncodedContent data = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("Lesson", "203"),
                    new KeyValuePair<string, string>("Temperature", (temp/10).ToString() + " deg C"),
                    new KeyValuePair<string, string>("Pressure", (pressure/10).ToString() + " Pa"),
                    new KeyValuePair<string, string>("Altitude", (altitude/10).ToString() + " m")
                });

                //Create a response message to post the data
                HttpResponseMessage response = await httpClient.PostAsync(
                                                    "http://adafruitsample.azurewebsites.net/api", data);
                //Ensure the data was posted successfully
                response.EnsureSuccessStatusCode();

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
