using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;
using Windows.UI;

namespace Lesson_205

{
    public class ColorData
    {
        public UInt16 Red { get; set; }
        public UInt16 Green { get; set; }
        public UInt16 Blue { get; set; }
        public UInt16 Clear { get; set; }
    }
    public class RgbData
    {
        public int Red { get; set; }
        public int Green { get; set; }
        public int Blue { get; set; }
    }

    class TCS34725
    {
        const byte TCS34725_Address = 0x29;

        const byte TCS34725_ENABLE = 0x00;
        const byte TCS34725_ENABLE_PON = 0x01;    /* Power on - Writing 1 activates the internal oscillator, 0 disables it */
        const byte TCS34725_ENABLE_AEN = 0x02;    /* RGBC Enable - Writing 1 actives the ADC, 0 disables it */

        const byte TCS34725_ID = 0x12;
        const byte TCS34725_CDATAL = 0x14;    /* Clear channel data */
        const byte TCS34725_CDATAH = 0x15;
        const byte TCS34725_RDATAL = 0x16;    /* Red channel data */
        const byte TCS34725_RDATAH = 0x17;
        const byte TCS34725_GDATAL = 0x18;    /* Green channel data */
        const byte TCS34725_GDATAH = 0x19;
        const byte TCS34725_BDATAL = 0x1A;    /* Blue channel data */
        const byte TCS34725_BDATAH = 0x1B;
        const byte TCS34725_ATIME = 0x01;   // Integration time
        const byte TCS34725_CONTROL = 0x0F;    /* Set the gain level for the sensor */

        const byte TCS34725_COMMAND_BIT = 0x80;  // Have to | addresses with this value when asking for values

        const string I2CControllerName = "I2C1";

        private I2cDevice colorSensor = null;

        private GpioController gpio;
        private GpioPin LedControlGPIOPin;
        private int LedControlPin;

        public struct KnownColor
        {
            public Color colorValue;
            public string colorName;

            public KnownColor(Color value, string name)
            {
                colorValue = value;
                colorName = name;
            }
        };
        private string[] limitColorList = { "Black", "White", "Blue", "Red", "Green", "Purple", "Yellow", "Orange", "DarkSlateBlue", "DarkGray", "Pink" };
        private List<KnownColor> colorList;

        // We will default the led control pin to GPIO12 (Pin 32)
        public TCS34725(int ledControlPin = 12)
        {
            Debug.WriteLine("New TCS34725");
            LedControlPin = ledControlPin;
        }

        public async Task Initialize()
        {
            Debug.WriteLine("TCS34725::Initialize");

            try
            {
                var settings = new I2cConnectionSettings(TCS34725_Address);
                settings.BusSpeed = I2cBusSpeed.FastMode;

                string aqs = I2cDevice.GetDeviceSelector(I2CControllerName);
                var dis = await DeviceInformation.FindAllAsync(aqs);
                colorSensor = await I2cDevice.FromIdAsync(dis[0].Id, settings);

                // Now setup the LedControlPin
                gpio = GpioController.GetDefault();

                LedControlGPIOPin = gpio.OpenPin(LedControlPin);
                LedControlGPIOPin.SetDriveMode(GpioPinDriveMode.Output);

                initColorList();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception: " + e.Message + "\n" + e.StackTrace);
                throw;
            }

        }

        public enum eLedState { On, Off };
        private eLedState _LedState = eLedState.On; // board defaults to on
        public eLedState LedState
        {
            get { return _LedState; }
            set
            {
                Debug.WriteLine("TCS34725::LedState::set");
                if (LedControlGPIOPin != null)
                {
                    GpioPinValue newValue = (value == eLedState.On ? GpioPinValue.High : GpioPinValue.Low);
                    LedControlGPIOPin.Write(newValue);
                    _LedState = value;
                }
            }
        }

        enum eTCS34725IntegrationTime
        {
            TCS34725_INTEGRATIONTIME_2_4MS = 0xFF,   /**<  2.4ms - 1 cycle    - Max Count: 1024  */
            TCS34725_INTEGRATIONTIME_24MS = 0xF6,   /**<  24ms  - 10 cycles  - Max Count: 10240 */
            TCS34725_INTEGRATIONTIME_50MS = 0xEB,   /**<  50ms  - 20 cycles  - Max Count: 20480 */
            TCS34725_INTEGRATIONTIME_101MS = 0xD5,   /**<  101ms - 42 cycles  - Max Count: 43008 */
            TCS34725_INTEGRATIONTIME_154MS = 0xC0,   /**<  154ms - 64 cycles  - Max Count: 65535 */
            TCS34725_INTEGRATIONTIME_700MS = 0x00    /**<  700ms - 256 cycles - Max Count: 65535 */
        };

        eTCS34725IntegrationTime _tcs34725IntegrationTime = eTCS34725IntegrationTime.TCS34725_INTEGRATIONTIME_700MS;

        enum eTCS34725Gain
        {
            TCS34725_GAIN_1X = 0x00,   /**<  No gain  */
            TCS34725_GAIN_4X = 0x01,   /**<  2x gain  */
            TCS34725_GAIN_16X = 0x02,   /**<  16x gain */
            TCS34725_GAIN_60X = 0x03    /**<  60x gain */
        };

        eTCS34725Gain _tcs34725Gain = eTCS34725Gain.TCS34725_GAIN_1X;


        bool Init = false;
        private async Task begin()
        {
            Debug.WriteLine("TCS34725::begin");
            byte[] WriteBuffer = new byte[] { TCS34725_ID | TCS34725_COMMAND_BIT };
            byte[] ReadBuffer = new byte[] { 0xFF };

            // Check the device signature
            colorSensor.WriteRead(WriteBuffer, ReadBuffer);
            Debug.WriteLine("TCS34725 Signature: " + ReadBuffer[0].ToString());

            if (ReadBuffer[0] != 0x44)
            {
                Debug.WriteLine("TCS34725::begin Signature MISMATCH!");
                return;
            }

            Init = true;

            // Set the default integration time
            setIntegrationTime(_tcs34725IntegrationTime);

            // set default gain
            setGain(_tcs34725Gain);

            // Note: By default the device is in power down mode on bootup so need to enable it.
            await Enable();
        }

        private async void setGain(eTCS34725Gain gain)
        {
            Debug.WriteLine("TCS34725::setGain");
            if (!Init) await begin();

            _tcs34725Gain = gain;
            byte[] WriteBuffer = new byte[] { TCS34725_CONTROL | TCS34725_COMMAND_BIT, (byte)_tcs34725Gain };

            colorSensor.Write(WriteBuffer);
        }

        private async void setIntegrationTime(eTCS34725IntegrationTime integrationTime)
        {
            Debug.WriteLine("TCS34725::setIntegrationTime");
            if (!Init) await begin();

            _tcs34725IntegrationTime = integrationTime;
            byte[] WriteBuffer = new byte[] { TCS34725_ATIME | TCS34725_COMMAND_BIT, (byte)_tcs34725IntegrationTime };

            colorSensor.Write(WriteBuffer);
        }

        private void initColorList()
        {
            colorList = new List<KnownColor>();

            foreach (PropertyInfo property in typeof(Colors).GetProperties())
            {
                if (limitColorList.Contains(property.Name))
                {
                    KnownColor temp = new KnownColor((Color)property.GetValue(null), property.Name);
                    colorList.Add(temp);
                }
            }
        }
        /**************************************************************************/
        /*!
            Enables the device
        */
        /**************************************************************************/
        public async Task Enable()
        {
            Debug.WriteLine("TCS34725::enable");
            if (!Init) await begin();

            byte[] WriteBuffer = new byte[] { 0x00, 0x00 };

            // Enable register 
            WriteBuffer[0] = TCS34725_ENABLE | TCS34725_COMMAND_BIT;

            // Send power on
            WriteBuffer[1] = TCS34725_ENABLE_PON;
            colorSensor.Write(WriteBuffer);

            // Pause between commands
            await Task.Delay(3);

            // Send ADC Enable
            WriteBuffer[1] = (TCS34725_ENABLE_PON | TCS34725_ENABLE_AEN);
            colorSensor.Write(WriteBuffer);
        }

        /**************************************************************************/
        /*!
            Disables the device (putting it in lower power sleep mode)
        */
        /**************************************************************************/
        public async Task Disable()
        {
            Debug.WriteLine("TCS34725::disable");
            if (!Init) await begin();

            /* Turn the device off to save power */
            byte[] WriteBuffer = new byte[] { TCS34725_ENABLE | TCS34725_COMMAND_BIT };
            byte[] ReadBuffer = new byte[] { 0xFF };

            colorSensor.WriteRead(WriteBuffer, ReadBuffer);

            byte foo = (TCS34725_ENABLE_PON | TCS34725_ENABLE_AEN);
            byte bar = (byte)~foo;
            bar &= ReadBuffer[0];
            byte[] OffBuffer = new byte[] { TCS34725_ENABLE, bar };

            colorSensor.Write(OffBuffer);
        }

        UInt16 ColorFromBuffer(byte[] buffer)
        {
            UInt16 color = 0x00;

            color = buffer[1];
            color <<= 8;
            color |= buffer[0];

            return color;
        }

        public async Task<ColorData> getRawData()
        {
            ColorData colorData = new ColorData();

            if (!Init) await begin();

            byte[] WriteBuffer = new byte[] { 0x00 };
            byte[] ReadBuffer = new byte[] { 0x00, 0x00 };

            WriteBuffer[0] = TCS34725_CDATAL | TCS34725_COMMAND_BIT;
            colorSensor.WriteRead(WriteBuffer, ReadBuffer);
            colorData.Clear = ColorFromBuffer(ReadBuffer);

            WriteBuffer[0] = TCS34725_RDATAL | TCS34725_COMMAND_BIT;
            colorSensor.WriteRead(WriteBuffer, ReadBuffer);
            colorData.Red = ColorFromBuffer(ReadBuffer);

            WriteBuffer[0] = TCS34725_GDATAL | TCS34725_COMMAND_BIT;
            colorSensor.WriteRead(WriteBuffer, ReadBuffer);
            colorData.Green = ColorFromBuffer(ReadBuffer);

            WriteBuffer[0] = TCS34725_BDATAL | TCS34725_COMMAND_BIT;
            colorSensor.WriteRead(WriteBuffer, ReadBuffer);
            colorData.Blue = ColorFromBuffer(ReadBuffer);

            Debug.WriteLine("Raw Data - red: {0}, green: {1}, blue: {2}, clear: {3}", colorData.Red, colorData.Green, colorData.Blue, colorData.Clear);

            ReadBuffer = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 };
            WriteBuffer[0] = TCS34725_CDATAL | TCS34725_COMMAND_BIT;
            colorSensor.WriteRead(WriteBuffer, ReadBuffer);

            return colorData;
        }

        public async Task<RgbData> getRgbData()
        { 
            RgbData rgbData = new RgbData();
            ColorData colorData = await getRawData();
            if (colorData.Clear > 0)
            {
                rgbData.Red = (colorData.Red * 255 / colorData.Clear);
                rgbData.Blue = (colorData.Blue * 255 / colorData.Clear);
                rgbData.Green = (colorData.Green * 255 / colorData.Clear);
            }
            Debug.WriteLine("RGB Data - red: {0}, green: {1}, blue: {2}", rgbData.Red, rgbData.Green, rgbData.Blue);

            return rgbData;
        }

        public async Task<string> getClosestColor()
        {
            RgbData rgbData = await getRgbData();
            KnownColor closestColor = colorList[7]; //Black by default
            double minDiff = double.MaxValue;

            foreach (var color in colorList)
            {
                Color colorValue = color.colorValue;
                double diff = Math.Pow((colorValue.R - rgbData.Red), 2) + Math.Pow((colorValue.G - rgbData.Green), 2) + Math.Pow((colorValue.B - rgbData.Blue), 2);
                diff = (int)Math.Sqrt(diff);
                if (diff < minDiff)
                {
                    minDiff = diff;
                    closestColor = color;
                }
            }
            Debug.WriteLine("Approximate color: " + closestColor.colorName + " - " + closestColor.colorValue.ToString());

            return closestColor.colorName;
        }
    }
}

