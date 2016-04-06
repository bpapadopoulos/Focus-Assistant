using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;

namespace Birger_Rack_Focus_Assistant
{
    public class SonarDataReceivedEventArgs : EventArgs
    {
        public string SonarData { get; set; }
    }

    public class SonarRangeFinder
    {
        //private SerialPort serialPort = new SerialPort("COM9", 57600, Parity.None, 8, StopBits.One);
        private SerialPort serialPort = new SerialPort("COM8", 57600, Parity.None, 8, StopBits.One);
        //private SerialPort serialPort = new SerialPort("COM7", 57600, Parity.None, 8, StopBits.One);
        public delegate void SonarDataReceivedEventHandler(object sender, SonarDataReceivedEventArgs e);
        public event SonarDataReceivedEventHandler SonarDataReceived;
        private double _currentMeasurement = 0;

        public SonarRangeFinder()
        {
            serialPort.DataReceived += new SerialDataReceivedEventHandler(serialPort_DataReceived);
        }

        public double CurrentMeasurement
        {
            get
            {
                return _currentMeasurement;
            }
            set
            {
                _currentMeasurement = value;
            }
        }

        void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string response = sp.ReadTo("\r");
            response = response.Substring(1, 3);
            _currentMeasurement = 0.0328084 * double.Parse(response);
            var copy = SonarDataReceived;
            SonarDataReceived(this, new SonarDataReceivedEventArgs { SonarData = String.Format("{0:0.00}", _currentMeasurement) + "ft" });
        }

        public bool Open()
        {
            try
            {
                serialPort.Open();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public void Close()
        {
            serialPort.Close();
        }

        public bool IsOpen()
        {
            return serialPort.IsOpen;
        }
    }
}
