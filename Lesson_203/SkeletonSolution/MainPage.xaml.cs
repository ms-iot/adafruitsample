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

        //A web client for making web API calls

        //A timer to read the sensor data regularly

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

                //Initialize the sensor

                //Create a new web

                //Create a new timer

                //Set the timer to run at 1 second intervals

                //Set the function that is called at every interval

                //Start the timer

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        //This method will be called at regular intervals to read sensor data and send it to the web API
        private async void readDataTimer_Tick(object sender, object e)
        {
            //Create variables to store the sensor data: temperature, pressure and altitude. 
            //Initialize them to 0.         


            //Create a constant for pressure at sea level. 
            //This is based on your local sea level pressure (Unit: Hectopascal)


            try {
                //Read the temperature and write the value to your debug console

                //Read the pressure and write the value to your debug console

                //Read the altitude and write the value to your debug console

                //Create the grouped content to send to the web API

                //Create a response message to post the data

                //Ensure the data was posted successfully

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
