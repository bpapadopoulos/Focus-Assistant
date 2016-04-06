
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using ManagedWinapi;
using ManagedWinapi.Hooks;
using ManagedWinapi.Windows;
using WindowsInput;
using System.Xml.Serialization;
using System.IO;
using System.Windows;
using System.Globalization;
using System.Text.RegularExpressions;
using CanonCameraAppLib;

namespace Birger_Rack_Focus_Assistant
{
    public partial class frmFocusPuller : Form
    {
        [DllImport("user32.dll")]
        static extern IntPtr FindWindow(IntPtr classname, string title);

        Stopwatch stopWatch = new Stopwatch();
        Stopwatch liveViewWatch = new Stopwatch();
        Stopwatch prgBarFocusWatch = new Stopwatch();
        bool prgBarFocusWatchLogged = false;
        FocusMoveList focusMoves = new FocusMoveList();
        string currentXML = string.Empty;
        FocusMove currentFocusMove = new FocusMove();
        FocusMove currentFocusMoveTemp = new FocusMove();
        bool enableBeeps = false;
        string xmlDirectory = Environment.CurrentDirectory + "\\FocusMoves";
        bool recording = false;
        Stopwatch recordingStopWatch = new Stopwatch();
        Thread rackFocusThread;
        Thread rackFocusBatchThread;
        bool rackingFocus = false;
        bool rackingFocusBatch = false;
        bool focusAborted = false;
        Logger logger = new Logger("E:\\Temp\\");
        bool errorHandled = false;

        BirgerSerial birgerSerial = new BirgerSerial();
        BackgroundWorker bwIsSerialOpen = new BackgroundWorker();
        BackgroundWorker bwWaitButton = new BackgroundWorker();
        bool apertureNeedsInitialization = false;

        int aperturePosValue = -1;
        bool initialApertureSet = false;

        int focusEncoderValue = -1;
        double positionToEncoderRatio = 0;
        bool initialFocusSet = false;
        int focusPosValue = -1;

        List<string> distStopList = new List<string>();
        public Dictionary<int, int> trackbarDictionary = new Dictionary<int, int>();
        public Dictionary<int, double> encoderDictionary = new Dictionary<int, double>();
        public Dictionary<double, int> feetDictionary = new Dictionary<double, int>();
        public Dictionary<double, int> majorDistDictionary = new Dictionary<double, int>();
        public EncoderItemList encoderItems = new EncoderItemList();
        public string currentLensName;
        string lensDirectory = Environment.CurrentDirectory + "\\LensInfo";
        double currentFocalLength = 0;
        double currentFstop = 0;
        public double hyperFocal = 0;
        double totalEncoderSteps = 0;
        public bool encoderDictionaryLoaded = false;
        public List<double> fStopList = new List<double>();
        bool booting = true;
        bool dslrMode = false;
        bool liveViewNotDetected = false;
        bool liveView = false;
        bool apertureNotDetected = false;
        bool tbIrisMoving = false;

        public bool pauseLensPlay = false;
        Stopwatch pauseLensPlayWatch = new Stopwatch();

        CameraAPI cameraAPI;

        int smallDelay = 2000;
        int medDelay = 2000;
        int lrgDelay = 2000;
        int prevDelay = 2000;
        public int tbFocusBuffer = 0;
        int tbIrisBuffer = 0;
        public Queue<double> commandQueue = new Queue<double>();
        Stopwatch focusBarWatch = new Stopwatch();
        Stopwatch irisBarWatch = new Stopwatch();
        Stopwatch lensPlayWatch = new Stopwatch();
        double stepCounter = 0;
        double stepCounterEx = 0;
        BackgroundWorker bgWorkerStepCount = new BackgroundWorker();
        public Thread lensPlayManagerThread;
        StringBuilder loggerStrBuilder = new StringBuilder();
        bool queueExecuting = false;

        int farStepCounter = 0;
        int nearStepCounter = 0;

        double encoderRemainderFar = 0;
        double encoderRemainderNear = 0;

        bool rackingNear = false;
        bool rackingFar = false;
        bool directionChanged = false;

        public bool lensPlayCompensationEnabled = true;
        public bool isBenchmarking = false;
        public double lensPlayCompensationRatio = 0;
        public int compensationAdjustments = 0;

        public frmFocusPuller()
        {
            InitializeComponent();

            //Instantiate the DSLR
            cameraAPI = CameraAPI.Instance;
            //Wire up the camera added event handler
            CameraAPI.OnCameraAdded += new CameraAddedEventHandler(CameraAPI_OnCameraAdded);

            //Background worker that updates the stepcounter text boxes
            bgWorkerStepCount.DoWork += new DoWorkEventHandler(bgWorkerStepCount_DoWork);
            bgWorkerStepCount.WorkerSupportsCancellation = true;

            //Background thread monitoring whether the iris is not currently moving, making it safe to update iris dependent controls
            Thread irisMovingThread = new Thread(CheckIrisMoving);
            irisMovingThread.IsBackground = true;
            irisMovingThread.Start();

            //Background thread that constantly executes the DSLR lens command queue
            Thread executeQueueThread = new Thread(ExecuteQueue);
            executeQueueThread.IsBackground = true;
            executeQueueThread.Start();

            //Background worker that constantly checks the position of the focus track bar and subsequently builds a command queue based on its deltas
            Thread focusBarMonitorThread = new Thread(FocusBarMonitor);
            focusBarMonitorThread.IsBackground = true;
            focusBarMonitorThread.Start();

            //Background worker that constantly checks the position of the iris track bar and sends update commands to the camera
            Thread irisBarMonitorThread = new Thread(IrisBarMonitor);
            irisBarMonitorThread.IsBackground = true;
            irisBarMonitorThread.Start();

            //Background thread that corrects the lens if it is manually set too far past its minimum focus
            //Also slowly makes its way to minimum focus if the user does not do so themselves on startup
            //Only starts after DSLR controls are updated and the encoder dictionary is loaded
            lensPlayManagerThread = new Thread(LensPlayManager);
            lensPlayManagerThread.IsBackground = true;

            //Starts helper stopwatches
            focusBarWatch.Start();
            irisBarWatch.Start();
            lensPlayWatch.Start();
            prgBarFocusWatch.Start();

            //Clears camera collection if any and attempts to detect current attached cameras
            cameraAPI.RefreshCameras();
            //If a camera is detected, wire up the shutdown and property changed event handlers. Then update the DSLR controls
            if (cameraAPI.Cameras != null)
            {
                if (cameraAPI.Cameras.Count > 0)
                {
                    foreach (Camera camera in cameraAPI.Cameras)
                    {
                        camera.OnShutdown += new ShutdownEventHandler(camera_OnShutdown);
                        camera.OnPropertyChanged += new CanonCameraAppLib.PropertyChangedEventHandler(camera_OnPropertyChanged);
                    }

                    UpdateDslrControls();
                }
            }

            //Moves window to center
            this.CenterToScreen();

            //Clears the form values and resets the buttons
            ClearForm(true);

            //Initialize the current focus move's duration
            currentFocusMove.FocusDuration = TimeSpan.MinValue;

            //Wire up the birger serial data recieved event handler
            birgerSerial.DataReceived += new BirgerSerial.CustomSerialDataReceivedEventHandler(birgerSerial_DataReceived);
            //Wire up the background worker that checks if the birger serial port is open and enables the UI if it is and disables it if it isnt
            bwIsSerialOpen.DoWork += new DoWorkEventHandler(bwIsSerialOpen_DoWork);
            bwIsSerialOpen.WorkerSupportsCancellation = true;
            //Wire up the background worker that puts the Connect button into busy state
            bwWaitButton.DoWork += new DoWorkEventHandler(bwWaitButton_DoWork);
            bwWaitButton.WorkerSupportsCancellation = true;

            if (birgerSerial.IsOpen())
            {
                MessageBox.Show("Serial Already Open");
            }
        }

        void bgWorkerStepCount_DoWork(object sender, DoWorkEventArgs e)
        {
            //Updates the stepcounter text boxes
            do
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    if (prgBarFocus.Visible)
                    {
                        if (!recording)
                        {
                            txtStepCounter.Text = stepCounter.ToString();
                            txtStepCounterEx.Text = stepCounterEx.ToString();
                            txtCurrentThumb.Text = tbFocus.Value.ToString();
                        }

                        if (stepCounterEx != 0 && stepCounterEx != 1)
                        {
                            if (stepCounter > stepCounterEx)
                            {
                                prgBarFocusWatchLogged = false;

                                if (stepCounter == 0 && stepCounterEx != 0)
                                {
                                    prgBarFocus.Value = (int)((double.Parse(stepCounterEx.ToString()) / double.Parse(stepCounter.ToString()) + 1) * 100);
                                }
                                else
                                {
                                    prgBarFocus.Value = (int)((double.Parse(stepCounterEx.ToString()) / double.Parse(stepCounter.ToString())) * 100);
                                }

                                if (stepCounterEx == stepCounter)
                                {
                                    prgBarFocus.Value = 100;
                                }
                            }
                            else
                            {
                                prgBarFocusWatchLogged = false;

                                if (stepCounter == 0 && stepCounterEx != 0)
                                {
                                    if (stepCounterEx > 100)
                                        prgBarFocus.Value = Math.Max(prgBarFocus.Minimum, (int)((1 - (stepCounterEx / 1000)) * 100));
                                    else if (stepCounterEx < 100)
                                        prgBarFocus.Value = (int)((1 - (stepCounterEx / 100)) * 100);
                                }
                                else
                                {
                                    prgBarFocus.Value = (int)((double.Parse(stepCounter.ToString()) / double.Parse(stepCounterEx.ToString())) * 100);
                                }

                                if (stepCounter == stepCounterEx)
                                {
                                    prgBarFocus.Value = 100;
                                }
                            }
                        }

                        if (stepCounter == 0 && stepCounterEx == 0)
                        {
                            prgBarFocus.Value = 0;
                        }
                    }
                });

                if (prgBarFocus.Value == 0 || prgBarFocus.Value == 100)
                {
                    if (!prgBarFocusWatchLogged)
                    {
                        //logger.LogError("prgBarFocusWatch: " + prgBarFocusWatch.ElapsedMilliseconds.ToString() + ", " + "stepCounter: " + stepCounter.ToString(), EventLogEntryType.Information);
                        prgBarFocusWatchLogged = true;
                        prgBarFocusWatch.Reset();
                        prgBarFocusWatch.Start();
                    }
                }

                Thread.Sleep(25);
            } while (!bgWorkerStepCount.CancellationPending);
        }

        void MoveCursorToError()
        {
            //Moves the cursor to error message box in the event that focusing is aborted
            int sleepCount = 0;
            do
            {
                Thread.Sleep(250);
                Cursor.Position = new Point(325, 265);

                sleepCount++;
            } while ((!errorHandled && sleepCount < 20) || (FindWindow(IntPtr.Zero, "Cancelled") != IntPtr.Zero));

            errorHandled = false;
            Thread.CurrentThread.Abort();
        }

        void HandleAbort()
        {
            //Checks if focusing has been aborted and cancels rackfocus threads and resets buttons to non busy state
            do
            {
                if (focusAborted)
                {
                    if (rackFocusThread != null)
                    {
                        rackFocusThread.Abort();
                    }
                    if (rackFocusBatchThread != null)
                    {
                        rackFocusBatchThread.Abort();
                    }

                    Thread errorHandledThread = new Thread(MoveCursorToError);
                    errorHandledThread.IsBackground = true;
                    errorHandledThread.Start();

                    FindAndMoveMsgBox("Cancelled", this.Location.X + this.Height / 2, this.Location.Y + this.Width / 8);
                    rackingFocus = false;
                    try
                    {
                        if (MessageBox.Show(this, "Focusing Aborted.", "Cancelled", MessageBoxButtons.OK) == DialogResult.OK)
                        {
                            focusAborted = false;
                            errorHandled = true;
                            try
                            {
                                btnBatchRackFocus.Enabled = true;
                                btnRackFocus.Text = "Rack Focus";
                                btnRackFocus.Height = 23;
                                btnRackFocus.ForeColor = Color.Black;
                                this.TopMost = false;
                            }
                            catch
                            {
                                BeginInvoke((MethodInvoker)delegate
                                {
                                    btnBatchRackFocus.Enabled = true;
                                    btnRackFocus.Text = "Rack Focus";
                                    btnRackFocus.Height = 23;
                                    btnRackFocus.ForeColor = Color.Black;
                                    this.TopMost = false;
                                });
                            }
                        }
                    }
                    catch
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            if (MessageBox.Show(this, "Focusing Aborted.", "Cancelled", MessageBoxButtons.OK) == DialogResult.OK)
                            {
                                focusAborted = false;
                                errorHandled = true;
                                try
                                {
                                    btnBatchRackFocus.Enabled = true;
                                    btnRackFocus.Text = "Rack Focus";
                                    btnRackFocus.Height = 23;
                                    btnRackFocus.ForeColor = Color.Black;
                                }
                                catch
                                {
                                    BeginInvoke((MethodInvoker)delegate
                                    {
                                        btnBatchRackFocus.Enabled = true;
                                        btnRackFocus.Text = "Rack Focus";
                                        btnRackFocus.Height = 23;
                                        btnRackFocus.ForeColor = Color.Black;
                                    });
                                }
                            }
                        });
                    }
                }
            } while (rackingFocus);
        }

        void LiveViewCheck()
        {
            Stopwatch liveViewWatch = new Stopwatch();
            liveViewWatch.Start();

            //Updates live view flag after checking if camera is in liveview mode
            while (!liveView && liveViewWatch.ElapsedMilliseconds < 10000)
            {
                if (cameraAPI.Cameras != null)
                {
                    if (cameraAPI.Cameras.Count > 0)
                    {
                        if (cameraAPI.Cameras[0].IsInLiveViewMode)
                        {
                            liveView = true;
                        }
                        else
                        {
                            liveView = false;
                        }
                    }
                }
                else
                {
                    liveView = false;
                }
            }

            if (cameraAPI.Cameras != null)
            {
                if (cameraAPI.Cameras.Count > 0)
                {
                    if (cameraAPI.Cameras[0].IsInLiveViewMode)
                    {
                        liveView = true;
                    }
                    else
                    {
                        liveView = false;
                    }
                }
            }
            else
            {
                liveView = false;
            }

            //Enables DSLR controls if camera is in livevew mode. Disables them if not
            if (liveView)
            {
                UpdateDslrControls();
            }

            Thread.CurrentThread.Abort();
        }

        public void camera_OnPropertyChanged(Camera sender, CanonCameraAppLib.PropertyChangedEventArgs e)
        {
            try
            {
                //Check lens status
                if (cameraAPI.Cameras.Count > 0)
                {
                    if (currentLensName != cameraAPI.Cameras[0].LensName)
                    {
                        encoderDictionaryLoaded = false;
                    }

                    //Loads the encoder dictionary if it is not currently loaded
                    if (!encoderDictionaryLoaded)
                    {
                        currentLensName = cameraAPI.Cameras[0].LensName;

                        //Don't attempt to load the encoder dictionary if no lens is present
                        if (currentLensName.Trim() != string.Empty)
                        {
                            currentFocalLength = double.Parse(currentLensName.Substring(2, currentLensName.IndexOf("mm") - 2));
                            Regex digitsOnly = new Regex(@"[^\d]");
                            currentFstop = double.Parse(digitsOnly.Replace(currentLensName.Substring(currentLensName.IndexOf("f") + 1, (currentLensName.IndexOf("f") - 2)), string.Empty)) / 10;
                            hyperFocal = (((currentFocalLength * currentFocalLength) / (0.01 * currentFstop)) + currentFocalLength) * 0.00328084;
                            if (File.Exists(lensDirectory + "\\" + currentLensName + ".xml"))
                            {
                                encoderDictionaryLoaded = true;
                                LoadEncoderDictionary(lensDirectory + "\\" + currentLensName + ".xml");
                                UpdateDslrControls();
                            }
                        }
                    }
                }

                //Check live view status
                if (cameraAPI.Cameras.Count > 0)
                {
                    if (cameraAPI.Cameras.Count > 0)
                    {
                        if (liveView && !cameraAPI.Cameras[0].IsInLiveViewMode)
                            UpdateDslrControls();
                    }

                    if (cameraAPI.Cameras.Count > 0)
                    {
                        if (fStopList.Count != cameraAPI.Cameras[0].ApertureValues.Count)
                        {
                            fStopList.Clear();
                            UpdateDslrControls();
                        }
                    }

                    //Check Iris Status
                    //Only update the Iris track bar if the iris is not currently moving
                    if (!tbIrisMoving)
                    {
                        if (!this.InvokeRequired)
                        {
                            if (cameraAPI.Cameras.Count > 0)
                            {
                                if (fStopList.IndexOf(cameraAPI.Cameras[0].CurrentAperture) != tbIris.Value)
                                {
                                    try
                                    {
                                        tbIris.Value = fStopList.IndexOf(cameraAPI.Cameras[0].CurrentAperture);
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }
                            }
                        }
                        else
                        {
                            BeginInvoke((MethodInvoker)delegate
                            {
                                if (cameraAPI.Cameras.Count > 0)
                                {
                                    if (fStopList.IndexOf(cameraAPI.Cameras[0].CurrentAperture) != tbIris.Value)
                                    {
                                        try
                                        {
                                            tbIris.Value = fStopList.IndexOf(cameraAPI.Cameras[0].CurrentAperture);
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                    }
                                }
                            });
                        }
                    }

                    //Launches background thread that detects if live view is active
                    Thread liveViewCheckThread = new Thread(LiveViewCheck);
                    liveViewCheckThread.IsBackground = true;
                    liveViewCheckThread.Start();
                }
            }
            catch (Exception ex)
            {

            }
        }

        public void CameraAPI_OnCameraAdded(CameraAddedEventArgs e)
        {
            //Wires up shutdown and property changed handlers if a camera is added
            cameraAPI.RefreshCameras();
            foreach (Camera camera in cameraAPI.Cameras)
            {
                camera.OnShutdown += new ShutdownEventHandler(camera_OnShutdown);
                camera.OnPropertyChanged += new CanonCameraAppLib.PropertyChangedEventHandler(camera_OnPropertyChanged);
            }

            //Enables controls if cmaera is added
            UpdateDslrControls();
        }

        public void camera_OnShutdown(Camera sender, ShutdownEventArgs e)
        {
            //Disables controls if camera is shut down
            Thread.Sleep(3000);
            cameraAPI.RefreshCameras();
            UpdateDslrControls();
        }

        public void UpdateDslrControls()
        {
            try
            {
                //If there is at least one camera attached and the 1st camera is in live view mode, enable the forms controls
                if (cameraAPI.Cameras.Count > 0 && cameraAPI.Cameras[0].IsInLiveViewMode)
                {
                    //Auto close the live view not detected dialog
                    FindAndCloseMsgBox("Live View Not Detected");

                    //Sleep so that the camera's aperture collection has a chance to refresh
                    Thread.Sleep(1000);

                    //Refresh the fstop collection with the DSLR's aperture values
                    fStopList.Clear();
                    fStopList = cameraAPI.Cameras[0].ApertureValues;

                    //Get the total number of aperture values
                    int irisStops = cameraAPI.Cameras[0].ApertureValues.Count;

                    if (irisStops < 1)
                    {
                        //Set iris label invisible and irisbar to disabled
                        if (!this.InvokeRequired)
                        {
                            lblIrisBarValue.Visible = false;
                            tbIris.Enabled = false;
                        }
                        else
                        {
                            BeginInvoke((MethodInvoker)delegate
                            {
                                lblIrisBarValue.Visible = false;
                                tbIris.Enabled = false;
                            });
                        }

                        //Checks if camera is currently in manual mode or a mode that allows aperture modification
                        Thread thr = new Thread(() => // create a new thread
                        {
                            FindAndMoveMsgBox("Aperture Unavailable", this.Location.X + this.Height / 2, this.Location.Y + this.Width / 8);
                            if (this.InvokeRequired)
                            {
                                BeginInvoke((MethodInvoker)delegate
                                {
                                    Thread.Sleep(100);
                                    if (!apertureNotDetected)
                                    {
                                        apertureNotDetected = true;

                                        if (MessageBox.Show(this, "Switch camera to mode supporting manual aperture in order to use iris control functionality.", "Aperture Unavailable", MessageBoxButtons.OK) == DialogResult.OK)
                                        {
                                            apertureNotDetected = false;
                                        }
                                    }
                                });
                            }
                            else
                            {
                                Thread.Sleep(100);
                                if (!apertureNotDetected)
                                {
                                    apertureNotDetected = true;

                                    if (MessageBox.Show(this, "Switch camera to mode supporting manual aperture in order to use iris control functionality.", "Aperture Unavailable", MessageBoxButtons.OK) == DialogResult.OK)
                                    {
                                        apertureNotDetected = false;
                                    }
                                }
                            }
                        });
                        thr.IsBackground = true;
                        thr.Start(); // starts the thread
                    }
                    else
                    {
                        //Set iris label visible and irisbar to enabled
                        if (!this.InvokeRequired)
                        {
                            lblIrisBarValue.Visible = true;
                            tbIris.Enabled = true;
                        }
                        else
                        {
                            BeginInvoke((MethodInvoker)delegate
                            {
                                lblIrisBarValue.Visible = true;
                                tbIris.Enabled = true;
                            });
                        }

                        //Auto close the live view not detected dialog
                        FindAndCloseMsgBox("Aperture Unavailable");

                        if (!this.InvokeRequired)
                        {
                            try
                            {
                                //Set the iris trackbar values
                                tbIris.Minimum = 0;
                                tbIris.Maximum = irisStops - 1;
                                if (cameraAPI.Cameras.Count > 0)
                                {
                                    if (cameraAPI.Cameras[0].CurrentAperture != 0)
                                    {
                                        //Set the iris trackbar to the current lens aperture value on the dslr
                                        tbIris.Value = fStopList.IndexOf(cameraAPI.Cameras[0].CurrentAperture);
                                    }

                                    //Update the fstop textbox
                                    if (fStopList.Count > 1)
                                        lblIrisBarValue.Text = "f" + string.Format("{0:0.0}", fStopList[tbIris.Value]);

                                    //If encoder collection is loaded, update the focus distance textbox
                                    if (encoderItems.EncoderItems.Count > 0)
                                    {
                                        try
                                        {
                                            if (encoderDictionary[tbFocus.Value] > hyperFocal)
                                            {
                                                lblTrackbarValue.Text = "\u221E";
                                            }
                                            else
                                            {
                                                lblTrackbarValue.Text = String.Format("{0:0.00}", encoderDictionary[tbFocus.Value]) + "ft";
                                            }
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        else
                        {
                            BeginInvoke((MethodInvoker)delegate
                            {
                                try
                                {
                                    tbIris.Minimum = 0;
                                    tbIris.Maximum = irisStops - 1;

                                    if (cameraAPI.Cameras.Count > 0)
                                    {
                                        if (cameraAPI.Cameras[0].CurrentAperture != 0)
                                        {
                                            tbIris.Value = fStopList.IndexOf(cameraAPI.Cameras[0].CurrentAperture);
                                        }

                                        if (fStopList.Count > 1)
                                            lblIrisBarValue.Text = "f" + string.Format("{0:0.0}", fStopList[tbIris.Value]);

                                        if (encoderItems.EncoderItems.Count > 0)
                                        {
                                            try
                                            {
                                                if (encoderDictionary[tbFocus.Value] > hyperFocal)
                                                {
                                                    lblTrackbarValue.Text = "\u221E";
                                                }
                                                else
                                                {
                                                    lblTrackbarValue.Text = String.Format("{0:0.00}", encoderDictionary[tbFocus.Value]) + "ft";
                                                }
                                            }
                                            catch (Exception ex)
                                            {

                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {

                                }
                            });
                        }
                    }

                    //Start the buttondisconnect background thread
                    Thread buttonDisconnectThread = new Thread(ButtonDslrDisconnect);
                    buttonDisconnectThread.IsBackground = true;
                    buttonDisconnectThread.Start();
                }
                else
                {
                    //If there are no cameras attached or no cameras in liveview mode start the button conenct background thread which disables buttons
                    Thread buttonConnectThread = new Thread(ButtonDslrConnect);
                    buttonConnectThread.IsBackground = true;
                    buttonConnectThread.Start();

                    Thread.Sleep(100);
                    if (!liveViewNotDetected)
                    {
                        if (liveViewWatch.ElapsedMilliseconds > 10000 || !liveViewWatch.IsRunning)
                        {
                            liveViewNotDetected = true;

                            Thread thr = new Thread(() => // create a new thread
                            {
                                FindAndMoveMsgBox("Live View Not Detected", this.Location.X + this.Height / 2, this.Location.Y + this.Width / 8);
                                if (this.InvokeRequired)
                                {
                                    BeginInvoke((MethodInvoker)delegate
                                    {
                                        if (MessageBox.Show(this, "Power on DSLR and activate Live View mode.", "Live View Not Detected", MessageBoxButtons.OK) == DialogResult.OK)
                                        {
                                            liveViewWatch.Reset();
                                            liveViewWatch.Start();
                                            liveViewNotDetected = false;
                                        }
                                    });
                                }
                                else
                                {
                                    if (MessageBox.Show(this, "Power on DSLR and activate Live View mode.", "Live View Not Detected", MessageBoxButtons.OK) == DialogResult.OK)
                                    {
                                        liveViewWatch.Reset();
                                        liveViewWatch.Start();
                                        liveViewNotDetected = false;
                                    }
                                }
                            });
                            thr.IsBackground = true;
                            thr.Start(); // starts the thread
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        void bwWaitButton_DoWork(object sender, DoWorkEventArgs e)
        {
            //Background worker that puts the Connect button into busy state
            if (bwWaitButton.CancellationPending)
            {
                e.Cancel = true;
            }

            if (e.Cancel == true)
            {

            }
            else
            {
                WaitButton();
            }
        }

        void WaitButton()
        {
            //Puts the Connect button into busy state
            BeginInvoke((MethodInvoker)delegate
            {
                btnConnect.Text = "Wait...";
                btnConnect.ForeColor = Color.Red;
            });
        }

        void bwIsSerialOpen_DoWork(object sender, DoWorkEventArgs e)
        {
            //Background worker that checks if the birger serial connection is open
            if (bwIsSerialOpen.CancellationPending)
            {
                e.Cancel = true;
            }

            if (e.Cancel == true)
            {

            }
            else
            {
                CheckSerialOpen();
            }
        }

        void CheckSerialOpen()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            do
            {
                if (stopWatch.ElapsedMilliseconds > 2000)
                {
                    stopWatch.Reset();
                    stopWatch.Start();

                    //Is the serial conenction open? If so, enable buttons
                    if (birgerSerial.IsOpen())
                    {
                        //Starts background thread that enables buttons and toggles Connect button
                        Thread buttonDisconnectThread = new Thread(ButtonDisconnect);
                        buttonDisconnectThread.IsBackground = true;
                        buttonDisconnectThread.Start();
                    }
                    else
                    {
                        //Starts background thread that disables buttons and toggles Connect button
                        Thread buttonConnectThread = new Thread(ButtonConnect);
                        buttonConnectThread.IsBackground = true;
                        buttonConnectThread.Start();
                    }
                }
            } while (!bwIsSerialOpen.CancellationPending);

            bwIsSerialOpen.CancelAsync();
        }

        void ButtonDisconnect()
        {
            //Enable buttons and toggle Connect button text
            BeginInvoke((MethodInvoker)delegate
            {
                btnLearnFocusStops.Enabled = true;

                tbFocus.Enabled = true;
                if (!initialFocusSet && focusPosValue != -1)
                {
                    tbFocus.Value = focusPosValue;
                    if (encoderDictionary[tbFocus.Value] > hyperFocal)
                    {
                        lblTrackbarValue.Text = "\u221E";
                    }
                    else
                    {
                        lblTrackbarValue.Text = String.Format("{0:0.00}", encoderDictionary[tbFocus.Value]) + "ft";
                    }
                    initialFocusSet = true;
                }

                if (!initialApertureSet && aperturePosValue != -1)
                {
                    tbIris.Value = aperturePosValue;
                    if (fStopList.Count > 1)
                        lblIrisBarValue.Text = "f" + string.Format("{0:0.0}", fStopList[aperturePosValue]);
                    initialApertureSet = true;
                }
                else
                {
                    if (fStopList.Count > 1)
                    {
                        lblIrisBarValue.Text = "f" + string.Format("{0:0.0}", fStopList[tbIris.Value]);
                    }
                    else
                    {
                        lblIrisBarValue.Text = tbIris.Value.ToString();
                    }
                }

                tbIris.Enabled = true;
                btnSetStartFocus.Enabled = true;
                btnSetEndFocus.Enabled = true;
                btnRackFocus.Enabled = true;
                btnJumpToStart.Enabled = true;
                btnJumpToEnd.Enabled = true;
                btnSwap.Enabled = true;

                if (rackingFocusBatch)
                    btnBatchRackFocus.Enabled = false;
                else
                    btnBatchRackFocus.Enabled = true;

                btnRecord.Enabled = true;
                btnJumpToEndIris.Enabled = true;
                btnJumpToStartIris.Enabled = true;
                btnRangefinder.Enabled = true;
                btnSetStartIris.Enabled = true;
                btnSetEndIris.Enabled = true;
                btnSwapIris.Enabled = true;
                btnAddToBatch.Enabled = true;
                btnDelete.Enabled = true;
                btnEdit.Enabled = true;

                btnConnect.Text = "Disconnect";
                btnConnect.ForeColor = Color.Red;
            });

            Thread.CurrentThread.Abort();
        }

        void ButtonDslrDisconnect()
        {
            //Enables the necessary UI buttons and toggles the Connect button text
            Thread.Sleep(500);
            BeginInvoke((MethodInvoker)delegate
            {
                try
                {
                    btnLearnFocusStops.Enabled = true;

                    tbFocus.Enabled = true;
                    if (!initialFocusSet && focusPosValue != -1)
                    {
                        tbFocus.Value = focusPosValue;
                        if (encoderDictionary[tbFocus.Value] > hyperFocal)
                        {
                            lblTrackbarValue.Text = "\u221E";
                        }
                        else
                        {
                            lblTrackbarValue.Text = String.Format("{0:0.00}", encoderDictionary[tbFocus.Value]) + "ft";
                        }
                        initialFocusSet = true;
                    }

                    if (!initialApertureSet && aperturePosValue != -1)
                    {
                        tbIris.Value = aperturePosValue;
                        if (fStopList.Count > 1)
                            lblIrisBarValue.Text = "f" + string.Format("{0:0.0}", fStopList[aperturePosValue]);
                        initialApertureSet = true;
                    }
                    else
                    {
                        if (fStopList.Count > 1)
                        {
                            lblIrisBarValue.Text = "f" + string.Format("{0:0.0}", fStopList[tbIris.Value]);
                        }
                        else
                        {
                            lblIrisBarValue.Text = tbIris.Value.ToString();
                        }
                    }

                    btnSetStartFocus.Enabled = true;
                    btnSetEndFocus.Enabled = true;
                    btnRackFocus.Enabled = true;
                    btnJumpToStart.Enabled = true;
                    btnJumpToEnd.Enabled = true;
                    btnSwap.Enabled = true;

                    if (rackingFocusBatch)
                        btnBatchRackFocus.Enabled = false;
                    else
                        btnBatchRackFocus.Enabled = true;

                    btnRecord.Enabled = true;
                    btnJumpToEndIris.Enabled = true;
                    btnJumpToStartIris.Enabled = true;
                    btnRangefinder.Enabled = true;
                    btnSetStartIris.Enabled = true;
                    btnSetEndIris.Enabled = true;
                    btnSwapIris.Enabled = true;
                    btnAddToBatch.Enabled = true;
                    btnDelete.Enabled = true;
                    btnEdit.Enabled = true;

                    if (cameraAPI.Cameras.Count > 0 && encoderDictionaryLoaded)
                    {
                        if (!lensPlayManagerThread.IsAlive)
                        {
                            lensPlayManagerThread = new Thread(LensPlayManager);
                            lensPlayManagerThread.IsBackground = true;
                            lensPlayManagerThread.Start();
                        }

                        if (this.InvokeRequired)
                        {
                            BeginInvoke((MethodInvoker)delegate
                            {
                                btnConnect.Text = "Re-Calibrate Lens";
                                btnConnect.ForeColor = Color.Black;

                                btnLearnFocusStops.Visible = false;

                                lblStepCounter.Visible = true;
                                txtStepCounter.Visible = true;
                                lblStepCounterEx.Visible = true;
                                txtStepCounterEx.Visible = true;
                                prgBarFocus.Visible = true;
                            });
                        }
                        else
                        {
                            btnConnect.Text = "Re-Calibrate Lens";
                            btnConnect.ForeColor = Color.Black;

                            btnLearnFocusStops.Visible = false;

                            lblStepCounter.Visible = true;
                            txtStepCounter.Visible = true;
                            lblStepCounterEx.Visible = true;
                            txtStepCounterEx.Visible = true;
                            prgBarFocus.Visible = true;
                        }
                    }
                    else
                    {
                        btnConnect.Text = "Calibrate Lens";
                        btnConnect.ForeColor = Color.Red;

                        btnLearnFocusStops.Visible = false;

                        lblStepCounter.Visible = true;
                        txtStepCounter.Visible = true;
                        lblStepCounterEx.Visible = true;
                        txtStepCounterEx.Visible = true;
                        prgBarFocus.Visible = true;
                    }
                }
                catch (Exception ex)
                {

                }
            });

            Thread.CurrentThread.Abort();
        }

        void ButtonConnect()
        {
            //Disable buttons and toggle Connect button text
            BeginInvoke((MethodInvoker)delegate
            {
                btnLearnFocusStops.Enabled = false;
                tbFocus.Enabled = false;
                tbIris.Enabled = false;
                btnConnect.Text = "Connect";
                btnConnect.ForeColor = Color.Black;

                btnLearnFocusStops.Visible = true;
                lblStepCounter.Visible = false;
                txtStepCounter.Visible = false;
                lblStepCounterEx.Visible = false;
                txtStepCounterEx.Visible = false;
                prgBarFocus.Visible = false;

                initialFocusSet = false;
                initialApertureSet = false;

                btnSetStartFocus.Enabled = false;
                btnSetEndFocus.Enabled = false;
                btnRackFocus.Enabled = false;
                btnJumpToStart.Enabled = false;
                btnJumpToEnd.Enabled = false;
                btnSwap.Enabled = false;
                btnBatchRackFocus.Enabled = false;
                btnRecord.Enabled = false;
                btnJumpToEndIris.Enabled = false;
                btnJumpToStartIris.Enabled = false;
                btnRangefinder.Enabled = false;
                btnSetStartIris.Enabled = false;
                btnSetEndIris.Enabled = false;
                btnSwapIris.Enabled = false;
                btnAddToBatch.Enabled = false;
                btnDelete.Enabled = false;
                btnEdit.Enabled = false;
            });

            Thread.CurrentThread.Abort();
        }

        void ButtonDslrConnect()
        {
            //Disables the UI buttons and toggles the Connect button text
            if (lensPlayManagerThread.IsAlive)
                lensPlayManagerThread.Abort();

            Thread.Sleep(500);
            if (this.InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    btnLearnFocusStops.Enabled = false;
                    tbFocus.Value = tbFocus.Minimum;
                    tbFocus.Enabled = false;
                    btnConnect.Text = "Connect";
                    btnConnect.ForeColor = Color.Black;

                    btnLearnFocusStops.Visible = true;
                    lblStepCounter.Visible = false;
                    txtStepCounter.Visible = false;
                    lblStepCounterEx.Visible = false;
                    txtStepCounterEx.Visible = false;
                    prgBarFocus.Visible = false;

                    initialFocusSet = false;
                    initialApertureSet = false;

                    btnSetStartFocus.Enabled = false;
                    btnSetEndFocus.Enabled = false;
                    btnRackFocus.Enabled = false;
                    btnJumpToStart.Enabled = false;
                    btnJumpToEnd.Enabled = false;
                    btnSwap.Enabled = false;
                    btnBatchRackFocus.Enabled = false;
                    btnRecord.Enabled = false;
                    btnJumpToEndIris.Enabled = false;
                    btnJumpToStartIris.Enabled = false;
                    btnRangefinder.Enabled = false;
                    btnSetStartIris.Enabled = false;
                    btnSetEndIris.Enabled = false;
                    btnSwapIris.Enabled = false;
                    btnAddToBatch.Enabled = false;
                    btnDelete.Enabled = false;
                    btnEdit.Enabled = false;
                });
            }

            Thread.CurrentThread.Abort();
        }

        void BuildFocusDictionary()
        {
            List<EncoderItem> encoderList = new List<EncoderItem>();
            EncoderItem encoderBuffer = new EncoderItem();
            encoderBuffer.Encoder = 0;
            encoderItems.EncoderItems.Clear();

            //Iterate through distance stop list and build major stops
            foreach (string s in distStopList)
            {
                double far;
                if (int.Parse(s.Substring(0, s.Length - 10), NumberStyles.AllowHexSpecifier) == int.Parse("ffff", NumberStyles.AllowHexSpecifier))
                {
                    far = (double)int.Parse(s.Substring(0, s.Length - 10), NumberStyles.AllowHexSpecifier);
                }
                else
                {
                    far = (double)int.Parse(s.Substring(0, s.Length - 10), NumberStyles.AllowHexSpecifier) * 0.0328084;
                }

                double near;
                if (int.Parse(s.Substring(5, s.Length - 10), NumberStyles.AllowHexSpecifier) == int.Parse("ffff", NumberStyles.AllowHexSpecifier))
                {
                    near = (double)int.Parse(s.Substring(5, s.Length - 10), NumberStyles.AllowHexSpecifier);
                }
                else
                {
                    near = (double)int.Parse(s.Substring(5, s.Length - 10), NumberStyles.AllowHexSpecifier) * 0.0328084;
                }

                string encoder = s.Substring(10, s.Length - 10);
                encoderList.Add(new EncoderItem(0, 0, near, far, int.Parse(encoder, NumberStyles.AllowHexSpecifier), 0, 0, 0));
            }

            //calculate major trackbar values for each distance stop
            double totalEncoderDelta = 0;
            for (int x = 0; x < encoderList.Count - 1; x++)
            {
                if ((encoderList[x].Far == (double)int.Parse("ffff", NumberStyles.AllowHexSpecifier)) && (encoderList[x].Near != (double)int.Parse("ffff", NumberStyles.AllowHexSpecifier)))
                {
                    encoderList[x].Far = hyperFocal;
                }
                if ((encoderList[x].Far == (double)int.Parse("ffff", NumberStyles.AllowHexSpecifier)) && (encoderList[x].Near == (double)int.Parse("ffff", NumberStyles.AllowHexSpecifier)))
                {
                    encoderList[x].Near = hyperFocal;
                }

                if (encoderList[x + 1].Encoder > encoderList[x].Encoder)
                {
                    encoderList[x].TrackBar = totalEncoderDelta * (16384 / totalEncoderSteps);
                    totalEncoderDelta = totalEncoderDelta + (encoderList[x + 1].Encoder - encoderList[x].Encoder);
                }
                else
                {
                    encoderList[x].TrackBar = totalEncoderDelta * (16384 / totalEncoderSteps);
                    totalEncoderDelta = totalEncoderDelta + ((65535 - encoderList[x].Encoder) + encoderList[x + 1].Encoder);
                }
            }

            //Detect hyperfocal infinity condition and recalculate footstep
            if ((encoderList[encoderList.Count - 1].Far == (double)int.Parse("ffff", NumberStyles.AllowHexSpecifier)) && (encoderList[encoderList.Count - 1].Near != (double)int.Parse("ffff", NumberStyles.AllowHexSpecifier)))
            {
                encoderList[encoderList.Count - 1].Far = hyperFocal;
            }
            if ((encoderList[encoderList.Count - 1].Far == (double)int.Parse("ffff", NumberStyles.AllowHexSpecifier)) && (encoderList[encoderList.Count - 1].Near == (double)int.Parse("ffff", NumberStyles.AllowHexSpecifier)))
            {
                encoderList[encoderList.Count - 1].Near = hyperFocal;
            }

            encoderList[encoderList.Count - 1].TrackBar = totalEncoderDelta * (16384 / totalEncoderSteps);

            double totalEncoderStepsDelta = 0;

            if (encoderList[encoderList.Count - 1].Encoder < encoderList[0].Encoder)
            {
                totalEncoderStepsDelta = totalEncoderSteps - ((65535 - encoderList[0].Encoder) + (encoderList[encoderList.Count - 1].Encoder + 1));
            }
            else
            {
                totalEncoderStepsDelta = totalEncoderSteps - (encoderList[encoderList.Count - 1].Encoder - encoderList[0].Encoder);
            }

            EncoderItem finalEncoderItem = new EncoderItem(encoderList[encoderList.Count - 1].Dist, encoderList[encoderList.Count - 1].DistStep,
                encoderList[encoderList.Count - 1].Near, encoderList[encoderList.Count - 1].Far, encoderList[encoderList.Count - 1].Encoder, encoderList[encoderList.Count - 1].TrackBar,
                0, 0);

            finalEncoderItem.Near = 65535;
            finalEncoderItem.Far = 100000;
            finalEncoderItem.Encoder = (int)Math.Round(finalEncoderItem.Encoder + totalEncoderStepsDelta);
            finalEncoderItem.TrackBar = 16383;

            encoderList.Add(finalEncoderItem);

            positionToEncoderRatio = 16384 / totalEncoderSteps;

            encoderBuffer.Encoder = 0;

            //Map focus positions to encoder values
            int encoderDelta = 0;
            double footStep = 0;
            double trackBarBuffer = 0;
            for (int x = 0; x < encoderList.Count - 1; x++)
            {
                if (encoderList[x + 1].Encoder < encoderList[x].Encoder)
                {
                    encoderDelta = (65535 - encoderList[x].Encoder) + encoderList[x + 1].Encoder;
                    footStep = (encoderList[x].Far - encoderList[x].Near) / encoderDelta;
                    positionToEncoderRatio = (encoderList[x + 1].TrackBar - encoderList[x].TrackBar) / encoderDelta;

                    encoderBuffer = new EncoderItem(encoderList[x].Dist, encoderList[x].DistStep, encoderList[x].Near, encoderList[x].Far, encoderList[x].Encoder, encoderList[x].TrackBar, 0, 0);
                    encoderBuffer.Dist = encoderBuffer.Near;
                    encoderBuffer.DistStep = footStep;
                    do
                    {
                        do
                        {
                            encoderItems.EncoderItems.Add(new EncoderItem(encoderBuffer.Dist, encoderBuffer.DistStep, encoderBuffer.Near, encoderBuffer.Far, encoderBuffer.Encoder, trackBarBuffer, 0, 0));
                            trackBarBuffer++;
                        } while (trackBarBuffer < (encoderBuffer.TrackBar + positionToEncoderRatio));
                        encoderBuffer.Encoder++;
                        encoderBuffer.Dist = encoderBuffer.Dist + footStep;
                        trackBarBuffer = encoderBuffer.TrackBar + positionToEncoderRatio;
                        encoderBuffer.TrackBar = trackBarBuffer;
                    } while (encoderBuffer.Encoder <= 65535);

                    encoderBuffer.Encoder = 0;
                    do
                    {
                        do
                        {
                            encoderItems.EncoderItems.Add(new EncoderItem(encoderBuffer.Dist, encoderBuffer.DistStep, encoderBuffer.Near, encoderBuffer.Far, encoderBuffer.Encoder, trackBarBuffer, 0, 0));
                            trackBarBuffer++;
                        } while (trackBarBuffer < (encoderBuffer.TrackBar + positionToEncoderRatio));
                        encoderBuffer.Encoder++;
                        encoderBuffer.Dist = encoderBuffer.Dist + footStep;
                        trackBarBuffer = encoderBuffer.TrackBar + positionToEncoderRatio;
                        encoderBuffer.TrackBar = trackBarBuffer;
                    } while (encoderBuffer.Encoder < encoderList[x + 1].Encoder);
                }
                else
                {
                    encoderDelta = encoderList[x + 1].Encoder - encoderList[x].Encoder;
                    footStep = (encoderList[x].Far - encoderList[x].Near) / encoderDelta;
                    positionToEncoderRatio = (encoderList[x + 1].TrackBar - encoderList[x].TrackBar) / encoderDelta;

                    encoderBuffer = new EncoderItem(encoderList[x].Dist, encoderList[x].DistStep, encoderList[x].Near, encoderList[x].Far, encoderList[x].Encoder, encoderList[x].TrackBar, 0, 0);

                    encoderBuffer.Dist = encoderBuffer.Near;
                    encoderBuffer.DistStep = footStep;
                    do
                    {
                        do
                        {
                            encoderItems.EncoderItems.Add(new EncoderItem(encoderBuffer.Dist, encoderBuffer.DistStep, encoderBuffer.Near, encoderBuffer.Far, encoderBuffer.Encoder, trackBarBuffer, 0, 0));
                            trackBarBuffer++;
                        } while (trackBarBuffer < (encoderBuffer.TrackBar + positionToEncoderRatio));
                        encoderBuffer.Encoder++;
                        encoderBuffer.Dist = encoderBuffer.Dist + footStep;
                        trackBarBuffer = encoderBuffer.TrackBar + positionToEncoderRatio;
                        encoderBuffer.TrackBar = trackBarBuffer;
                    } while (encoderBuffer.Encoder < encoderList[x + 1].Encoder);
                }
            }

            encoderBuffer = new EncoderItem(encoderList[encoderList.Count - 1].Dist, encoderList[encoderList.Count - 1].DistStep, encoderList[encoderList.Count - 1].Near,
                encoderList[encoderList.Count - 1].Far, encoderList[encoderList.Count - 1].Encoder, encoderList[encoderList.Count - 1].TrackBar, 0, 0);
            encoderBuffer.Dist = encoderBuffer.Near;
            encoderBuffer.DistStep = footStep;
            encoderItems.EncoderItems.Add(new EncoderItem(encoderBuffer.Dist, encoderBuffer.DistStep, encoderBuffer.Near,
                encoderBuffer.Far, encoderBuffer.Encoder, encoderBuffer.TrackBar, 0, 0));

            using (StreamWriter sw = new StreamWriter(lensDirectory + @"\encoderDictionary.txt"))
            {
                foreach (string s in distStopList)
                {
                    sw.WriteLine(s);
                }
                sw.WriteLine();
                sw.WriteLine();
                sw.WriteLine();

                foreach (EncoderItem e in encoderList)
                {
                    sw.WriteLine("e.Dist:" + e.Dist + ", " + "e.DistStep:" + e.DistStep + ", " + "e.Near:" + e.Near + ", " + "e.Far:" + e.Far + ", " + "e.Encoder:"
                        + e.Encoder + ", " + "e.Trackbar:" + e.TrackBar);
                }
                sw.WriteLine();
                sw.WriteLine();
                sw.WriteLine();

                foreach (EncoderItem e in encoderItems.EncoderItems)
                {
                    sw.WriteLine(e.Dist + "," + e.Near + "," + e.Far + "," + e.Encoder + "," + e.TrackBar);
                }
            }

            SaveEncoderDictionary(lensDirectory + "\\" + currentLensName + ".xml");
            Thread.Sleep(1500);
            LoadEncoderDictionary(lensDirectory + "\\" + currentLensName + ".xml");
        }

        void birgerSerial_DataReceived(object sender, CustomSerialDataReceivedEventArgs e)
        {
            //Data received handler
            string dataText = e.Data;

            //Get focus info
            Regex focusPositionPattern = new Regex(@"fmin\:[+-]?[0-9]{1,4}  fmax\:[+-]?[0-9]{1,4}  current\:[+-]?[0-9]{1,4}");
            foreach (Match match in focusPositionPattern.Matches(dataText))
            {
                int fminIndex = match.ToString().IndexOf("fmin:");
                int fmaxIndex = match.ToString().IndexOf("fmax:");
                int currentIndex = match.ToString().IndexOf("current:");
                double fmin = double.Parse(match.ToString().Substring(fminIndex + 5, fmaxIndex - 5));
                double fmax = double.Parse(match.ToString().Substring(fmaxIndex + 5, currentIndex - fmaxIndex - 5));

                totalEncoderSteps = (fmax - fmin) + 1;
            }

            //Get error
            Regex errorPattern = new Regex(@"ERR21");
            foreach (Match match in errorPattern.Matches(dataText))
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    MessageBox.Show("Lens not Ready. Please try again.");
                });
                Application.DoEvents();
            }

            Regex distStopsPattern = new Regex(@"\$\:[0-9a-zA-Z]{1,4}\:[0-9a-zA-Z]{1,4}\:[0-9a-zA-Z]{1,4}");

            //Get Distance Stops
            foreach (Match match in distStopsPattern.Matches(dataText))
            {
                string hexVal = match.ToString().Substring(2, match.ToString().Length - 2);
                distStopList.Add(hexVal);
            }

            if (dataText.Contains("$:DONE"))
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    MessageBox.Show("Building Focus Dictionary");
                });
                Application.DoEvents();

                encoderItems.EncoderItems.Clear();
                BuildFocusDictionary();
            }

            Regex focusPosValuePattern = new Regex(@"\%\:[0-9a-zA-Z]{1,4}");

            //Get Focus Position Value
            foreach (Match match in focusPosValuePattern.Matches(dataText))
            {
                string hexVal = match.ToString().Substring(2, match.ToString().Length - 2);
                focusPosValue = int.Parse(hexVal, NumberStyles.AllowHexSpecifier);

                BeginInvoke((MethodInvoker)delegate
                {
                    tbFocus.Value = focusPosValue;
                });
            }

            Regex aperturePosStatusPattern = new Regex(@"\&\:\?");

            //Check Aperture Position Status
            foreach (Match match in aperturePosStatusPattern.Matches(dataText))
            {
                apertureNeedsInitialization = true;

                if (apertureNeedsInitialization)
                {
                    Thread.Sleep(1000);
                    birgerSerial.Write("in");           //Initialize aperture
                    Thread.Sleep(1000);
                    birgerSerial.Write("da");           //Print the the aperture range

                    Thread.Sleep(250);
                    apertureNeedsInitialization = false;
                    initialApertureSet = true;
                    birgerSerial.Write("la");           //Learn the focus range.
                }
            }

            Regex doneLAPattern = new Regex(@"DONE:LA");
            foreach (Match match in doneLAPattern.Matches(dataText))
            {
                birgerSerial.Write("gs");           //Echo current device and lens statuses.
            }

            Regex aperturePosValuePattern = new Regex(@"\&\:[^\?]{1,2}");

            //Get Aperture Position Value
            foreach (Match match in aperturePosValuePattern.Matches(dataText))
            {
                string hexVal = match.ToString().Substring(2, match.ToString().Length - 2);
                aperturePosValue = int.Parse(hexVal, NumberStyles.AllowHexSpecifier);
                aperturePosValue = aperturePosValue / 2;

                BeginInvoke((MethodInvoker)delegate
                {
                    tbIris.Value = aperturePosValue;
                    initialApertureSet = true;
                });
            }

            Regex daPattern = new Regex(@"[f][0-9]{1,3}\,[0-9]{1,3}\,[f][0-9]{1,3}");

            //Define Aperture
            foreach (Match match in daPattern.Matches(dataText))
            {
                if (!(fStopList.Count > 2))
                {
                    fStopList.Clear();
                    int irisStops = int.Parse(match.ToString().Substring(match.ToString().IndexOf(",") + 1, (match.ToString().LastIndexOf(",") - match.ToString().IndexOf(",")) - 1));
                    double irisMax = double.Parse(match.ToString().Substring(1, match.ToString().IndexOf(",") - 1)) / 10;
                    double irisMin = double.Parse(match.ToString().Substring(match.ToString().LastIndexOf(",f") + 2, (match.ToString().Length - (match.ToString().LastIndexOf(",f") + 2)))) / 10;
                    double minMaxIrisRatio = irisMax / irisMin;
                    double lensConstant = Math.Pow(minMaxIrisRatio, 1.0 / (irisStops - 1));

                    for (int x = 0; x < irisStops; x++)
                    {
                        fStopList.Add(irisMin * Math.Pow(lensConstant, (irisStops - 1) - x));
                    }

                    Thread.Sleep(1500);
                    BeginInvoke((MethodInvoker)delegate
                    {
                        tbIris.Minimum = 0;
                        tbIris.Maximum = irisStops - 1;
                        if (fStopList.Count > 1)
                            lblIrisBarValue.Text = "f" + string.Format("{0:0.0}", fStopList[tbIris.Value]);
                    });
                }
            }

            Regex lensStatusPattern = new Regex(@"\@\:[0-9]{1,3}[m]{2}\,[f][0-9]{1,3}\,[0-9]{1,3}\,[f][0-9]{1,3}");

            //Define Aperture
            foreach (Match match in lensStatusPattern.Matches(dataText))
            {
                if (currentLensName != match.ToString().Substring(2, match.ToString().Length - 2))
                {
                    encoderDictionaryLoaded = false;
                }

                if (!encoderDictionaryLoaded)
                {
                    currentLensName = match.ToString().Substring(2, match.ToString().Length - 2);
                    currentFocalLength = double.Parse(currentLensName.Substring(0, currentLensName.IndexOf("mm")));
                    currentFstop = double.Parse(currentLensName.Substring(currentLensName.IndexOf(",f") + 2, (currentLensName.IndexOf(",") - 2))) / 10;
                    hyperFocal = (((currentFocalLength * currentFocalLength) / (0.01 * currentFstop)) + currentFocalLength) * 0.00328084;
                    if (File.Exists(lensDirectory + "\\" + currentLensName + ".xml"))
                    {
                        encoderDictionaryLoaded = true;
                        LoadEncoderDictionary(lensDirectory + "\\" + currentLensName + ".xml");
                    }
                }
            }
        }

        public void LoadEncoderDictionary(string fileName)
        {
            //Load encoder dictionary from XML
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(EncoderItemList));

                encoderItems.EncoderItems.Clear();
                using (TextReader textReader = new StreamReader(fileName))
                {
                    encoderItems = (EncoderItemList)xmlSerializer.Deserialize(textReader);
                    textReader.Close();
                }

                trackbarDictionary.Clear();
                foreach (EncoderItem encoderItem in encoderItems.EncoderItems)
                {
                    if (!trackbarDictionary.ContainsKey(encoderItem.TrackBarInt))
                    {
                        trackbarDictionary.Add(encoderItem.TrackBarInt, encoderItem.Encoder);
                    }
                }

                using (StreamWriter sw = new StreamWriter(lensDirectory + @"\trackbarDictionary.txt"))
                {
                    foreach (KeyValuePair<int, int> item in trackbarDictionary)
                    {
                        sw.WriteLine("item.Key:" + item.Key + ", " + "item.Value:" + item.Value);
                    }
                }

                encoderDictionary.Clear();
                foreach (EncoderItem encoderItem in encoderItems.EncoderItems)
                {
                    if (!encoderDictionary.ContainsKey(encoderItem.TrackBarInt))
                    {
                        encoderDictionary.Add(encoderItem.TrackBarInt, encoderItem.Dist);
                    }
                }

                feetDictionary.Clear();
                majorDistDictionary.Clear();
                foreach (EncoderItem encoderItem in encoderItems.EncoderItems)
                {
                    if (!feetDictionary.ContainsKey(double.Parse(String.Format("{0:0.00}", encoderItem.Dist))))
                    {
                        feetDictionary.Add(double.Parse(String.Format("{0:0.00}", encoderItem.Dist)), encoderItem.TrackBarInt);
                    }

                    if (!majorDistDictionary.ContainsKey(double.Parse(String.Format("{0:0.00}", encoderItem.Near))))
                    {
                        majorDistDictionary.Add(double.Parse(String.Format("{0:0.00}", encoderItem.Near)), encoderItem.TrackBarInt);
                    }
                }

                smallDelay = encoderItems.SmallDelay;
                medDelay = encoderItems.MedDelay;
                lrgDelay = encoderItems.LrgDelay;

                if (this.InvokeRequired)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        btnLearnFocusStops.Text = "Re-Learn Focus Scale";
                    });
                }
                else
                {
                    btnLearnFocusStops.Text = "Re-Learn Focus Scale";
                }
            }
            catch (Exception ex)
            {

            }
        }

        void SaveEncoderDictionary(string fileName)
        {
            //Save encoder dictionary to XML
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(EncoderItemList));

                using (TextWriter textWriter = new StreamWriter(fileName))
                {
                    serializer.Serialize(textWriter, encoderItems);
                    textWriter.Close();
                }
            }
            catch (Exception ex)
            {

            }
        }

        void StartBeeps()
        {
            //Beeps while racking focus flag is true if beeps are enabled
            do
            {
                Thread.Sleep(1000);
                System.Media.SystemSounds.Exclamation.Play();
            } while (rackingFocus);
        }

        void rackToEnd()
        {
            //Starts beep thread if beeps are enabled
            if (enableBeeps)
            {
                Thread newThread = new Thread(StartBeeps);
                newThread.IsBackground = true;
                newThread.Start();
            }

            //Starts handle abort background thread
            Thread handleAbortThread = new Thread(HandleAbort);
            handleAbortThread.IsBackground = true;
            handleAbortThread.Start();

            int thumbDelta = 0;
            int interval = 0;

            BeginInvoke((MethodInvoker)delegate
            {
                //Wait until all commmands in the queue are executed. Max wait time: 10 seconds
                Stopwatch waitWatch = new Stopwatch();
                waitWatch.Start();
                while ((commandQueue.Count > 1) && waitWatch.ElapsedMilliseconds < 10000 && tbFocus.Value != tbFocus.Minimum && tbFocus.Value != tbFocus.Maximum)
                {

                }

                if (waitWatch.ElapsedTicks > 10000)
                {
                    MessageBox.Show("waitWatch expired");
                    //logger.LogError("waitWatch: " + waitWatch.ElapsedMilliseconds, EventLogEntryType.Information);
                }

                if (currentFocusMove.StartingFocusThumb < currentFocusMove.EndingFocusThumb)
                {
                    tbFocus.Value = currentFocusMove.StartingFocusThumb;
                    thumbDelta = currentFocusMove.EndingFocusThumb - currentFocusMove.StartingFocusThumb;
                    interval = (int)currentFocusMove.FocusDuration.TotalMilliseconds / thumbDelta;

                    int currentThumb = currentFocusMove.StartingFocusThumb;

                    do
                    {
                        tbFocus.Value = tbFocus.Value + 1;
                        currentThumb++;
                        Application.DoEvents();
                        Thread.Sleep(interval);
                        thumbDelta = currentFocusMove.EndingFocusThumb - tbFocus.Value;
                    } while (currentThumb < (currentFocusMove.EndingFocusThumb - (tbFocus.SmallChange * 3)) && rackingFocus && !focusAborted && thumbDelta > tbFocus.SmallChange * 3);

                    tbFocus.Value = currentFocusMove.EndingFocusThumb;
                    txtEndingThumbActual.Text = tbFocus.Value.ToString();
                    Application.DoEvents();
                    rackingFocus = false;
                }
                else
                {
                    tbFocus.Value = currentFocusMove.StartingFocusThumb;
                    thumbDelta = currentFocusMove.StartingFocusThumb - currentFocusMove.EndingFocusThumb;
                    interval = (int)currentFocusMove.FocusDuration.TotalMilliseconds / thumbDelta;

                    int currentThumb = currentFocusMove.StartingFocusThumb;

                    do
                    {
                        tbFocus.Value = tbFocus.Value - 1;
                        currentThumb--;
                        Application.DoEvents();
                        Thread.Sleep(interval);
                        thumbDelta = tbFocus.Value - currentFocusMove.EndingFocusThumb;
                    } while (currentThumb > (currentFocusMove.EndingFocusThumb + (tbFocus.SmallChange * 3)) && rackingFocus && !focusAborted && thumbDelta > tbFocus.SmallChange * 3);

                    tbFocus.Value = currentFocusMove.EndingFocusThumb;
                    txtEndingThumbActual.Text = tbFocus.Value.ToString();
                    Application.DoEvents();
                    rackingFocus = false;
                }
            });
        }

        void FindAndMoveMsgBox(string title, int x, int y)
        {
            try
            {
                Thread thr = new Thread(() => // create a new thread
                {
                    IntPtr msgBox = IntPtr.Zero;
                    // while there's no MessageBox, FindWindow returns IntPtr.Zero
                    while ((msgBox = FindWindow(IntPtr.Zero, title)) == IntPtr.Zero) ;
                    // after the while loop, msgBox is the handle of your MessageBox
                    Rectangle r = new Rectangle();

                    ManagedWinapi.Windows.SystemWindow msgBoxWindow = new ManagedWinapi.Windows.SystemWindow(msgBox);
                    msgBoxWindow.Location = new Point(x, y);
                });
                thr.IsBackground = true;
                thr.Start(); // starts the thread
            }
            catch (Exception ex)
            {

            }
        }

        void FindAndCloseMsgBox(string title)
        {
            try
            {
                Stopwatch findCloseWatch = new Stopwatch();
                findCloseWatch.Start();
                Thread thr = new Thread(() => // create a new thread
                {
                    IntPtr msgBox = IntPtr.Zero;
                    // while there's no MessageBox, FindWindow returns IntPtr.Zero
                    while (((msgBox = FindWindow(IntPtr.Zero, title)) == IntPtr.Zero) && findCloseWatch.ElapsedMilliseconds < 3000) ;
                    // after the while loop, msgBox is the handle of your MessageBox
                    Rectangle r = new Rectangle();

                    ManagedWinapi.Windows.SystemWindow msgBoxWindow = new ManagedWinapi.Windows.SystemWindow(msgBox);
                    msgBoxWindow.SendClose();
                });
                thr.IsBackground = true;
                thr.Start(); // starts the thread
            }
            catch (Exception ex)
            {

            }
        }

        private void btnSetStartFocus_Click(object sender, EventArgs e)
        {
            if (cameraAPI.Cameras.Count > 0 || birgerSerial.IsOpen())
            {
                txtSetStartFocus.Text = lblTrackbarValue.Text;
                currentFocusMove.StartingFocusDistance = lblTrackbarValue.Text;

                currentFocusMove.StartingFocusThumb = tbFocus.Value;
                txtStartingThumb.Text = currentFocusMove.StartingFocusThumb.ToString();
            }
            else
            {
                FindAndMoveMsgBox("Lost Communication", this.Location.X + this.Height / 2, this.Location.Y + this.Width / 8);
                MessageBox.Show(this, "Please pair Interface with Birger Mount.", "Lost Communication");
            }
        }

        private void btnSetEndFocus_Click(object sender, EventArgs e)
        {
            if (cameraAPI.Cameras.Count > 0 || birgerSerial.IsOpen())
            {
                txtSetEndFocus.Text = lblTrackbarValue.Text;
                currentFocusMove.EndingFocusDistance = lblTrackbarValue.Text;

                currentFocusMove.EndingFocusThumb = tbFocus.Value;
                txtEndingThumb.Text = currentFocusMove.EndingFocusThumb.ToString();
            }
            else
            {
                FindAndMoveMsgBox("Lost Communication", this.Location.X + this.Height / 2, this.Location.Y + this.Width / 8);
                MessageBox.Show(this, "Please pair Interface with Birger Mount.", "Lost Communication");
            }
        }

        private void btnJumpToStart_Click(object sender, EventArgs e)
        {
            if (cameraAPI.Cameras.Count > 0 || birgerSerial.IsOpen())
            {
                int currentThumb = 0;

                currentThumb = tbFocus.Value;

                //Drag thumb to start position if not already there
                if (currentThumb != currentFocusMove.StartingFocusThumb)
                {
                    //logger.LogError("JumpToStart Thumb: " + currentThumb.ToString(), EventLogEntryType.Information);
                    tbFocus.Value = currentFocusMove.StartingFocusThumb;
                }
            }
            else
            {
                FindAndMoveMsgBox("Lost Communication", this.Location.X + this.Height / 2, this.Location.Y + this.Width / 8);
                MessageBox.Show(this, "Please pair Interface with Birger Mount.", "Lost Communication");
            }
        }

        private void btnJumpToEnd_Click(object sender, EventArgs e)
        {
            if (cameraAPI.Cameras.Count > 0 || birgerSerial.IsOpen())
            {
                int currentThumb = 0;

                currentThumb = tbFocus.Value;

                //Drag thumb to start position if not already there
                if (currentThumb != currentFocusMove.EndingFocusThumb)
                {
                    //logger.LogError("JumpToEnd Thumb: " + currentThumb.ToString(), EventLogEntryType.Information);

                    tbFocus.Value = currentFocusMove.EndingFocusThumb;
                }
            }
            else
            {
                FindAndMoveMsgBox("Lost Communication", this.Location.X + this.Height / 2, this.Location.Y + this.Width / 8);
                MessageBox.Show(this, "Please pair Interface with Birger Mount.", "Lost Communication");
            }
        }

        private void BindFocusMove(FocusMove focusMoveToBind)
        {
            //Populates controls with the properties of the current focus move
            currentFocusMove = focusMoveToBind;

            txtSetStartFocus.Text = focusMoveToBind.StartingFocusDistance;
            txtStartingThumb.Text = focusMoveToBind.StartingFocusThumb.ToString();
            txtSetEndFocus.Text = focusMoveToBind.EndingFocusDistance;
            txtEndingThumb.Text = focusMoveToBind.EndingFocusThumb.ToString();
            txtFocusDuration.Text = focusMoveToBind.FocusDuration.ToString();
            txtSetFocusDuration.Text = focusMoveToBind.FocusDuration.ToString();
            txtEndingThumbIris.Text = focusMoveToBind.EndingIrisThumb.ToString();
            txtSetEndIris.Text = focusMoveToBind.EndingFstop;
            txtSetStartIris.Text = focusMoveToBind.StartingFstop;
            txtStartingThumbIris.Text = focusMoveToBind.StartingIrisThumb.ToString();
        }

        private void ThreadAction(bool isBatch)
        {
            rackingFocus = true;
            List<FocusMove> threadFocusMove = new List<FocusMove>();

            if (isBatch)
            {
                threadFocusMove = focusMoves.FocusMoves;
            }
            else
            {
                threadFocusMove.Add(currentFocusMove);
            }

            foreach (FocusMove focusMove in threadFocusMove)
            {
                //Clear local batch list if abort detected during batch execution
                if (focusAborted)
                {
                    //Starts handle abort background thread
                    Thread handleAbortThread = new Thread(HandleAbort);
                    handleAbortThread.IsBackground = true;
                    handleAbortThread.Start();

                    break;
                }

                Stopwatch threadStopWatch = new Stopwatch();
                threadStopWatch.Start();

                int currentThumb = 0;
                int currentIrisThumb = 0;

                BeginInvoke((MethodInvoker)delegate
                {
                    tbFocus.Enabled = false;
                    BindFocusMove(focusMove);
                    currentThumb = tbFocus.Value;
                    currentIrisThumb = tbIris.Value;
                    Application.DoEvents();
                });

                //logger.LogError("startingFocus: " + focusMove.StartingFocusThumb.ToString(), EventLogEntryType.Information);
                //logger.LogError("endingFocus: " + focusMove.EndingFocusThumb.ToString(), EventLogEntryType.Information);
                //logger.LogError("duration: " + focusMove.FocusDuration.ToString(), EventLogEntryType.Information);

                if (cameraAPI.Cameras.Count > 0 || birgerSerial.IsOpen())
                {
                    Stopwatch preRackStopWatch = new Stopwatch();
                    preRackStopWatch.Start();

                    //Drag thumb to start position if not already there
                    if (currentThumb != focusMove.StartingFocusThumb)
                    {
                        //logger.LogError("currentThumb: " + currentThumb.ToString(), EventLogEntryType.Information);

                        BeginInvoke((MethodInvoker)delegate
                        {
                            while (currentThumb != focusMove.StartingFocusThumb)
                            {
                                tbFocus.Value = focusMove.StartingFocusThumb;
                                txtStartingThumbActual.Text = tbFocus.Value.ToString();
                                Application.DoEvents();
                                currentThumb = tbFocus.Value;
                            }
                        });
                    }

                    //Drag iris to start position if not already there
                    if (currentIrisThumb != focusMove.StartingIrisThumb)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            tbIris.Value = focusMove.StartingIrisThumb;
                            Application.DoEvents();
                        });
                    }

                    if (currentFocusMove.StartingFocusThumb != currentFocusMove.EndingFocusThumb)
                    {
                        //Perform rack focus
                        Stopwatch rackToEndStopWatch = new Stopwatch();
                        rackToEndStopWatch.Start();

                        //logger.LogError("Begin rackToEnd", EventLogEntryType.Information);
                        rackingFocus = true;
                        if (enableBeeps)
                            System.Media.SystemSounds.Beep.Play();

                        //Wait until command queue has been fully executed
                        Thread.Sleep(lrgDelay * 5);

                        while (commandQueue.Count > 0)
                        {
                            Thread.Sleep(lrgDelay * 5);
                        }

                        rackToEnd();

                        //Pause here while racking focus since rachToEnd happens in parallel to this
                        while (rackingFocus)
                        {

                        }

                        //logger.LogError("End rackToEnd", EventLogEntryType.Information);

                        //logger.LogError("currentThumb: " + currentThumb.ToString(), EventLogEntryType.Information);

                        rackingFocus = false;
                        //logger.LogError("Rack to End Stopwatch: " + rackToEndStopWatch.Elapsed.ToString(), EventLogEntryType.Information);
                    }
                    else
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            txtEndingThumbActual.Text = tbFocus.Value.ToString();
                            Application.DoEvents();
                        });

                        while ((int)currentFocusMove.FocusDuration.TotalMilliseconds < 0)
                        {

                        }

                        try
                        {
                            Thread.Sleep((int)currentFocusMove.FocusDuration.TotalMilliseconds);
                        }
                        catch (Exception ex)
                        {

                        }

                        rackingFocus = false;
                    }

                    //Set Iris to End if not already there
                    if (currentFocusMove.StartingIrisThumb != currentFocusMove.EndingIrisThumb)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            tbIris.Value = focusMove.EndingIrisThumb;
                            Application.DoEvents();
                        });
                    }
                }
                else
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        rackingFocus = false;
                        rackingFocusBatch = false;
                        btnBatchRackFocus.Enabled = true;
                        btnRackFocus.Text = "Rack Focus";
                        btnRackFocus.Height = 23;
                        btnRackFocus.ForeColor = Color.Black;
                    });

                    FindAndMoveMsgBox("Lost Communication", this.Location.X + this.Height / 2, this.Location.Y + this.Width / 8);
                    MessageBox.Show(this, "Please pair Interface with Birger Mount.", "Lost Communication");
                }

                //logger.LogError("Actual focus duration: " + threadStopWatch.Elapsed.ToString(), EventLogEntryType.Information);
                //logger.LogError("Delta: " + TimeSpan.FromTicks(focusMove.FocusDuration.Ticks - threadStopWatch.Elapsed.Ticks).ToString(), EventLogEntryType.Information);
                threadStopWatch.Reset();
            }

            BeginInvoke((MethodInvoker)delegate
            {
                tbFocus.Enabled = true;
                rackingFocus = false;
                rackingFocusBatch = false;
                btnBatchRackFocus.Enabled = true;
                btnRackFocus.Text = "Rack Focus";
                btnRackFocus.Height = 23;
                btnRackFocus.ForeColor = Color.Black;
            });
        }

        private void btnBatchRackFocus_Click(object sender, EventArgs e)
        {
            if (focusMoves.FocusMoves.Count > 0)
            {
                rackingFocusBatch = true;
                btnBatchRackFocus.Enabled = false;
                btnRackFocus.Text = "Click to Cancel";
                btnRackFocus.Height = 35;
                btnRackFocus.ForeColor = Color.Red;

                rackFocusBatchThread = new Thread(() => ThreadAction(true));
                rackFocusBatchThread.IsBackground = true;
                rackFocusBatchThread.Start();
            }
            else
            {
                FindAndMoveMsgBox("Add Moves", this.Location.X + this.Height / 2, this.Location.Y + this.Width / 8);
                MessageBox.Show(this, "Please add one or more focus moves to the batch.", "Add Moves");
            }
        }

        private void btnRackFocus_Click(object sender, EventArgs e)
        {
            //If start and end iris not manually set, use current iris setting
            if (txtStartingThumbIris.Text.Trim() == string.Empty)
            {
                btnSetStartIris_Click(sender, e);
            }
            if (txtEndingThumbIris.Text.Trim() == string.Empty)
            {
                btnSetEndIris_Click(sender, e);
            }

            //If currently performing a batch, abort on click
            if (!btnBatchRackFocus.Enabled)
            {
                focusAborted = true;
            }
            else
            {
                if (currentFocusMove.IsValid())
                {
                    btnBatchRackFocus.Enabled = false;
                    btnRackFocus.Text = "Click to Cancel";
                    btnRackFocus.Height = 35;
                    btnRackFocus.ForeColor = Color.Red;

                    rackFocusThread = new Thread(() => ThreadAction(false));
                    rackFocusThread.IsBackground = true;
                    rackFocusThread.Start();
                }
                else
                {
                    FindAndMoveMsgBox("Invalid Move", this.Location.X + this.Height / 2, this.Location.Y + this.Width / 8);
                    MessageBox.Show(this, "Please input a valid focus move or edit from batch.", "Invalid Move");
                }
            }
        }

        private void btnTimeFocusMove_Click(object sender, EventArgs e)
        {
            if (stopWatch.IsRunning)
            {
                btnTimeFocusMove.Text = "Time Focus Move";
                btnTimeFocusMove.ForeColor = Color.Black;

                if (txtSetFocusDuration.Text != txtFocusDuration.Text)
                {
                    btnSetFocusDuration.ForeColor = Color.Red;
                }
                else
                {
                    btnSetFocusDuration.ForeColor = Color.Black;
                }

                stopWatch.Stop();
                timer1.Stop();
            }
            else
            {
                stopWatch.Reset();
                stopWatch.Start();
                timer1.Start();
                btnTimeFocusMove.Text = "Click to Stop";
                btnTimeFocusMove.ForeColor = Color.Red;
            }
        }

        private void timer1_tick(object sender, EventArgs e)
        {
            txtFocusTime.Text = String.Format("{0:D2}:{1:D2}:{2:D2}.{3:D3}", stopWatch.Elapsed.Duration().Hours, stopWatch.Elapsed.Duration().Minutes, stopWatch.Elapsed.Duration().Seconds, stopWatch.Elapsed.Duration().Milliseconds);
            txtSetFocusDuration.Text = String.Format("{0:D2}:{1:D2}:{2:D2}.{3:D3}", stopWatch.Elapsed.Duration().Hours, stopWatch.Elapsed.Duration().Minutes, stopWatch.Elapsed.Duration().Seconds, stopWatch.Elapsed.Duration().Milliseconds);
        }

        private void btnSetFocusDuration_Click(object sender, EventArgs e)
        {
            TimeSpan timeSpan = new TimeSpan();
            if (TimeSpan.TryParse(txtSetFocusDuration.Text, out timeSpan))
            {
                txtFocusDuration.Text = txtSetFocusDuration.Text;

                if (txtSetFocusDuration.Text != txtFocusDuration.Text)
                {
                    btnSetFocusDuration.ForeColor = Color.Red;
                }
                else
                {
                    btnSetFocusDuration.ForeColor = Color.Black;
                }

                string formattedTimepan;
                if (timeSpan.TotalMinutes < 1.0)
                {
                    formattedTimepan = String.Format("{0}.{1:D3}s", timeSpan.Seconds, timeSpan.Milliseconds);
                }
                else if (timeSpan.TotalHours < 1.0)
                {
                    formattedTimepan = String.Format("{0}m:{1:D2}.{2:D3}s", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
                }
                else // more than 1 hour
                {
                    formattedTimepan = String.Format("{0}h:{1:D2}m:{2:D2}.{3:D3}s", (int)timeSpan.TotalHours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
                }

                lblFormattedTimespan.Text = formattedTimepan;
                currentFocusMove.FocusDuration = timeSpan;
            }
            else
            {
                FindAndMoveMsgBox("Invalid Duration", this.Location.X + this.Height / 2, this.Location.Y + this.Width / 8);
                MessageBox.Show(this, "Please enter a valid duration - 00:00:00", "Invalid Duration");
            }

        }

        private void btnSwap_Click(object sender, EventArgs e)
        {
            //Swap control values for starting and ending focus moves
            int startingFocusTemp = currentFocusMove.StartingFocusThumb;
            int endingFocusTemp = currentFocusMove.EndingFocusThumb;
            string txtSetStartFocusTemp = txtSetStartFocus.Text;
            string txtStartingThumbTemp = txtStartingThumb.Text;
            string txtSetEndFocusTemp = txtSetEndFocus.Text;
            string txtEndingThumbTemp = txtEndingThumb.Text;

            currentFocusMove.StartingFocusThumb = endingFocusTemp;
            currentFocusMove.EndingFocusThumb = startingFocusTemp;

            txtSetStartFocus.Text = txtSetEndFocusTemp;
            txtStartingThumb.Text = txtEndingThumbTemp;
            txtSetEndFocus.Text = txtSetStartFocusTemp;
            txtEndingThumb.Text = txtStartingThumbTemp;

            currentFocusMove.StartingFocusDistance = txtSetEndFocusTemp;
            currentFocusMove.StartingFocusThumb = endingFocusTemp;
            currentFocusMove.EndingFocusDistance = txtSetStartFocusTemp;
            currentFocusMove.EndingFocusThumb = startingFocusTemp;
        }

        void ClearForm(bool buttonsToo)
        {
            //Resets form
            currentFocusMove = new FocusMove();
            currentFocusMoveTemp = new FocusMove();

            foreach (Control control in this.Controls)
            {
                if (control.GetType() == typeof(TextBox))
                {
                    ((TextBox)(control)).Text = string.Empty;
                }
            }

            if (buttonsToo)
            {
                foreach (Control control in this.Controls)
                {
                    if (control.GetType() == typeof(Button))
                    {
                        ((Button)(control)).ForeColor = Color.Black;
                    }
                }
            }

            lblFormattedTimespan.Text = string.Empty;

            btnSave.Visible = false;
            btnCopy.Visible = false;

            currentFocusMove.FocusDuration = TimeSpan.MinValue;
            currentFocusMoveTemp.FocusDuration = TimeSpan.MaxValue;

            if (stopWatch.IsRunning)
            {
                btnTimeFocusMove.Text = "Time Focus Move";
                btnTimeFocusMove.ForeColor = Color.Black;

                if (txtSetFocusDuration.Text != txtFocusDuration.Text)
                {
                    btnSetFocusDuration.ForeColor = Color.Red;
                }
                else
                {
                    btnSetFocusDuration.ForeColor = Color.Black;
                }

                stopWatch.Stop();
                timer1.Stop();
            }
        }

        private void btnAddToBatch_Click(object sender, EventArgs e)
        {
            //If start and end iris not manually set, use current iris setting
            if (txtStartingThumbIris.Text.Trim() == string.Empty)
            {
                btnSetStartIris_Click(sender, e);
            }
            if (txtEndingThumbIris.Text.Trim() == string.Empty)
            {
                btnSetEndIris_Click(sender, e);
            }

            //Add input focus move to the batch
            if (currentFocusMove.IsValid())
            {
                focusMoves.FocusMoves.Add(currentFocusMove);
                listBox1.DataSource = null;
                listBox1.DataSource = focusMoves.FocusMoves;
                ClearForm(true);
            }
            else
            {
                FindAndMoveMsgBox("Invalid Focus Move", this.Location.X + this.Height / 2, this.Location.Y + this.Width / 8);
                MessageBox.Show(this, "Please input a valid focus move.", "Invalid Focus Move");
            }
        }

        private void chkEnableBeeps_CheckedChanged(object sender, EventArgs e)
        {
            //Toggles enable beep flag
            if (chkEnableBeeps.Checked)
            {
                enableBeeps = true;
            }
            else
            {
                enableBeeps = false;
            }
        }

        private void miSaveBatch_Click(object sender, EventArgs e)
        {
            //Opens the Save Batch dialog
            if (focusMoves.FocusMoves.Count > 0)
            {
                dlgSave.InitialDirectory = xmlDirectory;
                dlgSave.DefaultExt = "xml";
                dlgSave.Filter = "XML File|*.xml";
                dlgSave.Title = "Save Focus Move Batch";
                dlgSave.FileName = currentXML;
                dlgSave.RestoreDirectory = true;
                dlgSave.ShowDialog();
            }
            else
            {
                FindAndMoveMsgBox("Add A Move", this.Location.X + this.Height / 2, this.Location.Y + this.Width / 8);
                MessageBox.Show(this, "Please add focus moves to batch before saving.", "Add A Move");
            }
        }

        private void miOpenBatch_Click(object sender, EventArgs e)
        {
            //Opens the Open Batch dialog
            dlgOpen.InitialDirectory = xmlDirectory;
            dlgOpen.Filter = "XML File|*.xml";
            dlgOpen.Title = "Open Focus Move Batch";
            dlgOpen.RestoreDirectory = true;
            dlgOpen.ShowDialog();
        }

        private void dlgSave_FileOk(object sender, CancelEventArgs e)
        {
            //Serializes batch to XML from focus move list
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(FocusMoveList));
                using (TextWriter textWriter = new StreamWriter(dlgSave.FileName))
                {
                    serializer.Serialize(textWriter, focusMoves);
                    textWriter.Close();
                }
            }
            catch (Exception ex)
            {

            }
            dlgSave.Dispose();
        }

        private void dlgOpen_FileOk(object sender, CancelEventArgs e)
        {
            //Deserializes batch from XML to focus move list
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(FocusMoveList));

                using (TextReader textReader = new StreamReader(dlgOpen.FileName))
                {
                    focusMoves = (FocusMoveList)xmlSerializer.Deserialize(textReader);
                    textReader.Close();
                }

                ClearForm(true);
                dlgOpen.Dispose();
                listBox1.DataSource = null;
                listBox1.DataSource = focusMoves.FocusMoves;
            }
            catch (Exception ex)
            {

            }
        }

        private void miNewBatch_Click(object sender, EventArgs e)
        {
            //Clears form and starts a new batch collection
            currentFocusMove = new FocusMove();
            focusMoves = new FocusMoveList();
            listBox1.DataSource = null;
            listBox1.DataSource = focusMoves.FocusMoves;

            ClearForm(true);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            //Save current focus move to batch
            currentFocusMoveTemp.EndingFocusDistance = currentFocusMove.EndingFocusDistance;
            currentFocusMoveTemp.EndingFocusThumb = currentFocusMove.EndingFocusThumb;
            currentFocusMoveTemp.FocusDuration = currentFocusMove.FocusDuration;
            currentFocusMoveTemp.FocusDurationString = currentFocusMove.FocusDurationString;
            currentFocusMoveTemp.StartingFocusDistance = currentFocusMove.StartingFocusDistance;
            currentFocusMoveTemp.StartingFocusThumb = currentFocusMove.StartingFocusThumb;
            currentFocusMoveTemp.StartingIrisThumb = currentFocusMove.StartingIrisThumb;
            currentFocusMoveTemp.EndingIrisThumb = currentFocusMove.EndingIrisThumb;
            currentFocusMoveTemp.StartingFstop = currentFocusMove.StartingFstop;
            currentFocusMoveTemp.EndingFstop = currentFocusMove.EndingFstop;

            listBox1.DataSource = null;
            listBox1.DataSource = focusMoves.FocusMoves;

            ClearForm(true);
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            //Copy focus move, add to collection and select it
            focusMoves.FocusMoves.Add(currentFocusMove);
            listBox1.DataSource = null;
            listBox1.DataSource = focusMoves.FocusMoves;
            listBox1.SelectedIndex = focusMoves.FocusMoves.Count - 1;

            ClearForm(true);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            //Clear the form
            ClearForm(true);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            //Delete the focus move and update the collection
            if (listBox1.SelectedIndex >= 0)
            {
                FindAndMoveMsgBox("Confirm Delete", this.Location.X + this.Height / 2, this.Location.Y + this.Width / 8);
                if (MessageBox.Show(this, "Are You Sure?", "Confirm Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    focusMoves.FocusMoves.Remove(focusMoves.FocusMoves[listBox1.SelectedIndex]);
                    listBox1.DataSource = null;
                    listBox1.DataSource = focusMoves.FocusMoves;
                }
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            //Edit the selected focus move and populate controls so it can be changed
            if (listBox1.SelectedIndex >= 0)
            {
                currentFocusMoveTemp = focusMoves.FocusMoves[listBox1.SelectedIndex];

                currentFocusMove.EndingFocusDistance = currentFocusMoveTemp.EndingFocusDistance;
                currentFocusMove.EndingFocusThumb = currentFocusMoveTemp.EndingFocusThumb;
                currentFocusMove.FocusDuration = currentFocusMoveTemp.FocusDuration;
                currentFocusMove.FocusDurationString = currentFocusMoveTemp.FocusDurationString;
                currentFocusMove.StartingFocusDistance = currentFocusMoveTemp.StartingFocusDistance;
                currentFocusMove.StartingFocusThumb = currentFocusMoveTemp.StartingFocusThumb;
                currentFocusMove.StartingIrisThumb = currentFocusMoveTemp.StartingIrisThumb;
                currentFocusMove.StartingFstop = currentFocusMoveTemp.StartingFstop;
                currentFocusMove.EndingIrisThumb = currentFocusMoveTemp.EndingIrisThumb;
                currentFocusMove.EndingFstop = currentFocusMoveTemp.EndingFstop;

                BindFocusMove(currentFocusMove);
                btnSave.Visible = true;
                btnCopy.Visible = true;
            }
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            //Move focus move up in the list/collection hierarchy
            try
            {
                focusMoves.FocusMoves.Reverse(listBox1.SelectedIndex - 1, 2);
                listBox1.DataSource = null;
                listBox1.DataSource = focusMoves.FocusMoves;
                listBox1.SelectedIndex = listBox1.SelectedIndex - 1;
            }
            catch (Exception ex)
            {

            }
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            //Move focus move down in the list/collection hierarchy
            try
            {
                focusMoves.FocusMoves.Reverse(listBox1.SelectedIndex, 2);
                listBox1.DataSource = null;
                listBox1.DataSource = focusMoves.FocusMoves;
                listBox1.SelectedIndex = listBox1.SelectedIndex + 1;
            }
            catch (Exception ex)
            {

            }
        }

        private void btnRecord_Click(object sender, EventArgs e)
        {
            //Record focus move

            //Set recording flag to the opposite of its current value
            recording = !recording;

            int currentThumb = tbFocus.Value;
            int currentThumbIris = tbIris.Value;
            if (recording)
            {
                if (focusMoves.FocusMoves.Count > 0)
                {
                    FindAndMoveMsgBox("Begin Recording", this.Location.X + this.Height / 2, this.Location.Y + this.Width / 8);
                    if (MessageBox.Show(this, "Clear current batch and start Realtime Recording Session?", "Begin Recording", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        //Clear current batch and listbox
                        focusMoves.FocusMoves = new List<FocusMove>();
                        listBox1.DataSource = null;
                        listBox1.DataSource = focusMoves.FocusMoves;

                        btnRecord.Text = "Stop Recording";
                        btnRecord.ForeColor = Color.Red;
                        recordTimer.Interval = 25;
                        recordTimer.Start();
                    }
                }
                else
                {
                    btnRecord.Text = "Stop Recording";
                    btnRecord.ForeColor = Color.Red;
                    recordTimer.Interval = 25;
                    recordTimer.Start();
                }
            }
            else
            {
                recordTimer.Stop();
                btnRecord.Text = "Begin Realtime Recording";
                btnRecord.ForeColor = Color.Black;
                listBox1.DataSource = null;
                listBox1.DataSource = focusMoves.FocusMoves;
            }
        }

        private void recordTimer_Tick(object sender, EventArgs e)
        {
            //Every recording timer tick, create a new focus move based on current focus settings
            currentFocusMove.StartingFocusDistance = lblTrackbarValue.Text;
            currentFocusMove.EndingFocusDistance = lblTrackbarValue.Text;
            currentFocusMove.FocusDuration = TimeSpan.FromMilliseconds(recordTimer.Interval);
            currentFocusMove.StartingFocusThumb = tbFocus.Value;
            currentFocusMove.EndingFocusThumb = tbFocus.Value;
            currentFocusMove.StartingIrisThumb = tbIris.Value;
            currentFocusMove.EndingIrisThumb = tbIris.Value;
            currentFocusMove.StartingFstop = lblIrisBarValue.Text;
            currentFocusMove.EndingFstop = lblIrisBarValue.Text;

            BindFocusMove(currentFocusMove);
            focusMoves.FocusMoves.Add(currentFocusMove);
            recordingStopWatch.Reset();
            recordingStopWatch.Start();

            ClearForm(false);
        }

        private void btnSetStartIris_Click(object sender, EventArgs e)
        {
            //Set starting iris
            currentFocusMove.StartingIrisThumb = tbIris.Value;
            txtStartingThumbIris.Text = currentFocusMove.StartingIrisThumb.ToString();
            currentFocusMove.StartingFstop = lblIrisBarValue.Text;
            txtSetStartIris.Text = currentFocusMove.StartingFstop;
        }

        private void btnJumpToStartIris_Click(object sender, EventArgs e)
        {
            //Jump to starting iris
            if (tbIris.Value != currentFocusMove.StartingIrisThumb)
            {
                tbIris.Value = currentFocusMove.StartingIrisThumb;
            }
        }

        private void btnSetEndIris_Click(object sender, EventArgs e)
        {
            //Set ending iris
            currentFocusMove.EndingIrisThumb = tbIris.Value;
            txtEndingThumbIris.Text = currentFocusMove.EndingIrisThumb.ToString();
            currentFocusMove.EndingFstop = lblIrisBarValue.Text;
            txtSetEndIris.Text = currentFocusMove.EndingFstop;
        }

        private void btnJumpToEndIris_Click(object sender, EventArgs e)
        {
            //Jump to ending iris
            if (tbIris.Value != currentFocusMove.EndingIrisThumb)
            {
                tbIris.Value = currentFocusMove.EndingIrisThumb;
            }
        }

        private void btnSwapIris_Click(object sender, EventArgs e)
        {
            //Swap starting and ending iris values
            int startingIrisTemp = currentFocusMove.StartingIrisThumb;
            int endingIrisTemp = currentFocusMove.EndingIrisThumb;
            string txtSetStartIrisTemp = txtSetStartIris.Text;
            string txtStartingThumbIrisTemp = txtStartingThumbIris.Text;
            string txtSetEndIrisTemp = txtSetEndIris.Text;
            string txtEndingThumbIrisTemp = txtEndingThumbIris.Text;

            currentFocusMove.StartingIrisThumb = endingIrisTemp;
            currentFocusMove.EndingIrisThumb = startingIrisTemp;

            txtSetStartIris.Text = txtSetEndIrisTemp;
            txtStartingThumbIris.Text = txtEndingThumbIrisTemp;
            txtSetEndIris.Text = txtSetStartIrisTemp;
            txtEndingThumbIris.Text = txtStartingThumbIrisTemp;

            currentFocusMove.StartingIrisThumb = endingIrisTemp;
            currentFocusMove.EndingIrisThumb = startingIrisTemp;
        }

        private void btnRangefinder_Click(object sender, EventArgs e)
        {
            //Launch rangefinder dialog
            FindAndMoveMsgBox("RangeFinder", this.Location.X + this.Height / 2, this.Location.Y + this.Width / 8);

            RangeFinder frmRangeFinder = new RangeFinder(this, birgerSerial, tbFocus.Value, tbIris.Value, tbIris.Maximum, encoderItems, encoderDictionary, feetDictionary, hyperFocal,
                trackbarDictionary, cameraAPI.Cameras[0]);
            frmRangeFinder.ShowDialog(this);

            pauseLensPlayWatch.Start();
        }

        void ExecuteQueue()
        {
            //Drive lens near or far based on the command at the top of queue
            while (cameraAPI.Cameras != null)
            {
                while (cameraAPI.Cameras.Count > 0)
                {
                    if (commandQueue.Count > 0)
                    {
                        //What size is the command at the top of the queue
                        double stepSize = commandQueue.Peek();

                        //Execute the command based on size and polarity, sleep for proper duration, then remove from queue
                        if (stepSize == encoderItems.EncoderItems[0].MediumRatio)
                        {
                            uint result;
                            if (cameraAPI.Cameras[0] != null)
                            {
                                //Safety delay pre lens movement
                                Thread.Sleep(Math.Max(0, (medDelay - prevDelay)));
                                queueExecuting = true;
                                result = cameraAPI.Cameras[0].DriveLensFarTwo();

                                stepCounterEx = stepCounterEx + encoderItems.EncoderItems[0].MediumRatio;

                                if (stepCounterEx > encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder)
                                {
                                    stepCounterEx = encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder;
                                }

                                //Safety delay after lens movement to ensure accuracy
                                Thread.Sleep(medDelay);
                                prevDelay = medDelay;

                                if (nearStepCounter != 0)
                                {
                                    //logger.LogError("nearStepCounter: " + nearStepCounter, EventLogEntryType.Information);
                                    nearStepCounter = 0;
                                }

                                farStepCounter = farStepCounter + (int)encoderItems.EncoderItems[0].MediumRatio;
                            }

                            queueExecuting = false;

                            try
                            {
                                commandQueue.Dequeue();
                            }
                            catch (Exception ex)
                            {
                                //logger.LogError("Dequeue exception: " + ex.Message, EventLogEntryType.Error);
                            }
                        }
                        else if (stepSize == encoderItems.EncoderItems[0].LargeRatio)
                        {
                            uint result;
                            if (cameraAPI.Cameras[0] != null)
                            {
                                //Safety delay pre lens movement
                                Thread.Sleep(Math.Max(0, (lrgDelay - prevDelay)));
                                queueExecuting = true;
                                result = cameraAPI.Cameras[0].DriveLensFarThree();

                                stepCounterEx = stepCounterEx + encoderItems.EncoderItems[0].LargeRatio;

                                if (stepCounterEx > encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder)
                                {
                                    stepCounterEx = encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder;
                                }

                                //Safety delay after lens movement to ensure accuracy
                                Thread.Sleep(lrgDelay);
                                prevDelay = lrgDelay;

                                if (nearStepCounter != 0)
                                {
                                    //logger.LogError("nearStepCounter: " + nearStepCounter, EventLogEntryType.Information);
                                    nearStepCounter = 0;
                                }

                                farStepCounter = farStepCounter + (int)encoderItems.EncoderItems[0].LargeRatio;
                            }

                            queueExecuting = false;

                            try
                            {
                                commandQueue.Dequeue();
                            }
                            catch (Exception ex)
                            {
                                //logger.LogError("Dequeue exception: " + ex.Message, EventLogEntryType.Error);
                            }
                        }
                        else if (stepSize == 1.0)
                        {
                            uint result;
                            if (cameraAPI.Cameras[0] != null)
                            {
                                //Safety delay pre lens movement
                                Thread.Sleep(Math.Max(0, (smallDelay - prevDelay)));
                                queueExecuting = true;
                                result = cameraAPI.Cameras[0].DriveLensFarOne();

                                stepCounterEx = stepCounterEx + 1.0;

                                if (stepCounterEx > encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder)
                                {
                                    stepCounterEx = encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder;
                                }

                                //Safety delay after lens movement to ensure accuracy
                                Thread.Sleep(smallDelay);
                                prevDelay = smallDelay;

                                if (nearStepCounter != 0)
                                {
                                    //logger.LogError("nearStepCounter: " + nearStepCounter, EventLogEntryType.Information);
                                    nearStepCounter = 0;
                                }

                                farStepCounter = farStepCounter + (int)1.0;
                            }

                            queueExecuting = false;

                            try
                            {
                                commandQueue.Dequeue();
                            }
                            catch (Exception ex)
                            {
                                //logger.LogError("Dequeue exception: " + ex.Message, EventLogEntryType.Error);
                            }
                        }
                        else if (stepSize == -encoderItems.EncoderItems[0].MediumRatio)
                        {
                            uint result;
                            if (cameraAPI.Cameras[0] != null)
                            {
                                //Safety delay pre lens movement
                                Thread.Sleep(Math.Max(0, (medDelay - prevDelay)));
                                queueExecuting = true;
                                result = cameraAPI.Cameras[0].DriveLensNearTwo();

                                stepCounterEx = stepCounterEx - encoderItems.EncoderItems[0].MediumRatio;

                                if (stepCounterEx < 0)
                                    stepCounterEx = 0;

                                //Safety delay after lens movement to ensure accuracy
                                Thread.Sleep(medDelay);
                                prevDelay = medDelay;

                                if (farStepCounter != 0)
                                {
                                    //logger.LogError("farStepCounter: " + farStepCounter, EventLogEntryType.Information);
                                    farStepCounter = 0;
                                }

                                nearStepCounter = nearStepCounter + (int)encoderItems.EncoderItems[0].MediumRatio;
                            }

                            queueExecuting = false;

                            try
                            {
                                commandQueue.Dequeue();
                            }
                            catch (Exception ex)
                            {
                                //logger.LogError("Dequeue exception: " + ex.Message, EventLogEntryType.Error);
                            }
                        }
                        else if (stepSize == -encoderItems.EncoderItems[0].LargeRatio)
                        {
                            uint result;
                            if (cameraAPI.Cameras[0] != null)
                            {
                                //Safety delay pre lens movement
                                Thread.Sleep(Math.Max(0, (lrgDelay - prevDelay)));
                                queueExecuting = true;
                                result = cameraAPI.Cameras[0].DriveLensNearThree();

                                stepCounterEx = stepCounterEx - encoderItems.EncoderItems[0].LargeRatio;

                                if (stepCounterEx < 0)
                                    stepCounterEx = 0;

                                //Safety delay after lens movement to ensure accuracy
                                Thread.Sleep(lrgDelay);
                                prevDelay = lrgDelay;

                                if (farStepCounter != 0)
                                {
                                    //logger.LogError("farStepCounter: " + farStepCounter, EventLogEntryType.Information);
                                    farStepCounter = 0;
                                }

                                nearStepCounter = nearStepCounter + (int)encoderItems.EncoderItems[0].LargeRatio;
                            }

                            queueExecuting = false;

                            try
                            {
                                commandQueue.Dequeue();
                            }
                            catch (Exception ex)
                            {
                                //logger.LogError("Dequeue exception: " + ex.Message, EventLogEntryType.Error);
                            }
                        }
                        else if (stepSize == -1.0)
                        {
                            uint result;
                            if (cameraAPI.Cameras[0] != null)
                            {
                                //Safety delay pre lens movement
                                Thread.Sleep(Math.Max(0, (smallDelay - prevDelay)));
                                queueExecuting = true;
                                result = cameraAPI.Cameras[0].DriveLensNearOne();

                                stepCounterEx = stepCounterEx - 1.0;

                                if (stepCounterEx < 0)
                                    stepCounterEx = 0;

                                //Safety delay after lens movement to ensure accuracy
                                Thread.Sleep(smallDelay);
                                prevDelay = smallDelay;

                                if (farStepCounter != 0)
                                {
                                    //logger.LogError("farStepCounter: " + farStepCounter, EventLogEntryType.Information);
                                    farStepCounter = 0;
                                }

                                nearStepCounter = nearStepCounter + (int)1.0;
                            }

                            queueExecuting = false;

                            try
                            {
                                commandQueue.Dequeue();
                            }
                            catch (Exception ex)
                            {
                                //logger.LogError("Dequeue exception: " + ex.Message, EventLogEntryType.Error);
                            }
                        }
                    }
                    else
                    {
                        //Log both far and near step counts if the command queue is empty
                        if (farStepCounter != 0 && nearStepCounter != 0)
                        {
                            //logger.LogError("farStepCounter: " + farStepCounter, EventLogEntryType.Information);
                            farStepCounter = 0;

                            //logger.LogError("nearStepCounter: " + nearStepCounter, EventLogEntryType.Information);
                            nearStepCounter = 0;
                        }

                        //If command queue is empty, set prevDelay back to two seconds
                        prevDelay = 2000;
                    }
                }
            }
        }

        public void LensPlayManager()
        {
            lensPlayWatch.Reset();
            lensPlayWatch.Start();

            //If the focus bar value is zero, drive lens far for .5 seconds at small steps, then drive near for 1 second at large steps
            while (1 == 1)
            {
                bool lensPlayCompensationFormOpen = false;

                if (cameraAPI.Cameras.Count > 0 && !pauseLensPlay)
                {
                    //Deal with lens play
                    if (this.InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            if (this.OwnedForms.ToList<Form>().Count > 0)
                            {
                                if (this.OwnedForms.ToList<Form>()[0].OwnedForms.ToList<Form>().Count > 0)
                                {
                                    if (this.OwnedForms.ToList<Form>()[0].Name == "DSLRCalibration" && this.OwnedForms.ToList<Form>()[0].OwnedForms.ToList<Form>()[0].Name == "LensPlayCompensation")
                                    {
                                        lensPlayCompensationFormOpen = true;
                                    }
                                }
                            }

                            if (this.OwnedForms.ToList<Form>().Count == 0 || lensPlayCompensationFormOpen)
                            {
                                if (tbFocus.Value == 0)
                                {
                                    do
                                    {
                                        commandQueue.Enqueue(1.0);
                                        stepCounter = stepCounter + 1.0;

                                        Thread.Sleep(smallDelay);
                                    } while (lensPlayWatch.ElapsedMilliseconds < (smallDelay * 3) && commandQueue.Count == 0);

                                    lensPlayWatch.Reset();
                                    lensPlayWatch.Start();
                                    do
                                    {
                                        commandQueue.Enqueue(-encoderItems.EncoderItems[0].LargeRatio);
                                        stepCounter = 0;

                                        Thread.Sleep(lrgDelay);
                                    } while (lensPlayWatch.ElapsedMilliseconds < (lrgDelay * 3) && commandQueue.Count == 0);

                                    lensPlayWatch.Reset();
                                    lensPlayWatch.Start();
                                }
                                else
                                    if (tbFocus.Value == tbFocus.Maximum)
                                    {
                                        do
                                        {
                                            commandQueue.Enqueue(encoderItems.EncoderItems[0].LargeRatio);
                                            stepCounter = stepCounter + encoderItems.EncoderItems[0].LargeRatio;
                                            if (stepCounter > encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder)
                                            {
                                                stepCounter = encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder;
                                            }

                                            Thread.Sleep(lrgDelay);
                                        } while (lensPlayWatch.ElapsedMilliseconds < lrgDelay * 3);

                                        lensPlayWatch.Reset();
                                        lensPlayWatch.Start();
                                    }
                            }
                        });
                    }
                }

                Thread.Sleep(1000);

                //Toggle lens play pause flag after three seconds
                if (pauseLensPlayWatch.ElapsedMilliseconds > 3000)
                {
                    pauseLensPlayWatch.Stop();
                    pauseLensPlayWatch.Reset();

                    if (pauseLensPlay == true)
                        pauseLensPlay = false;
                }
            }
        }

        void IrisBarMonitor()
        {
            while (1 == 1)
            {
                if (cameraAPI.Cameras.Count > 0)
                {
                    try
                    {
                        //Check if lens is attached before executing logic
                        if (cameraAPI.Cameras[0].LensName.Trim() != string.Empty)
                        {
                            Thread.Sleep(25);
                            Application.DoEvents();

                            if (irisBarWatch.ElapsedMilliseconds > 250)
                            {
                                if (this.InvokeRequired)
                                {
                                    BeginInvoke((MethodInvoker)delegate
                                    {
                                        //Check the iris bar and set the new aperture based on it as long as the commandqueue is not currently setting focus also
                                        if (!queueExecuting)
                                        {
                                            if (tbIris.Value != tbIrisBuffer)
                                            {
                                                cameraAPI.Cameras[0].SetAperture(tbIris.Value);
                                            }

                                            tbIrisBuffer = tbIris.Value;
                                            irisBarWatch.Reset();
                                            irisBarWatch.Start();
                                        }
                                    });
                                }
                                else
                                {
                                    if (!queueExecuting)
                                    {
                                        if (tbIris.Value != tbIrisBuffer)
                                        {
                                            cameraAPI.Cameras[0].SetAperture(tbIris.Value);
                                        }

                                        tbIrisBuffer = tbIris.Value;
                                        irisBarWatch.Reset();
                                        irisBarWatch.Start();
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        void FocusBarMonitor()
        {
            int delayFactor = 1;
            int rackingNearCount = 0;
            int rackingFarCount = 0;
            loggerStrBuilder.AppendLine();

            while (1 == 1)
            {
                Thread.Sleep(25);
                Application.DoEvents();

                //Determine the focus bar delta at a particular interval and build the command queue based on the size of the deltas.

                if (focusBarWatch.ElapsedMilliseconds > (lrgDelay * delayFactor))
                {
                    if (this.InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            if (cameraAPI.Cameras != null)
                            {
                                if (cameraAPI.Cameras.Count > 0)
                                {
                                    if (encoderItems.EncoderItems.Count > 0 && tbFocus.Value != tbFocusBuffer)
                                    {
                                        if (tbFocus.Value > tbFocusBuffer)
                                        {
                                            if (rackingNear == true && rackingFar == false)
                                            {
                                                directionChanged = true;
                                                rackingFarCount++;
                                            }
                                            else
                                            {
                                                directionChanged = false;
                                            }

                                            rackingNear = false;
                                            rackingFar = true;

                                            if (tbFocusBuffer == 0)
                                            {
                                                commandQueue.Enqueue(-encoderItems.EncoderItems[0].LargeRatio);
                                            }

                                            if (tbFocus.Value == tbFocus.Maximum)
                                            {
                                                encoderRemainderNear = 0;
                                                encoderRemainderFar = 0;
                                            }

                                            double encoderDelta;

                                            if (directionChanged && tbFocusBuffer != 0 && lensPlayCompensationEnabled)
                                            {
                                                encoderDelta = (double)trackbarDictionary[tbFocus.Value] - trackbarDictionary[tbFocusBuffer] + encoderRemainderFar;
                                                //encoderDelta = encoderDelta + (encoderDelta * encoderItems.LensPlayCompensationRatio);
                                            }
                                            else
                                                encoderDelta = (double)trackbarDictionary[tbFocus.Value] - trackbarDictionary[tbFocusBuffer] + encoderRemainderFar;

                                            loggerStrBuilder.AppendLine();
                                            loggerStrBuilder.AppendLine();
                                            loggerStrBuilder.AppendLine("tbFocusBuffer: " + tbFocusBuffer);
                                            loggerStrBuilder.AppendLine("encoderDelta: " + encoderDelta);
                                            loggerStrBuilder.AppendLine("delayFactor: " + delayFactor);
                                            loggerStrBuilder.AppendLine("encoderRemainderNear: " + encoderRemainderNear);
                                            loggerStrBuilder.AppendLine("encoderRemainderFar: " + encoderRemainderFar);

                                            tbFocusBuffer = tbFocus.Value;

                                            if (encoderDelta < encoderItems.EncoderItems[0].MediumRatio)
                                            {
                                                delayFactor = 1;
                                            }
                                            else if (encoderDelta < encoderItems.EncoderItems[0].LargeRatio)
                                            {
                                                delayFactor = 3;
                                            }
                                            else
                                            {
                                                delayFactor = 5;
                                            }

                                            while (encoderDelta >= encoderItems.EncoderItems[0].LargeRatio)
                                            {
                                                commandQueue.Enqueue(encoderItems.EncoderItems[0].LargeRatio);
                                                stepCounter = stepCounter + encoderItems.EncoderItems[0].LargeRatio;
                                                encoderDelta = (double)encoderDelta - encoderItems.EncoderItems[0].LargeRatio;
                                            }

                                            while (encoderDelta >= encoderItems.EncoderItems[0].MediumRatio)
                                            {
                                                commandQueue.Enqueue(encoderItems.EncoderItems[0].MediumRatio);
                                                stepCounter = stepCounter + encoderItems.EncoderItems[0].MediumRatio;
                                                encoderDelta = (double)encoderDelta - encoderItems.EncoderItems[0].MediumRatio;
                                            }

                                            while (encoderDelta >= 1)
                                            {
                                                commandQueue.Enqueue(1.0);
                                                stepCounter = stepCounter + 1.0;
                                                encoderDelta = (double)encoderDelta - 1;
                                            }

                                            if (encoderDelta < 1)
                                            {
                                                encoderRemainderFar = encoderDelta;
                                            }

                                            focusBarWatch.Reset();
                                            focusBarWatch.Start();
                                        }
                                        else
                                        {
                                            if (rackingNear == false && rackingFar == true)
                                            {
                                                directionChanged = true;
                                                rackingNearCount++;
                                            }
                                            else
                                                directionChanged = false;

                                            rackingNear = true;
                                            rackingFar = false;

                                            if (tbFocusBuffer == tbFocus.Maximum)
                                            {
                                                encoderRemainderNear = 0;
                                                encoderRemainderFar = 0;
                                            }

                                            double encoderDelta;

                                            if (directionChanged && tbFocusBuffer != tbFocus.Maximum && rackingNearCount % 2 == 0 && lensPlayCompensationEnabled)
                                            {
                                                double currentEncoder = (double)trackbarDictionary[tbFocus.Value];
                                                double bufferEncoder = (double)trackbarDictionary[tbFocusBuffer];

                                                double fiveEncoder = encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder * ((double).05);
                                                double fiveDist = encoderItems.EncoderItems.Find(delegate(EncoderItem ei) { return ei.Encoder == (int)(Math.Round(fiveEncoder)); }).Dist;
                                                double oneEigthEncoder = encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder * ((double)1 / 8);
                                                double oneEigthDist = encoderItems.EncoderItems.Find(delegate(EncoderItem ei) { return ei.Encoder == (int)(Math.Round(oneEigthEncoder)); }).Dist;
                                                double oneQuarterEncoder = encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder * ((double)1 / 4);
                                                double oneQuarterDist = encoderItems.EncoderItems.Find(delegate(EncoderItem ei) { return ei.Encoder == (int)(Math.Round(oneQuarterEncoder)); }).Dist;
                                                double midEncoder = encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder * ((double)1 / 2);
                                                double midDist = encoderItems.EncoderItems.Find(delegate(EncoderItem ei) { return ei.Encoder == (int)(Math.Round(midEncoder)); }).Dist;
                                                double threeFourthsEncoder = encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder * ((double)3 / 4);
                                                double threeFourthsDist = encoderItems.EncoderItems.Find(delegate(EncoderItem ei) { return ei.Encoder == (int)(Math.Round(threeFourthsEncoder)); }).Dist;
                                                double sevenEighthsEncoder = encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder * ((double)7 / 8);
                                                double sevenEighthsDist = encoderItems.EncoderItems.Find(delegate(EncoderItem ei) { return ei.Encoder == (int)(Math.Round(sevenEighthsEncoder)); }).Dist;
                                                double ninetyFiveEncoder = encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder * ((double).95);
                                                double ninetyFiveDist = encoderItems.EncoderItems.Find(delegate(EncoderItem ei) { return ei.Encoder == (int)(Math.Round(ninetyFiveEncoder)); }).Dist;
                                                double maxEncoder = encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder;
                                                double maxDist = encoderItems.EncoderItems.Find(delegate(EncoderItem ei) { return ei.Encoder == (int)(Math.Round(maxEncoder)); }).Dist;

                                                //Traveling from or to 5% of the lens distance
                                                if ((currentEncoder >= 0 && currentEncoder < oneEigthEncoder && bufferEncoder <= oneEigthEncoder) ||
                                                    (bufferEncoder >= 0 && bufferEncoder < oneEigthEncoder && currentEncoder <= oneEigthEncoder))
                                                {
                                                    //5% to one eighth
                                                    double boundaryEncoderDiff;
                                                    double actualEncoderDiff;
                                                    double lensPlayFactor;
                                                    double trackBarRatio = 16384 / encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder;

                                                    double boundaryDistDiff = Math.Abs(oneEigthDist - fiveDist);
                                                    double boundaryDistDiff2 = Math.Abs(oneEigthDist - fiveDist);

                                                    boundaryEncoderDiff = oneEigthEncoder - fiveEncoder;
                                                    actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                    lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                    //Both endpoints slightly off
                                                    if ((currentEncoder - fiveEncoder > (boundaryEncoderDiff * .1)) &&
                                                        (oneEigthEncoder - bufferEncoder > (boundaryEncoderDiff * .1)))
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = (1 + ((currentEncoder - fiveEncoder) / boundaryEncoderDiff) / 8)
                                                                * encoderItems.LensPlay5ToOneEighth;
                                                    }
                                                    //Only one of the endpoints slightly off
                                                    else if ((currentEncoder - fiveEncoder > (boundaryEncoderDiff * .1)) ||
                                                        (oneEigthEncoder - bufferEncoder > (boundaryEncoderDiff * .1)))
                                                    {
                                                        //If near endpoint is off
                                                        if (currentEncoder - fiveEncoder > (boundaryEncoderDiff * .1))
                                                        {
                                                            if (!isBenchmarking)
                                                                lensPlayCompensationRatio = (1 + ((currentEncoder - fiveEncoder) / boundaryEncoderDiff) / 4)
                                                                    * encoderItems.LensPlay5ToOneEighth;
                                                        }
                                                        //If far endpoint is off
                                                        else
                                                        {
                                                            if (!isBenchmarking)
                                                                lensPlayCompensationRatio = (1 + ((oneEigthEncoder - bufferEncoder) / boundaryEncoderDiff) / 4)
                                                                    * encoderItems.LensPlay5ToOneEighth;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlay5ToOneEighth;
                                                    }

                                                }
                                                else if ((currentEncoder >= 0 && currentEncoder < oneEigthEncoder && bufferEncoder <= oneQuarterEncoder) ||
                                                    (bufferEncoder >= 0 && bufferEncoder < oneEigthEncoder && currentEncoder <= oneQuarterEncoder))
                                                {
                                                    //5% to one quarter
                                                    double boundaryEncoderDiff;
                                                    double actualEncoderDiff;
                                                    double lensPlayFactor;
                                                    double trackBarRatio = 16384 / encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder;

                                                    double boundaryDistDiff = Math.Abs(oneEigthDist - fiveDist);
                                                    double boundaryDistDiff2 = Math.Abs(oneQuarterDist - oneEigthDist);

                                                    boundaryEncoderDiff = oneQuarterEncoder - fiveEncoder;
                                                    actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                    lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                    //Both endpoints slightly off
                                                    if ((encoderDictionary[tbFocus.Value] - fiveDist > (boundaryDistDiff / 2)) &&
                                                        (oneQuarterDist - encoderDictionary[tbFocusBuffer] > (boundaryDistDiff2 / 2)))
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = encoderItems.LensPlay5ToQuarter;
                                                    }
                                                    //Only one of the endpoints slightly off
                                                    else if ((encoderDictionary[tbFocus.Value] - fiveDist > (boundaryDistDiff / 2)) ||
                                                        (oneQuarterDist - encoderDictionary[tbFocusBuffer] > (boundaryDistDiff2 / 2)))
                                                    {
                                                        if (!isBenchmarking)
                                                        {
                                                            lensPlayCompensationRatio = (1.038) * encoderItems.LensPlay5ToQuarter;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlay5ToQuarter;
                                                    }
                                                }
                                                else if ((currentEncoder >= 0 && currentEncoder < oneEigthEncoder && bufferEncoder <= midEncoder) ||
                                                    (bufferEncoder >= 0 && bufferEncoder < oneEigthEncoder && currentEncoder <= midEncoder))
                                                {
                                                    //5% to mid
                                                    double boundaryEncoderDiff;
                                                    double actualEncoderDiff;
                                                    double lensPlayFactor;
                                                    double trackBarRatio = 16384 / encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder;

                                                    double boundaryDistDiff = Math.Abs(oneEigthDist - fiveDist);
                                                    double boundaryDistDiff2 = Math.Abs(midDist - oneQuarterDist);

                                                    boundaryEncoderDiff = midEncoder - fiveEncoder;
                                                    actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                    lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                    //Both endpoints slightly off
                                                    if ((encoderDictionary[tbFocus.Value] - fiveDist > (boundaryDistDiff / 2)) &&
                                                        (midDist - encoderDictionary[tbFocusBuffer] > (boundaryDistDiff2 / 2)))
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = encoderItems.LensPlay5ToMid;
                                                    }
                                                    //Only one of the endpoints slightly off
                                                    else if ((encoderDictionary[tbFocus.Value] - fiveDist > (boundaryDistDiff / 2)) ||
                                                        (midDist - encoderDictionary[tbFocusBuffer] > (boundaryDistDiff2 / 2)))
                                                    {
                                                        if (!isBenchmarking)
                                                        {
                                                            lensPlayCompensationRatio = (1.004) * encoderItems.LensPlay5ToMid;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlay5ToMid;
                                                    }
                                                }
                                                else if ((currentEncoder >= 0 && currentEncoder < oneEigthEncoder && bufferEncoder <= threeFourthsEncoder) ||
                                                    (bufferEncoder >= 0 && bufferEncoder < oneEigthEncoder && currentEncoder <= threeFourthsEncoder))
                                                {
                                                    //5% to three fourths
                                                    double boundaryEncoderDiff;
                                                    double actualEncoderDiff;
                                                    double lensPlayFactor;
                                                    double trackBarRatio = 16384 / encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder;

                                                    double boundaryDistDiff = Math.Abs(oneEigthDist - fiveDist);
                                                    double boundaryDistDiff2 = Math.Abs(threeFourthsDist - midDist);

                                                    boundaryEncoderDiff = threeFourthsEncoder - fiveEncoder;
                                                    actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                    lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                    //Both endpoints slightly off
                                                    if ((encoderDictionary[tbFocus.Value] - fiveDist > (boundaryDistDiff / 2)) &&
                                                        (threeFourthsDist - encoderDictionary[tbFocusBuffer] > (boundaryDistDiff2 / 2)))
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = encoderItems.LensPlay5ToThreeFourths;
                                                    }
                                                    //Only one of the endpoints slightly off
                                                    else if ((encoderDictionary[tbFocus.Value] - fiveDist > (boundaryDistDiff / 2)) ||
                                                        (threeFourthsDist - encoderDictionary[tbFocusBuffer] > (boundaryDistDiff2 / 2)))
                                                    {
                                                        if (!isBenchmarking)
                                                        {
                                                            lensPlayCompensationRatio = (1.020) * encoderItems.LensPlay5ToThreeFourths;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlay5ToThreeFourths;
                                                    }
                                                }
                                                else if ((currentEncoder >= 0 && currentEncoder < oneEigthEncoder && bufferEncoder <= sevenEighthsEncoder) ||
                                                    (bufferEncoder >= 0 && bufferEncoder < oneEigthEncoder && currentEncoder <= sevenEighthsEncoder))
                                                {
                                                    //5% to seven eighths
                                                    double boundaryEncoderDiff;
                                                    double actualEncoderDiff;
                                                    double lensPlayFactor;

                                                    boundaryEncoderDiff = sevenEighthsEncoder - fiveEncoder;
                                                    actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                    lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                    if (!isBenchmarking)
                                                        lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlay5ToSevenEigths;
                                                }
                                                else if ((currentEncoder >= 0 && currentEncoder < oneEigthEncoder && bufferEncoder <= ninetyFiveEncoder) ||
                                                    (bufferEncoder >= 0 && bufferEncoder < oneEigthEncoder && currentEncoder <= ninetyFiveEncoder))
                                                {
                                                    //5% to 95%
                                                    double boundaryEncoderDiff;
                                                    double actualEncoderDiff;
                                                    double lensPlayFactor;
                                                    double trackBarRatio = 16384 / encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder;

                                                    double boundaryDistDiff = Math.Abs(oneEigthDist - fiveDist);
                                                    double boundaryDistDiff2 = Math.Abs(ninetyFiveDist - sevenEighthsDist);

                                                    boundaryEncoderDiff = ninetyFiveEncoder - fiveEncoder;
                                                    actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                    lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                    //Both endpoints slightly off
                                                    if ((encoderDictionary[tbFocus.Value] - fiveDist > (boundaryDistDiff / 2)) &&
                                                        (ninetyFiveDist - encoderDictionary[tbFocusBuffer] > (boundaryDistDiff2 / 2)))
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = encoderItems.LensPlay5To95;
                                                    }
                                                    //Only one of the endpoints slightly off
                                                    else if ((encoderDictionary[tbFocus.Value] - fiveDist > (boundaryDistDiff / 2)) ||
                                                        (ninetyFiveDist - encoderDictionary[tbFocusBuffer] > (boundaryDistDiff2 / 2)))
                                                    {
                                                        if (!isBenchmarking)
                                                        {
                                                            lensPlayCompensationRatio = (1.024) * encoderItems.LensPlay5To95;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlay5To95;
                                                    }
                                                }
                                                else if ((currentEncoder >= 0 && currentEncoder < oneEigthEncoder && bufferEncoder > ninetyFiveEncoder) ||
                                                    (bufferEncoder >= 0 && bufferEncoder < oneEigthEncoder && currentEncoder > ninetyFiveEncoder))
                                                {
                                                    //5% to max
                                                    double boundaryEncoderDiff;
                                                    double actualEncoderDiff;
                                                    double lensPlayFactor;
                                                    double trackBarRatio = 16384 / encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder;

                                                    double boundaryDistDiff = Math.Abs(oneEigthDist - fiveDist);
                                                    double boundaryDistDiff2 = Math.Abs(maxDist - ninetyFiveDist);

                                                    boundaryEncoderDiff = maxEncoder - fiveEncoder;
                                                    actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                    lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                    //Both endpoints slightly off
                                                    if ((encoderDictionary[tbFocus.Value] - fiveDist > (boundaryDistDiff / 2)) &&
                                                        (maxDist - encoderDictionary[tbFocusBuffer] > (boundaryDistDiff2 / 2)))
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = encoderItems.LensPlay5To95;
                                                    }
                                                    //Only one of the endpoints slightly off
                                                    else if ((encoderDictionary[tbFocus.Value] - fiveDist > (boundaryDistDiff / 2)) ||
                                                        (maxDist - encoderDictionary[tbFocusBuffer] > (boundaryDistDiff2 / 2)))
                                                    {
                                                        if (!isBenchmarking)
                                                        {
                                                            lensPlayCompensationRatio = (1.154) * encoderItems.LensPlay5To95;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlay5To95;
                                                    }
                                                }

                                                //Traveling from or to one eighth of the lens distance
                                                else if ((currentEncoder >= oneEigthEncoder && currentEncoder < oneQuarterEncoder && bufferEncoder <= oneQuarterEncoder) ||
                                                    (bufferEncoder >= oneEigthEncoder && bufferEncoder < oneQuarterEncoder && currentEncoder <= oneQuarterEncoder))
                                                {
                                                    //one eighth to one quarter
                                                    double boundaryEncoderDiff;
                                                    double actualEncoderDiff;
                                                    double lensPlayFactor;
                                                    double trackBarRatio = 16384 / encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder;

                                                    double boundaryDistDiff = Math.Abs(oneQuarterDist - oneEigthDist);
                                                    double boundaryDistDiff2 = Math.Abs(oneQuarterDist - oneEigthDist);

                                                    boundaryEncoderDiff = oneQuarterEncoder - oneEigthEncoder;
                                                    actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                    lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                    //Both endpoints slightly off
                                                    if ((currentEncoder - oneEigthEncoder > (boundaryEncoderDiff * .2)) &&
                                                    (oneQuarterEncoder - bufferEncoder > (boundaryEncoderDiff * .2)))
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = (1 + ((currentEncoder - oneEigthEncoder) / boundaryEncoderDiff) / 8)
                                                                * encoderItems.LensPlayOneEighthToQuarter;
                                                    }
                                                    //Only one of the endpoints slightly off
                                                    else if ((currentEncoder - oneEigthEncoder > (boundaryEncoderDiff * .2)) ||
                                                        (oneQuarterEncoder - bufferEncoder > (boundaryEncoderDiff * .2)))
                                                    {
                                                        if (!isBenchmarking)
                                                        {
                                                            //If near endpoint is off
                                                            if (currentEncoder - oneEigthEncoder > (boundaryEncoderDiff * .2))
                                                            {
                                                                lensPlayCompensationRatio = (1 + ((currentEncoder - oneEigthEncoder) / boundaryEncoderDiff) / 8)
                                                                    * encoderItems.LensPlayOneEighthToQuarter;
                                                            }
                                                            //If far endpoint is off
                                                            else
                                                            {
                                                                lensPlayCompensationRatio = (1 + ((oneQuarterEncoder - bufferEncoder) / boundaryEncoderDiff) / 8)
                                                                    * encoderItems.LensPlayOneEighthToQuarter;
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlayOneEighthToQuarter;
                                                    }
                                                }
                                                else if ((currentEncoder >= oneEigthEncoder && currentEncoder < oneQuarterEncoder && bufferEncoder <= midEncoder) ||
                                                    (bufferEncoder >= oneEigthEncoder && bufferEncoder < oneQuarterEncoder && currentEncoder <= midEncoder))
                                                {
                                                    //one eighth to mid
                                                    double boundaryEncoderDiff;
                                                    double actualEncoderDiff;
                                                    double lensPlayFactor;
                                                    double trackBarRatio = 16384 / encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder;

                                                    double boundaryDistDiff = Math.Abs(oneQuarterDist - oneEigthDist);
                                                    double boundaryDistDiff2 = Math.Abs(midDist - oneQuarterDist);

                                                    boundaryEncoderDiff = midEncoder - oneEigthEncoder;
                                                    actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                    lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                    //Both endpoints slightly off
                                                    if ((encoderDictionary[tbFocus.Value] - oneEigthDist > (boundaryDistDiff / 4)) &&
                                                        (midDist - encoderDictionary[tbFocusBuffer] > (boundaryDistDiff2 / 4)))
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = encoderItems.LensPlayOneEighthToMid;
                                                    }
                                                    //Only one of the endpoints slightly off
                                                    else if ((encoderDictionary[tbFocus.Value] - oneEigthDist > (boundaryDistDiff / 4)) ||
                                                        (midDist - encoderDictionary[tbFocusBuffer] > (boundaryDistDiff2 / 4)))
                                                    {
                                                        if (!isBenchmarking)
                                                        {
                                                            lensPlayCompensationRatio = (1.004) * encoderItems.LensPlayOneEighthToMid;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlayOneEighthToMid;
                                                    }
                                                }
                                                else if ((currentEncoder >= oneEigthEncoder && currentEncoder < oneQuarterEncoder && bufferEncoder <= threeFourthsEncoder) ||
                                                    (bufferEncoder >= oneEigthEncoder && bufferEncoder < oneQuarterEncoder && currentEncoder <= threeFourthsEncoder))
                                                {
                                                    //one eighth to three fourths
                                                    double boundaryEncoderDiff;
                                                    double actualEncoderDiff;
                                                    double lensPlayFactor;
                                                    double trackBarRatio = 16384 / encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder;

                                                    double boundaryDistDiff = Math.Abs(oneQuarterDist - oneEigthDist);
                                                    double boundaryDistDiff2 = Math.Abs(threeFourthsDist - midDist);

                                                    boundaryEncoderDiff = threeFourthsEncoder - oneEigthEncoder;
                                                    actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                    lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                    //Both endpoints slightly off
                                                    if ((encoderDictionary[tbFocus.Value] - oneEigthDist > (boundaryDistDiff / 4)) &&
                                                        (threeFourthsDist - encoderDictionary[tbFocusBuffer] > (boundaryDistDiff2 / 4)))
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = encoderItems.LensPlayOneEighthToThreeFourths;
                                                    }
                                                    //Only one of the endpoints slightly off
                                                    else if ((encoderDictionary[tbFocus.Value] - oneEigthDist > (boundaryDistDiff / 4)) ||
                                                        (threeFourthsDist - encoderDictionary[tbFocusBuffer] > (boundaryDistDiff2 / 4)))
                                                    {
                                                        if (!isBenchmarking)
                                                        {
                                                            lensPlayCompensationRatio = (1.020) * encoderItems.LensPlayOneEighthToThreeFourths;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlayOneEighthToThreeFourths;
                                                    }
                                                }
                                                else if ((currentEncoder >= oneEigthEncoder && currentEncoder < oneQuarterEncoder && bufferEncoder <= sevenEighthsEncoder) ||
                                                    (bufferEncoder >= oneEigthEncoder && bufferEncoder < oneQuarterEncoder && currentEncoder <= sevenEighthsEncoder))
                                                {
                                                    //one eighth to seven eighths
                                                    double boundaryEncoderDiff;
                                                    double actualEncoderDiff;
                                                    double lensPlayFactor;
                                                    double trackBarRatio = 16384 / encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder;

                                                    double boundaryDistDiff = Math.Abs(oneQuarterDist - oneEigthDist);
                                                    double boundaryDistDiff2 = Math.Abs(sevenEighthsDist - threeFourthsDist);

                                                    boundaryEncoderDiff = sevenEighthsEncoder - oneEigthEncoder;
                                                    actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                    lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                    //Both endpoints slightly off
                                                    if ((encoderDictionary[tbFocus.Value] - oneEigthDist > (boundaryDistDiff / 4)) &&
                                                        (sevenEighthsDist - encoderDictionary[tbFocusBuffer] > (boundaryDistDiff2 / 4)))
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = encoderItems.LensPlayOneEighthToSevenEigths;
                                                    }
                                                    //Only one of the endpoints slightly off
                                                    else if ((encoderDictionary[tbFocus.Value] - oneEigthDist > (boundaryDistDiff / 4)) ||
                                                        (sevenEighthsDist - encoderDictionary[tbFocusBuffer] > (boundaryDistDiff2 / 4)))
                                                    {
                                                        if (!isBenchmarking)
                                                        {
                                                            lensPlayCompensationRatio = (1.254) * encoderItems.LensPlayOneEighthToSevenEigths;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlayOneEighthToSevenEigths;
                                                    }
                                                }
                                                else if ((currentEncoder >= oneEigthEncoder && currentEncoder < oneQuarterEncoder && bufferEncoder <= ninetyFiveEncoder) ||
                                                    (bufferEncoder >= oneEigthEncoder && bufferEncoder < oneQuarterEncoder && currentEncoder <= ninetyFiveEncoder))
                                                {
                                                    //one eighth to 95%
                                                    double boundaryEncoderDiff;
                                                    double actualEncoderDiff;
                                                    double lensPlayFactor;
                                                    double trackBarRatio = 16384 / encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder;

                                                    double boundaryDistDiff = Math.Abs(oneQuarterDist - oneEigthDist);
                                                    double boundaryDistDiff2 = Math.Abs(ninetyFiveDist - sevenEighthsDist);

                                                    boundaryEncoderDiff = ninetyFiveEncoder - oneEigthEncoder;
                                                    actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                    lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                    //Both endpoints slightly off
                                                    if ((encoderDictionary[tbFocus.Value] - oneEigthDist > (boundaryDistDiff / 4)) &&
                                                        (ninetyFiveDist - encoderDictionary[tbFocusBuffer] > (boundaryDistDiff2 / 4)))
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = encoderItems.LensPlayOneEighthTo95;
                                                    }
                                                    //Only one of the endpoints slightly off
                                                    else if ((encoderDictionary[tbFocus.Value] - oneEigthDist > (boundaryDistDiff / 4)) ||
                                                        (ninetyFiveDist - encoderDictionary[tbFocusBuffer] > (boundaryDistDiff2 / 4)))
                                                    {
                                                        if (!isBenchmarking)
                                                        {
                                                            lensPlayCompensationRatio = (1.154) * encoderItems.LensPlayOneEighthTo95;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlayOneEighthTo95;
                                                    }
                                                }
                                                else if ((currentEncoder >= oneEigthEncoder && currentEncoder < oneQuarterEncoder && bufferEncoder > ninetyFiveEncoder) ||
                                                    (bufferEncoder >= oneEigthEncoder && bufferEncoder < oneQuarterEncoder && currentEncoder > ninetyFiveEncoder))
                                                {
                                                    //one eighth to max
                                                    double boundaryEncoderDiff;
                                                    double actualEncoderDiff;
                                                    double lensPlayFactor;
                                                    double trackBarRatio = 16384 / encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder;

                                                    double boundaryDistDiff = Math.Abs(oneQuarterDist - oneEigthDist);
                                                    double boundaryDistDiff2 = Math.Abs(maxDist - ninetyFiveDist);

                                                    boundaryEncoderDiff = maxEncoder - oneEigthEncoder;
                                                    actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                    lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                    //Both endpoints slightly off
                                                    if ((encoderDictionary[tbFocus.Value] - oneEigthDist > (boundaryDistDiff / 2)) &&
                                                        (maxDist - encoderDictionary[tbFocusBuffer] > (boundaryDistDiff2 / 2)))
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = encoderItems.LensPlayOneEighthTo95;
                                                    }
                                                    //Only one of the endpoints slightly off
                                                    else if ((encoderDictionary[tbFocus.Value] - oneEigthDist > (boundaryDistDiff / 2)) ||
                                                        (maxDist - encoderDictionary[tbFocusBuffer] > (boundaryDistDiff2 / 2)))
                                                    {
                                                        if (!isBenchmarking)
                                                        {
                                                            lensPlayCompensationRatio = (1.154) * encoderItems.LensPlayOneEighthTo95;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlayOneEighthTo95;
                                                    }
                                                }

                                                //Travelling from or to one quarter of the lens distance
                                                else if ((currentEncoder >= oneQuarterEncoder && currentEncoder < midEncoder && bufferEncoder <= midEncoder) ||
                                                    (bufferEncoder >= oneQuarterEncoder && bufferEncoder < midEncoder && currentEncoder <= midEncoder))
                                                {
                                                    //one quarter to mid
                                                    double boundaryEncoderDiff;
                                                    double actualEncoderDiff;
                                                    double lensPlayFactor;
                                                    double trackBarRatio = 16384 / encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder;

                                                    double boundaryDistDiff = Math.Abs(midDist - oneQuarterDist);
                                                    double boundaryDistDiff2 = Math.Abs(midDist - oneQuarterDist);

                                                    boundaryEncoderDiff = midEncoder - oneQuarterEncoder;
                                                    actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                    lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                    //Both endpoints slightly off
                                                    if ((currentEncoder - oneQuarterEncoder > (boundaryEncoderDiff * .1)) &&
                                                        (midEncoder - bufferEncoder > (boundaryEncoderDiff * .1)))
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = (1 + ((currentEncoder - oneQuarterEncoder) / boundaryEncoderDiff) / 8)
                                                                * encoderItems.LensPlayQuarterToMid;
                                                    }
                                                    //Only one of the endpoints slightly off
                                                    else if ((currentEncoder - oneQuarterEncoder > (boundaryEncoderDiff * .1)) ||
                                                        (midEncoder - bufferEncoder > (boundaryEncoderDiff * .1)))
                                                    {
                                                        if (!isBenchmarking)
                                                        {
                                                            //If near endpoint is off
                                                            if (currentEncoder - oneQuarterEncoder > (boundaryEncoderDiff * .1))
                                                            {
                                                                lensPlayCompensationRatio = (1 + ((currentEncoder - oneQuarterEncoder) / boundaryDistDiff) / 16)
                                                                    * encoderItems.LensPlayQuarterToMid;
                                                            }
                                                            //If far endpoint is off
                                                            else
                                                            {
                                                                lensPlayCompensationRatio = (1 + ((midEncoder - bufferEncoder) / boundaryDistDiff) / 16)
                                                                    * encoderItems.LensPlayQuarterToMid;
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlayQuarterToMid;
                                                    }
                                                }
                                                else if ((currentEncoder >= oneQuarterEncoder && currentEncoder < midEncoder && bufferEncoder <= threeFourthsEncoder) ||
                                                    (bufferEncoder >= oneQuarterEncoder && bufferEncoder < midEncoder && currentEncoder <= threeFourthsEncoder))
                                                {
                                                    //one quarter to three fourths
                                                    double boundaryEncoderDiff;
                                                    double actualEncoderDiff;
                                                    double lensPlayFactor;
                                                    double trackBarRatio = 16384 / encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder;

                                                    double boundaryDistDiff = Math.Abs(midDist - oneQuarterDist);
                                                    double boundaryDistDiff2 = Math.Abs(threeFourthsDist - midDist);

                                                    boundaryEncoderDiff = threeFourthsEncoder - oneQuarterEncoder;
                                                    actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                    lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                    //Both endpoints slightly off
                                                    if ((encoderDictionary[tbFocus.Value] - oneQuarterDist > (boundaryDistDiff / 4)) &&
                                                        (threeFourthsDist - encoderDictionary[tbFocusBuffer] > (boundaryDistDiff2 / 4)))
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = encoderItems.LensPlayQuarterToThreeFourths;
                                                    }
                                                    //Only one of the endpoints slightly off
                                                    else if ((encoderDictionary[tbFocus.Value] - oneQuarterDist > (boundaryDistDiff / 4)) ||
                                                        (threeFourthsDist - encoderDictionary[tbFocusBuffer] > (boundaryDistDiff2 / 4)))
                                                    {
                                                        if (!isBenchmarking)
                                                        {
                                                            lensPlayCompensationRatio = (1.0280) * encoderItems.LensPlayQuarterToThreeFourths;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlayQuarterToThreeFourths;
                                                    }
                                                }
                                                else if ((currentEncoder >= oneQuarterEncoder && currentEncoder < midEncoder && bufferEncoder <= sevenEighthsEncoder) ||
                                                    (bufferEncoder >= oneQuarterEncoder && bufferEncoder < midEncoder && currentEncoder <= sevenEighthsEncoder))
                                                {
                                                    //one quarter to seven eighths
                                                    double boundaryEncoderDiff;
                                                    double actualEncoderDiff;
                                                    double lensPlayFactor;
                                                    double trackBarRatio = 16384 / encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder;

                                                    double boundaryDistDiff = Math.Abs(midDist - oneQuarterDist);
                                                    double boundaryDistDiff2 = Math.Abs(sevenEighthsDist - threeFourthsDist);

                                                    boundaryEncoderDiff = sevenEighthsEncoder - oneQuarterEncoder;
                                                    actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                    lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                    //Both endpoints slightly off
                                                    if ((encoderDictionary[tbFocus.Value] - oneQuarterDist > (boundaryDistDiff / 4)) &&
                                                        (sevenEighthsDist - encoderDictionary[tbFocusBuffer] > (boundaryDistDiff2 / 4)))
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = encoderItems.LensPlayQuarterToSevenEigths;
                                                    }
                                                    //Only one of the endpoints slightly off
                                                    else if ((encoderDictionary[tbFocus.Value] - oneQuarterDist > (boundaryDistDiff / 4)) ||
                                                        (sevenEighthsDist - encoderDictionary[tbFocusBuffer] > (boundaryDistDiff2 / 4)))
                                                    {
                                                        if (!isBenchmarking)
                                                        {
                                                            lensPlayCompensationRatio = (1.2540) * encoderItems.LensPlayQuarterToSevenEigths;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlayQuarterToSevenEigths;
                                                    }
                                                }
                                                else if ((currentEncoder >= oneQuarterEncoder && currentEncoder < midEncoder && bufferEncoder <= ninetyFiveEncoder) ||
                                                    (bufferEncoder >= oneQuarterEncoder && bufferEncoder < midEncoder && currentEncoder <= ninetyFiveEncoder))
                                                {
                                                    //one quarter to 95%
                                                    double boundaryEncoderDiff;
                                                    double actualEncoderDiff;
                                                    double lensPlayFactor;
                                                    double trackBarRatio = 16384 / encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder;

                                                    double boundaryDistDiff = Math.Abs(midDist - oneQuarterDist);
                                                    double boundaryDistDiff2 = Math.Abs(ninetyFiveDist - sevenEighthsDist);

                                                    boundaryEncoderDiff = ninetyFiveEncoder - oneQuarterEncoder;
                                                    actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                    lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                    //Both endpoints slightly off
                                                    if ((encoderDictionary[tbFocus.Value] - oneQuarterDist > (boundaryDistDiff / 4)) &&
                                                        (ninetyFiveDist - encoderDictionary[tbFocusBuffer] > (boundaryDistDiff2 / 4)))
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = encoderItems.LensPlayQuarterTo95;
                                                    }
                                                    //Only one of the endpoints slightly off
                                                    else if ((encoderDictionary[tbFocus.Value] - oneQuarterDist > (boundaryDistDiff / 4)) ||
                                                        (ninetyFiveDist - encoderDictionary[tbFocusBuffer] > (boundaryDistDiff2 / 4)))
                                                    {
                                                        if (!isBenchmarking)
                                                        {
                                                            lensPlayCompensationRatio = (1.2348) * encoderItems.LensPlayQuarterTo95;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlayQuarterTo95;
                                                    }
                                                }
                                                else if ((currentEncoder >= oneQuarterEncoder && currentEncoder < midEncoder && bufferEncoder > ninetyFiveEncoder) ||
                                                    (bufferEncoder >= oneQuarterEncoder && bufferEncoder < midEncoder && currentEncoder > ninetyFiveEncoder))
                                                {
                                                    //one quarter to max
                                                    double boundaryEncoderDiff;
                                                    double actualEncoderDiff;
                                                    double lensPlayFactor;
                                                    double trackBarRatio = 16384 / encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder;

                                                    double boundaryDistDiff = Math.Abs(midDist - oneQuarterDist);
                                                    double boundaryDistDiff2 = Math.Abs(maxDist - ninetyFiveDist);

                                                    boundaryEncoderDiff = maxDist - oneQuarterEncoder;
                                                    actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                    lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                    //Both endpoints slightly off
                                                    if ((encoderDictionary[tbFocus.Value] - oneQuarterDist > (boundaryDistDiff / 4)) &&
                                                        (maxDist - encoderDictionary[tbFocusBuffer] > (boundaryDistDiff2 / 4)))
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = encoderItems.LensPlayQuarterTo95;
                                                    }
                                                    //Only one of the endpoints slightly off
                                                    else if ((encoderDictionary[tbFocus.Value] - oneQuarterDist > (boundaryDistDiff / 4)) ||
                                                        (maxDist - encoderDictionary[tbFocusBuffer] > (boundaryDistDiff2 / 4)))
                                                    {
                                                        if (!isBenchmarking)
                                                        {
                                                            lensPlayCompensationRatio = (1.2948) * encoderItems.LensPlayQuarterTo95;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlayQuarterTo95;
                                                    }
                                                }

                                                //Travelling from or to mid point of lens distance
                                                else if ((currentEncoder >= midEncoder && currentEncoder < threeFourthsEncoder && bufferEncoder <= threeFourthsEncoder) ||
                                                    (bufferEncoder >= midEncoder && bufferEncoder < threeFourthsEncoder && currentEncoder <= threeFourthsEncoder))
                                                {
                                                    //mid to three fourths
                                                    double boundaryEncoderDiff;
                                                    double actualEncoderDiff;
                                                    double lensPlayFactor;
                                                    double trackBarRatio = 16384 / encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder;

                                                    double boundaryDistDiff = Math.Abs(threeFourthsDist - midDist);
                                                    double boundaryDistDiff2 = Math.Abs(threeFourthsDist - midDist);

                                                    boundaryEncoderDiff = threeFourthsEncoder - midEncoder;
                                                    actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                    lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                    //Both endpoints slightly off (by 10% of the boundary distance)
                                                    if ((currentEncoder - midEncoder > (boundaryEncoderDiff * .1)) &&
                                                        (threeFourthsEncoder - bufferEncoder > (boundaryEncoderDiff * .1)))
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlayMidToThreeFourths;
                                                    }
                                                    //Only one of the endpoints slightly off (by 10% of the boundary distance)
                                                    else if ((currentEncoder - midEncoder > (boundaryEncoderDiff * .1)) ||
                                                        (threeFourthsEncoder - bufferEncoder > (boundaryEncoderDiff * .1)))
                                                    {
                                                        if (!isBenchmarking)
                                                        {
                                                            //If near endpoint is off
                                                            if (currentEncoder - midEncoder > (boundaryEncoderDiff * .1))
                                                            {
                                                                lensPlayCompensationRatio = (.5 + (lensPlayFactor / (currentEncoder - midEncoder)))
                                                                    * encoderItems.LensPlayMidToThreeFourths;
                                                            }
                                                            //If far endpoint is off
                                                            else
                                                            {
                                                                lensPlayCompensationRatio = (.5 + (lensPlayFactor / (threeFourthsEncoder - bufferEncoder)))
                                                                    * encoderItems.LensPlayMidToThreeFourths;
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlayMidToThreeFourths;
                                                    }
                                                }
                                                else if ((currentEncoder >= midEncoder && currentEncoder < threeFourthsEncoder && bufferEncoder <= sevenEighthsEncoder) ||
                                                    (bufferEncoder >= midEncoder && bufferEncoder < threeFourthsEncoder && currentEncoder <= sevenEighthsEncoder))
                                                {
                                                    //mid to seven eighths
                                                    double boundaryEncoderDiff;
                                                    double actualEncoderDiff;
                                                    double lensPlayFactor;
                                                    double trackBarRatio = 16384 / encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder;

                                                    double boundaryDistDiff = Math.Abs(threeFourthsDist - midDist);
                                                    double boundaryDistDiff2 = Math.Abs(sevenEighthsDist - threeFourthsDist);

                                                    boundaryEncoderDiff = sevenEighthsEncoder - midEncoder;
                                                    actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                    lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                    //Both endpoints slightly off
                                                    if ((encoderDictionary[tbFocus.Value] - midDist > (boundaryDistDiff / 4)) &&
                                                        (sevenEighthsDist - encoderDictionary[tbFocusBuffer] > (boundaryDistDiff2 / 4)))
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = encoderItems.LensPlayMidToSevenEigths;
                                                    }
                                                    //Only one of the endpoints slightly off
                                                    else if ((encoderDictionary[tbFocus.Value] - midDist > (boundaryDistDiff / 4)) ||
                                                        (sevenEighthsDist - encoderDictionary[tbFocusBuffer] > (boundaryDistDiff2 / 4)))
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = (1.0280) * encoderItems.LensPlayMidToSevenEigths;
                                                    }
                                                    else
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlayMidToSevenEigths;
                                                    }
                                                }
                                                else if ((currentEncoder >= midEncoder && currentEncoder < threeFourthsEncoder && bufferEncoder <= ninetyFiveEncoder) ||
                                                    (bufferEncoder >= midEncoder && bufferEncoder < threeFourthsEncoder && currentEncoder <= ninetyFiveEncoder))
                                                {
                                                    //mid to 95%
                                                    double boundaryEncoderDiff;
                                                    double actualEncoderDiff;
                                                    double lensPlayFactor;
                                                    double trackBarRatio = 16384 / encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder;

                                                    double boundaryDistDiff = Math.Abs(threeFourthsDist - midDist);
                                                    double boundaryDistDiff2 = Math.Abs(ninetyFiveDist - sevenEighthsDist);

                                                    boundaryEncoderDiff = ninetyFiveEncoder - midEncoder;
                                                    actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                    lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                    //Both endpoints slightly off
                                                    if ((encoderDictionary[tbFocus.Value] - midDist > (boundaryDistDiff / 4)) &&
                                                        (ninetyFiveDist - encoderDictionary[tbFocusBuffer] > (boundaryDistDiff2 / 4)))
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = encoderItems.LensPlayMidTo95;
                                                    }
                                                    //Only one of the endpoints slightly off
                                                    else if ((encoderDictionary[tbFocus.Value] - midDist > (boundaryDistDiff / 4)) ||
                                                        (ninetyFiveDist - encoderDictionary[tbFocusBuffer] > (boundaryDistDiff2 / 4)))
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = (1.0680) * encoderItems.LensPlayMidTo95;
                                                    }
                                                    else
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlayMidTo95;
                                                    }
                                                }
                                                else if ((currentEncoder >= midEncoder && currentEncoder < threeFourthsEncoder && bufferEncoder > ninetyFiveEncoder) ||
                                                    (bufferEncoder >= midEncoder && bufferEncoder < threeFourthsEncoder && currentEncoder > ninetyFiveEncoder))
                                                {
                                                    //mid to max
                                                    double boundaryEncoderDiff;
                                                    double actualEncoderDiff;
                                                    double lensPlayFactor;
                                                    double trackBarRatio = 16384 / encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder;

                                                    double boundaryDistDiff = Math.Abs(threeFourthsDist - midDist);
                                                    double boundaryDistDiff2 = Math.Abs(maxDist - ninetyFiveDist);

                                                    boundaryEncoderDiff = maxEncoder - midEncoder;
                                                    actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                    lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                    //Both endpoints slightly off
                                                    if ((encoderDictionary[tbFocus.Value] - midDist > (boundaryDistDiff / 4)) &&
                                                        (maxDist - encoderDictionary[tbFocusBuffer] > (boundaryDistDiff2 / 4)))
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = encoderItems.LensPlayMidTo95;
                                                    }
                                                    //Only one of the endpoints slightly off
                                                    else if ((encoderDictionary[tbFocus.Value] - midDist > (boundaryDistDiff / 4)) ||
                                                        (maxDist - encoderDictionary[tbFocusBuffer] > (boundaryDistDiff2 / 4)))
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = (1.1260) * encoderItems.LensPlayMidTo95;
                                                    }
                                                    else
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlayMidTo95;
                                                    }
                                                }

                                                //Travelling from or to three fourths of lens distance
                                                else if ((currentEncoder >= threeFourthsEncoder && currentEncoder < sevenEighthsEncoder && bufferEncoder <= sevenEighthsEncoder) ||
                                                    (bufferEncoder >= threeFourthsEncoder && bufferEncoder < sevenEighthsEncoder && currentEncoder <= sevenEighthsEncoder))
                                                {
                                                    //three fourths to seven eights
                                                    double boundaryEncoderDiff;
                                                    double actualEncoderDiff;
                                                    double lensPlayFactor;
                                                    double trackBarRatio = 16384 / encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder;

                                                    double boundaryDistDiff = Math.Abs(sevenEighthsDist - threeFourthsDist);
                                                    double boundaryDistDiff2 = Math.Abs(sevenEighthsDist - threeFourthsDist);

                                                    boundaryEncoderDiff = sevenEighthsEncoder - threeFourthsEncoder;
                                                    actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                    lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                    //Both endpoints slightly off
                                                    if ((currentEncoder - threeFourthsEncoder > (boundaryEncoderDiff * .1)) &&
                                                        (sevenEighthsEncoder - bufferEncoder > (boundaryEncoderDiff * .1)))
                                                    {
                                                        if (!isBenchmarking)
                                                            //In the event of both endpoints being off more than 10%, we double the lensplay compensation 
                                                            //ratio based on the lens play factor and threefourths to seveneighths benchmark
                                                            lensPlayCompensationRatio = (lensPlayFactor * encoderItems.LensPlayThreeFourthsToSevenEigths) * 2;
                                                    }
                                                    //Only one of the endpoints slightly off
                                                    else if ((currentEncoder - threeFourthsEncoder > (boundaryEncoderDiff * .1)) ||
                                                        (sevenEighthsEncoder - bufferEncoder > (boundaryEncoderDiff * .1)))
                                                    {
                                                        if (!isBenchmarking)
                                                        {
                                                            //If near endpoint is off
                                                            if (currentEncoder - threeFourthsEncoder > (boundaryEncoderDiff * .1))
                                                            {
                                                                lensPlayCompensationRatio = (1 + ((currentEncoder - threeFourthsEncoder) / boundaryEncoderDiff) * 2)
                                                                        * encoderItems.LensPlayThreeFourthsToSevenEigths;
                                                            }
                                                            //If far endpoint is off
                                                            else
                                                            {
                                                                lensPlayCompensationRatio = (1 + ((sevenEighthsEncoder - bufferEncoder) / boundaryEncoderDiff) * 2)
                                                                        * encoderItems.LensPlayThreeFourthsToSevenEigths;
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlayThreeFourthsToSevenEigths;
                                                    }
                                                }
                                                else if ((currentEncoder >= threeFourthsEncoder && currentEncoder < sevenEighthsEncoder && bufferEncoder <= ninetyFiveEncoder) ||
                                                    (bufferEncoder >= threeFourthsEncoder && bufferEncoder < sevenEighthsEncoder && currentEncoder <= ninetyFiveEncoder))
                                                {
                                                    //three fourths to 95%
                                                    double boundaryEncoderDiff;
                                                    double actualEncoderDiff;
                                                    double lensPlayFactor;
                                                    double trackBarRatio = 16384 / encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder;

                                                    double boundaryDistDiff = Math.Abs(sevenEighthsDist - threeFourthsDist);
                                                    double boundaryDistDiff2 = Math.Abs(ninetyFiveDist - sevenEighthsDist);

                                                    boundaryEncoderDiff = ninetyFiveEncoder - threeFourthsEncoder;
                                                    actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                    lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                    //Both endpoints slightly off
                                                    if ((encoderDictionary[tbFocus.Value] - threeFourthsDist > (boundaryDistDiff / 4)) &&
                                                        (ninetyFiveDist - encoderDictionary[tbFocusBuffer] > (boundaryDistDiff2 / 4)))
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = encoderItems.LensPlayThreeFourthsTo95;
                                                    }
                                                    //Only one of the endpoints slightly off
                                                    else if ((encoderDictionary[tbFocus.Value] - threeFourthsDist > (boundaryDistDiff / 4)) ||
                                                        (ninetyFiveDist - encoderDictionary[tbFocusBuffer] > (boundaryDistDiff2 / 4)))
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = (1.0024) * encoderItems.LensPlayThreeFourthsTo95;
                                                    }
                                                    else
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlayThreeFourthsTo95;
                                                    }
                                                }
                                                else if ((currentEncoder >= threeFourthsEncoder && currentEncoder < sevenEighthsEncoder && bufferEncoder > ninetyFiveEncoder) ||
                                                    (bufferEncoder >= threeFourthsEncoder && bufferEncoder < sevenEighthsEncoder && currentEncoder > ninetyFiveEncoder))
                                                {
                                                    //three fourths to max
                                                    double boundaryEncoderDiff;
                                                    double actualEncoderDiff;
                                                    double lensPlayFactor;
                                                    double trackBarRatio = 16384 / encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder;

                                                    double boundaryDistDiff = Math.Abs(sevenEighthsDist - threeFourthsDist);
                                                    double boundaryDistDiff2 = Math.Abs(maxDist - ninetyFiveDist);

                                                    boundaryEncoderDiff = maxEncoder - threeFourthsEncoder;
                                                    actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                    lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                    //Both endpoints slightly off
                                                    if ((encoderDictionary[tbFocus.Value] - threeFourthsDist > (boundaryDistDiff / 4)) &&
                                                        (maxDist - encoderDictionary[tbFocusBuffer] > (boundaryDistDiff2 / 4)))
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = encoderItems.LensPlayThreeFourthsTo95;
                                                    }
                                                    //Only one of the endpoints slightly off
                                                    else if ((encoderDictionary[tbFocus.Value] - threeFourthsDist > (boundaryDistDiff / 4)) ||
                                                        (maxDist - encoderDictionary[tbFocusBuffer] > (boundaryDistDiff2 / 4)))
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = (1.1250) * encoderItems.LensPlayThreeFourthsTo95;
                                                    }
                                                    else
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlayThreeFourthsTo95;
                                                    }
                                                }

                                                //Travelling from or to seven eighths of lens distance
                                                else if ((currentEncoder >= sevenEighthsEncoder && currentEncoder < ninetyFiveEncoder && bufferEncoder <= ninetyFiveEncoder) ||
                                                    (bufferEncoder >= sevenEighthsEncoder && bufferEncoder < ninetyFiveEncoder && currentEncoder <= ninetyFiveEncoder))
                                                {
                                                    //seven eighths to 95%
                                                    double boundaryEncoderDiff;
                                                    double actualEncoderDiff;
                                                    double lensPlayFactor;
                                                    double trackBarRatio = 16384 / encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder;

                                                    double boundaryDistDiff = Math.Abs(ninetyFiveDist - sevenEighthsDist);
                                                    double boundaryDistDiff2 = Math.Abs(ninetyFiveDist - sevenEighthsDist);

                                                    boundaryEncoderDiff = ninetyFiveEncoder - sevenEighthsEncoder;
                                                    actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                    lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                    //Both endpoints slightly off
                                                    if ((currentEncoder - sevenEighthsEncoder > (boundaryEncoderDiff * .05)) &&
                                                        (ninetyFiveEncoder - bufferEncoder > (boundaryEncoderDiff * .05)))
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = (lensPlayFactor * encoderItems.LensPlaySevenEigthsTo95) * 1.5;
                                                    }
                                                    //Only one of the endpoints slightly off
                                                    else if ((currentEncoder - sevenEighthsEncoder > (boundaryEncoderDiff * .05)) ||
                                                        (ninetyFiveEncoder - bufferEncoder > (boundaryEncoderDiff * .05)))
                                                    {
                                                        if (!isBenchmarking)
                                                        {
                                                            //If near endpoint is off
                                                            if (currentEncoder - sevenEighthsEncoder > (boundaryEncoderDiff * .05))
                                                            {
                                                                lensPlayCompensationRatio = (1 + ((currentEncoder - sevenEighthsEncoder) / boundaryEncoderDiff) / 2)
                                                                    * encoderItems.LensPlaySevenEigthsTo95;
                                                            }
                                                            //If far endpoint is off
                                                            else
                                                            {
                                                                lensPlayCompensationRatio = (1 + ((ninetyFiveEncoder - bufferEncoder) / boundaryEncoderDiff) / 2)
                                                                    * encoderItems.LensPlaySevenEigthsTo95;
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlaySevenEigthsTo95;
                                                    }
                                                }
                                                else if ((currentEncoder >= sevenEighthsEncoder && currentEncoder < ninetyFiveEncoder && bufferEncoder > ninetyFiveEncoder) ||
                                                    (bufferEncoder >= sevenEighthsEncoder && bufferEncoder < ninetyFiveEncoder && currentEncoder > ninetyFiveEncoder))
                                                {
                                                    //seven eighths to max
                                                    double boundaryEncoderDiff;
                                                    double actualEncoderDiff;
                                                    double lensPlayFactor;
                                                    double trackBarRatio = 16384 / encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder;

                                                    double boundaryDistDiff = Math.Abs(ninetyFiveDist - sevenEighthsDist);
                                                    double boundaryDistDiff2 = Math.Abs(maxDist - ninetyFiveDist);

                                                    boundaryEncoderDiff = maxEncoder - sevenEighthsEncoder;
                                                    actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                    lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                    //Both endpoints slightly off
                                                    if ((encoderDictionary[tbFocus.Value] - sevenEighthsDist > (boundaryDistDiff / 4)) &&
                                                        (maxDist - encoderDictionary[tbFocusBuffer] > (boundaryDistDiff2 / 4)))
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = encoderItems.LensPlaySevenEigthsTo95;
                                                    }
                                                    //Only one of the endpoints slightly off
                                                    else if ((encoderDictionary[tbFocus.Value] - sevenEighthsDist > (boundaryDistDiff / 4)) ||
                                                        (maxDist - encoderDictionary[tbFocusBuffer] > (boundaryDistDiff2 / 4)))
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = (1.0024) * encoderItems.LensPlaySevenEigthsTo95;
                                                    }
                                                    else
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlaySevenEigthsTo95;
                                                    }
                                                }

                                                //Travelling from or to 95% of lens distance
                                                else if ((currentEncoder >= ninetyFiveEncoder && bufferEncoder > ninetyFiveEncoder) ||
                                                    (bufferEncoder >= ninetyFiveEncoder && currentEncoder > ninetyFiveEncoder))
                                                {
                                                    //95% to max
                                                    double boundaryEncoderDiff;
                                                    double actualEncoderDiff;
                                                    double lensPlayFactor;
                                                    double trackBarRatio = 16384 / encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder;

                                                    double boundaryDistDiff = Math.Abs(maxDist - ninetyFiveDist);
                                                    double boundaryDistDiff2 = Math.Abs(maxDist - ninetyFiveDist);

                                                    boundaryEncoderDiff = maxEncoder - ninetyFiveEncoder;
                                                    actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                    lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                    //Both endpoints slightly off
                                                    if ((currentEncoder - ninetyFiveEncoder > (boundaryEncoderDiff * .05)) &&
                                                        (maxEncoder - bufferEncoder > (boundaryEncoderDiff * .05)))
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = (1 + (lensPlayFactor / (currentEncoder - ninetyFiveEncoder) * 4))
                                                                * encoderItems.LensPlaySevenEigthsTo95;
                                                    }
                                                    //Only one of the endpoints slightly off
                                                    else if ((currentEncoder - ninetyFiveEncoder > (boundaryEncoderDiff * .05)) ||
                                                        (maxEncoder - bufferEncoder > (boundaryEncoderDiff * .05)))
                                                    {
                                                        if (!isBenchmarking)
                                                        {
                                                            //If near endpoint is off
                                                            if (currentEncoder - ninetyFiveEncoder > (boundaryEncoderDiff * .05))
                                                            {
                                                                lensPlayCompensationRatio = (1 + (lensPlayFactor / (currentEncoder - ninetyFiveEncoder) * 4))
                                                                    * encoderItems.LensPlaySevenEigthsTo95;
                                                            }
                                                            //If far endpoint is off
                                                            else
                                                            {
                                                                lensPlayCompensationRatio = (1 + (lensPlayFactor / (maxEncoder - bufferEncoder) * 4))
                                                                    * encoderItems.LensPlaySevenEigthsTo95;
                                                            }
                                                        }

                                                        //lensPlayCompensationRatio = (1.0280) * encoderItems.LensPlaySevenEigthsTo95;
                                                    }
                                                    else
                                                    {
                                                        if (!isBenchmarking)
                                                            lensPlayCompensationRatio = (1 + (lensPlayFactor / (maxEncoder - bufferEncoder) * 4))
                                                                    * encoderItems.LensPlaySevenEigthsTo95;
                                                    }
                                                }

                                                if (lensPlayCompensationRatio == 0)
                                                    lensPlayCompensationRatio = encoderItems.LensPlayCompensationRatio;

                                                encoderDelta = (double)trackbarDictionary[tbFocusBuffer] - trackbarDictionary[tbFocus.Value] + encoderRemainderNear;
                                                encoderDelta = encoderDelta + (encoderDelta * lensPlayCompensationRatio);
                                            }
                                            else
                                                encoderDelta = (double)trackbarDictionary[tbFocusBuffer] - trackbarDictionary[tbFocus.Value] + encoderRemainderNear;

                                            loggerStrBuilder.AppendLine();
                                            loggerStrBuilder.AppendLine();
                                            loggerStrBuilder.AppendLine("tbFocusBuffer: " + tbFocusBuffer);
                                            loggerStrBuilder.AppendLine("tbFocus.Value: " + tbFocus.Value);
                                            loggerStrBuilder.AppendLine("encoderDelta: " + encoderDelta);
                                            loggerStrBuilder.AppendLine("delayFactor: " + delayFactor);
                                            loggerStrBuilder.AppendLine("encoderRemainderNear: " + encoderRemainderNear);
                                            loggerStrBuilder.AppendLine("encoderRemainderFar: " + encoderRemainderFar);

                                            tbFocusBuffer = tbFocus.Value;

                                            if (encoderDelta < encoderItems.EncoderItems[0].MediumRatio)
                                            {
                                                delayFactor = 1;
                                            }
                                            else if (encoderDelta < encoderItems.EncoderItems[0].LargeRatio)
                                            {
                                                delayFactor = 3;
                                            }
                                            else
                                            {
                                                delayFactor = 5;
                                            }

                                            while (encoderDelta >= encoderItems.EncoderItems[0].LargeRatio)
                                            {
                                                commandQueue.Enqueue(-encoderItems.EncoderItems[0].LargeRatio);
                                                stepCounter = stepCounter - encoderItems.EncoderItems[0].LargeRatio;

                                                if (stepCounter < 0)
                                                    stepCounter = 0;

                                                encoderDelta = (double)encoderDelta - encoderItems.EncoderItems[0].LargeRatio;
                                            }

                                            while (encoderDelta >= encoderItems.EncoderItems[0].MediumRatio)
                                            {
                                                commandQueue.Enqueue(-encoderItems.EncoderItems[0].MediumRatio);
                                                stepCounter = stepCounter - encoderItems.EncoderItems[0].MediumRatio;

                                                if (stepCounter < 0)
                                                    stepCounter = 0;

                                                encoderDelta = (double)encoderDelta - encoderItems.EncoderItems[0].MediumRatio;
                                            }

                                            while (encoderDelta >= 1)
                                            {
                                                commandQueue.Enqueue(-1.0);
                                                stepCounter = stepCounter - 1.0;

                                                if (stepCounter < 0)
                                                    stepCounter = 0;

                                                encoderDelta = (double)encoderDelta - 1;
                                            }

                                            if (encoderDelta < 1)
                                            {
                                                encoderRemainderNear = encoderDelta;
                                            }

                                            focusBarWatch.Reset();
                                            focusBarWatch.Start();
                                        }

                                        try
                                        {
                                            if (encoderDictionary[tbFocus.Value] > hyperFocal)
                                            {
                                                lblTrackbarValue.Text = "\u221E";
                                            }
                                            else
                                            {
                                                lblTrackbarValue.Text = String.Format("{0:0.00}", encoderDictionary[tbFocus.Value]) + "ft";
                                            }
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                    }
                                    else
                                    {
                                        logger.LogError(loggerStrBuilder.ToString(), EventLogEntryType.Information);
                                        loggerStrBuilder.Clear();
                                    }
                                }
                            }
                        });
                    }
                    else
                    {
                        if (cameraAPI.Cameras != null)
                        {
                            if (cameraAPI.Cameras.Count > 0)
                            {
                                if (encoderItems.EncoderItems.Count > 0 && tbFocus.Value != tbFocusBuffer)
                                {
                                    if (tbFocus.Value > tbFocusBuffer)
                                    {
                                        if (rackingNear == true && rackingFar == false)
                                        {
                                            directionChanged = true;
                                            rackingFarCount++;
                                        }
                                        else
                                            directionChanged = false;

                                        rackingNear = false;
                                        rackingFar = true;

                                        if (tbFocusBuffer == 0)
                                        {
                                            commandQueue.Enqueue(-encoderItems.EncoderItems[0].LargeRatio);
                                        }

                                        if (tbFocus.Value == tbFocus.Maximum)
                                        {
                                            encoderRemainderNear = 0;
                                            encoderRemainderFar = 0;
                                        }

                                        double encoderDelta;

                                        if (directionChanged && tbFocusBuffer != 0 && lensPlayCompensationEnabled)
                                        {
                                            encoderDelta = (double)trackbarDictionary[tbFocus.Value] - trackbarDictionary[tbFocusBuffer] + encoderRemainderFar;
                                            //encoderDelta = encoderDelta + (encoderDelta * encoderItems.LensPlayCompensationRatio);
                                        }
                                        else
                                            encoderDelta = (double)trackbarDictionary[tbFocus.Value] - trackbarDictionary[tbFocusBuffer] + encoderRemainderFar;

                                        loggerStrBuilder.AppendLine();
                                        loggerStrBuilder.AppendLine();
                                        loggerStrBuilder.AppendLine("tbFocusBuffer: " + tbFocusBuffer);
                                        loggerStrBuilder.AppendLine("encoderDelta: " + encoderDelta);
                                        loggerStrBuilder.AppendLine("delayFactor: " + delayFactor);
                                        loggerStrBuilder.AppendLine("encoderRemainderNear: " + encoderRemainderNear);
                                        loggerStrBuilder.AppendLine("encoderRemainderFar: " + encoderRemainderFar);

                                        tbFocusBuffer = tbFocus.Value;

                                        if (encoderDelta < encoderItems.EncoderItems[0].MediumRatio)
                                        {
                                            delayFactor = 1;
                                        }
                                        else if (encoderDelta < encoderItems.EncoderItems[0].LargeRatio)
                                        {
                                            delayFactor = 3;
                                        }
                                        else
                                        {
                                            delayFactor = 5;
                                        }

                                        if (rackingFocus)
                                        {
                                            loggerStrBuilder.AppendLine("tbFocus.Value: " + tbFocus.Value);
                                            loggerStrBuilder.AppendLine("tbFocusBuffer: " + tbFocusBuffer);
                                            loggerStrBuilder.AppendLine("encoderDelta: " + encoderDelta);
                                            loggerStrBuilder.AppendLine("delayFactor: " + delayFactor);
                                            loggerStrBuilder.AppendLine("encoderRemainderNear: " + encoderRemainderNear);
                                            loggerStrBuilder.AppendLine("encoderRemainderFar: " + encoderRemainderFar);
                                        }

                                        while (encoderDelta >= encoderItems.EncoderItems[0].LargeRatio)
                                        {
                                            commandQueue.Enqueue(encoderItems.EncoderItems[0].LargeRatio);
                                            stepCounter = stepCounter + encoderItems.EncoderItems[0].LargeRatio;
                                            encoderDelta = (double)encoderDelta - encoderItems.EncoderItems[0].LargeRatio;
                                        }

                                        while (encoderDelta >= encoderItems.EncoderItems[0].MediumRatio)
                                        {
                                            commandQueue.Enqueue(encoderItems.EncoderItems[0].MediumRatio);
                                            stepCounter = stepCounter + encoderItems.EncoderItems[0].MediumRatio;
                                            encoderDelta = (double)encoderDelta - encoderItems.EncoderItems[0].MediumRatio;
                                        }

                                        while (encoderDelta >= 1)
                                        {
                                            commandQueue.Enqueue(1.0);
                                            stepCounter = stepCounter + 1.0;
                                            encoderDelta = (double)encoderDelta - 1;
                                        }

                                        if (encoderDelta < 1)
                                        {
                                            encoderRemainderFar = encoderDelta;
                                        }

                                        focusBarWatch.Reset();
                                        focusBarWatch.Start();
                                    }
                                    else
                                    {
                                        if (rackingNear == false && rackingFar == true)
                                        {
                                            directionChanged = true;
                                            rackingNearCount++;
                                        }
                                        else
                                            directionChanged = false;

                                        rackingNear = true;
                                        rackingFar = false;

                                        if (tbFocusBuffer == tbFocus.Maximum)
                                        {
                                            encoderRemainderNear = 0;
                                            encoderRemainderFar = 0;
                                        }

                                        double encoderDelta;

                                        if (directionChanged && tbFocusBuffer != tbFocus.Maximum && rackingNearCount % 2 == 0 && lensPlayCompensationEnabled && encoderItems.LensPlayCompensationRatio.ToString() != "Infinity")
                                        {
                                            double currentEncoder = (double)trackbarDictionary[tbFocus.Value];
                                            double bufferEncoder = (double)trackbarDictionary[tbFocusBuffer];
                                            double lensPlayCompensationRatio = 0;

                                            double fiveEncoder = encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder * ((double).05);
                                            double oneEigthEncoder = encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder * ((double)1 / 8);
                                            double oneQuarterEncoder = encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder * ((double)1 / 4);
                                            double midEncoder = encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder * ((double)1 / 2);
                                            double threeFourthsEncoder = encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder * ((double)3 / 4);
                                            double sevenEighthsEncoder = encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder * ((double)7 / 8);
                                            double ninetyFiveEncoder = encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder * ((double).95);
                                            double maxEncoder = encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder;

                                            //Traveling from or to 5% of the lens distance
                                            if ((currentEncoder >= 0 && currentEncoder < oneEigthEncoder && bufferEncoder <= oneEigthEncoder) ||
                                                (bufferEncoder >= 0 && bufferEncoder < oneEigthEncoder && currentEncoder <= oneEigthEncoder))
                                            {
                                                double boundaryEncoderDiff;
                                                double actualEncoderDiff;
                                                double lensPlayFactor;

                                                boundaryEncoderDiff = oneEigthEncoder - fiveEncoder;
                                                actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlay5ToOneEighth;
                                            }
                                            else if ((currentEncoder >= 0 && currentEncoder < oneEigthEncoder && bufferEncoder <= oneQuarterEncoder) ||
                                                (bufferEncoder >= 0 && bufferEncoder < oneEigthEncoder && currentEncoder <= oneQuarterEncoder))
                                            {
                                                double boundaryEncoderDiff;
                                                double actualEncoderDiff;
                                                double lensPlayFactor;

                                                boundaryEncoderDiff = oneQuarterEncoder - fiveEncoder;
                                                actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlay5ToQuarter;
                                            }
                                            else if ((currentEncoder >= 0 && currentEncoder < oneEigthEncoder && bufferEncoder <= midEncoder) ||
                                                (bufferEncoder >= 0 && bufferEncoder < oneEigthEncoder && currentEncoder <= midEncoder))
                                            {
                                                double boundaryEncoderDiff;
                                                double actualEncoderDiff;
                                                double lensPlayFactor;

                                                boundaryEncoderDiff = midEncoder - fiveEncoder;
                                                actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlay5ToMid;
                                            }
                                            else if ((currentEncoder >= 0 && currentEncoder < oneEigthEncoder && bufferEncoder <= threeFourthsEncoder) ||
                                                (bufferEncoder >= 0 && bufferEncoder < oneEigthEncoder && currentEncoder <= threeFourthsEncoder))
                                            {
                                                double boundaryEncoderDiff;
                                                double actualEncoderDiff;
                                                double lensPlayFactor;

                                                boundaryEncoderDiff = threeFourthsEncoder - fiveEncoder;
                                                actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlay5ToThreeFourths;
                                            }
                                            else if ((currentEncoder >= 0 && currentEncoder < oneEigthEncoder && bufferEncoder <= sevenEighthsEncoder) ||
                                                (bufferEncoder >= 0 && bufferEncoder < oneEigthEncoder && currentEncoder <= sevenEighthsEncoder))
                                            {
                                                double boundaryEncoderDiff;
                                                double actualEncoderDiff;
                                                double lensPlayFactor;

                                                boundaryEncoderDiff = sevenEighthsEncoder - fiveEncoder;
                                                actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlay5ToSevenEigths;
                                            }
                                            else if ((currentEncoder >= 0 && currentEncoder < oneEigthEncoder && bufferEncoder <= ninetyFiveEncoder) ||
                                                (bufferEncoder >= 0 && bufferEncoder < oneEigthEncoder && currentEncoder <= ninetyFiveEncoder))
                                            {
                                                double boundaryEncoderDiff;
                                                double actualEncoderDiff;
                                                double lensPlayFactor;

                                                boundaryEncoderDiff = ninetyFiveEncoder - fiveEncoder;
                                                actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlay5To95;
                                            }
                                            else if ((currentEncoder >= 0 && currentEncoder < oneEigthEncoder && bufferEncoder > ninetyFiveEncoder) ||
                                                (bufferEncoder >= 0 && bufferEncoder < oneEigthEncoder && currentEncoder > ninetyFiveEncoder))
                                            {
                                                lensPlayCompensationRatio = encoderItems.LensPlay5To95;
                                            }

                                            //Traveling from or to one eighth of the lens distance
                                            else if ((currentEncoder >= oneEigthEncoder && currentEncoder < oneQuarterEncoder && bufferEncoder <= oneQuarterEncoder) ||
                                                (bufferEncoder >= oneEigthEncoder && bufferEncoder < oneQuarterEncoder && currentEncoder <= oneQuarterEncoder))
                                            {
                                                double boundaryEncoderDiff;
                                                double actualEncoderDiff;
                                                double lensPlayFactor;

                                                boundaryEncoderDiff = oneQuarterEncoder - oneEigthEncoder;
                                                actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlayOneEighthToQuarter;
                                            }
                                            else if ((currentEncoder >= oneEigthEncoder && currentEncoder < oneQuarterEncoder && bufferEncoder <= midEncoder) ||
                                                (bufferEncoder >= oneEigthEncoder && bufferEncoder < oneQuarterEncoder && currentEncoder <= midEncoder))
                                            {
                                                double boundaryEncoderDiff;
                                                double actualEncoderDiff;
                                                double lensPlayFactor;

                                                boundaryEncoderDiff = midEncoder - oneEigthEncoder;
                                                actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlayOneEighthToMid;
                                            }
                                            else if ((currentEncoder >= oneEigthEncoder && currentEncoder < oneQuarterEncoder && bufferEncoder <= threeFourthsEncoder) ||
                                                (bufferEncoder >= oneEigthEncoder && bufferEncoder < oneQuarterEncoder && currentEncoder <= threeFourthsEncoder))
                                            {
                                                double boundaryEncoderDiff;
                                                double actualEncoderDiff;
                                                double lensPlayFactor;

                                                boundaryEncoderDiff = threeFourthsEncoder - oneEigthEncoder;
                                                actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlayOneEighthToThreeFourths;
                                            }
                                            else if ((currentEncoder >= oneEigthEncoder && currentEncoder < oneQuarterEncoder && bufferEncoder <= sevenEighthsEncoder) ||
                                                (bufferEncoder >= oneEigthEncoder && bufferEncoder < oneQuarterEncoder && currentEncoder <= sevenEighthsEncoder))
                                            {
                                                double boundaryEncoderDiff;
                                                double actualEncoderDiff;
                                                double lensPlayFactor;

                                                boundaryEncoderDiff = sevenEighthsEncoder - oneEigthEncoder;
                                                actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlayOneEighthToSevenEigths;
                                            }
                                            else if ((currentEncoder >= oneEigthEncoder && currentEncoder < oneQuarterEncoder && bufferEncoder <= ninetyFiveEncoder) ||
                                                (bufferEncoder >= oneEigthEncoder && bufferEncoder < oneQuarterEncoder && currentEncoder <= ninetyFiveEncoder))
                                            {
                                                double boundaryEncoderDiff;
                                                double actualEncoderDiff;
                                                double lensPlayFactor;

                                                boundaryEncoderDiff = ninetyFiveEncoder - oneEigthEncoder;
                                                actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlayOneEighthTo95;
                                            }
                                            else if ((currentEncoder >= oneEigthEncoder && currentEncoder < oneQuarterEncoder && bufferEncoder > ninetyFiveEncoder) ||
                                                (bufferEncoder >= oneEigthEncoder && bufferEncoder < oneQuarterEncoder && currentEncoder > ninetyFiveEncoder))
                                            {
                                                lensPlayCompensationRatio = encoderItems.LensPlayOneEighthTo95;
                                            }

                                            //Travelling from or to one quarter of the lens distance
                                            else if ((currentEncoder >= oneQuarterEncoder && currentEncoder < midEncoder && bufferEncoder <= midEncoder) ||
                                                (bufferEncoder >= oneQuarterEncoder && bufferEncoder < midEncoder && currentEncoder <= midEncoder))
                                            {
                                                double boundaryEncoderDiff;
                                                double actualEncoderDiff;
                                                double lensPlayFactor;

                                                boundaryEncoderDiff = midEncoder - oneQuarterEncoder;
                                                actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlayQuarterToMid;
                                            }
                                            else if ((currentEncoder >= oneQuarterEncoder && currentEncoder < midEncoder && bufferEncoder <= threeFourthsEncoder) ||
                                                (bufferEncoder >= oneQuarterEncoder && bufferEncoder < midEncoder && currentEncoder <= threeFourthsEncoder))
                                            {
                                                double boundaryEncoderDiff;
                                                double actualEncoderDiff;
                                                double lensPlayFactor;

                                                boundaryEncoderDiff = threeFourthsEncoder - oneQuarterEncoder;
                                                actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlayQuarterToThreeFourths;
                                            }
                                            else if ((currentEncoder >= oneQuarterEncoder && currentEncoder < midEncoder && bufferEncoder <= sevenEighthsEncoder) ||
                                                (bufferEncoder >= oneQuarterEncoder && bufferEncoder < midEncoder && currentEncoder <= sevenEighthsEncoder))
                                            {
                                                double boundaryEncoderDiff;
                                                double actualEncoderDiff;
                                                double lensPlayFactor;

                                                boundaryEncoderDiff = sevenEighthsEncoder - oneQuarterEncoder;
                                                actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlayQuarterToSevenEigths;
                                            }
                                            else if ((currentEncoder >= oneQuarterEncoder && currentEncoder < midEncoder && bufferEncoder <= ninetyFiveEncoder) ||
                                                (bufferEncoder >= oneQuarterEncoder && bufferEncoder < midEncoder && currentEncoder <= ninetyFiveEncoder))
                                            {
                                                double boundaryEncoderDiff;
                                                double actualEncoderDiff;
                                                double lensPlayFactor;

                                                boundaryEncoderDiff = ninetyFiveEncoder - oneQuarterEncoder;
                                                actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlayQuarterTo95;
                                            }
                                            else if ((currentEncoder >= oneQuarterEncoder && currentEncoder < midEncoder && bufferEncoder > ninetyFiveEncoder) ||
                                                (bufferEncoder >= oneQuarterEncoder && bufferEncoder < midEncoder && currentEncoder > ninetyFiveEncoder))
                                            {
                                                lensPlayCompensationRatio = encoderItems.LensPlayQuarterTo95;
                                            }

                                            //Travelling from or to mid point of lens distance
                                            else if ((currentEncoder >= midEncoder && currentEncoder < threeFourthsEncoder && bufferEncoder <= threeFourthsEncoder) ||
                                                (bufferEncoder >= midEncoder && bufferEncoder < threeFourthsEncoder && currentEncoder <= threeFourthsEncoder))
                                            {
                                                double boundaryEncoderDiff;
                                                double actualEncoderDiff;
                                                double lensPlayFactor;

                                                boundaryEncoderDiff = threeFourthsEncoder - midEncoder;
                                                actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlayMidToThreeFourths;
                                            }
                                            else if ((currentEncoder >= midEncoder && currentEncoder < threeFourthsEncoder && bufferEncoder <= sevenEighthsEncoder) ||
                                                (bufferEncoder >= midEncoder && bufferEncoder < threeFourthsEncoder && currentEncoder <= sevenEighthsEncoder))
                                            {
                                                double boundaryEncoderDiff;
                                                double actualEncoderDiff;
                                                double lensPlayFactor;

                                                boundaryEncoderDiff = sevenEighthsEncoder - midEncoder;
                                                actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlayMidToSevenEigths;
                                            }
                                            else if ((currentEncoder >= midEncoder && currentEncoder < threeFourthsEncoder && bufferEncoder <= ninetyFiveEncoder) ||
                                                (bufferEncoder >= midEncoder && bufferEncoder < threeFourthsEncoder && currentEncoder <= ninetyFiveEncoder))
                                            {
                                                double boundaryEncoderDiff;
                                                double actualEncoderDiff;
                                                double lensPlayFactor;

                                                boundaryEncoderDiff = ninetyFiveEncoder - midEncoder;
                                                actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlayMidTo95;
                                            }
                                            else if ((currentEncoder >= midEncoder && currentEncoder < threeFourthsEncoder && bufferEncoder > ninetyFiveEncoder) ||
                                                (bufferEncoder >= midEncoder && bufferEncoder < threeFourthsEncoder && currentEncoder > ninetyFiveEncoder))
                                            {
                                                lensPlayCompensationRatio = encoderItems.LensPlayMidTo95;
                                            }

                                            //Travelling from or to three fourths of lens distance
                                            else if ((currentEncoder >= threeFourthsEncoder && currentEncoder < sevenEighthsEncoder && bufferEncoder <= sevenEighthsEncoder) ||
                                                (bufferEncoder >= threeFourthsEncoder && bufferEncoder < sevenEighthsEncoder && currentEncoder <= sevenEighthsEncoder))
                                            {
                                                double boundaryEncoderDiff;
                                                double actualEncoderDiff;
                                                double lensPlayFactor;

                                                boundaryEncoderDiff = sevenEighthsEncoder - threeFourthsEncoder;
                                                actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlayThreeFourthsToSevenEigths;
                                            }
                                            else if ((currentEncoder >= threeFourthsEncoder && currentEncoder < sevenEighthsEncoder && bufferEncoder <= ninetyFiveEncoder) ||
                                                (bufferEncoder >= threeFourthsEncoder && bufferEncoder < sevenEighthsEncoder && currentEncoder <= ninetyFiveEncoder))
                                            {
                                                double boundaryEncoderDiff;
                                                double actualEncoderDiff;
                                                double lensPlayFactor;

                                                boundaryEncoderDiff = ninetyFiveEncoder - threeFourthsEncoder;
                                                actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlayThreeFourthsTo95;
                                            }
                                            else if ((currentEncoder >= threeFourthsEncoder && currentEncoder < sevenEighthsEncoder && bufferEncoder > ninetyFiveEncoder) ||
                                                (bufferEncoder >= threeFourthsEncoder && bufferEncoder < sevenEighthsEncoder && currentEncoder > ninetyFiveEncoder))
                                            {
                                                lensPlayCompensationRatio = encoderItems.LensPlayThreeFourthsTo95;
                                            }

                                            //Travelling from or to seven eighths of lens distance
                                            else if ((currentEncoder >= sevenEighthsEncoder && currentEncoder < ninetyFiveEncoder && bufferEncoder <= ninetyFiveEncoder) ||
                                                (bufferEncoder >= sevenEighthsEncoder && bufferEncoder < ninetyFiveEncoder && currentEncoder <= ninetyFiveEncoder))
                                            {
                                                double boundaryEncoderDiff;
                                                double actualEncoderDiff;
                                                double lensPlayFactor;

                                                boundaryEncoderDiff = ninetyFiveEncoder - sevenEighthsEncoder;
                                                actualEncoderDiff = Math.Abs(bufferEncoder - currentEncoder);
                                                lensPlayFactor = actualEncoderDiff / boundaryEncoderDiff;

                                                lensPlayCompensationRatio = lensPlayFactor * encoderItems.LensPlaySevenEigthsTo95;
                                            }
                                            else if ((currentEncoder >= sevenEighthsEncoder && currentEncoder < ninetyFiveEncoder && bufferEncoder > ninetyFiveEncoder) ||
                                                (bufferEncoder >= sevenEighthsEncoder && bufferEncoder < ninetyFiveEncoder && currentEncoder > ninetyFiveEncoder))
                                            {
                                                lensPlayCompensationRatio = encoderItems.LensPlaySevenEigthsTo95;
                                            }

                                            //Travelling from or to 95% of lens distance
                                            else if ((currentEncoder >= ninetyFiveEncoder && bufferEncoder > ninetyFiveEncoder) ||
                                                (bufferEncoder >= ninetyFiveEncoder && currentEncoder > ninetyFiveEncoder))
                                            {
                                                lensPlayCompensationRatio = encoderItems.LensPlaySevenEigthsTo95;
                                            }

                                            if (lensPlayCompensationRatio == 0)
                                                lensPlayCompensationRatio = encoderItems.LensPlayCompensationRatio;

                                            encoderDelta = (double)trackbarDictionary[tbFocusBuffer] - trackbarDictionary[tbFocus.Value] + encoderRemainderNear;
                                            encoderDelta = encoderDelta + (encoderDelta * lensPlayCompensationRatio);
                                        }
                                        else
                                            encoderDelta = (double)trackbarDictionary[tbFocusBuffer] - trackbarDictionary[tbFocus.Value] + encoderRemainderNear;

                                        loggerStrBuilder.AppendLine();
                                        loggerStrBuilder.AppendLine();
                                        loggerStrBuilder.AppendLine("tbFocusBuffer: " + tbFocusBuffer);
                                        loggerStrBuilder.AppendLine("tbFocus.Value: " + tbFocus.Value);
                                        loggerStrBuilder.AppendLine("encoderDelta: " + encoderDelta);
                                        loggerStrBuilder.AppendLine("delayFactor: " + delayFactor);
                                        loggerStrBuilder.AppendLine("encoderRemainderNear: " + encoderRemainderNear);
                                        loggerStrBuilder.AppendLine("encoderRemainderFar: " + encoderRemainderFar);

                                        tbFocusBuffer = tbFocus.Value;

                                        if (encoderDelta < encoderItems.EncoderItems[0].MediumRatio)
                                        {
                                            delayFactor = 1;
                                        }
                                        else if (encoderDelta < encoderItems.EncoderItems[0].LargeRatio)
                                        {
                                            delayFactor = 3;
                                        }
                                        else
                                        {
                                            delayFactor = 5;
                                        }

                                        while (encoderDelta >= encoderItems.EncoderItems[0].LargeRatio)
                                        {
                                            commandQueue.Enqueue(-encoderItems.EncoderItems[0].LargeRatio);
                                            stepCounter = stepCounter - encoderItems.EncoderItems[0].LargeRatio;

                                            if (stepCounter < 0)
                                                stepCounter = 0;

                                            encoderDelta = (double)encoderDelta - encoderItems.EncoderItems[0].LargeRatio;
                                        }

                                        while (encoderDelta >= encoderItems.EncoderItems[0].MediumRatio)
                                        {
                                            commandQueue.Enqueue(-encoderItems.EncoderItems[0].MediumRatio);
                                            stepCounter = stepCounter - encoderItems.EncoderItems[0].MediumRatio;

                                            if (stepCounter < 0)
                                                stepCounter = 0;

                                            encoderDelta = (double)encoderDelta - encoderItems.EncoderItems[0].MediumRatio;
                                        }

                                        while (encoderDelta >= 1)
                                        {
                                            commandQueue.Enqueue(-1.0);
                                            stepCounter = stepCounter - 1.0;

                                            if (stepCounter < 0)
                                                stepCounter = 0;

                                            encoderDelta = (double)encoderDelta - 1;
                                        }

                                        if (encoderDelta < 1)
                                        {
                                            encoderRemainderNear = encoderDelta;
                                        }

                                        focusBarWatch.Reset();
                                        focusBarWatch.Start();
                                    }

                                    try
                                    {
                                        if (encoderDictionary[tbFocus.Value] > hyperFocal)
                                        {
                                            lblTrackbarValue.Text = "\u221E";
                                        }
                                        else
                                        {
                                            lblTrackbarValue.Text = String.Format("{0:0.00}", encoderDictionary[tbFocus.Value]) + "ft";
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }
                                else
                                {
                                    //logger.LogError(loggerStrBuilder.ToString(), EventLogEntryType.Information);
                                    loggerStrBuilder.Clear();
                                }
                            }
                        }
                    }
                }
            }
        }

        public void tbFocus_KeyDown(object sender, KeyEventArgs e)
        {
            //Set flag to pause the lens play manager and empty the command queue when mouse click on track bar is detected
            pauseLensPlay = true;

            if (tbFocus.Value == tbFocus.Minimum)
            {
                commandQueue.Clear();

                //After queue is emptied, drive lens as far back as possible
                Thread.Sleep(lrgDelay);
                commandQueue.Enqueue(-encoderItems.EncoderItems[0].LargeRatio);
                stepCounter = 0;
                Thread.Sleep(lrgDelay);
            }
            else if (tbFocus.Value == tbFocus.Maximum)
            {
                commandQueue.Clear();
            }

            pauseLensPlayWatch.Start();

        }

        public void tbFocus_MouseDown(object sender, MouseEventArgs e)
        {
            //Set flag to pause the lens play manager and empty the command queue when mouse click on track bar is detected
            pauseLensPlay = true;

            if (tbFocus.Value == tbFocus.Minimum)
            {
                commandQueue.Clear();

                //After queue is emptied, drive lens as far back as possible
                Thread.Sleep(lrgDelay);
                commandQueue.Enqueue(-encoderItems.EncoderItems[0].LargeRatio);
                stepCounter = 0;

                Thread.Sleep(lrgDelay);
            }
            else if (tbFocus.Value == tbFocus.Maximum)
            {
                commandQueue.Clear();
            }

            pauseLensPlayWatch.Start();
        }

        private void tbFocus_ValueChanged(object sender, EventArgs e)
        {
            //If DSLR detected, start background worker that updates the step counter text boxes
            if (cameraAPI.Cameras != null)
            {
                if (cameraAPI.Cameras.Count > 0)
                {
                    if (!bgWorkerStepCount.IsBusy)
                    {
                        bgWorkerStepCount.RunWorkerAsync();
                    }

                    if (commandQueue.Count > 0)
                    {
                        if (!rackingFocus && !rackingFocusBatch)
                        {
                            if (tbFocusBuffer != 0 && tbFocusBuffer != tbFocus.Maximum)
                            {
                                tbFocus.Value = tbFocusBuffer;
                            }
                        }
                    }
                }
            }
            else
            {
                //If no DSLR detected, convert trackbar value to hex, perform xOr and write to serial port
                string hex = tbFocus.Value.ToString("X");
                if (hex.Length < 4)
                {
                    do
                    {
                        hex = "0" + hex;
                    } while (hex.Length < 4);
                }

                string xOr = Convert.ToString(int.Parse(hex[0].ToString(), NumberStyles.AllowHexSpecifier) ^ int.Parse(hex[1].ToString(), NumberStyles.AllowHexSpecifier) ^ int.Parse(hex[2].ToString(), NumberStyles.AllowHexSpecifier) ^ int.Parse(hex[3].ToString(), NumberStyles.AllowHexSpecifier));
                xOr = (int.Parse(xOr)).ToString("X");

                birgerSerial.Write(("eh" + hex + "," + xOr).ToLower());
                if (encoderItems.EncoderItems.Count > 0)
                {
                    try
                    {
                        //If the trackbar's distance is greater than the hyperfocal distance, update distance text with infinity symbol, otherwise update distance with dictionary value
                        if (encoderDictionary[tbFocus.Value] > hyperFocal)
                        {
                            lblTrackbarValue.Text = "\u221E";
                        }
                        else
                        {
                            lblTrackbarValue.Text = String.Format("{0:0.00}", encoderDictionary[tbFocus.Value]) + "ft";
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
        }

        private void CloseSerial()
        {
            //Close birger serial port
            birgerSerial.Close();
        }

        private void Connect()
        {
            //Open birger serial port and start background worker that checks if the serial port is open
            birgerSerial.Open();
            if (!bwIsSerialOpen.IsBusy)
                bwIsSerialOpen.RunWorkerAsync();

            //If serial port is open, write initialization strings to it
            if (birgerSerial.IsOpen())
            {
                birgerSerial.Write("routeesc,0");   //Escape from port routing.
                Thread.Sleep(50);
                birgerSerial.Write("rm0,1");        //Set response mode terse with new protocol. 
                Thread.Sleep(50);
                birgerSerial.Write("vs");           //Print the short version string.
                Thread.Sleep(50);
                birgerSerial.Write("lc");           //Print cached lens status
                Thread.Sleep(50);
                birgerSerial.Write("rm0,1");        //Set response mode terse with new protocol. 
                Thread.Sleep(50);
                birgerSerial.Write("sm0");          //Set Special Modes to 0 (reserved)
                Thread.Sleep(50);
                birgerSerial.Write("cl41");         //?
                Thread.Sleep(50);
                birgerSerial.Write("sm0");          //Set Special Modes to 0 (reserved)
                Thread.Sleep(50);
                birgerSerial.Write("sr0");          //Set spontaneous responses off
                Thread.Sleep(50);
                birgerSerial.Write("lp");           //Lens presence?
                Thread.Sleep(50);
                birgerSerial.Write("dz");           //Print the zoom range
                Thread.Sleep(50);
                birgerSerial.Write("ls");           //Query lens for status immediately and print.
                Thread.Sleep(50);
                birgerSerial.Write("fp");           //Print the raw focus positions
                Thread.Sleep(50);
                birgerSerial.Write("mm0");          //?
                Thread.Sleep(50);
                birgerSerial.Write("se9,2");        //Temporarily set non-volatile (EEPROM) byte 9 to value 2
                Thread.Sleep(50);
                birgerSerial.Write("sm29");         //Set special modes. Switch 'ma' command to use the servo aperture routine. (Aperture to position 9)
                Thread.Sleep(50);
                birgerSerial.Write("sr1");          //Set spontaneous responses on.
                Thread.Sleep(50);
                birgerSerial.Write("gs");           //Echo current device and lens statuses.

                //Wait a bit before attempting to get aperture info so we dont step on other potential aperture operations
                Thread.Sleep(3000);
                if (apertureNeedsInitialization == false)
                {
                    if (!(fStopList.Count > 2))
                    {
                        birgerSerial.Write("da");           //Print the the aperture range
                        Thread.Sleep(50);
                    }
                }

                booting = false;
            }
            else
            {

            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (cameraAPI.Cameras.Count > 0)
            {
                //If DSLR detected, abort lens play manager thread so it wont affect calibration
                if (lensPlayManagerThread.IsAlive)
                {
                    lensPlayManagerThread.Abort();
                }

                //If dslr is in live view mode, launch calibration dialog
                if (cameraAPI.Cameras[0].IsInLiveViewMode)
                {
                    FindAndMoveMsgBox("Lens Calibration", this.Location.X + this.Height / 2, this.Location.Y + this.Width / 8);
                    DSLRCalibration dslrCalibration = new DSLRCalibration(this, cameraAPI.Cameras[0], hyperFocal, encoderItems);
                    dslrCalibration.ShowDialog(this);
                }
                else
                {
                    //Show live view error if DSLR is not in live view mode
                    Thread.Sleep(100);
                    if (!liveViewNotDetected)
                    {
                        liveViewNotDetected = true;

                        Thread thr = new Thread(() => // create a new thread
                        {
                            FindAndMoveMsgBox("Live View Not Detected", this.Location.X + this.Height / 2, this.Location.Y + this.Width / 8);
                            if (this.InvokeRequired)
                            {
                                BeginInvoke((MethodInvoker)delegate
                                {
                                    if (MessageBox.Show(this, "Power on DSLR and activate Live View mode.", "Live View Not Detected", MessageBoxButtons.OK) == DialogResult.OK)
                                    {
                                        liveViewNotDetected = false;
                                    }
                                });
                            }
                            else
                            {
                                if (MessageBox.Show(this, "Power on DSLR and activate Live View mode.", "Live View Not Detected", MessageBoxButtons.OK) == DialogResult.OK)
                                {
                                    liveViewNotDetected = false;
                                }
                            }
                        });
                        thr.IsBackground = true;
                        thr.Start(); // starts the thread
                    }
                }
            }
            else
            {
                //Close the birger serial port if it is currently open
                if (birgerSerial.IsOpen())
                {
                    birgerSerial.Write("sm0");      //Set Special Modes to 0 (reserved)
                    birgerSerial.Write("sr0");      //Set spontaneous responses off

                    Thread.Sleep(1000);
                    Thread closeSerialThread = new Thread(CloseSerial);
                    closeSerialThread.IsBackground = true;
                    closeSerialThread.Start();
                    bwIsSerialOpen.CancelAsync();
                    Thread buttonConnectThread = new Thread(ButtonConnect);
                    buttonConnectThread.IsBackground = true;
                    buttonConnectThread.Start();
                }
                else
                {
                    //Start wait button background worker to put Connect button in busy mode
                    bwWaitButton.RunWorkerAsync();

                    //Start serial connection background thread
                    Thread connectThread = new Thread(Connect);
                    connectThread.IsBackground = true;
                    connectThread.Start();
                    Application.DoEvents();
                }
            }
        }

        void CheckIrisMoving()
        {
            //Background thread monitoring whether the iris is not currently moving, making it safe to update iris dependent controls
            while (1 == 1)
            {
                Stopwatch irisMovingWatch = new Stopwatch();
                irisMovingWatch.Start();

                if (tbIrisMoving)
                {
                    while (irisMovingWatch.ElapsedMilliseconds < 3000)
                    {

                    }

                    tbIrisMoving = false;
                }
            }
        }

        private void tbIris_ValueChanged(object sender, EventArgs e)
        {
            //Move iris by sending command to birger serial or sending command to DSLR. Set iris moving flag true

            if (birgerSerial.IsOpen())
            {
                string ma = "ma" + tbIris.Value.ToString();
                birgerSerial.Write(ma);
            }

            tbIrisMoving = true;

            if (this.InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    if (fStopList.Count > 1)
                        lblIrisBarValue.Text = "f" + string.Format("{0:0.0}", fStopList[tbIris.Value]);
                });
            }
            else
            {
                if (fStopList.Count > 1)
                    lblIrisBarValue.Text = "f" + string.Format("{0:0.0}", fStopList[tbIris.Value]);
            }
        }

        private void btnLearnFocusStops_Click(object sender, EventArgs e)
        {
            //Clears encoder item collection and sends learn focus stop command to birger serial port
            encoderItems.EncoderItems.Clear();
            birgerSerial.Write("ds1");
        }

        //Current focus value propterty
        public int FocusValue
        {
            get
            {
                return tbFocus.Value;
            }
            set
            {
                tbFocus.Value = value;
            }
        }

        //Current Iris Value property
        public int IrisValue
        {
            get
            {
                return tbIris.Value;
            }
            set
            {
                tbIris.Value = value;
            }
        }

        //Closes thread if form is closing
        private void frmFocusPuller_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.ExitThread();
            Application.Exit();
        }
    }
}
