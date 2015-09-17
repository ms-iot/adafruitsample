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
                

                //Create a new web client                

                //Create variables to store the sensor data: temperature, pressure and altitude. 
                //Initialize them to 0.
               

                //Create a constant for pressure at sea level. 
                //This is based on your local sea level pressure (Unit: Hectopascal)
                

                //Read 10 samples of the data
                

                //Write the average values to your debug console
                

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
