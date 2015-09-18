using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace Lesson_201
{
    /// <summary>
    /// A class which wraps a GPIO interfaced LED which can get it's display state set based on
    /// a web API.
    /// </summary>
    class InternetLed
    {
        // GPIO controller code
        private GpioController gpio;
        private GpioPin LedControlGPIOPin;
        private int LedControlPin;

        // An enumeration to store the state of the led
        public enum eLedState { Off, On };
        private eLedState _LedState;

        public InternetLed(int ledControlPin = 12)
        {
            Debug.WriteLine("InternetLed::New InternetLed");

            // Store the selected GPIO control pin id for use when we initialize the GPIO
            LedControlPin = ledControlPin;
        }


        public void InitalizeLed()
        {
            Debug.WriteLine("InternetLed::InitalizeLed");

            // Now setup the LedControlPin
            gpio = GpioController.GetDefault();

            LedControlGPIOPin = gpio.OpenPin(LedControlPin);
            LedControlGPIOPin.SetDriveMode(GpioPinDriveMode.Output);

            // Get the current pin value
            GpioPinValue startingValue = LedControlGPIOPin.Read();
            _LedState = (startingValue == GpioPinValue.Low) ? eLedState.On : eLedState.Off;

        }

        // A public property for interacting with the LED from code.
        public eLedState LedState
        {
            get { return _LedState; }
            set
            {
                Debug.WriteLine("InternetLed::LedState::set " + value.ToString());
                if (LedControlGPIOPin != null)
                {
                    GpioPinValue newValue = (value == eLedState.On ? GpioPinValue.High : GpioPinValue.Low);
                    LedControlGPIOPin.Write(newValue);
                    _LedState = value;
                }
            }
        }

        // This will call an exposed web API at the indicated URL
        // the API will return the current time as a string.
        // Example return: "2015-08-31T21:56:25.766Z"
        // This will perform a web API call and then depending on the returned millisecond value
        // return an eLedState value
        // millisecond value > 5 = eLedState.On
        // anything else = eLedState.off 
        const string WebAPIURL = "http://adafruitsample.azurewebsites.net/TimeApi";
        public async Task<eLedState> MakeWebApiCall()
        {
            Debug.WriteLine("InternetLed::MakeWebApiCall");
            eLedState computedLedState = eLedState.On;

            string responseString = "No response";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Make the call
                    responseString = await client.GetStringAsync(WebAPIURL);

                    // Let us know what the returned string was
                    Debug.WriteLine(String.Format("Response string: [{0}]", responseString));
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

            // Now we are going to do something with the result so we get a blinking LED
            if (responseString[19] > '5')
            {
                // If the millisecond part of the string is greater than 5 turn on the led
                computedLedState = eLedState.On;
            }
            else
            {
                // Otherwise turn it off
                computedLedState = eLedState.Off;
            }

            // return the computed led state
            return computedLedState;
        }

    }

}
