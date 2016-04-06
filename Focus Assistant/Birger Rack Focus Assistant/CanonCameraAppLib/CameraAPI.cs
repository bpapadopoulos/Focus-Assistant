using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EDSDKLib;

namespace CanonCameraAppLib
{
    public sealed class CameraAPI
    {
        #region Private Variables

        private EDSDK.EdsCameraAddedHandler edsCameraAddedHandler;
        List<Camera> cameras = new List<Camera>();
        private Dictionary<int, double> apertureDictionary = new Dictionary<int, double>();
        private Dictionary<int, int> apertureInverseDictionary = new Dictionary<int, int>();

        #endregion

        #region Registered Events

        /// <summary>
        /// Registers camera added events
        /// </summary>
        private void registerEvents()
        {
            edsCameraAddedHandler = new EDSDK.EdsCameraAddedHandler(cameraAddedHandler);

            uint error = EDSDK.EdsSetCameraAddedHandler(edsCameraAddedHandler, IntPtr.Zero);

            if (EDSDK.EDS_ERR_OK != error)
            {
                throw new CameraEventRegistrationException("Unable to register cameraAddedEventHandler!", error);
            }
        }

        /// <summary>
        /// Handler for when a camera is connected
        /// </summary>
        /// <param name="inContext">Unmodified IntPtr passed in at event registration.</param>
        /// <returns></returns>
        public uint cameraAddedHandler(IntPtr inContext)
        {
            fireCameraAddedEvent();
            return 0x0;
        }

        #endregion

        #region Public Events

        /// <summary>
        /// Occurs when a camera is connected to the host
        /// </summary>
        public static event CameraAddedEventHandler OnCameraAdded;

        #endregion

        #region Event Invokers

        /// <summary>
        /// Invokes the OnCameraAdded event
        /// </summary>
        private static void fireCameraAddedEvent()
        {
            if (OnCameraAdded != null)
            {
                OnCameraAdded(new CameraAddedEventArgs());
            }
        }

        #endregion

        #region Instance Methods and Properties

        /// <summary>
        /// Get the list of Camera objects attached to the host
        /// </summary>
        /// <exception cref="CameraException"></exception>    
        /// <exception cref="CameraNotFoundException"></exception>
        /// <exception cref="CameraCommunicationException"></exception>            
        public List<Camera> Cameras
        {
            get
            {
                return cameras;
            }
        }

        public void RefreshCameras()
        {
            cameras.Clear();

            try
            {
                IntPtr cameraList = new IntPtr();

                uint error = EDSDK.EdsGetCameraList(out cameraList);
                if (EDSDK.EDS_ERR_OK != error)
                {
                    throw new CameraException("Unable to get camera list!", error);
                }
                else
                {
                    int cameraCount = 0;
                    error = EDSDK.EdsGetChildCount(cameraList, out cameraCount);

                    if (EDSDK.EDS_ERR_OK != error)
                    {
                        throw new CameraException("Unable to get camera count!", error);
                    }
                    else
                    {
                        if (cameraCount <= 0)
                        {
                            throw new CameraNotFoundException("No camera was detected to be connected to the host.");
                        }

                        for (int i = 0; i < cameraCount; i++)
                        {
                            IntPtr cameraDev = new IntPtr(i);

                            error = EDSDK.EdsGetChildAtIndex(cameraList, i, out cameraDev);
                            if (EDSDK.EDS_ERR_OK != error)
                            {
                                throw new CameraException("Unable to get camera at index (" + i + ")", error);
                            }

                            EDSDK.EdsDeviceInfo deviceInfo;
                            error = EDSDK.EdsGetDeviceInfo(cameraDev, out deviceInfo);

                            if (EDSDK.EDS_ERR_OK != error)
                            {
                                throw new CameraException("Unable to get device information at index (" + i + ")", error);
                            }

                            Camera camera = new Camera(cameraDev);

                            if (!camera.SessionOpened)
                            {
                                error = EDSDK.EdsOpenSession(cameraDev);

                                if (EDSDK.EDS_ERR_OK != error)
                                {
                                    switch (error)
                                    {
                                        case EDSDK.EDS_ERR_DEVICE_NOT_FOUND:
                                            throw new CameraCommunicationException("Unable to open session with camera at index (" + i + ") [" + deviceInfo.szDeviceDescription + "] because it was not found!");

                                        default:
                                            throw new CameraCommunicationException("Unable to open session with camera at index (" + i + ") [" + deviceInfo.szDeviceDescription + "]!", error);
                                    }
                                }
                                else
                                {
                                    camera.SessionOpened = true;
                                }
                            }

                            camera.Name = deviceInfo.szDeviceDescription;
                            camera.PortName = deviceInfo.szPortName;

                            //  Firmware Version
                            EDSDK.EdsTime firmwareDate;
                            error = EDSDK.EdsGetPropertyData(cameraDev, EDSDK.PropID_FirmwareVersion, 0, out firmwareDate);
                            if (EDSDK.EDS_ERR_OK == error)
                            {
                                camera.FirmwareVersion = firmwareDate.ToString();
                            }
                            else
                            {
                                camera.FirmwareVersion = CameraConstants.PROPERTY_UNAVAILABLE;
                            }

                            //  Serial Number
                            uint serialNumber;
                            error = EDSDK.EdsGetPropertyData(cameraDev, EDSDK.PropID_BodyIDEx, 0, out serialNumber);
                            if (EDSDK.EDS_ERR_OK == error)
                            {
                                camera.SerialNumber = serialNumber.ToString();
                            }
                            else
                            {
                                camera.SerialNumber = CameraConstants.PROPERTY_UNAVAILABLE;
                            }

                            cameras.Add(camera);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        public Dictionary<int, double> ApertureDictionary
        {
            get
            {
                return apertureDictionary;
            }
        }

        public Dictionary<int, int> ApertureInverseDictionary
        {
            get
            {
                return apertureInverseDictionary;
            }

            set
            {
                apertureInverseDictionary = value;
            }
        }

        #endregion

        private void buildApertureDictionary()
        {
            try
            {
                apertureDictionary.Clear();

                apertureDictionary.Add(8, 1);
                apertureDictionary.Add(11, 1.1);
                apertureDictionary.Add(12, 1.2);
                apertureDictionary.Add(13, 1.2);
                apertureDictionary.Add(16, 1.4);
                apertureDictionary.Add(19, 1.6);
                apertureDictionary.Add(20, 1.8);
                apertureDictionary.Add(21, 1.8);
                apertureDictionary.Add(24, 2);
                apertureDictionary.Add(27, 2.2);
                apertureDictionary.Add(28, 2.5);
                apertureDictionary.Add(29, 2.5);
                apertureDictionary.Add(32, 2.8);
                apertureDictionary.Add(35, 3.2);
                apertureDictionary.Add(36, 3.5);
                apertureDictionary.Add(37, 3.5);
                apertureDictionary.Add(40, 4);
                apertureDictionary.Add(43, 4.5);
                apertureDictionary.Add(44, 4.5);
                apertureDictionary.Add(45, 5.0);
                apertureDictionary.Add(48, 5.6);
                apertureDictionary.Add(51, 6.3);
                apertureDictionary.Add(52, 6.7);
                apertureDictionary.Add(53, 7.1);
                apertureDictionary.Add(56, 8);
                apertureDictionary.Add(59, 9);
                apertureDictionary.Add(60, 9.5);
                apertureDictionary.Add(61, 10);
                apertureDictionary.Add(64, 11);
                apertureDictionary.Add(67, 13);
                apertureDictionary.Add(68, 13);
                apertureDictionary.Add(69, 14);
                apertureDictionary.Add(72, 16);
                apertureDictionary.Add(75, 18);
                apertureDictionary.Add(76, 19);
                apertureDictionary.Add(77, 20);
                apertureDictionary.Add(80, 22);
                apertureDictionary.Add(83, 25);
                apertureDictionary.Add(84, 27);
                apertureDictionary.Add(85, 29);
                apertureDictionary.Add(88, 32);
                apertureDictionary.Add(91, 36);
                apertureDictionary.Add(92, 38);
                apertureDictionary.Add(93, 40);
                apertureDictionary.Add(96, 45);
                apertureDictionary.Add(99, 51);
                apertureDictionary.Add(100, 54);
                apertureDictionary.Add(101, 57);
                apertureDictionary.Add(104, 64);
                apertureDictionary.Add(107, 72);
                apertureDictionary.Add(108, 76);
                apertureDictionary.Add(109, 80);
                apertureDictionary.Add(112, 91);
                apertureDictionary.Add(268435455, 268435455);
            }
            catch (Exception ex)
            {

            }
        }

        #region Singleton

        private static readonly CameraAPI cameraApi = new CameraAPI();

        /// <summary>
        /// Get instance of a CameraAPI object
        /// </summary>
        public static CameraAPI Instance
        {
            get { return cameraApi; }
        }

        /// <summary>
        /// Constructor that initializes the EDSDK SDK and registers events.
        /// </summary>
        private CameraAPI()
        {
            uint error = EDSDK.EdsInitializeSDK();
            if (EDSDK.EDS_ERR_OK != error)
            {
                throw new CameraException("Unable to initialize SDK.", error);
            }

            registerEvents();
            buildApertureDictionary();
        }

        #endregion

        #region Destructor
        /// <summary>
        /// Explicit destructor for unregistering unmanaged code events and objects
        /// </summary>
        /// <exception cref="CameraException"></exception>
        ~CameraAPI()
        {
            //  Unhook events
            this.edsCameraAddedHandler = null;

            //  Terminate anything open to the SDK
            uint error = EDSDK.EdsTerminateSDK();
            if (EDSDK.EDS_ERR_OK != error)
            {
                throw new CameraException("Unable to terminate the SDK.", error);
            }
        }
        #endregion
    }
}