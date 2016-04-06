using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CanonCameraAppLib;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using ManagedWinapi;
using ManagedWinapi.Hooks;
using ManagedWinapi.Windows;
using WindowsInput;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Birger_Rack_Focus_Assistant
{
    public partial class RangeFinder : Form
    {
        const int threadSleep = 25;
        int currentThumb = 0;
        FocusMove currentFocusMove = new FocusMove();
        Logger logger = new Logger("c:\\");
        Point initialPosition = new Point(Cursor.Position.X, Cursor.Position.Y);
        At90Usb rangeFinder = new At90Usb();
        bool laserOn = false;
        double measurement;
        double measurementBuffer = 0;
        double measurementPreOffset = 0;

        double startingDistanceBuffer;
        bool readError = false;
        bool isSonar = false;
        bool isLaser = true;
        SonarRangeFinder sonarRangeFinder = new SonarRangeFinder();

        BackgroundWorker bwActivateLaser = new BackgroundWorker();
        BackgroundWorker bwSeekDistance = new BackgroundWorker();
        BackgroundWorker bwWaitButton = new BackgroundWorker();
        BackgroundWorker bwDeactivateButton = new BackgroundWorker();
        BirgerSerial birgerSerial = new BirgerSerial();
        BackgroundWorker bwIsSerialOpen = new BackgroundWorker();

        int aperturePosValue = -1;
        int focusPosValue = -1;
        bool initialApertureSet = true;
        bool initialFocusSet = true;

        Dictionary<int, double> encoderDictionary = new Dictionary<int, double>();
        Dictionary<double, int> feetDictionary = new Dictionary<double, int>();
        EncoderItemList encoderItems = new EncoderItemList();
        double hyperFocal = 0;
        double focusOffset = 0;
        Camera camera;
        bool liveView = true;
        Dictionary<int, int> trackbarDictionary = new Dictionary<int, int>();

        Stopwatch tbFocusStopWatch = new Stopwatch();

        frmFocusPuller parentForm;

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(String sClassName, String sAppName);
        [DllImport("user32.dll")]
        static extern IntPtr FindWindow(IntPtr classname, string title);

        public RangeFinder(frmFocusPuller parentFormIn, BirgerSerial birgerSerialIn, int focusValue, int irisValue, int irisMax, EncoderItemList encoderItemsIn, Dictionary<int, double> encoderDictionaryIn,
            Dictionary<double, int> feetDictionaryIn, double hyperFocalIn, Dictionary<int, int> trackbarDictionaryIn, Camera cameraIn)
        {
            InitializeComponent();

            parentForm = parentFormIn;
            camera = cameraIn;
            camera.OnShutdown += new ShutdownEventHandler(camera_OnShutdown);
            camera.OnPropertyChanged += new CanonCameraAppLib.PropertyChangedEventHandler(camera_OnPropertyChanged);

            birgerSerial = birgerSerialIn;
            focusPosValue = focusValue;
            aperturePosValue = irisValue;
            tbIris.Maximum = irisMax;
            encoderItems = encoderItemsIn;
            encoderDictionary = encoderDictionaryIn;
            feetDictionary = feetDictionaryIn;
            hyperFocal = hyperFocalIn;
            trackbarDictionary = trackbarDictionaryIn;

            tbFocus.Value = focusValue;
            tbIris.Value = irisValue;

            if (encoderDictionary[tbFocus.Value] > hyperFocal)
            {
                lblTrackbarValue.Text = "\u221E";
            }
            else
            {
                lblTrackbarValue.Text = String.Format("{0:0.00}", encoderDictionary[tbFocus.Value]) + "ft";
            }

            if (parentForm.fStopList.Count > 1)
                lblIrisBarValue.Text = "f" + string.Format("{0:0.0}", parentForm.fStopList[tbIris.Value]);

            bwActivateLaser.DoWork += new DoWorkEventHandler(bwActivateLaser_DoWork);
            bwActivateLaser.WorkerSupportsCancellation = true;
            bwSeekDistance.DoWork += new DoWorkEventHandler(bwSeekDistance_DoWork);
            bwSeekDistance.WorkerSupportsCancellation = true;
            bwWaitButton.DoWork += new DoWorkEventHandler(bwWaitButton_DoWork);
            bwWaitButton.WorkerSupportsCancellation = true;
            bwDeactivateButton.DoWork += new DoWorkEventHandler(bwDeactivateButton_DoWork);
            bwDeactivateButton.WorkerSupportsCancellation = true;

            birgerSerial.DataReceived += new BirgerSerial.CustomSerialDataReceivedEventHandler(birgerSerial_DataReceived);
            bwIsSerialOpen.DoWork += new DoWorkEventHandler(bwIsSerialOpen_DoWork);
            bwIsSerialOpen.WorkerSupportsCancellation = true;

            tbFocusStopWatch.Start();

            //Start offset monitor watcher thread which detects whether the user needs to reset the focus offset
            Thread offsetMonitorThread = new Thread(OffsetMonitor);
            offsetMonitorThread.IsBackground = true;
            offsetMonitorThread.Start();

            //Start parent monitor watcher thread which detects whether the parent iris controls are enabled or disable so the local ones can sync to them
            Thread parentIrisMonitorThread = new Thread(ParentIrisMonitor);
            parentIrisMonitorThread.IsBackground = true;
            parentIrisMonitorThread.Start();

            ////Check if the laser is already on
            //if (ReadLaser())
            //{

            //}
        }

        void ParentIrisMonitor()
        {
            while (1 == 1)
            {
                if (parentForm.tbIris.Enabled)
                {

                }
                else
                {
                    if (this.InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            lblIrisBarValue.Visible = false;
                            tbIris.Enabled = false;
                        });
                    }
                    else
                    {
                        lblIrisBarValue.Visible = false;
                        tbIris.Enabled = false;
                    }
                }

                Thread.Sleep(1000);
            }
        }

        void OffsetMonitor()
        {
            while (1 == 1)
            {
                if (focusOffset != 0)
                {
                    if (this.InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            if (Math.Round(measurementPreOffset - encoderDictionary[tbFocus.Value], 2) > (focusOffset - .2) &&
                                Math.Round(measurementPreOffset - encoderDictionary[tbFocus.Value], 2) < (focusOffset + .2))
                            {
                                txtFocusOffset.BackColor = txtFocusOffset.BackColor;
                                txtFocusOffset.ForeColor = Color.Black;
                                btnSetOffset.ForeColor = Color.Black;
                                
                                if(!bwSeekDistance.IsBusy && bwActivateLaser.IsBusy)
                                    btnFocusControl.ForeColor = Color.Green;
                                else if (bwSeekDistance.IsBusy && bwActivateLaser.IsBusy)
                                    btnFocusControl.ForeColor = Color.Red;
                                else
                                    btnFocusControl.ForeColor = Color.Black;
                            }
                            else
                            {
                                txtFocusOffset.BackColor = txtFocusOffset.BackColor;
                                txtFocusOffset.ForeColor = Color.Red;
                                btnSetOffset.ForeColor = Color.Red;
                                if (!bwSeekDistance.IsBusy)
                                    btnFocusControl.ForeColor = Color.Black;
                                else

                                    btnFocusControl.ForeColor = Color.Red;
                            }
                        });
                    }
                    else
                    {
                    }
                }
                else
                {
                    if (laserOn)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            txtFocusOffset.BackColor = txtFocusOffset.BackColor;
                            txtFocusOffset.ForeColor = Color.Red;
                            btnSetOffset.ForeColor = Color.Red;
                        });
                    }
                }

                Thread.Sleep(50);
            }
        }

        public void camera_OnPropertyChanged(Camera sender, CanonCameraAppLib.PropertyChangedEventArgs e)
        {
            //Detects if liveview is lost
            if (camera.IsInLiveViewMode)
            {
                liveView = true;
            }
            else
            {
                liveView = false;
                parentForm.camera_OnPropertyChanged(sender, e);

                //Checks if liveview is still false 5 seconds from now, and closes the form if so
                Thread thr = new Thread(() => // create a new thread
                {
                    if (this.InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            Thread.Sleep(8000);
                            if (!camera.IsInLiveViewMode)
                                this.Close();
                        });
                    }
                    else
                    {
                        Thread.Sleep(8000);
                        if (!camera.IsInLiveViewMode)
                            this.Close();
                    }
                });
                thr.IsBackground = true;
                thr.Start(); // starts the thread
            }
        }

        public void camera_OnShutdown(Camera sender, ShutdownEventArgs e)
        {
            //Detects if camera has been shut down
            parentForm.camera_OnShutdown(sender, e);
            this.Close();
        }

        void bwIsSerialOpen_DoWork(object sender, DoWorkEventArgs e)
        {
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

                    if (birgerSerial.IsOpen())
                    {
                        Thread buttonDisconnectThread = new Thread(ButtonDisconnect);
                        buttonDisconnectThread.IsBackground = true;
                        buttonDisconnectThread.Start();
                    }
                    else
                    {
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
            BeginInvoke((MethodInvoker)delegate
            {
                tbFocus.Enabled = false;
                if (!initialFocusSet && focusPosValue != -1)
                {
                    tbFocus.Value = focusPosValue;
                    initialFocusSet = true;
                }

                if (!initialApertureSet && aperturePosValue != -1)
                {
                    tbIris.Value = aperturePosValue;
                    if (parentForm.fStopList.Count > 1)
                        lblIrisBarValue.Text = "f" + string.Format("{0:0.0}", parentForm.fStopList[aperturePosValue]);
                    initialApertureSet = true;
                }
                else if (aperturePosValue != -1)
                {
                    if (parentForm.fStopList.Count > 1)
                        lblIrisBarValue.Text = "f" + string.Format("{0:0.0}", parentForm.fStopList[tbIris.Value]);
                }
                else
                {
                    lblIrisBarValue.Text = tbIris.Value.ToString();
                }
                tbIris.Enabled = false;

                Thread.Sleep(50);
                Application.DoEvents();
            });
        }

        void ButtonConnect()
        {
            BeginInvoke((MethodInvoker)delegate
            {
                tbFocus.Enabled = false;
                tbIris.Enabled = false;

                Thread.Sleep(50);
                Application.DoEvents();
            });
        }

        void birgerSerial_DataReceived(object sender, CustomSerialDataReceivedEventArgs e)
        {
            string dataText = e.Data;

            Regex errorPattern = new Regex(@"ERR21");
            foreach (Match match in errorPattern.Matches(dataText))
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    MessageBox.Show("Lens not Ready. Please try again.");

                    Thread.Sleep(50);
                    Application.DoEvents();
                });
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

                    Thread.Sleep(50);
                    Application.DoEvents();
                });
            }
        }

        void bwSeekDistance_DoWork(object sender, DoWorkEventArgs e)
        {
            if (bwSeekDistance.CancellationPending)
            {
                e.Cancel = true;
            }

            if (e.Cancel == true)
            {
            }
            else
            {
                SeekDistance();
            }
        }

        void bwActivateLaser_DoWork(object sender, DoWorkEventArgs e)
        {
            if (bwActivateLaser.CancellationPending)
            {
                e.Cancel = true;
            }

            if (e.Cancel == true)
            {
            }
            else
            {
                ActivateRangeFinder();
            }
        }

        private void btnActivate_Click(object sender, EventArgs e)
        {
            txtFocusOffset.Text = "";

            if (isLaser)
            {
                //Laser isn't on. Default condition. Load everything.
                if (laserOn == false)
                {
                    logger.LogError("laserOn == false", EventLogEntryType.Information);
                    logger.LogError("bwWaitButton.RunWorkerAsync()", EventLogEntryType.Information);
                    bwWaitButton.RunWorkerAsync();

                    logger.LogError("bwActivateLaser.RunWorkerAsync()", EventLogEntryType.Information);
                    bwActivateLaser.RunWorkerAsync();

                    logger.LogError("bwDeactivateButton.RunWorkerAsync()", EventLogEntryType.Information);
                    bwDeactivateButton.RunWorkerAsync();

                    Application.DoEvents();
                }
                else
                {
                    //Laser flag is on, but the backgroundworker running it isn't busy
                    logger.LogError("laserOn == true", EventLogEntryType.Information);
                    if (laserOn == true && !bwActivateLaser.IsBusy)
                    {
                        logger.LogError("bwWaitButton.RunWorkerAsync()", EventLogEntryType.Information);
                        bwWaitButton.RunWorkerAsync();

                        logger.LogError("bwActivateLaser.RunWorkerAsync()", EventLogEntryType.Information);
                        bwActivateLaser.RunWorkerAsync();

                        logger.LogError("bwDeactivateButton.RunWorkerAsync()", EventLogEntryType.Information);
                        bwDeactivateButton.RunWorkerAsync();

                        Application.DoEvents();
                    }
                    //Laser flag is on and the backgroundwork running it IS busy
                    else if (laserOn == true && bwActivateLaser.IsBusy)
                    {
                        logger.LogError("Aborting Activate Thread and turning off laser.", EventLogEntryType.Information);
                        logger.LogError("bwActivateLaser.CancelAsync()", EventLogEntryType.Information);
                        logger.LogError("bwDeactivateButton.CancelAsync()", EventLogEntryType.Information);
                        logger.LogError("bwWaitButton.CancelAsync()", EventLogEntryType.Information);
                        bwActivateLaser.CancelAsync();
                        bwDeactivateButton.CancelAsync();
                        bwWaitButton.CancelAsync();

                        Application.DoEvents();
                    }
                }
            }
            else if (isSonar)
            {
                if (!bwActivateLaser.IsBusy)
                {
                    logger.LogError("bwActivateLaser.RunWorkerAsync()", EventLogEntryType.Information);
                    bwActivateLaser.RunWorkerAsync();

                    btnActivate.Text = "Deactivate RangeFinder";
                    btnActivate.ForeColor = Color.Red;

                    Application.DoEvents();
                }
                else if (bwActivateLaser.IsBusy)
                {
                    logger.LogError("Aborting Activate Thread and turning off sonar.", EventLogEntryType.Information);
                    logger.LogError("bwActivateLaser.CancelAsync()", EventLogEntryType.Information);
                    bwActivateLaser.CancelAsync();

                    btnActivate.Text = "Activate RangeFinder";
                    btnActivate.ForeColor = Color.Black;
                    txtMeasurement.Text = string.Empty;

                    Application.DoEvents();
                }
            }
        }

        void CancelButton()
        {
            BeginInvoke((MethodInvoker)delegate
            {
                logger.LogError("Change button text: Cancel Focus Control", EventLogEntryType.Information);
                btnFocusControl.Text = "Cancel Focus Control";
                btnFocusControl.ForeColor = Color.Red;

                Thread.Sleep(50);
                Application.DoEvents();
            });
        }

        void SeekWaitButton()
        {
            BeginInvoke((MethodInvoker)delegate
            {
                logger.LogError("btnFocusControl.Text = Wait...", EventLogEntryType.Information);
                btnFocusControl.Text = "Wait...";
                btnFocusControl.ForeColor = Color.Red;

                Thread.Sleep(50);
                Application.DoEvents();
            });
        }

        void WaitButton()
        {
            do
            {
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                if (stopWatch.ElapsedMilliseconds > 25)
                {
                    stopWatch.Reset();
                    stopWatch.Start();
                    BeginInvoke((MethodInvoker)delegate
                    {
                        logger.LogError("btnActivate.Text = Wait...", EventLogEntryType.Information);
                        btnActivate.Text = "Wait...";
                        btnActivate.ForeColor = Color.Red;

                        Thread.Sleep(50);
                        Application.DoEvents();
                    });
                }

                Thread.Sleep(50);
            } while (laserOn == false && !readError);
        }

        void DeactivateButton()
        {
            logger.LogError("Change button text: Deactivate RangeFinder", EventLogEntryType.Information);
            do
            {
                Thread.Sleep(75);
                if (laserOn)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        btnActivate.Text = "Deactivate RangeFinder";
                        btnActivate.ForeColor = Color.Red;

                        btnSetOffset.Enabled = true;
                    });
                }

                Application.DoEvents();
            } while (laserOn == true && !readError);

            Thread.Sleep(75);
            BeginInvoke((MethodInvoker)delegate
            {
                txtFocusOffset.Text = "";
                focusOffset = 0;
                btnSetOffset.Enabled = false;
            });

            Application.DoEvents();
        }


        bool GetConfig()
        {
            byte[] getConfigBytes = new byte[8];
            getConfigBytes[0] = 0x00;
            return rangeFinder.WriteData(getConfigBytes);
        }

        bool ReadLaser()
        {
            byte[] buffer = new byte[8];
            buffer[0] = 0xCC;
            buffer[1] = 0xCC;
            buffer[2] = 0xCC;
            buffer[3] = 0xCC;
            buffer[4] = 0xCC;
            return rangeFinder.ReadData(buffer);
        }

        bool TurnOnLaser()
        {
            byte[] setConfigBytes = new byte[8];
            setConfigBytes[0] = 0x01;
            setConfigBytes[1] = 0x00;
            setConfigBytes[2] = 0x80;
            setConfigBytes[3] = 0x00;
            setConfigBytes[4] = 0x00;
            return rangeFinder.WriteData(setConfigBytes);
        }

        bool TurnOffLaser()
        {
            byte[] setConfigBytes = new byte[8];
            setConfigBytes[0] = 0x01;
            setConfigBytes[1] = 0x01;
            setConfigBytes[2] = 0x00;
            setConfigBytes[3] = 0x00;
            setConfigBytes[4] = 0x00;
            return rangeFinder.WriteData(setConfigBytes);
        }

        void FindAndMoveMsgBox(string title, int x, int y)
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
            thr.Start(); // starts the thread
        }

        void ActivateRangeFinder()
        {
            if (isLaser)
            {
                //If laser is current type of rangefinder turn laser on and start reporting measurements
                rangeFinder.find_Hid_Device();
                if (TurnOnLaser())
                    laserOn = true;
                else
                    laserOn = false;

                List<double> measurements = new List<double>();
                int measureCount = 0;

                readError = false;
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                do
                {
                    if (bwActivateLaser.CancellationPending)
                    {
                        logger.LogError("CancellationPending", EventLogEntryType.Information);
                    }

                    if (!bwActivateLaser.CancellationPending)
                    {
                        if (ReadLaser())
                        {
                            laserOn = true;
                            measurement = rangeFinder.CurrentMeasurement * 0.00328084;

                            //Put measurement in measurementPreOffset buffer and handle small random variation
                            if (measurementPreOffset == 0)
                            {
                                measurementPreOffset = measurement;
                            }
                            else if ((measurement < measurementPreOffset - .02) || (measurement > measurementPreOffset + .02))
                            {
                                if (measurement < measurementPreOffset)
                                {
                                    if ((double)((measurementPreOffset - measurement) / measurementPreOffset) >= .5)
                                    {
                                        Thread.Sleep(250);
                                        measurement = rangeFinder.CurrentMeasurement * 0.00328084;
                                    }
                                }
                                else
                                {
                                    if ((double)((measurement - measurementPreOffset) / measurement) >= .5)
                                    {
                                        Thread.Sleep(250);
                                        measurement = rangeFinder.CurrentMeasurement * 0.00328084;
                                    }
                                }

                                //Capture the measurement before the offset is applied
                                measurementPreOffset = measurement;
                            }
                            
                            //Apply measurement offset, subtracting the offset fom the rangefinder measurement
                            measurementBuffer = measurementPreOffset - focusOffset;

                            measurements.Add(measurementBuffer);
                            measureCount++;

                            if (stopWatch.ElapsedMilliseconds > 25)
                            {
                                stopWatch.Reset();
                                stopWatch.Start();
                                BeginInvoke((MethodInvoker)delegate
                                {
                                    txtMeasurement.Text = String.Format("{0:0.00}", measurementPreOffset) + "ft";

                                    btnActivate.Text = "Deactivate RangeFinder";
                                    btnActivate.ForeColor = Color.Red;

                                    Application.DoEvents();
                                });
                            }
                        }
                        else
                        {
                            readError = true;
                            laserOn = false;
                            if (bwSeekDistance.IsBusy)
                            {
                                bwSeekDistance.CancelAsync();

                                if (stopWatch.ElapsedMilliseconds > 3000)
                                {
                                    stopWatch.Reset();
                                    stopWatch.Start();
                                    BeginInvoke((MethodInvoker)delegate
                                    {
                                        btnFocusControl.Text = "Start Focus Control";
                                        btnFocusControl.ForeColor = Color.Black;

                                        Thread.Sleep(50);
                                        Application.DoEvents();
                                    });

                                    System.Threading.Thread.Sleep(3000);
                                }
                            }

                            bwActivateLaser.CancelAsync();

                            System.Threading.Thread.Sleep(3000);

                            FindAndMoveMsgBox("Error", this.Location.X + this.Height / 2, this.Location.Y + this.Width / 8);

                            logger.LogError("readError = true", EventLogEntryType.Information);

                            if (stopWatch.ElapsedMilliseconds > 250)
                            {
                                stopWatch.Reset();
                                stopWatch.Start();
                                BeginInvoke((MethodInvoker)delegate
                                {
                                    MessageBox.Show(this, "Rangefinder read error. Please ensure that laser is not blocked, and check for proper connection and power.", "Error");

                                    Application.DoEvents();
                                });
                            }
                        }
                    }
                } while (!readError && !bwActivateLaser.CancellationPending);

                TurnOffLaser();
                laserOn = false;
                rangeFinder.close_Device();
                logger.LogError("laserOn = false", EventLogEntryType.Information);

                if (stopWatch.ElapsedMilliseconds > 250)
                {
                    stopWatch.Reset();
                    stopWatch.Start();
                    BeginInvoke((MethodInvoker)delegate
                    {
                        btnActivate.Text = "Activate RangeFinder";
                        btnActivate.ForeColor = Color.Black;
                        txtMeasurement.Text = string.Empty;

                        Thread.Sleep(50);
                        Application.DoEvents();
                    });
                }


                if (stopWatch.ElapsedMilliseconds > 4000)
                {
                    stopWatch.Reset();
                    stopWatch.Start();
                    BeginInvoke((MethodInvoker)delegate
                    {
                        btnFocusControl.Text = "Start Focus Control";
                        btnFocusControl.ForeColor = Color.Black;

                        Thread.Sleep(50);
                        Application.DoEvents();
                    });
                }

                bwDeactivateButton.CancelAsync();
                bwWaitButton.CancelAsync();
                bwSeekDistance.CancelAsync();

                System.Threading.Thread.Sleep(3000);
            }
            else if (isSonar)
            {
                if (sonarRangeFinder.IsOpen())
                {
                    laserOn = true;

                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();
                    do
                    {
                        if (stopWatch.ElapsedMilliseconds > 250)
                        {
                            stopWatch.Reset();
                            stopWatch.Start();

                            measurement = sonarRangeFinder.CurrentMeasurement;

                            //Put measurement in measurement buffer and handle small random variation
                            if (measurementBuffer == 0)
                            {
                                measurementBuffer = measurement;
                            }
                            else if ((measurement < measurementBuffer - .02) || (measurement > measurementBuffer + .02))
                            {
                                measurementBuffer = measurement;
                            }

                            //Capture the measurement before the offset is applied
                            measurementPreOffset = measurementBuffer;

                            //Apply measurement offset, subtracting the offset fom the rangefinder measurement
                            measurementBuffer = measurementBuffer - focusOffset;

                            BeginInvoke((MethodInvoker)delegate
                            {
                                txtMeasurement.Text = String.Format("{0:0.00}", measurementPreOffset) + "ft";

                                Thread.Sleep(50);
                                Application.DoEvents();
                            });
                        }
                    } while (!bwActivateLaser.CancellationPending && sonarRangeFinder.IsOpen());
                }
                else
                {
                    laserOn = false;
                }

                bwDeactivateButton.CancelAsync();
                bwWaitButton.CancelAsync();
                bwSeekDistance.CancelAsync();

                Stopwatch activateStopWatch = new Stopwatch();
                activateStopWatch.Start();
                if (activateStopWatch.ElapsedMilliseconds > 3000)
                {
                    activateStopWatch.Reset();
                    activateStopWatch.Start();
                    BeginInvoke((MethodInvoker)delegate
                    {
                        btnActivate.Text = "Activate RangeFinder";
                        btnActivate.ForeColor = Color.Black;
                        txtMeasurement.Text = string.Empty;

                        Thread.Sleep(50);
                        Application.DoEvents();
                    });

                    BeginInvoke((MethodInvoker)delegate
                    {
                        btnFocusControl.Text = "Start Focus Control";
                        btnFocusControl.ForeColor = Color.Black;

                        Thread.Sleep(50);
                        Application.DoEvents();
                    });
                }

                System.Threading.Thread.Sleep(3000);
            }
        }

        private void SeekDistance()
        {
            if (laserOn == true)
            {

            }
            else
            {
                Thread.Sleep(5000);
            }

            parentForm.pauseLensPlay = true;

            BackgroundWorker bwCancelFocus = new BackgroundWorker();
            bwCancelFocus.DoWork += new DoWorkEventHandler(bwCancelFocus_DoWork);
            bwCancelFocus.RunWorkerAsync();


            Stopwatch seekDistWatch = new Stopwatch();
            seekDistWatch.Start();
            do
            {
                List<double> feetDictionaryList = new List<double>();
                foreach (KeyValuePair<double, int> pair in feetDictionary)
                {
                    feetDictionaryList.Add(pair.Key);
                }

                string message = String.Format("{0:0.00}", measurementBuffer);
                double measurementMessage = double.Parse(message);

                string messageOld = String.Format("{0:0.00}", measurementBuffer);
                double measurementOld = double.Parse(messageOld);
                int searchValue = 0;

                //Attempt to find current measurement in dictionary
                try
                {
                    searchValue = feetDictionaryList.BinarySearch(measurementMessage);
                    if (searchValue >= 0)
                    {
                        message = feetDictionary.ElementAt(searchValue).Value.ToString();
                    }
                    else if (searchValue == -1)
                    {
                        message = feetDictionary.ElementAt(~searchValue).Value.ToString();
                    }
                    else
                    {
                        if (Math.Abs(measurementMessage - feetDictionary.ElementAt(~searchValue).Key) < Math.Abs(measurementMessage - feetDictionary.ElementAt(~searchValue - 1).Key))
                        {
                            message = feetDictionary.ElementAt(~searchValue).Value.ToString();
                        }
                        else
                        {
                            message = feetDictionary.ElementAt(~searchValue - 1).Value.ToString();
                        }
                    }
                }
                catch (Exception ex)
                {

                }


                //try
                //{
                //    //Attempt to find current measurement in dictionary
                //    message = feetDictionary[measurementMessage].ToString();
                //}
                //catch (Exception ex)
                //{
                //    if (measurementMessage < encoderDictionary[0])
                //    {
                //        //If measurement too low, use the minimum distance 
                //        measurementMessage = encoderDictionary[0];
                //        measurementMessage = Math.Round(measurementMessage, 2);
                //        message = feetDictionary[measurementMessage].ToString();
                //    }
                //    else
                //    {
                //        //Try higher and lower measurement values in dictionary until a match is found
                //        double measurementHigher = measurementMessage;
                //        int higherValue = 0;
                //        double measurementLower = measurementMessage;
                //        int lowerValue = 0;
                //        bool highMatch = false;
                //        bool lowMatch = false;

                //        do
                //        {
                //            measurementHigher = measurementHigher + .01;
                //            measurementHigher = Math.Round(measurementHigher, 2);
                //            measurementLower = measurementLower - .01;
                //            measurementLower = Math.Round(measurementLower, 2);

                //            if (feetDictionary.TryGetValue(measurementHigher, out higherValue))
                //            {
                //                highMatch = true;
                //            }

                //            if (feetDictionary.TryGetValue(measurementLower, out lowerValue))
                //            {
                //                lowMatch = true;
                //            }
                //        } while (!highMatch || !lowMatch);

                //        if (highMatch && !lowMatch)
                //        {
                //            try
                //            {
                //                message = feetDictionary[measurementHigher].ToString();
                //            }
                //            catch (Exception ex2)
                //            {
                //                message = "0";
                //            }
                //        }
                //        else if (!highMatch && lowMatch)
                //        {
                //            try
                //            {
                //                message = feetDictionary[measurementLower].ToString();
                //            }
                //            catch (Exception ex2)
                //            {
                //                message = "0";
                //            }
                //        }
                //        else if (highMatch && lowMatch)
                //        {
                //            //If a match is found on both, use the one with the least difference
                //            if ((measurementMessage - measurementLower) < (measurementHigher - measurementMessage))
                //            {
                //                try
                //                {
                //                    message = feetDictionary[measurementLower].ToString();
                //                }
                //                catch (Exception ex2)
                //                {
                //                    message = "0";
                //                }
                //            }
                //            else if ((measurementMessage - measurementLower) == (measurementHigher - measurementMessage))
                //            {
                //                try
                //                {
                //                    message = feetDictionary[measurementHigher].ToString();
                //                }
                //                catch (Exception ex2)
                //                {
                //                    message = "0";
                //                }
                //            }
                //            else
                //            {
                //                try
                //                {
                //                    message = feetDictionary[measurementHigher].ToString();
                //                }
                //                catch (Exception ex2)
                //                {
                //                    message = "0";
                //                }
                //            }
                //        }
                //    }
                //}

                bool seeking = true;
                parentForm.pauseLensPlay = true;

                int stepSize = 1;
                string messageNew = String.Format("{0:0.00}", measurementBuffer);
                double measurementNew = double.Parse(messageNew);
                while (seeking && !readError && !bwSeekDistance.CancellationPending && measurementNew == measurementOld)
                {
                    if (seekDistWatch.ElapsedMilliseconds > 50)
                    {
                        messageNew = String.Format("{0:0.00}", measurementBuffer);
                        measurementNew = double.Parse(messageNew);
                        BeginInvoke((MethodInvoker)delegate
                        {
                            if (tbFocus.Value == int.Parse(message))
                            {
                                seeking = false;
                            }
                            else
                            {
                                //Roll to distance destination with step size based on delta
                                if (tbFocus.Value < int.Parse(message))
                                {
                                    if ((int.Parse(message) - tbFocus.Value) < 10)
                                        stepSize = 1;
                                    else if ((int.Parse(message) - tbFocus.Value) < 100)
                                        stepSize = 1;
                                    else if ((int.Parse(message) - tbFocus.Value) < 500)
                                        stepSize = 5;
                                    else if ((int.Parse(message) - tbFocus.Value) < 1000)
                                        stepSize = 10;
                                    else if ((int.Parse(message) - tbFocus.Value) < 1100)
                                        stepSize = 20;
                                    else
                                    {
                                        tbFocus.Value = int.Parse(message);
                                        parentForm.pauseLensPlay = true;
                                        Application.DoEvents();
                                    }

                                    if (tbFocus.Value + stepSize >= tbFocus.Maximum)
                                        tbFocus.Value = tbFocus.Maximum;
                                    else
                                        tbFocus.Value = tbFocus.Value + stepSize;
                                }
                                else
                                {
                                    if ((tbFocus.Value - int.Parse(message)) < 10)
                                        stepSize = 1;
                                    else if ((tbFocus.Value - int.Parse(message)) < 100)
                                        stepSize = 1;
                                    else if ((tbFocus.Value - int.Parse(message)) < 500)
                                        stepSize = 5;
                                    else if ((tbFocus.Value - int.Parse(message)) < 1000)
                                        stepSize = 10;
                                    else if ((tbFocus.Value - int.Parse(message)) < 1100)
                                        stepSize = 20;
                                    else
                                    {
                                        tbFocus.Value = int.Parse(message);
                                        parentForm.pauseLensPlay = true;
                                        Application.DoEvents();
                                    }

                                    if (tbFocus.Value - stepSize <= tbFocus.Minimum)
                                        tbFocus.Value = tbFocus.Minimum;
                                    else
                                        tbFocus.Value = tbFocus.Value - stepSize;
                                }
                            }



                            lblTrackbarValue.Text = String.Format("{0:0.00}", encoderDictionary[tbFocus.Value]) + "ft";

                            Application.DoEvents();
                        });

                        seekDistWatch.Reset();
                        seekDistWatch.Start();
                    }
                }

                Application.DoEvents();
            } while (!readError && !bwSeekDistance.CancellationPending);


            BeginInvoke((MethodInvoker)delegate
            {
                btnFocusControl.Text = "Start Focus Control";
                btnFocusControl.ForeColor = Color.Black;

                Thread.Sleep(50);
                Application.DoEvents();
            });
        }

        private void btnFocusControl_Click(object sender, EventArgs e)
        {
            //Cancels focus control if seek worker is busy. Otherwise starts seek worker
            if (laserOn == false)
            {
                if (!bwActivateLaser.IsBusy)
                {
                    logger.LogError("btnFocusControl_Click. laseron False. bwActivateLaser NOT busy. Starting all background workers", EventLogEntryType.Information);

                    bwWaitButton.RunWorkerAsync();

                    bwActivateLaser.RunWorkerAsync();

                    bwDeactivateButton.RunWorkerAsync();
                }
                else
                {
                    logger.LogError("btnFocusControl_Click. laseron False. bwActivateLaser IS busy. Starting Wait and Deactivate button background workers", EventLogEntryType.Information);

                    bwWaitButton.RunWorkerAsync();

                    bwDeactivateButton.RunWorkerAsync();
                }


                if (bwSeekDistance.IsBusy)
                {
                    logger.LogError("btnFocusControl_Click. laseron False. bwSeekDistance IS busy. Stopping Seek and resetting its button.", EventLogEntryType.Information);

                    bwSeekDistance.CancelAsync();

                    btnFocusControl.Text = "Start Focus Control";
                    btnFocusControl.ForeColor = Color.Black;

                    System.Threading.Thread.Sleep(3000);
                }
                else
                {
                    logger.LogError("btnFocusControl_Click. laseron False. bwSeekDistance NOT busy. Starting Seek and setting its button to Wait.", EventLogEntryType.Information);

                    parentForm.pauseLensPlay = true;

                    bwSeekDistance.RunWorkerAsync();

                    BackgroundWorker bwSeekWaitButton = new BackgroundWorker();
                    bwSeekWaitButton.DoWork += new DoWorkEventHandler(bwSeekWaitButton_DoWork);
                    bwSeekWaitButton.RunWorkerAsync();

                    System.Threading.Thread.Sleep(3000);
                }
            }
            else
            {
                if (laserOn == true && !bwSeekDistance.IsBusy)
                {
                    Stopwatch laserSyncWatch = new Stopwatch();
                    if (!bwActivateLaser.IsBusy)
                    {
                        logger.LogError("btnFocusControl_Click. laseron true. bwSeekDistance NOT busy. bwActivateLaser NOT busy. Starting Activate Laser.", EventLogEntryType.Information);

                        bwActivateLaser.RunWorkerAsync();
                        laserSyncWatch.Start();
                    }


                    logger.LogError("Starting wait and deactivate button workers.", EventLogEntryType.Information);

                    if (!bwWaitButton.IsBusy)
                        bwWaitButton.RunWorkerAsync();
                    if (!bwDeactivateButton.IsBusy)
                        bwDeactivateButton.RunWorkerAsync();

                    if (laserSyncWatch.IsRunning)
                    {
                        logger.LogError("wait until 6 seconds if Laser wasnt on before.", EventLogEntryType.Information);

                        do
                        {
                        } while (laserSyncWatch.ElapsedMilliseconds < 6000);
                    }

                    logger.LogError("Starting Seek Focus.", EventLogEntryType.Information);

                    bwSeekDistance.RunWorkerAsync();

                    btnFocusControl.Text = "Cancel Focus Control";
                    btnFocusControl.ForeColor = Color.Red;

                    System.Threading.Thread.Sleep(3000);
                }
                else if (laserOn == true && bwSeekDistance.IsBusy)
                {
                    logger.LogError("btnFocusControl_Click. laseron true. bwSeekDistance IS busy. Cancelling Seek and resetting its button.", EventLogEntryType.Information);

                    bwSeekDistance.CancelAsync();

                    btnFocusControl.Text = "Start Focus Control";
                    btnFocusControl.ForeColor = Color.Black;

                    System.Threading.Thread.Sleep(3000);
                }
            }
        }

        void bwSeekWaitButton_DoWork(object sender, DoWorkEventArgs e)
        {
            SeekWaitButton();
        }

        void bwCancelFocus_DoWork(object sender, DoWorkEventArgs e)
        {
            CancelButton();
        }

        void bwDeactivateButton_DoWork(object sender, DoWorkEventArgs e)
        {
            DeactivateButton();
        }

        void bwWaitButton_DoWork(object sender, DoWorkEventArgs e)
        {
            WaitButton();
        }

        private void RangeFinder_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Cancel all background workers and turn off laser range finder on form close. Set parent bar values to current ones
            if (!bwSeekDistance.IsBusy)
            {
            }
            else
            {
                bwSeekDistance.CancelAsync();

                btnFocusControl.Text = "Start Focus Control";
                btnFocusControl.ForeColor = Color.Black;

                Application.DoEvents();
            }

            if (bwActivateLaser.IsBusy)
            {
                bwActivateLaser.CancelAsync();

                Application.DoEvents();
            }

            TurnOffLaser();
            laserOn = false;
            rangeFinder.close_Device();

            btnActivate.Text = "Activate RangeFinder";
            btnActivate.ForeColor = Color.Black;
            txtMeasurement.Text = string.Empty;

            parentForm.FocusValue = tbFocus.Value;
            parentForm.IrisValue = tbIris.Value;
        }

        private void rdoSonar_CheckedChanged(object sender, EventArgs e)
        {
            //Sets boolean influencing code paths of laser or sonar rangefinding
            if (rdoSonar.Checked)
            {
                rdoLaser.Checked = false;
                isLaser = false;
                isSonar = true;
                sonarRangeFinder.SonarDataReceived += new SonarRangeFinder.SonarDataReceivedEventHandler(sonarRangeFinder_SonarDataReceived);
                if (!sonarRangeFinder.Open())
                {
                    rdoLaser.Checked = true;
                    rdoSonar.Checked = false;

                    FindAndMoveMsgBox("Sonar RangeFinder Not Detected", this.Location.X + this.Height / 2, this.Location.Y + this.Width / 8);
                    MessageBox.Show(this, "Please connect the sonar rangefinder.", "Sonar RangeFinder Not Detected");
                }
            }
        }

        void sonarRangeFinder_SonarDataReceived(object sender, SonarDataReceivedEventArgs e)
        {
        }

        private void rdoLaser_CheckedChanged(object sender, EventArgs e)
        {
            //Sets boolean influencing code paths of laser or sonar rangefinding
            if (rdoLaser.Checked)
            {
                rdoSonar.Checked = false;
                isLaser = true;
                isSonar = false;
            }
        }

        private void tbFocus_ValueChanged(object sender, EventArgs e)
        {
            //Use the bar changed logic from the parant form
            parentForm.tbFocus.Value = tbFocus.Value;

            if (encoderItems.EncoderItems.Count > 0)
            {
                try
                {
                    tbFocus.Value = parentForm.tbFocus.Value;

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

        private void tbIris_ValueChanged(object sender, EventArgs e)
        {
            //Use the bar changed logic from the parant form
            parentForm.tbIris.Value = tbIris.Value;

            if (parentForm.fStopList.Count > 1)
                lblIrisBarValue.Text = "f" + string.Format("{0:0.0}", parentForm.fStopList[tbIris.Value]);
        }

        private void btnSetOffset_Click(object sender, EventArgs e)
        {
            //Calculate the offset between the range finder and the lens distance, subtracting the lens distance from the rangefinder measurement
            focusOffset = Math.Round(measurementPreOffset - encoderDictionary[tbFocus.Value], 2);

            txtFocusOffset.Text = focusOffset.ToString() + "ft";
        }

        private void tbFocus_KeyDown(object sender, KeyEventArgs e)
        {
            parentForm.tbFocus_KeyDown(sender, e);
        }

        private void tbFocus_MouseDown(object sender, MouseEventArgs e)
        {
            parentForm.tbFocus_MouseDown(sender, e);
        }
    }
}
