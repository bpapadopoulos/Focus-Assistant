using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;

namespace Birger_Rack_Focus_Assistant
{
    public class CustomSerialDataReceivedEventArgs : EventArgs
    {
        private string _data = string.Empty;

        public CustomSerialDataReceivedEventArgs(string data)
        {
            _data = data;
        }

        public string Data 
        {
            get
            {
                return _data;
            }
            set
            {
                _data = value;
            }
        }
    }

    public class BirgerSerial
    {
        private SerialPort serialPort = new SerialPort("COM3", 115200, Parity.None, 8, StopBits.One);
        public delegate void CustomSerialDataReceivedEventHandler(object sender, CustomSerialDataReceivedEventArgs e);
        public event CustomSerialDataReceivedEventHandler DataReceived;
        private string _data = string.Empty;

        List<string> _outputList = new List<string>();

        public BirgerSerial()
        {
            serialPort.DataReceived += new SerialDataReceivedEventHandler(serialPort_DataReceived);
        }

        public void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string response = sp.ReadExisting();
            DataReceived(this, new CustomSerialDataReceivedEventArgs(response));
        }

        public List<string> OutputList
        {
            get
            {
                return _outputList;
            }
        }

        public void Open()
        {
            try
            {
                serialPort.Open();
            }
            catch (Exception ex)
            {
                int retryCount = 0;
                do
                {
                    if (!serialPort.IsOpen)
                    {
                        try
                        {
                            serialPort.DiscardInBuffer();
                        }
                        catch
                        {

                        }
                        serialPort.RtsEnable = false;
                        serialPort.DtrEnable = false;
                        serialPort.Close();
                        Thread.Sleep(2000);
                        try
                        {
                            serialPort.Open();
                        }
                        catch
                        {

                        }
                    }
                    retryCount++;
                } while (retryCount < 3 && !serialPort.IsOpen);
            }
        }

        public void Close()
        {
            serialPort.DiscardInBuffer();
            serialPort.RtsEnable = false;
            serialPort.DtrEnable = false;
            serialPort.Close();
        }

        public bool IsOpen()
        {
            return serialPort.IsOpen;
        }

        public void Write(string message)
        {
            try
            {
                if (this.IsOpen())
                    serialPort.Write(message + "\r");
            }
            catch (Exception ex)
            {

            }
        }
    }
}
