using System;
using System.Diagnostics;
using System.Threading.Tasks;
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
        public MainPage()
        {
            this.InitializeComponent();
        }
        BMP280_Data data;
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

            data = await BMP280.Read();
            OnReadButtonClick(null, null);
        }

        private async void OnReadButtonClick(object sender, RoutedEventArgs e)
        {
            float temp = 0;
            float pressure = 0;
            float altitude = 0;
            float seaLevelPressure = 1013.25f; //TODO: Update this based on your local sea level pressure (Unit: Hectopascal)
            for (int i = 0; i < 10; i++)
            {
                temp = await BMP280.ReadTemperature();
                Debug.WriteLine("Temperature: " + temp.ToString() + " deg C");

                pressure = await BMP280.ReadPreasure();
                Debug.WriteLine("Pressure: " + pressure.ToString() + " Pa");

                altitude = await BMP280.ReadAltitude(seaLevelPressure);
                Debug.WriteLine("Altitude: " + altitude.ToString() + " m");

                await Task.Delay(100);
            }

        }
    }
}
