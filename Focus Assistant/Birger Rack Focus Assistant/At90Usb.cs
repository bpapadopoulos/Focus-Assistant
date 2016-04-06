using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Birger_Rack_Focus_Assistant
{
    public class At90Usb
    {
        private const UInt32 VendorID = 0x0417;
        private const UInt32 ProductID = 0xDD03;

        public UInt32 Vendor_ID
        {
            get
            {
                return VendorID;
            }
        }
        public UInt32 Product_ID
        {
            get
            {
                return ProductID;
            }
        }

        double _currentMeasurement = 0;
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

        [DllImport("AtUsbHid.dll")]
        private static extern Boolean findHidDevice(UInt32 VendorID, UInt32 ProductID);
        public Boolean find_Hid_Device()
        {
            return findHidDevice(VendorID, ProductID);

        }

        [DllImport("AtUsbHid.dll")]
        private static extern void closeDevice();
        public void close_Device()
        {
            try
            {
                closeDevice();
            }
            catch
            {

            }
        }

        [DllImport("AtUsbHid.dll")]
        public static extern Boolean writeData(byte[] buffer);
        public bool WriteData(byte[] buffer)
        {
            return writeData(buffer);
        }

        [DllImport("AtUsbHid.dll")]
        private static extern Boolean readData(byte[] buffer);
        public bool ReadData(byte[] buffer)
        {
            bool retval = false;
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            do
            {
                retval = readData(buffer);
            } while (!retval && stopWatch.ElapsedMilliseconds < 20000);

            if (retval)
                _currentMeasurement = (buffer[2] * 256) + buffer[1];
            else
                _currentMeasurement = 0;

            return retval;
        }

        [DllImport("AtUsbHid.dll")]
        private static extern int hidRegisterDeviceNotification(IntPtr hWnd);

        [DllImport("AtUsbHid.dll")]
        private static extern void hidUnregisterDeviceNotification(IntPtr hWnd);

        [DllImport("AtUsbHid.dll")]
        private static extern int isMyDeviceNotification(UInt32 dwData);

        [DllImport("AtUsbHid.dll")]
        private static extern Boolean setFeature(byte type, byte direction, UInt32 length);
    }
}
