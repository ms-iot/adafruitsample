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
        BMP280 BMP280 = new BMP280();
        HttpClient httpClient = new HttpClient();
        static DispatcherTimer readDataTimer;

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs navArgs)
        {
            Debug.WriteLine("MainPage::OnNavigatedTo");
            try
            {
                await BMP280.Initialize();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

            readDataTimer = new DispatcherTimer();
            readDataTimer.Interval = TimeSpan.FromSeconds(1);
            readDataTimer.Tick += readDataTimer_Tick;
            readDataTimer.Start();
        }

        private async void readDataTimer_Tick(object sender, object e)
        {
            float temp = 0;
            float pressure = 0;
            float altitude = 0;
            float seaLevelPressure = 1013.25f; //TODO: Update this based on your local sea level pressure (Unit: Hectopascal)

            temp = await BMP280.ReadTemperature();
            Debug.WriteLine("Temperature: " + temp.ToString() + " deg C");

            pressure = await BMP280.ReadPreasure();
            Debug.WriteLine("Pressure: " + pressure.ToString() + " Pa");

            altitude = await BMP280.ReadAltitude(seaLevelPressure);
            Debug.WriteLine("Altitude: " + altitude.ToString() + " m");
            var data = new FormUrlEncodedContent(new[]
            {
                    new KeyValuePair<string, string>("Lesson", "203"),
                    new KeyValuePair<string, string>("Temperature" , temp.ToString() + " deg C"),
                    new KeyValuePair<string, string>("Pressure", pressure.ToString() + " Pa"),
                    new KeyValuePair<string, string>("Altitude", altitude.ToString() + " m")
            });
            var response = await httpClient.PostAsync("http://adafruitsample.azurewebsites.net/api", data);
            response.EnsureSuccessStatusCode();
        }
    }
}
