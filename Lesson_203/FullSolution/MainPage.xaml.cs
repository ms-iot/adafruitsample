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
        //A timer to read the sensor data regularly
        static DispatcherTimer readDataTimer;

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

                //Create a new web
                httpClient = new HttpClient();

                //Create a new timer
                readDataTimer = new DispatcherTimer();
                //Set the timer to run at 1 second intervals
                readDataTimer.Interval = TimeSpan.FromSeconds(1);
                //Set the function that is called at every interval
                readDataTimer.Tick += readDataTimer_Tick;
                //Start the timer
                readDataTimer.Start();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        //This method will be called at regular intervals to read sensor data and send it to the web API
        private async void readDataTimer_Tick(object sender, object e)
        {
            //Create variables to store the sensor data: temperature, pressure and altitude. Initialize them to 0.
            float temp = 0;
            float pressure = 0;
            float altitude = 0;

            //Create a constant for pressure at sea level. This is based on your local sea level pressure (Unit: Hectopascal)
            const float seaLevelPressure = 1013.25f;

            try {
                //Read the temperature and write the value to your debug console
                temp = await BMP280.ReadTemperature();
                Debug.WriteLine("Temperature: " + temp.ToString() + " deg C");

                //Read the pressure and write the value to your debug console
                pressure = await BMP280.ReadPreasure();
                Debug.WriteLine("Pressure: " + pressure.ToString() + " Pa");

                //Read the altitude and write the value to your debug console
                altitude = await BMP280.ReadAltitude(seaLevelPressure);
                Debug.WriteLine("Altitude: " + altitude.ToString() + " m");

                //Create the grouped content to send to the web API
                FormUrlEncodedContent data = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("Lesson", "203"),
                    new KeyValuePair<string, string>("Temperature" , temp.ToString() + " deg C"),
                    new KeyValuePair<string, string>("Pressure", pressure.ToString() + " Pa"),
                    new KeyValuePair<string, string>("Altitude", altitude.ToString() + " m")
                });

                //Create a response message to post the data
                HttpResponseMessage response = await httpClient.PostAsync("http://adafruitsample.azurewebsites.net/api", data);
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
