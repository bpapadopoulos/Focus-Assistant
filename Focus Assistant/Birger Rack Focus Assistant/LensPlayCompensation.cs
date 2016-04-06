using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CanonCameraAppLib;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Birger_Rack_Focus_Assistant
{
    public partial class LensPlayCompensation : Form
    {
        frmFocusPuller grandParentForm;
        DSLRCalibration parentForm;
        bool liveView = true;
        int StartingFocusThumb = 0;
        int EndingFocusThumb = 0;
        int DriftedFocusThumb = 0;
        int rackNum = 0;
        BackgroundWorker bwLensPlaySim = new BackgroundWorker();
        double encoderDelta;
        double driftedEncoderDelta;
        double lensPlayCompensationRatio;
        bool beginSetDrifted = false;

        bool lensPlaySevenEigths = false;
        bool lensPlayThreeFourths = false;
        bool lensPlayMid = false;
        bool lensPlayQuarter = false;

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(String sClassName, String sAppName);
        [DllImport("user32.dll")]
        static extern IntPtr FindWindow(IntPtr classname, string title);

        public LensPlayCompensation(frmFocusPuller grandParentFormIn, DSLRCalibration parentFormIn)
        {
            InitializeComponent();

            grandParentForm = grandParentFormIn;
            parentForm = parentFormIn;
            parentForm.camera.OnShutdown += new ShutdownEventHandler(camera_OnShutdown);
            parentForm.camera.OnPropertyChanged += new CanonCameraAppLib.PropertyChangedEventHandler(camera_OnPropertyChanged);

            //Sets main form's benchmarking flag to true so persisted lensplay compensation is ignored
            grandParentForm.isBenchmarking = true;

            //Restarts the parent form's lens play manager background thread
            if (!grandParentForm.lensPlayManagerThread.IsAlive && grandParentForm.encoderItems.EncoderItems.Count > 1)
            {
                grandParentForm.lensPlayManagerThread = new Thread(grandParentForm.LensPlayManager);
                grandParentForm.lensPlayManagerThread.IsBackground = true;
                grandParentForm.lensPlayManagerThread.Start();
            }

            //Reset compensation counter on form load
            grandParentForm.compensationAdjustments = 0;

            bwLensPlaySim.DoWork += new DoWorkEventHandler(bwLensPlaySim_DoWork);
            bwLensPlaySim.WorkerSupportsCancellation = true;

            //Backgroud thread that tracks the number of racks and updates the label accordingly
            Thread rackNumMonitorThread = new Thread(RackNumMonitor);
            rackNumMonitorThread.IsBackground = true;
            rackNumMonitorThread.Start();

            //Backgroud thread that updates the lens play ratio based on its textbox
            Thread ratioMonitorThread = new Thread(RatioMonitor);
            ratioMonitorThread.IsBackground = true;
            ratioMonitorThread.Start();

            //Display the currently persisted lens play compensation ratio and store it in the local variable
            lensPlayCompensationRatio = grandParentForm.encoderItems.LensPlayCompensationRatio;
            //Set the main forms lens play compensation ratio to the one in the benchmark context
            grandParentForm.lensPlayCompensationRatio = lensPlayCompensationRatio;

            txtCompensationRatio.Text = lensPlayCompensationRatio.ToString();

            if (grandParentForm.lensPlayCompensationEnabled)
                rdoEnabled.Checked = true;
            else
                rdoDisabled.Checked = true;

            //Auto set userfriendly starting and ending focus, allowing for easy detection of lens play
            if (StartingFocusThumb == 0)
            {
                foreach (KeyValuePair<double, int> dist in grandParentForm.majorDistDictionary)
                {
                    if (dist.Key > 2.0)
                    {
                        StartingFocusThumb = dist.Value;

                        if (grandParentForm.encoderDictionary[StartingFocusThumb] > parentForm.hyperFocal)
                        {
                            txtSetStartFocus.Text = "\u221E";
                        }
                        else
                        {
                            txtSetStartFocus.Text = String.Format("{0:0.00}", grandParentForm.encoderDictionary[StartingFocusThumb]) + "ft";
                        }

                        btnSetEndFocus.Enabled = true;
                        btnJumpToStart.Enabled = true;

                        break;
                    }
                }
            }

            if (EndingFocusThumb == 0)
            {
                foreach (KeyValuePair<double, int> dist in grandParentForm.majorDistDictionary)
                {
                    if (dist.Key == parentForm.hyperFocal || dist.Key == Math.Round(parentForm.hyperFocal, 2))
                    {
                        foreach (KeyValuePair<double, int> feetDist in grandParentForm.feetDictionary)
                        {
                            if (feetDist.Key > dist.Key)
                            {
                                EndingFocusThumb = feetDist.Value + 1;
                                break;
                            }
                        }

                        if (grandParentForm.encoderDictionary[EndingFocusThumb] > parentForm.hyperFocal)
                        {
                            txtSetEndFocus.Text = "\u221E";
                        }
                        else
                        {
                            txtSetEndFocus.Text = String.Format("{0:0.00}", grandParentForm.encoderDictionary[EndingFocusThumb]) + "ft";
                        }

                        btnStartLensPlaySimulation.Enabled = true;
                        btnJumpToEnd.Enabled = true;

                        encoderDelta = Math.Abs((double)grandParentForm.trackbarDictionary[EndingFocusThumb] - grandParentForm.trackbarDictionary[StartingFocusThumb]);
                        lblEncoderDelta.Text = encoderDelta.ToString();

                        break;
                    }
                }
            }
        }

        void RatioMonitor()
        {
            while (1 == 1)
            {
                if (this.InvokeRequired)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        double result = 0;
                        if (double.TryParse(txtCompensationRatio.Text, out result))
                        {
                            lensPlayCompensationRatio = double.Parse(txtCompensationRatio.Text);
                            Thread.Sleep(25);
                            grandParentForm.lensPlayCompensationRatio = lensPlayCompensationRatio;
                        }
                    });
                }
                else
                {
                    double result = 0;
                    if (double.TryParse(txtCompensationRatio.Text, out result))
                    {
                        lensPlayCompensationRatio = double.Parse(txtCompensationRatio.Text);
                        Thread.Sleep(25);
                        grandParentForm.lensPlayCompensationRatio = lensPlayCompensationRatio;
                    }
                }

                Thread.Sleep(3000);
                Application.DoEvents();
            }
        }

        void RackNumMonitor()
        {
            while (1 == 1)
            {
                if (this.InvokeRequired)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        lblRackNum.Text = rackNum.ToString();
                    });
                }
                else
                {
                    try
                    {
                        lblRackNum.Text = rackNum.ToString();
                    }
                    catch (Exception ex)
                    {

                    }
                }

                Thread.Sleep(5000);
            }
        }

        void bwLensPlaySim_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                //If the simulation is starting from scratch or was reset, execute logic sending trackbar to beginning
                if (rackNum == 0)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        btnBeginSetDrifted.Enabled = false;
                    });

                    BeginInvoke((MethodInvoker)delegate
                    {
                        btnJumpToMinimum_Click(sender, e);
                    });

                    Thread.Sleep(10000);

                    Stopwatch stopWatchMin = new Stopwatch();
                    stopWatchMin.Start();
                    while (grandParentForm.commandQueue.Count > 0 && stopWatchMin.ElapsedMilliseconds < 5000)
                    {

                    }

                    Thread.Sleep(250);

                    BeginInvoke((MethodInvoker)delegate
                    {
                        btnJumpToStart_Click(sender, e);
                    });

                    Thread.Sleep(250);

                    Stopwatch stopWatchStart = new Stopwatch();
                    stopWatchStart.Start();
                    while (grandParentForm.commandQueue.Count > 0 && stopWatchStart.ElapsedMilliseconds < 5000)
                    {

                    }

                    Thread.Sleep(3000);
                }

                do
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        btnJumpToEnd_Click(sender, e);
                    });

                    rackNum++;

                    if (rackNum % 25 == 0)
                    {
                        bwLensPlaySim.CancelAsync();

                        FindAndMoveMsgBox("Simulation Paused", this.Location.X + this.Height / 2, this.Location.Y + this.Width / 8);
                        BeginInvoke((MethodInvoker)delegate
                        {
                            btnStartLensPlaySimulation.Text = "Start Lens Play Simulation";
                            System.Media.SystemSounds.Exclamation.Play();
                            MessageBox.Show(this, "Simulation paused at " + rackNum + " focus racks.", "Simulation Paused");
                        });
                    }

                    Thread.Sleep(250);

                    Stopwatch stopWatchEnd = new Stopwatch();
                    stopWatchEnd.Start();
                    while (grandParentForm.commandQueue.Count > 0 && stopWatchEnd.ElapsedMilliseconds < 5000)
                    {

                    }

                    Thread.Sleep(3000);

                    BeginInvoke((MethodInvoker)delegate
                    {
                        btnJumpToStart_Click(sender, e);
                    });

                    Thread.Sleep(250);

                    Stopwatch stopWatchStart = new Stopwatch();
                    stopWatchStart.Start();
                    while (grandParentForm.commandQueue.Count > 0 && stopWatchStart.ElapsedMilliseconds < 5000)
                    {

                    }

                    Thread.Sleep(3000);

                } while (!bwLensPlaySim.CancellationPending);

                BeginInvoke((MethodInvoker)delegate
                {
                    btnBeginSetDrifted.Enabled = true;
                });

                if (bwLensPlaySim.CancellationPending)
                {
                    e.Cancel = true;
                }
            }
            catch (Exception ex)
            {

            }
        }

        public void camera_OnPropertyChanged(Camera sender, CanonCameraAppLib.PropertyChangedEventArgs e)
        {
            //Detects if liveview is lost
            if (parentForm.camera.IsInLiveViewMode)
            {
                liveView = true;
            }
            else
            {
                liveView = false;
                grandParentForm.camera_OnPropertyChanged(sender, e);

                //Checks if liveview is still false 5 seconds from now, and closes the form if so
                Thread thr = new Thread(() => // create a new thread
                {
                    if (this.InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            Thread.Sleep(8000);
                            if (!parentForm.camera.IsInLiveViewMode)
                                this.Close();
                        });
                    }
                    else
                    {
                        Thread.Sleep(8000);
                        if (!parentForm.camera.IsInLiveViewMode)
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
            grandParentForm.camera_OnShutdown(sender, e);
            this.Close();
        }

        private void tbFocus_ValueChanged(object sender, EventArgs e)
        {
            //Use the bar changed logic from the parant form
            grandParentForm.tbFocus.Value = tbFocus.Value;

            if (grandParentForm.encoderItems.EncoderItems.Count > 0)
            {
                try
                {
                    tbFocus.Value = grandParentForm.tbFocus.Value;

                    if (grandParentForm.encoderDictionary[tbFocus.Value] > parentForm.hyperFocal)
                    {
                        lblTrackbarValue.Text = "\u221E";
                    }
                    else
                    {
                        lblTrackbarValue.Text = String.Format("{0:0.00}", grandParentForm.encoderDictionary[tbFocus.Value]) + "ft";
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }

        private void tbFocus_KeyDown(object sender, KeyEventArgs e)
        {
            grandParentForm.tbFocus_KeyDown(sender, e);
            lensPlaySevenEigths = false;
            lensPlayThreeFourths = false;
            lensPlayMid = false;
            lensPlayQuarter = false;
            txtCompensationRatio.Text = lensPlayCompensationRatio.ToString();
        }

        private void tbFocus_MouseDown(object sender, MouseEventArgs e)
        {
            grandParentForm.tbFocus_MouseDown(sender, e);
            lensPlaySevenEigths = false;
            lensPlayThreeFourths = false;
            lensPlayMid = false;
            lensPlayQuarter = false;
            txtCompensationRatio.Text = lensPlayCompensationRatio.ToString();
        }

        private void btnSetStartFocus_Click(object sender, EventArgs e)
        {
            txtSetStartFocus.Text = lblTrackbarValue.Text;

            StartingFocusThumb = tbFocus.Value;

            btnSetEndFocus.Enabled = true;
            btnJumpToStart.Enabled = true;
            btnSave.Enabled = false;

            encoderDelta = Math.Abs((double)grandParentForm.trackbarDictionary[EndingFocusThumb] - grandParentForm.trackbarDictionary[StartingFocusThumb]);
            lblEncoderDelta.Text = encoderDelta.ToString();
        }

        private void btnSetEndFocus_Click(object sender, EventArgs e)
        {
            txtSetEndFocus.Text = lblTrackbarValue.Text;

            EndingFocusThumb = tbFocus.Value;

            btnStartLensPlaySimulation.Enabled = true;
            btnJumpToEnd.Enabled = true;
            btnSave.Enabled = false;

            encoderDelta = Math.Abs((double)grandParentForm.trackbarDictionary[EndingFocusThumb] - grandParentForm.trackbarDictionary[StartingFocusThumb]);
            lblEncoderDelta.Text = encoderDelta.ToString();
        }

        private void btnStartLensPlaySimulation_Click(object sender, EventArgs e)
        {
            if (bwLensPlaySim.IsBusy)
            {
                btnStartLensPlaySimulation.Text = "Start Lens Play Simulation";
                bwLensPlaySim.CancelAsync();
            }
            else
            {
                btnSave.Enabled = false;
                btnStartLensPlaySimulation.Text = "Pause Lens Play Simulation";
                bwLensPlaySim.RunWorkerAsync();
            }
        }

        private void btnSetDriftedFocus_Click(object sender, EventArgs e)
        {
            txtSetStartFocusDrift.Text = lblTrackbarValue.Text;

            DriftedFocusThumb = tbFocus.Value;

            btnCompensationRatio.Enabled = true;

            driftedEncoderDelta = grandParentForm.trackbarDictionary[DriftedFocusThumb] - grandParentForm.trackbarDictionary[StartingFocusThumb];
            lblEncoderDrift.Text = driftedEncoderDelta.ToString();
        }

        private void btnCompensationRatio_Click(object sender, EventArgs e)
        {
            double encoderDrift = driftedEncoderDelta / rackNum;

            if (lensPlaySevenEigths)
            {
                grandParentForm.encoderItems.LensPlaySevenEigths = grandParentForm.encoderItems.LensPlaySevenEigths + (encoderDrift / encoderDelta);
                grandParentForm.encoderItems.LensPlayOneEighth = grandParentForm.encoderItems.LensPlayQuarter;
            }
            else if (lensPlayThreeFourths)
            {
                grandParentForm.encoderItems.LensPlayThreeFourths = grandParentForm.encoderItems.LensPlayThreeFourths + (encoderDrift / encoderDelta);
            }
            else if (lensPlayMid)
            {
                grandParentForm.encoderItems.LensPlayMid = grandParentForm.encoderItems.LensPlayMid + (encoderDrift / encoderDelta);
            }
            else if (lensPlayQuarter)
            {
                grandParentForm.encoderItems.LensPlayQuarter = grandParentForm.encoderItems.LensPlayQuarter + (encoderDrift / encoderDelta);
                grandParentForm.encoderItems.LensPlayOneEighth = grandParentForm.encoderItems.LensPlayQuarter;
            }
            else
            {
                lensPlayCompensationRatio = lensPlayCompensationRatio + (encoderDrift / encoderDelta);
                grandParentForm.encoderItems.LensPlayCompensationRatio = lensPlayCompensationRatio;
                txtCompensationRatio.Text = lensPlayCompensationRatio.ToString();
            }

            btnSave.Enabled = true;
            btnCompensationRatio.Enabled = false;
        }

        private void btnJumpToMinimum_Click(object sender, EventArgs e)
        {
            int currentThumb = 0;

            currentThumb = tbFocus.Value;

            //Drag thumb to start position if not already there
            if (currentThumb != tbFocus.Minimum)
            {
                tbFocus.Value = tbFocus.Minimum;
            }
        }

        private void btnJumpToStart_Click(object sender, EventArgs e)
        {
            int currentThumb = 0;

            currentThumb = tbFocus.Value;

            //Drag thumb to start position if not already there
            if (currentThumb != StartingFocusThumb)
            {
                tbFocus.Value = StartingFocusThumb;
            }
        }

        private void btnJumpToEnd_Click(object sender, EventArgs e)
        {
            int currentThumb = 0;

            currentThumb = tbFocus.Value;

            //Drag thumb to start position if not already there
            if (currentThumb != EndingFocusThumb)
            {
                tbFocus.Value = EndingFocusThumb;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            parentForm.ExecuteSave();
            this.Close();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            FindAndMoveMsgBox("Delete Ratio", this.Location.X + this.Height / 2, this.Location.Y + this.Width / 8);
            if (MessageBox.Show(this, "Delete Existing Compensation Ratio and start from scratch?", "Delete Ratio", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                if (lensPlaySevenEigths)
                {
                    grandParentForm.encoderItems.LensPlaySevenEigths = 0;
                    grandParentForm.encoderItems.LensPlayOneEighth = 0;

                    txtCompensationRatio.Text = grandParentForm.encoderItems.LensPlaySevenEigths.ToString();
                    btnSave.Enabled = true;
                    btnSetDriftedFocus.Enabled = false;
                    btnCompensationRatio.Enabled = false;
                    DriftedFocusThumb = 0;
                    rackNum = 0;
                    txtSetStartFocusDrift.Text = string.Empty;
                }
                else if (lensPlayThreeFourths)
                {
                    grandParentForm.encoderItems.LensPlayThreeFourths = 0;

                    txtCompensationRatio.Text = grandParentForm.encoderItems.LensPlayThreeFourths.ToString();
                    btnSave.Enabled = true;
                    btnSetDriftedFocus.Enabled = false;
                    btnCompensationRatio.Enabled = false;
                    DriftedFocusThumb = 0;
                    rackNum = 0;
                    txtSetStartFocusDrift.Text = string.Empty;
                }
                else if (lensPlayMid)
                {
                    grandParentForm.encoderItems.LensPlayMid = 0;

                    txtCompensationRatio.Text = grandParentForm.encoderItems.LensPlayMid.ToString();
                    btnSave.Enabled = true;
                    btnSetDriftedFocus.Enabled = false;
                    btnCompensationRatio.Enabled = false;
                    DriftedFocusThumb = 0;
                    rackNum = 0;
                    txtSetStartFocusDrift.Text = string.Empty;
                }
                else if (lensPlayQuarter)
                {
                    grandParentForm.encoderItems.LensPlayQuarter = 0;
                    grandParentForm.encoderItems.LensPlayOneEighth = 0;

                    txtCompensationRatio.Text = grandParentForm.encoderItems.LensPlayQuarter.ToString();
                    btnSave.Enabled = true;
                    btnSetDriftedFocus.Enabled = false;
                    btnCompensationRatio.Enabled = false;
                    DriftedFocusThumb = 0;
                    rackNum = 0;
                    txtSetStartFocusDrift.Text = string.Empty;
                }
                else
                {
                    grandParentForm.encoderItems.LensPlayCompensationRatio = 0;
                    lensPlayCompensationRatio = grandParentForm.encoderItems.LensPlayCompensationRatio;

                    txtCompensationRatio.Text = lensPlayCompensationRatio.ToString();
                    btnSave.Enabled = true;
                    btnSetDriftedFocus.Enabled = false;
                    btnCompensationRatio.Enabled = false;
                    DriftedFocusThumb = 0;
                    rackNum = 0;
                    txtSetStartFocusDrift.Text = string.Empty;
                }
            }
        }

        void FindAndMoveMsgBox(string title, int x, int y)
        {
            //Moves a messagebox to the desired position
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

        private void btnBeginSetDrifted_Click(object sender, EventArgs e)
        {
            if (!beginSetDrifted)
            {
                tbFocus.Value = tbFocus.Minimum;
                beginSetDrifted = true;
                btnJumpToEnd.Enabled = false;
                btnJumpToStart.Enabled = false;
                btnSetEndFocus.Enabled = false;
                btnSetStartFocus.Enabled = false;
                btnStartLensPlaySimulation.Enabled = false;

                btnSetDriftedFocus.Enabled = true;
                btnBeginSetDrifted.Text = "Back to Simulation";
            }
            else
            {
                tbFocus.Value = tbFocus.Minimum;
                DriftedFocusThumb = 0;
                rackNum = 0;
                txtSetStartFocusDrift.Text = string.Empty;
                lblEncoderDrift.Text = string.Empty;

                beginSetDrifted = false;
                btnSetDriftedFocus.Enabled = false;
                btnCompensationRatio.Enabled = false;
                btnSave.Enabled = false;

                btnJumpToEnd.Enabled = true;
                btnJumpToStart.Enabled = true;
                btnSetEndFocus.Enabled = true;
                btnSetStartFocus.Enabled = true;
                btnStartLensPlaySimulation.Enabled = true;
                btnBeginSetDrifted.Text = "Begin Setting Drifted";
            }
        }

        private void rdoEnabled_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoEnabled.Checked)
                grandParentForm.lensPlayCompensationEnabled = true;
        }

        private void rdoDisabled_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoDisabled.Checked)
                grandParentForm.lensPlayCompensationEnabled = false;
        }

        private void btnStep1_Click(object sender, EventArgs e)
        {
            lensPlaySevenEigths = false;
            lensPlayThreeFourths = false;
            lensPlayMid = false;
            lensPlayQuarter = true;

            int maxEncoderValue = grandParentForm.encoderItems.EncoderItems[grandParentForm.encoderItems.EncoderItems.Count - 1].Encoder;
            int midEncoderValue = maxEncoderValue / 2;
            int quarterEncoderValue = midEncoderValue / 2;
            int threeFourthsEncoderValue = midEncoderValue + ((maxEncoderValue - midEncoderValue) / 2);
            int oneEighthsEncoderValue = quarterEncoderValue / 2;
            int sevenEighthsEncoderValue = threeFourthsEncoderValue + oneEighthsEncoderValue;

            //Set oneEighthsEncoderValue as start focus
            tbFocus.Value = grandParentForm.encoderItems.EncoderItems.Find(delegate(EncoderItem ei) { return ei.Encoder == oneEighthsEncoderValue; }).TrackBarInt;

            txtSetStartFocus.Text = lblTrackbarValue.Text;
            Thread.Sleep(1000);
            StartingFocusThumb = tbFocus.Value;

            btnSetEndFocus.Enabled = true;
            btnJumpToStart.Enabled = true;
            btnSave.Enabled = false;

            encoderDelta = Math.Abs((double)grandParentForm.trackbarDictionary[EndingFocusThumb] - grandParentForm.trackbarDictionary[StartingFocusThumb]);
            lblEncoderDelta.Text = encoderDelta.ToString();

            Thread.Sleep(2000);
            Stopwatch stopWatchStep1 = new Stopwatch();
            stopWatchStep1.Start();
            while (grandParentForm.commandQueue.Count > 0 && stopWatchStep1.ElapsedMilliseconds < 5000)
            {

            }
            Thread.Sleep(250);

            //Set quarterEncoderValue as start focus
            tbFocus.Value = grandParentForm.encoderItems.EncoderItems.Find(delegate(EncoderItem ei) { return ei.Encoder == quarterEncoderValue; }).TrackBarInt;

            txtSetEndFocus.Text = lblTrackbarValue.Text;
            Thread.Sleep(1000);
            EndingFocusThumb = tbFocus.Value;

            btnStartLensPlaySimulation.Enabled = true;
            btnJumpToEnd.Enabled = true;
            btnSave.Enabled = false;

            encoderDelta = Math.Abs((double)grandParentForm.trackbarDictionary[EndingFocusThumb] - grandParentForm.trackbarDictionary[StartingFocusThumb]);
            lblEncoderDelta.Text = encoderDelta.ToString();
            txtCompensationRatio.Text = grandParentForm.encoderItems.LensPlayQuarter.ToString();
        }

        private void btnStep2_Click(object sender, EventArgs e)
        {
            lensPlaySevenEigths = false;
            lensPlayThreeFourths = false;
            lensPlayMid = true;
            lensPlayQuarter = false;

            int maxEncoderValue = grandParentForm.encoderItems.EncoderItems[grandParentForm.encoderItems.EncoderItems.Count - 1].Encoder;
            int midEncoderValue = maxEncoderValue / 2;
            int quarterEncoderValue = midEncoderValue / 2;
            int threeFourthsEncoderValue = midEncoderValue + ((maxEncoderValue - midEncoderValue) / 2);
            int oneEighthsEncoderValue = quarterEncoderValue / 2;
            int sevenEighthsEncoderValue = threeFourthsEncoderValue + oneEighthsEncoderValue;

            //Set quarterEncoderValue as start focus
            tbFocus.Value = grandParentForm.encoderItems.EncoderItems.Find(delegate(EncoderItem ei) { return ei.Encoder == quarterEncoderValue; }).TrackBarInt;

            txtSetStartFocus.Text = lblTrackbarValue.Text;
            Thread.Sleep(1000);
            StartingFocusThumb = tbFocus.Value;

            btnSetEndFocus.Enabled = true;
            btnJumpToStart.Enabled = true;
            btnSave.Enabled = false;

            encoderDelta = Math.Abs((double)grandParentForm.trackbarDictionary[EndingFocusThumb] - grandParentForm.trackbarDictionary[StartingFocusThumb]);
            lblEncoderDelta.Text = encoderDelta.ToString();

            Thread.Sleep(2000);
            Stopwatch stopWatchStep2 = new Stopwatch();
            stopWatchStep2.Start();
            while (grandParentForm.commandQueue.Count > 0 && stopWatchStep2.ElapsedMilliseconds < 5000)
            {

            }
            Thread.Sleep(250);

            //Set midEncoderValue as start focus
            tbFocus.Value = grandParentForm.encoderItems.EncoderItems.Find(delegate(EncoderItem ei) { return ei.Encoder == midEncoderValue; }).TrackBarInt;

            txtSetEndFocus.Text = lblTrackbarValue.Text;
            Thread.Sleep(1000);
            EndingFocusThumb = tbFocus.Value;

            btnStartLensPlaySimulation.Enabled = true;
            btnJumpToEnd.Enabled = true;
            btnSave.Enabled = false;

            encoderDelta = Math.Abs((double)grandParentForm.trackbarDictionary[EndingFocusThumb] - grandParentForm.trackbarDictionary[StartingFocusThumb]);
            lblEncoderDelta.Text = encoderDelta.ToString();
            txtCompensationRatio.Text = grandParentForm.encoderItems.LensPlayMid.ToString();
        }

        private void btnStep3_Click(object sender, EventArgs e)
        {
            lensPlaySevenEigths = false;
            lensPlayThreeFourths = true;
            lensPlayMid = false;
            lensPlayQuarter = false;

            int maxEncoderValue = grandParentForm.encoderItems.EncoderItems[grandParentForm.encoderItems.EncoderItems.Count - 1].Encoder;
            int midEncoderValue = maxEncoderValue / 2;
            int quarterEncoderValue = midEncoderValue / 2;
            int threeFourthsEncoderValue = midEncoderValue + ((maxEncoderValue - midEncoderValue) / 2);
            int oneEighthsEncoderValue = quarterEncoderValue / 2;
            int sevenEighthsEncoderValue = threeFourthsEncoderValue + oneEighthsEncoderValue;

            //Set midEncoderValue as start focus
            tbFocus.Value = grandParentForm.encoderItems.EncoderItems.Find(delegate(EncoderItem ei) { return ei.Encoder == midEncoderValue; }).TrackBarInt;

            txtSetStartFocus.Text = lblTrackbarValue.Text;
            Thread.Sleep(1000);
            StartingFocusThumb = tbFocus.Value;

            btnSetEndFocus.Enabled = true;
            btnJumpToStart.Enabled = true;
            btnSave.Enabled = false;

            encoderDelta = Math.Abs((double)grandParentForm.trackbarDictionary[EndingFocusThumb] - grandParentForm.trackbarDictionary[StartingFocusThumb]);
            lblEncoderDelta.Text = encoderDelta.ToString();

            Thread.Sleep(2000);
            Stopwatch stopWatchStep3 = new Stopwatch();
            stopWatchStep3.Start();
            while (grandParentForm.commandQueue.Count > 0 && stopWatchStep3.ElapsedMilliseconds < 5000)
            {

            }
            Thread.Sleep(250);

            //Set threeFourthsEncoderValue as start focus
            tbFocus.Value = grandParentForm.encoderItems.EncoderItems.Find(delegate(EncoderItem ei) { return ei.Encoder == threeFourthsEncoderValue; }).TrackBarInt;

            txtSetEndFocus.Text = lblTrackbarValue.Text;
            Thread.Sleep(1000);
            EndingFocusThumb = tbFocus.Value;

            btnStartLensPlaySimulation.Enabled = true;
            btnJumpToEnd.Enabled = true;
            btnSave.Enabled = false;

            encoderDelta = Math.Abs((double)grandParentForm.trackbarDictionary[EndingFocusThumb] - grandParentForm.trackbarDictionary[StartingFocusThumb]);
            lblEncoderDelta.Text = encoderDelta.ToString();
            txtCompensationRatio.Text = grandParentForm.encoderItems.LensPlayThreeFourths.ToString();
        }

        private void btnStep4_Click(object sender, EventArgs e)
        {
            lensPlaySevenEigths = true;
            lensPlayThreeFourths = false;
            lensPlayMid = false;
            lensPlayQuarter = false;

            int maxEncoderValue = grandParentForm.encoderItems.EncoderItems[grandParentForm.encoderItems.EncoderItems.Count - 1].Encoder;
            int midEncoderValue = maxEncoderValue / 2;
            int quarterEncoderValue = midEncoderValue / 2;
            int threeFourthsEncoderValue = midEncoderValue + ((maxEncoderValue - midEncoderValue) / 2);
            int oneEighthsEncoderValue = quarterEncoderValue / 2;
            int sevenEighthsEncoderValue = threeFourthsEncoderValue + oneEighthsEncoderValue;

            //Set threeFourthsEncoderValue as start focus
            tbFocus.Value = grandParentForm.encoderItems.EncoderItems.Find(delegate(EncoderItem ei) { return ei.Encoder == threeFourthsEncoderValue; }).TrackBarInt;

            txtSetStartFocus.Text = lblTrackbarValue.Text;
            Thread.Sleep(1000);
            StartingFocusThumb = tbFocus.Value;

            btnSetEndFocus.Enabled = true;
            btnJumpToStart.Enabled = true;
            btnSave.Enabled = false;

            encoderDelta = Math.Abs((double)grandParentForm.trackbarDictionary[EndingFocusThumb] - grandParentForm.trackbarDictionary[StartingFocusThumb]);
            lblEncoderDelta.Text = encoderDelta.ToString();

            Thread.Sleep(2000);
            Stopwatch stopWatchStep4 = new Stopwatch();
            stopWatchStep4.Start();
            while (grandParentForm.commandQueue.Count > 0 && stopWatchStep4.ElapsedMilliseconds < 5000)
            {

            }
            Thread.Sleep(250);

            //Set sevenEighthsEncoderValue as start focus
            tbFocus.Value = grandParentForm.encoderItems.EncoderItems.Find(delegate(EncoderItem ei) { return ei.Encoder == sevenEighthsEncoderValue; }).TrackBarInt;

            txtSetEndFocus.Text = lblTrackbarValue.Text;
            Thread.Sleep(1000);
            EndingFocusThumb = tbFocus.Value;

            btnStartLensPlaySimulation.Enabled = true;
            btnJumpToEnd.Enabled = true;
            btnSave.Enabled = false;

            encoderDelta = Math.Abs((double)grandParentForm.trackbarDictionary[EndingFocusThumb] - grandParentForm.trackbarDictionary[StartingFocusThumb]);
            lblEncoderDelta.Text = encoderDelta.ToString();
            txtCompensationRatio.Text = grandParentForm.encoderItems.LensPlaySevenEigths.ToString();
        }

        private void chkUseSavedCompensation_CheckedChanged(object sender, EventArgs e)
        {
            if (chkUseSavedCompensation.Checked)
            {
                grandParentForm.isBenchmarking = false;
            }
            else
            {
                grandParentForm.isBenchmarking = true;
            }
        }
    }
}
