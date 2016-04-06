using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CanonCameraAppLib;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using System.IO;

namespace Birger_Rack_Focus_Assistant
{
    public partial class DSLRCalibration : Form
    {
        public Camera camera;
        Stopwatch stopWatch = new Stopwatch();
        frmFocusPuller parentForm;
        int stepCounter = 0;
        int smallStepCounter = 0;
        Dictionary<int, double> calibrationDictionary = new Dictionary<int, double>();
        bool formClosing = false;
        bool saved = false;
        bool singleStep = false;
        int smallDelay = 2000;
        int medDelay = 2000;
        int lrgDelay = 2000;
        bool liveView = true;
        public EncoderItemList encoderItems = new EncoderItemList();
        List<EncoderItem> encoderList = new List<EncoderItem>();
        string lensDirectory = Environment.CurrentDirectory + "\\LensInfo";
        double largeRatio = 0;
        double mediumRatio = 0;

        bool smallCalc = false;
        bool mediumCalc = false;
        bool largeCalc = false;

        public double hyperFocal;


        BackgroundWorker bgWorkerFar1 = new BackgroundWorker();
        BackgroundWorker bgWorkerFar2 = new BackgroundWorker();
        BackgroundWorker bgWorkerFar3 = new BackgroundWorker();

        BackgroundWorker bgWorkerNear1 = new BackgroundWorker();
        BackgroundWorker bgWorkerNear2 = new BackgroundWorker();
        BackgroundWorker bgWorkerNear3 = new BackgroundWorker();

        BackgroundWorker bgWorkerStepCount = new BackgroundWorker();

        Stopwatch smallStepWatch = new Stopwatch();

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(String sClassName, String sAppName);
        [DllImport("user32.dll")]
        static extern IntPtr FindWindow(IntPtr classname, string title);

        public DSLRCalibration(frmFocusPuller parentFormIn, Camera cameraIn, double hyperFocalIn, EncoderItemList encoderItemsIn)
        {
            InitializeComponent();

            //Start smallstep watcher thread with enables buttons as needed
            Thread smallStepThread = new Thread(SmallStepEnable);
            smallStepThread.IsBackground = true;
            smallStepThread.Start();

            //Start delay watcher thread which sets delay between lens commands
            Thread getDelaysThread = new Thread(GetDelays);
            getDelaysThread.IsBackground = true;
            getDelaysThread.Start();

            btnInfinity.Text = "\u221E";
            btnInfinityMax.Text = "\u221E" + " " + "\u1DAC\u1D43\u02E3";

            parentForm = parentFormIn;
            camera = cameraIn;
            camera.OnShutdown += new ShutdownEventHandler(camera_OnShutdown);
            camera.OnPropertyChanged += new CanonCameraAppLib.PropertyChangedEventHandler(camera_OnPropertyChanged);
            hyperFocal = hyperFocalIn;
            encoderItems = encoderItemsIn;

            //Allow lens play compensation benchmarking if encoder dictionary is loaded in parent form
            if (parentForm.encoderDictionaryLoaded)
                btnLensPlayComp.Enabled = true;
            else
                btnLensPlayComp.Enabled = false;

            stopWatch.Start();

            //Start enable buttons thread
            Thread stopWatchThread = new Thread(EnableButtons);
            stopWatchThread.IsBackground = true;
            stopWatchThread.Start();

            //Button click background workers
            bgWorkerFar1.DoWork += new DoWorkEventHandler(bgWorkerFar1_DoWork);
            bgWorkerFar1.WorkerSupportsCancellation = true;
            bgWorkerFar2.DoWork += new DoWorkEventHandler(bgWorkerFar2_DoWork);
            bgWorkerFar2.WorkerSupportsCancellation = true;
            bgWorkerFar3.DoWork += new DoWorkEventHandler(bgWorkerFar3_DoWork);
            bgWorkerFar3.WorkerSupportsCancellation = true;

            bgWorkerNear1.DoWork += new DoWorkEventHandler(bgWorkerNear1_DoWork);
            bgWorkerNear1.WorkerSupportsCancellation = true;
            bgWorkerNear2.DoWork += new DoWorkEventHandler(bgWorkerNear2_DoWork);
            bgWorkerNear2.WorkerSupportsCancellation = true;
            bgWorkerNear3.DoWork += new DoWorkEventHandler(bgWorkerNear3_DoWork);
            bgWorkerNear3.WorkerSupportsCancellation = true;

            bgWorkerStepCount.DoWork += new DoWorkEventHandler(bgWorkerStepCount_DoWork);
            bgWorkerStepCount.WorkerSupportsCancellation = true;
        }

        void GetDelays()
        {
            //Background thread that should probably be deprecated now. Used to check text boxes for delay times
            while (1 == 1)
            {
                Thread.Sleep(75);

                smallDelay = encoderItems.SmallDelay * 5;
                medDelay = encoderItems.MedDelay * 5;
                lrgDelay = encoderItems.LrgDelay * 5;
            }
        }

        void SmallStepEnable()
        {
            //Checks if smallStepCounter is no longer zero and enables buttons accordingly
            while (1 == 1)
            {
                if (smallCalc && smallStepCounter < 1)
                {
                    Thread.Sleep(2000);
                    if (this.InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                            {
                                if (bgWorkerNear1.IsBusy)
                                {
                                    bgWorkerNear1.CancelAsync();
                                }

                                if (smallStepWatch.ElapsedMilliseconds < 2000)
                                {
                                    btnNear1.Enabled = false;
                                    btnFar1.Enabled = false;
                                }

                                if (rdoMedium.Checked)
                                {
                                    btnFar2.Enabled = true;
                                    btnNear2.Enabled = true;
                                }

                                if (rdoLarge.Checked)
                                {
                                    btnFar3.Enabled = true;
                                    btnNear3.Enabled = true;
                                }
                            });

                        Application.DoEvents();
                    }
                    else
                    {
                        if (bgWorkerNear1.IsBusy)
                        {
                            bgWorkerNear1.CancelAsync();
                        }

                        if (smallStepWatch.ElapsedMilliseconds < 2000)
                        {
                            if (stopWatch.ElapsedMilliseconds > smallDelay)
                            {
                                btnNear1.Enabled = false;
                                btnFar1.Enabled = false;
                            }
                        }

                        if (rdoMedium.Checked)
                        {
                            btnFar2.Enabled = true;
                            btnNear2.Enabled = true;
                        }

                        if (rdoLarge.Checked)
                        {
                            btnFar3.Enabled = true;
                            btnNear3.Enabled = true;
                        }
                    }
                }
                else if (smallCalc && smallStepCounter > 0)
                {
                    Thread.Sleep(75);
                    if (this.InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            if (stopWatch.ElapsedMilliseconds > smallDelay)
                                btnNear1.Enabled = true;
                            if (smallStepCounter > 0)
                            {
                                if (stopWatch.ElapsedMilliseconds > smallDelay)
                                    btnFar1.Enabled = true;
                                btnFar2.Enabled = false;
                                btnNear2.Enabled = false;
                                btnFar3.Enabled = false;
                                btnNear3.Enabled = false;
                            }
                        });
                    }
                    else
                    {
                        if (stopWatch.ElapsedMilliseconds > smallDelay)
                            btnNear1.Enabled = true;
                        if (smallStepCounter > 0)
                        {
                            if (stopWatch.ElapsedMilliseconds > smallDelay)
                                btnFar1.Enabled = true;
                            btnFar2.Enabled = false;
                            btnNear2.Enabled = false;
                            btnFar3.Enabled = false;
                            btnNear3.Enabled = false;
                        }
                    }
                }

                if (smallStepWatch.ElapsedMilliseconds > 2000)
                {
                    if (this.InvokeRequired)
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            if (stopWatch.ElapsedMilliseconds > smallDelay)
                                btnFar1.Enabled = true;
                            if (smallStepCounter > 0)
                            {
                                if (stopWatch.ElapsedMilliseconds > smallDelay)
                                    btnNear1.Enabled = true;
                            }
                        });
                    }
                    else
                    {
                        if (stopWatch.ElapsedMilliseconds > smallDelay)
                            btnFar1.Enabled = true;
                        if (smallStepCounter > 0)
                        {
                            if (stopWatch.ElapsedMilliseconds > smallDelay)
                                btnNear1.Enabled = true;
                        }
                    }
                }
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
                this.Close();
            }
        }

        void bgWorkerStepCount_DoWork(object sender, DoWorkEventArgs e)
        {
            //Updates the stepcounter textbox with either the stepcounter or smallstepcounter value
            do
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    if (smallStepCounter > 0)
                    {
                        txtStepCounter.Text = smallStepCounter.ToString();
                    }
                    else
                    {
                        txtStepCounter.Text = stepCounter.ToString();
                    }
                });

                Thread.Sleep(500);
            } while (!bgWorkerStepCount.CancellationPending);
        }

        void bgWorkerNear3_DoWork(object sender, DoWorkEventArgs e)
        {
            //Starts and stops rack burst
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            do
            {
                uint result;
                if (camera != null)
                {
                    result = camera.DriveLensNearThree();

                    if (stepCounter != 0)
                        stepCounter--;
                }

                Thread.Sleep(lrgDelay);

            } while (stopWatch.ElapsedMilliseconds < (lrgDelay * 5) && !bgWorkerNear3.CancellationPending);

            if (bgWorkerNear3.IsBusy)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    btnNear3.Text = "<<<";
                    if (!btnInfinity.Enabled)
                    {
                        btnFar3.Enabled = true;
                        btnNear3.Enabled = true;
                    }
                    btnNear3.ForeColor = Color.Black;
                });

                bgWorkerNear3.CancelAsync();
            }

            if (bgWorkerNear3.CancellationPending)
            {
                e.Cancel = true;
            }
        }

        void bgWorkerNear2_DoWork(object sender, DoWorkEventArgs e)
        {
            //Starts and stops rack burst
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            do
            {
                uint result;
                if (camera != null)
                {
                    result = camera.DriveLensNearTwo();

                    if (stepCounter != 0)
                        stepCounter--;
                }

                Thread.Sleep(medDelay);

            } while (stopWatch.ElapsedMilliseconds < (medDelay * 45) && !bgWorkerNear2.CancellationPending);

            if (bgWorkerNear2.IsBusy)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    btnNear2.Text = "<<";
                    if (btnInfinity.Enabled && !smallCalc)
                    {
                        btnFar2.Enabled = true;
                        btnNear2.Enabled = true;
                    }
                    btnNear2.ForeColor = Color.Black;
                });

                bgWorkerNear2.CancelAsync();
            }

            if (bgWorkerNear2.CancellationPending)
            {
                e.Cancel = true;
                return;
            }
        }

        void bgWorkerNear1_DoWork(object sender, DoWorkEventArgs e)
        {
            //Starts and stops rack burst
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            do
            {
                uint result;
                if (camera != null)
                {
                    result = camera.DriveLensNearOne();


                    if (smallCalc)
                    {
                        if (smallStepCounter != 0)
                            smallStepCounter--;
                    }
                    else
                    {
                        if (stepCounter != 0)
                            stepCounter--;
                    }
                }

                Thread.Sleep(smallDelay);

            } while (stopWatch.ElapsedMilliseconds < (smallDelay * 175)
                && !bgWorkerNear1.CancellationPending);

            if (bgWorkerNear1.IsBusy)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    btnNear1.Text = "<";
                    if (btnInfinity.Enabled && !smallCalc)
                    {
                        btnFar1.Enabled = true;
                        btnNear1.Enabled = true;
                    }
                    btnNear1.ForeColor = Color.Black;
                });

                bgWorkerNear1.CancelAsync();
            }

            if (bgWorkerNear1.CancellationPending)
            {
                e.Cancel = true;
            }
        }

        void bgWorkerFar3_DoWork(object sender, DoWorkEventArgs e)
        {
            //Starts and stops rack burst
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            //Deal with lens play
            if (stepCounter == 0)
            {
                do
                {
                    uint result;
                    if (camera != null)
                    {
                        result = camera.DriveLensFarThree();
                    }

                    Thread.Sleep(lrgDelay);
                } while (stopWatch.ElapsedMilliseconds < (lrgDelay * 3));

                stopWatch.Reset();
                stopWatch.Start();
                do
                {
                    uint result;
                    if (camera != null)
                    {
                        result = camera.DriveLensNearThree();
                    }

                    Thread.Sleep(lrgDelay);
                } while (stopWatch.ElapsedMilliseconds < (lrgDelay * 10));

                stopWatch.Reset();
                stopWatch.Start();
            }

            Thread.Sleep(lrgDelay);

            do
            {
                uint result;
                if (camera != null)
                {
                    result = camera.DriveLensFarThree();
                    stepCounter++;
                }

                Thread.Sleep(lrgDelay);

            } while (stopWatch.ElapsedMilliseconds < (lrgDelay * 5) && !bgWorkerFar3.CancellationPending);

            if (bgWorkerFar3.IsBusy)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    btnFar3.Text = ">>>";
                    if (btnInfinity.Enabled && !smallCalc)
                    {
                        btnNear3.Enabled = true;
                        btnFar3.Enabled = true;
                    }
                    btnFar3.ForeColor = Color.Black;
                });

                bgWorkerFar3.CancelAsync();
            }

            if (bgWorkerFar3.CancellationPending)
            {
                e.Cancel = true;
            }
        }

        void bgWorkerFar2_DoWork(object sender, DoWorkEventArgs e)
        {
            //Starts and stops rack burst
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            //Deal with lens play
            if (stepCounter == 0)
            {
                do
                {
                    uint result;
                    if (camera != null)
                    {
                        result = camera.DriveLensFarThree();
                    }

                    Thread.Sleep(lrgDelay);
                } while (stopWatch.ElapsedMilliseconds < (lrgDelay * 3));

                stopWatch.Reset();
                stopWatch.Start();
                do
                {
                    uint result;
                    if (camera != null)
                    {
                        result = camera.DriveLensNearThree();
                    }

                    Thread.Sleep(lrgDelay);
                } while (stopWatch.ElapsedMilliseconds < (lrgDelay * 10));

                stopWatch.Reset();
                stopWatch.Start();
            }

            Thread.Sleep(lrgDelay);

            do
            {
                uint result;
                if (camera != null)
                {
                    result = camera.DriveLensFarTwo();
                    stepCounter++;
                }

                Thread.Sleep(medDelay);

            } while (stopWatch.ElapsedMilliseconds < (medDelay * 45) && !bgWorkerFar2.CancellationPending);

            if (bgWorkerFar2.IsBusy)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    btnFar2.Text = ">>";
                    if (btnInfinity.Enabled && !smallCalc)
                    {
                        btnNear2.Enabled = true;
                        btnFar2.Enabled = true;
                    }
                    btnFar2.ForeColor = Color.Black;
                });

                bgWorkerFar2.CancelAsync();
            }

            if (bgWorkerFar2.CancellationPending)
            {
                e.Cancel = true;
            }
        }

        void bgWorkerFar1_DoWork(object sender, DoWorkEventArgs e)
        {
            //Starts and stops rack burst
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            //Deal with lens play
            if (stepCounter == 0)
            {
                do
                {
                    uint result;
                    if (camera != null)
                    {
                        result = camera.DriveLensFarThree();
                    }

                    Thread.Sleep(lrgDelay);
                } while (stopWatch.ElapsedMilliseconds < (lrgDelay * 3));

                stopWatch.Reset();
                stopWatch.Start();
                do
                {
                    uint result;
                    if (camera != null)
                    {
                        result = camera.DriveLensNearThree();
                    }

                    Thread.Sleep(lrgDelay);
                } while (stopWatch.ElapsedMilliseconds < (lrgDelay * 10));

                stopWatch.Reset();
                stopWatch.Start();
            }

            Thread.Sleep(lrgDelay);

            do
            {
                uint result;
                if (camera != null)
                {
                    result = camera.DriveLensFarOne();
                    if (smallCalc)
                        smallStepCounter++;
                    else
                        stepCounter++;
                }

                Thread.Sleep(smallDelay);

            } while (stopWatch.ElapsedMilliseconds < (smallDelay * 175)
                && !bgWorkerFar1.CancellationPending);

            if (bgWorkerFar1.IsBusy)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    btnFar1.Text = ">";
                    if (btnInfinity.Enabled && !smallCalc)
                    {
                        btnNear1.Enabled = true;
                        btnFar1.Enabled = true;
                    }
                    btnFar1.ForeColor = Color.Black;
                });

                bgWorkerFar1.CancelAsync();
            }

            if (bgWorkerFar1.CancellationPending)
            {
                e.Cancel = true;
            }
        }

        public void camera_OnShutdown(Camera sender, ShutdownEventArgs e)
        {
            //Detects if camera has been shut down
            parentForm.camera_OnShutdown(sender, e);
            this.Close();
        }

        void EnableButtons()
        {
            //Background thread that enables buttons that need to be enabled and sets the benchmark button to red if it has default values
            do
            {
                if (encoderItems.LrgDelay == 2000 || encoderItems.MedDelay == 2000 || encoderItems.SmallDelay == 2000)
                {
                    btnBenchmarks.ForeColor = Color.Red;
                }

                if (singleStep)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        if (camera != null)
                        {
                            if (stopWatch.ElapsedMilliseconds > smallDelay)
                            {
                                if (rdoSmall.Checked)
                                {
                                    if (smallStepCounter < 1)
                                    {
                                        btnFar1.Enabled = true;
                                        btnNear1.Enabled = true;
                                    }
                                }
                            }

                            if (stopWatch.ElapsedMilliseconds > medDelay)
                            {
                                if (rdoMedium.Checked)
                                {
                                    if (smallStepCounter < 1)
                                    {
                                        btnFar2.Enabled = true;
                                        btnNear2.Enabled = true;
                                    }
                                }
                            }

                            if (stopWatch.ElapsedMilliseconds > lrgDelay)
                            {
                                if (rdoLarge.Checked)
                                {
                                    if (smallStepCounter < 1)
                                    {
                                        btnFar3.Enabled = true;
                                        btnNear3.Enabled = true;
                                    }
                                }
                            }
                        }
                    });

                    Application.DoEvents();
                }
                Thread.Sleep(50);
            } while (!formClosing);
        }

        private void btnNear1_Click(object sender, EventArgs e)
        {
            //Checks if any rack bursts are active and cancels them
            if (bgWorkerNear1.IsBusy)
            {
                bgWorkerNear1.CancelAsync();
                btnNear1.Text = "<";
                btnNear1.ForeColor = Color.Black;
            }

            if (bgWorkerFar1.IsBusy)
            {
                bgWorkerFar1.CancelAsync();
                btnFar1.Text = ">";
                btnFar1.ForeColor = Color.Black;
            }

            if (bgWorkerNear2.IsBusy)
            {
                bgWorkerNear2.CancelAsync();
                btnNear2.Text = "<<";
                btnNear2.ForeColor = Color.Black;
            }

            if (bgWorkerFar2.IsBusy)
            {
                bgWorkerFar2.CancelAsync();
                btnFar2.Text = ">>";
                btnFar2.ForeColor = Color.Black;
            }

            if (bgWorkerNear3.IsBusy)
            {
                bgWorkerNear3.CancelAsync();
                btnNear3.Text = "<<<";
                btnNear3.ForeColor = Color.Black;
            }

            if (bgWorkerFar3.IsBusy)
            {
                bgWorkerFar3.CancelAsync();
                btnFar3.Text = ">>>";
                btnFar3.ForeColor = Color.Black;
            }

            if (singleStep)
            {
                //If not a rack burst, disable all buttons and start stopwatch
                stopWatch.Reset();
                stopWatch.Start();

                btnFar1.Enabled = false;
                btnFar2.Enabled = false;
                btnFar3.Enabled = false;

                btnNear1.Enabled = false;
                btnNear2.Enabled = false;
                btnNear3.Enabled = false;

                uint result;
                if (camera != null)
                {
                    result = camera.DriveLensNearOne();

                    if (smallCalc && !mediumCalc && !largeCalc)
                    {
                        //If currently calculating medium ratio handle the smallstepcounter variable
                        if (smallStepCounter != 0)
                            smallStepCounter--;

                        //Disable other buttons once smallstep granularity adjustments have begun
                        if (smallStepCounter > 0)
                        {
                            btnNear2.Enabled = false;
                            btnNear3.Enabled = false;
                            btnFar2.Enabled = false;
                            btnFar3.Enabled = false;
                        }
                        else
                        {
                            //If smallstep granularity adjustments has not begun, check the selected radio button and enable the proper set of buttons
                            if (rdoMedium.Checked)
                            {
                                btnNear2.Enabled = true;
                                btnFar2.Enabled = true;
                            }
                            else if (rdoLarge.Checked)
                            {
                                btnNear3.Enabled = true;
                                btnFar3.Enabled = true;
                            }
                        }
                    }
                    else if (smallCalc && mediumCalc && !largeCalc)
                    {
                        //If currently calculating large ratio handle the smallstepcounter variable
                        if (smallStepCounter != 0)
                            smallStepCounter--;

                        //Disable other buttons once smallstep granularity adjustments have begun
                        if (smallStepCounter > 0)
                        {
                            btnNear2.Enabled = false;
                            btnNear3.Enabled = false;
                            btnFar2.Enabled = false;
                            btnFar3.Enabled = false;
                        }
                        else
                        {
                            //If smallstep granularity adjustments has not begun, check the selected radio button and enable the proper set of buttons
                            if (rdoMedium.Checked)
                            {
                                btnNear2.Enabled = true;
                                btnFar2.Enabled = true;
                            }
                            else if (rdoLarge.Checked)
                            {
                                btnNear3.Enabled = true;
                                btnFar3.Enabled = true;
                            }
                        }
                    }
                    else
                    {
                        //If currently calibrating distance marks handle the regular stepcounter variable
                        if (stepCounter != 0)
                            stepCounter--;
                    }
                }

            }
            else
            {
                //Rackburst condition
                if (bgWorkerNear1.IsBusy)
                {
                    //Cancel rackburst if currently bursting and reset buttons to initial state
                    bgWorkerNear1.CancelAsync();
                    btnNear1.Text = "<";
                    btnFar1.Enabled = true;
                    btnNear1.Enabled = true;
                    btnNear1.ForeColor = Color.Black;
                }
                else
                {
                    //Begin rackburst and set buttons to busy state
                    bgWorkerNear1.RunWorkerAsync();
                    btnNear1.Text = "| |";
                    btnFar1.Enabled = false;
                    btnNear1.ForeColor = Color.Red;
                }
            }

            //Start the stepcount worker to update counter text box
            if (!bgWorkerStepCount.IsBusy)
            {
                bgWorkerStepCount.RunWorkerAsync();
            }
        }

        private void btnFar1_Click(object sender, EventArgs e)
        {
            if (bgWorkerNear1.IsBusy)
            {
                bgWorkerNear1.CancelAsync();
                btnNear1.Text = "<";
                btnNear1.ForeColor = Color.Black;
            }

            if (bgWorkerFar1.IsBusy)
            {
                bgWorkerFar1.CancelAsync();
                btnFar1.Text = ">";
                btnFar1.ForeColor = Color.Black;
            }

            if (bgWorkerNear2.IsBusy)
            {
                bgWorkerNear2.CancelAsync();
                btnNear2.Text = "<<";
                btnNear2.ForeColor = Color.Black;
            }

            if (bgWorkerFar2.IsBusy)
            {
                bgWorkerFar2.CancelAsync();
                btnFar2.Text = ">>";
                btnFar2.ForeColor = Color.Black;
            }

            if (bgWorkerNear3.IsBusy)
            {
                bgWorkerNear3.CancelAsync();
                btnNear3.Text = "<<<";
                btnNear3.ForeColor = Color.Black;
            }

            if (bgWorkerFar3.IsBusy)
            {
                bgWorkerFar3.CancelAsync();
                btnFar3.Text = ">>>";
                btnFar3.ForeColor = Color.Black;
            }

            if (singleStep)
            {
                stopWatch.Reset();
                stopWatch.Start();

                //Deal with lens play
                if (stepCounter == 0)
                {
                    do
                    {
                        uint result;
                        if (camera != null)
                        {
                            result = camera.DriveLensFarOne();
                        }

                        Thread.Sleep(smallDelay);
                    } while (stopWatch.ElapsedMilliseconds < 500);

                    stopWatch.Reset();
                    stopWatch.Start();
                    do
                    {
                        uint result;
                        if (camera != null)
                        {
                            result = camera.DriveLensNearOne();
                        }

                        Thread.Sleep(smallDelay);
                    } while (stopWatch.ElapsedMilliseconds < 1000);

                    stopWatch.Reset();
                    stopWatch.Start();
                }

                btnFar1.Enabled = false;
                btnFar2.Enabled = false;
                btnFar3.Enabled = false;

                btnNear1.Enabled = false;
                btnNear2.Enabled = false;
                btnNear3.Enabled = false;

                uint results;
                if (camera != null)
                {
                    results = camera.DriveLensFarOne();
                    if (smallCalc && !mediumCalc && !largeCalc)
                    {
                        smallStepCounter++;

                        if (smallStepCounter > 0)
                        {
                            btnNear2.Enabled = false;
                            btnNear3.Enabled = false;
                            btnFar2.Enabled = false;
                            btnFar3.Enabled = false;
                        }
                        else
                        {
                            btnNear2.Enabled = true;
                            btnNear3.Enabled = true;
                            btnFar2.Enabled = true;
                            btnFar3.Enabled = true;
                        }
                    }
                    else if (smallCalc && mediumCalc && !largeCalc)
                    {
                        smallStepCounter++;

                        if (smallStepCounter > 0)
                        {
                            btnNear2.Enabled = false;
                            btnNear3.Enabled = false;
                            btnFar2.Enabled = false;
                            btnFar3.Enabled = false;
                        }
                        else
                        {
                            btnNear2.Enabled = true;
                            btnNear3.Enabled = true;
                            btnFar2.Enabled = true;
                            btnFar3.Enabled = true;
                        }
                    }
                    else
                    {
                        stepCounter++;
                    }
                }

            }
            else
            {
                if (bgWorkerFar1.IsBusy)
                {
                    bgWorkerFar1.CancelAsync();
                    btnFar1.Text = ">";
                    btnNear1.Enabled = true;
                    btnFar1.Enabled = true;
                    btnFar1.ForeColor = Color.Black;
                }
                else
                {
                    bgWorkerFar1.RunWorkerAsync();
                    btnFar1.Text = "| |";
                    btnNear1.Enabled = false;
                    btnFar1.ForeColor = Color.Red;
                }
            }

            if (!bgWorkerStepCount.IsBusy)
            {
                bgWorkerStepCount.RunWorkerAsync();
            }
        }

        private void btnNear2_Click(object sender, EventArgs e)
        {
            if (bgWorkerNear1.IsBusy)
            {
                bgWorkerNear1.CancelAsync();
                btnNear1.Text = "<";
                btnNear1.ForeColor = Color.Black;
            }

            if (bgWorkerFar1.IsBusy)
            {
                bgWorkerFar1.CancelAsync();
                btnFar1.Text = ">";
                btnFar1.ForeColor = Color.Black;
            }

            if (bgWorkerNear2.IsBusy)
            {
                bgWorkerNear2.CancelAsync();
                btnNear2.Text = "<<";
                btnNear2.ForeColor = Color.Black;
            }

            if (bgWorkerFar2.IsBusy)
            {
                bgWorkerFar2.CancelAsync();
                btnFar2.Text = ">>";
                btnFar2.ForeColor = Color.Black;
            }

            if (bgWorkerNear3.IsBusy)
            {
                bgWorkerNear3.CancelAsync();
                btnNear3.Text = "<<<";
                btnNear3.ForeColor = Color.Black;
            }

            if (bgWorkerFar3.IsBusy)
            {
                bgWorkerFar3.CancelAsync();
                btnFar3.Text = ">>>";
                btnFar3.ForeColor = Color.Black;
            }

            if (singleStep)
            {
                stopWatch.Reset();
                stopWatch.Start();

                btnFar1.Enabled = false;
                btnFar2.Enabled = false;
                btnFar3.Enabled = false;

                btnNear1.Enabled = false;
                btnNear2.Enabled = false;
                btnNear3.Enabled = false;

                uint result;
                if (camera != null)
                {
                    result = camera.DriveLensNearTwo();

                    if (stepCounter != 0)
                        stepCounter--;
                }

            }
            else
            {
                if (bgWorkerNear2.IsBusy)
                {
                    bgWorkerNear2.CancelAsync();
                    btnNear2.Text = "<<";
                    btnFar2.Enabled = true;
                    btnNear2.Enabled = true;
                    btnNear2.ForeColor = Color.Black;
                }
                else
                {
                    bgWorkerNear2.RunWorkerAsync();
                    btnNear2.Text = "| |";
                    btnFar2.Enabled = false;
                    btnNear2.ForeColor = Color.Red;
                }
            }

            if (!bgWorkerStepCount.IsBusy)
            {
                bgWorkerStepCount.RunWorkerAsync();
            }
        }

        private void btnFar2_Click(object sender, EventArgs e)
        {
            if (bgWorkerNear1.IsBusy)
            {
                bgWorkerNear1.CancelAsync();
                btnNear1.Text = "<";
                btnNear1.ForeColor = Color.Black;
            }

            if (bgWorkerFar1.IsBusy)
            {
                bgWorkerFar1.CancelAsync();
                btnFar1.Text = ">";
                btnFar1.ForeColor = Color.Black;
            }

            if (bgWorkerNear2.IsBusy)
            {
                bgWorkerNear2.CancelAsync();
                btnNear2.Text = "<<";
                btnNear2.ForeColor = Color.Black;
            }

            if (bgWorkerFar2.IsBusy)
            {
                bgWorkerFar2.CancelAsync();
                btnFar2.Text = ">>";
                btnFar2.ForeColor = Color.Black;
            }

            if (bgWorkerNear3.IsBusy)
            {
                bgWorkerNear3.CancelAsync();
                btnNear3.Text = "<<<";
                btnNear3.ForeColor = Color.Black;
            }

            if (bgWorkerFar3.IsBusy)
            {
                bgWorkerFar3.CancelAsync();
                btnFar3.Text = ">>>";
                btnFar3.ForeColor = Color.Black;
            }

            if (!smallStepWatch.IsRunning)
            {
                smallStepWatch.Start();
            }

            if (singleStep)
            {
                stopWatch.Reset();
                stopWatch.Start();

                //Deal with lens play
                if (stepCounter == 0)
                {
                    do
                    {
                        uint result;
                        if (camera != null)
                        {
                            result = camera.DriveLensFarTwo();
                        }

                        Thread.Sleep(medDelay);
                    } while (stopWatch.ElapsedMilliseconds < 250);

                    stopWatch.Reset();
                    stopWatch.Start();
                    do
                    {
                        uint result;
                        if (camera != null)
                        {
                            result = camera.DriveLensNearTwo();
                        }

                        Thread.Sleep(medDelay);
                    } while (stopWatch.ElapsedMilliseconds < 500);

                    stopWatch.Reset();
                    stopWatch.Start();
                }

                btnFar1.Enabled = false;
                btnFar2.Enabled = false;
                btnFar3.Enabled = false;

                btnNear1.Enabled = false;
                btnNear2.Enabled = false;
                btnNear3.Enabled = false;

                uint results;
                if (camera != null)
                {
                    results = camera.DriveLensFarTwo();
                    stepCounter++;
                }

            }
            else
            {
                if (bgWorkerFar2.IsBusy)
                {
                    bgWorkerFar2.CancelAsync();
                    btnFar2.Text = ">>";
                    btnNear2.Enabled = true;
                    btnFar2.Enabled = true;
                    btnFar2.ForeColor = Color.Black;
                }
                else
                {
                    bgWorkerFar2.RunWorkerAsync();
                    btnFar2.Text = "| |";
                    btnNear2.Enabled = false;
                    btnFar2.ForeColor = Color.Red;
                }
            }

            if (!bgWorkerStepCount.IsBusy)
            {
                bgWorkerStepCount.RunWorkerAsync();
            }
        }

        private void btnNear3_Click(object sender, EventArgs e)
        {
            if (bgWorkerNear1.IsBusy)
            {
                bgWorkerNear1.CancelAsync();
                btnNear1.Text = "<";
                btnNear1.ForeColor = Color.Black;
            }

            if (bgWorkerFar1.IsBusy)
            {
                bgWorkerFar1.CancelAsync();
                btnFar1.Text = ">";
                btnFar1.ForeColor = Color.Black;
            }

            if (bgWorkerNear2.IsBusy)
            {
                bgWorkerNear2.CancelAsync();
                btnNear2.Text = "<<";
                btnNear2.ForeColor = Color.Black;
            }

            if (bgWorkerFar2.IsBusy)
            {
                bgWorkerFar2.CancelAsync();
                btnFar2.Text = ">>";
                btnFar2.ForeColor = Color.Black;
            }

            if (bgWorkerNear3.IsBusy)
            {
                bgWorkerNear3.CancelAsync();
                btnNear3.Text = "<<<";
                btnNear3.ForeColor = Color.Black;
            }

            if (bgWorkerFar3.IsBusy)
            {
                bgWorkerFar3.CancelAsync();
                btnFar3.Text = ">>>";
                btnFar3.ForeColor = Color.Black;
            }

            if (singleStep)
            {
                stopWatch.Reset();
                stopWatch.Start();

                btnFar1.Enabled = false;
                btnFar2.Enabled = false;
                btnFar3.Enabled = false;

                btnNear1.Enabled = false;
                btnNear2.Enabled = false;
                btnNear3.Enabled = false;

                uint result;
                if (camera != null)
                {
                    result = camera.DriveLensNearThree();
                    if (stepCounter != 0)
                        stepCounter--;
                }

            }
            else
            {
                if (bgWorkerNear3.IsBusy)
                {
                    bgWorkerNear3.CancelAsync();
                    btnNear3.Text = "<<<";
                    btnFar3.Enabled = true;
                    btnNear3.Enabled = true;
                    btnNear3.ForeColor = Color.Black;
                }
                else
                {
                    bgWorkerNear3.RunWorkerAsync();
                    btnNear3.Text = "| |";
                    btnFar3.Enabled = false;
                    btnNear3.ForeColor = Color.Red;
                }
            }

            if (!bgWorkerStepCount.IsBusy)
            {
                bgWorkerStepCount.RunWorkerAsync();
            }
        }

        private void btnFar3_Click(object sender, EventArgs e)
        {
            if (bgWorkerNear1.IsBusy)
            {
                bgWorkerNear1.CancelAsync();
                btnNear1.Text = "<";
                btnNear1.ForeColor = Color.Black;
            }

            if (bgWorkerFar1.IsBusy)
            {
                bgWorkerFar1.CancelAsync();
                btnFar1.Text = ">";
                btnFar1.ForeColor = Color.Black;
            }

            if (bgWorkerNear2.IsBusy)
            {
                bgWorkerNear2.CancelAsync();
                btnNear2.Text = "<<";
                btnNear2.ForeColor = Color.Black;
            }

            if (bgWorkerFar2.IsBusy)
            {
                bgWorkerFar2.CancelAsync();
                btnFar2.Text = ">>";
                btnFar2.ForeColor = Color.Black;
            }

            if (bgWorkerNear3.IsBusy)
            {
                bgWorkerNear3.CancelAsync();
                btnNear3.Text = "<<<";
                btnNear3.ForeColor = Color.Black;
            }

            if (bgWorkerFar3.IsBusy)
            {
                bgWorkerFar3.CancelAsync();
                btnFar3.Text = ">>>";
                btnFar3.ForeColor = Color.Black;
            }

            if (!smallStepWatch.IsRunning)
            {
                smallStepWatch.Start();
            }

            if (singleStep)
            {
                stopWatch.Reset();
                stopWatch.Start();

                //Deal with lens play
                if (stepCounter == 0)
                {
                    do
                    {
                        uint result;
                        if (camera != null)
                        {
                            result = camera.DriveLensFarThree();
                        }

                        Thread.Sleep(lrgDelay);
                    } while (stopWatch.ElapsedMilliseconds < 125);

                    stopWatch.Reset();
                    stopWatch.Start();
                    do
                    {
                        uint result;
                        if (camera != null)
                        {
                            result = camera.DriveLensNearThree();
                        }

                        Thread.Sleep(lrgDelay);
                    } while (stopWatch.ElapsedMilliseconds < 250);

                    stopWatch.Reset();
                    stopWatch.Start();
                }

                btnFar1.Enabled = false;
                btnFar2.Enabled = false;
                btnFar3.Enabled = false;

                btnNear1.Enabled = false;
                btnNear2.Enabled = false;
                btnNear3.Enabled = false;

                uint results;
                if (camera != null)
                {
                    results = camera.DriveLensFarThree();
                    stepCounter++;
                }

            }
            else
            {
                if (bgWorkerFar3.IsBusy)
                {
                    bgWorkerFar3.CancelAsync();
                    btnFar3.Text = ">>>";
                    btnNear3.Enabled = true;
                    btnFar3.Enabled = true;
                    btnFar3.ForeColor = Color.Black;
                }
                else
                {
                    bgWorkerFar3.RunWorkerAsync();
                    btnFar3.Text = "| |";
                    btnNear3.Enabled = false;
                    btnFar3.ForeColor = Color.Red;
                }
            }

            if (!bgWorkerStepCount.IsBusy)
            {
                bgWorkerStepCount.RunWorkerAsync();
            }
        }

        private void DSLRCalibration_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Exits the loop in the EnableButtons thread
            formClosing = true;
        }

        private void btnAddDistance_Click(object sender, EventArgs e)
        {
            //Checks to see if we are calibrating distances stops or medium and large ratios
            if (!smallCalc)
            {
                //If calibrating distance stops
                double result;
                //Check if valid number is input
                if (double.TryParse(txtDistance.Text, out result))
                {
                    //User friendly check to see if minimum distance at 0th step has been input
                    if (encoderList.Count < 1 && stepCounter != 0)
                    {
                        FindAndMoveMsgBox("Confirm Distance", this.Location.X + this.Height / 2, this.Location.Y + this.Width / 8);
                        if (MessageBox.Show(this, "Minimum Distance at Step Zero not yet input. Continue anyway?", "Confirm Distance", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            //Clear all encoder items in collection if we are attempting to calibrate while a calibration collection is already in memory
                            if (trvDistanceMark.Nodes.Count < 1)
                            {
                                encoderItems.EncoderItems.Clear();
                                encoderList.Clear();
                            }

                            try
                            {
                                //Add a major distance marker to the encoder list which will later be used to calculate granular encoder steps
                                calibrationDictionary.Add(stepCounter, double.Parse(txtDistance.Text));
                                encoderList.Add(new EncoderItem(double.Parse(txtDistance.Text), 0, double.Parse(txtDistance.Text), 0, stepCounter, 0, 0, 0));
                            }
                            catch (Exception ex)
                            {

                            }

                            //Clear the distance mark treeview and re build it based on the items in the major distance encoder list
                            trvDistanceMark.Nodes.Clear();
                            foreach (EncoderItem ei in encoderList)
                            {
                                trvDistanceMark.BeginUpdate();
                                TreeNode treeNodeDist = new TreeNode(ei.Dist + "ft.");
                                trvDistanceMark.Nodes.Add(treeNodeDist);
                                trvDistanceMark.EndUpdate();
                                trvDistanceMark.Nodes[trvDistanceMark.Nodes.Count - 1].Nodes.Add(ei.Encoder.ToString() + " steps");
                            }

                            //Clear the distance input box
                            txtDistance.Text = string.Empty;
                        }
                    }
                    else
                    {
                        try
                        {
                            //Clear all encoder items in collection if we are attempting to calibrate while a calibration collection is already in memory
                            if (trvDistanceMark.Nodes.Count < 1)
                            {
                                encoderItems.EncoderItems.Clear();
                                encoderList.Clear();
                            }

                            //Add a major distance marker to the encoder list which will later be used to calculate granular encoder steps
                            calibrationDictionary.Add(stepCounter, double.Parse(txtDistance.Text));
                            encoderList.Add(new EncoderItem(double.Parse(txtDistance.Text), 0, double.Parse(txtDistance.Text), 0, stepCounter, 0, 0, 0));
                        }
                        catch (Exception ex)
                        {

                        }

                        //Clear the distance mark treeview and re build it based on the items in the major distance encoder list
                        trvDistanceMark.Nodes.Clear();
                        foreach (EncoderItem ei in encoderList)
                        {
                            trvDistanceMark.BeginUpdate();
                            TreeNode treeNodeDist = new TreeNode(ei.Dist + "ft.");
                            trvDistanceMark.Nodes.Add(treeNodeDist);
                            trvDistanceMark.EndUpdate();
                            trvDistanceMark.Nodes[trvDistanceMark.Nodes.Count - 1].Nodes.Add(ei.Encoder.ToString() + " steps");
                        }

                        //Clear the distance input box
                        txtDistance.Text = string.Empty;
                    }
                }
                else if (txtDistance.Text == "\u221E" || txtDistance.Text == "\u221E" + " " + "\u1DAC\u1D43\u02E3")
                {
                    if (txtDistance.Text == "\u221E")
                    {
                        //If the user is inputing infinity, enable the infinity max button
                        try
                        {
                            if (trvDistanceMark.Nodes.Count < 1)
                            {
                                encoderItems.EncoderItems.Clear();
                                encoderList.Clear();
                            }

                            btnInfinityMax.Enabled = true;
                            calibrationDictionary.Add(stepCounter, hyperFocal);
                            encoderList.Add(new EncoderItem(hyperFocal, 0, hyperFocal, 0, stepCounter, 0, 0, 0));
                        }
                        catch (Exception ex)
                        {

                        }

                        //Clear the distance mark treeview and re build it based on the items in the major distance encoder list
                        trvDistanceMark.Nodes.Clear();
                        foreach (EncoderItem ei in encoderList)
                        {
                            if (ei.Dist > hyperFocal)
                            {
                                //Add infinity max to the tree view when value greater than hyperfocal dist is detected
                                trvDistanceMark.BeginUpdate();
                                TreeNode treeNodeDist = new TreeNode("\u221E" + " " + "\u1DAC\u1D43\u02E3");
                                trvDistanceMark.Nodes.Add(treeNodeDist);
                                trvDistanceMark.EndUpdate();
                                trvDistanceMark.Nodes[trvDistanceMark.Nodes.Count - 1].Nodes.Add(ei.Encoder.ToString() + " steps");
                            }
                            else if (ei.Dist == hyperFocal)
                            {
                                //Add infinity to the tree view when hyperfocal dist is detected
                                trvDistanceMark.BeginUpdate();
                                TreeNode treeNodeDist = new TreeNode("\u221E");
                                trvDistanceMark.Nodes.Add(treeNodeDist);
                                trvDistanceMark.EndUpdate();
                                trvDistanceMark.Nodes[trvDistanceMark.Nodes.Count - 1].Nodes.Add(ei.Encoder.ToString() + " steps");
                            }
                            else
                            {

                                trvDistanceMark.BeginUpdate();
                                TreeNode treeNodeDist = new TreeNode(ei.Dist + "ft.");
                                trvDistanceMark.Nodes.Add(treeNodeDist);
                                trvDistanceMark.EndUpdate();
                                trvDistanceMark.Nodes[trvDistanceMark.Nodes.Count - 1].Nodes.Add(ei.Encoder.ToString() + " steps");
                            }
                        }

                        //Clear the distance input box
                        txtDistance.Text = string.Empty;
                        btnInfinity.Enabled = false;
                    }
                    else
                    {
                        //If the user is inputing infinity max disable distance add button and stop all rackburst workers and set buttons to non busy state
                        if (bgWorkerNear1.IsBusy)
                        {
                            bgWorkerNear1.CancelAsync();
                            btnNear1.Text = "<";
                            btnNear1.ForeColor = Color.Black;
                        }

                        if (bgWorkerNear2.IsBusy)
                        {
                            bgWorkerNear2.CancelAsync();
                            btnNear2.Text = "<<";
                            btnNear2.ForeColor = Color.Black;
                        }

                        if (bgWorkerNear3.IsBusy)
                        {
                            bgWorkerNear3.CancelAsync();
                            btnNear3.Text = "<<<";
                            btnNear3.ForeColor = Color.Black;
                        }

                        if (bgWorkerFar1.IsBusy)
                        {
                            bgWorkerFar1.CancelAsync();
                            btnFar1.Text = ">";
                            btnFar1.ForeColor = Color.Black;
                        }

                        if (bgWorkerFar2.IsBusy)
                        {
                            bgWorkerFar2.CancelAsync();
                            btnFar2.Text = ">>";
                            btnFar2.ForeColor = Color.Black;
                        }

                        if (bgWorkerFar3.IsBusy)
                        {
                            bgWorkerFar3.CancelAsync();
                            btnFar3.Text = ">>>";
                            btnFar3.ForeColor = Color.Black;
                        }

                        btnAddDistance.Enabled = false;
                        Thread.Sleep(2000);

                        try
                        {
                            calibrationDictionary.Add(stepCounter, double.Parse("65535"));
                            encoderList.Add(new EncoderItem(double.Parse("65535"), 0, double.Parse("65535"), 0, stepCounter, 0, 0, 0));
                        }
                        catch (Exception ex)
                        {

                        }

                        trvDistanceMark.Nodes.Clear();
                        foreach (EncoderItem ei in encoderList)
                        {
                            if (ei.Dist > hyperFocal)
                            {
                                trvDistanceMark.BeginUpdate();
                                TreeNode treeNodeDist = new TreeNode("\u221E" + " " + "\u1DAC\u1D43\u02E3");
                                trvDistanceMark.Nodes.Add(treeNodeDist);
                                trvDistanceMark.EndUpdate();
                                trvDistanceMark.Nodes[trvDistanceMark.Nodes.Count - 1].Nodes.Add(ei.Encoder.ToString() + " steps");
                            }
                            else if (ei.Dist == hyperFocal)
                            {
                                trvDistanceMark.BeginUpdate();
                                TreeNode treeNodeDist = new TreeNode("\u221E");
                                trvDistanceMark.Nodes.Add(treeNodeDist);
                                trvDistanceMark.EndUpdate();
                                trvDistanceMark.Nodes[trvDistanceMark.Nodes.Count - 1].Nodes.Add(ei.Encoder.ToString() + " steps");
                            }
                            else
                            {

                                trvDistanceMark.BeginUpdate();
                                TreeNode treeNodeDist = new TreeNode(ei.Dist + "ft.");
                                trvDistanceMark.Nodes.Add(treeNodeDist);
                                trvDistanceMark.EndUpdate();
                                trvDistanceMark.Nodes[trvDistanceMark.Nodes.Count - 1].Nodes.Add(ei.Encoder.ToString() + " steps");
                            }
                        }

                        //Disable small step buttons, distance add, infinities, and small step radio button
                        txtDistance.Text = string.Empty;
                        btnInfinityMax.Enabled = false;
                        btnAddDistance.Enabled = false;
                        btnIncrease.Enabled = false;
                        btnDecrease.Enabled = false;
                        rdoSmall.Enabled = false;
                        btnFar1.Enabled = false;
                        btnNear1.Enabled = false;
                    }
                }
                else
                {
                    FindAndMoveMsgBox("Invalid Distance", this.Location.X + this.Height / 2, this.Location.Y + this.Width / 8);
                    MessageBox.Show(this, "Please input a valid distance.", "Invalid Distance");
                }
            }
            else if (smallCalc && !mediumCalc && !largeCalc)
            {
                //If initial calibration has already happened and currently calculating medium ratio deduct smallsteps from collection and calculate medium ratio
                if (smallStepCounter > 0)
                {
                    mediumRatio = (double)(encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder - smallStepCounter) / stepCounter;
                    btnAddDistance.ForeColor = Color.Black;
                    btnSave.Enabled = true;
                }
                else
                {
                    mediumRatio = (double)encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder / stepCounter;
                    btnAddDistance.ForeColor = Color.Black;
                    btnSave.Enabled = true;
                }
            }
            else if (smallCalc && mediumCalc && !largeCalc)
            {
                //If initial calibration has already happened and currently calculating large ratio deduct smallsteps from collection and calculate large ratio
                if (smallStepCounter > 0)
                {
                    largeRatio = (double)(encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder - smallStepCounter) / stepCounter;
                    btnAddDistance.ForeColor = Color.Black;
                    btnSave.Enabled = true;
                }
                else
                {
                    largeRatio = (double)encoderItems.EncoderItems[encoderItems.EncoderItems.Count - 1].Encoder / stepCounter;
                    btnAddDistance.ForeColor = Color.Black;
                    btnSave.Enabled = true;
                }
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            smallStepWatch.Stop();
            smallStepWatch.Reset();

            //Disable small step buttons
            btnFar1.Enabled = false;
            btnNear1.Enabled = false;
            //Reset smallstep counter and its textbox
            smallStepCounter = 0;
            txtStepCounter.Text = stepCounter.ToString();

            if (encoderList.Count < 1)
            {
                //Check for a major dist mapping before proceeding
                FindAndMoveMsgBox("Error", this.Location.X + this.Height / 2, this.Location.Y + this.Width / 8);
                MessageBox.Show(this, "Please input distance marks.", "Error");
            }
            else if (!smallCalc)
            {
                //If this is the initial small step calibration build encoderitems collection
                double totalEncoderSteps = encoderList[encoderList.Count - 1].Encoder;
                EncoderItem encoderBuffer = new EncoderItem();
                encoderBuffer.Encoder = 0;
                double positionToEncoderRatio = 16384 / totalEncoderSteps;

                //calculate major trackbar and far values for each distance stop
                double totalEncoderDelta = 0;
                for (int x = 0; x < encoderList.Count - 1; x++)
                {
                    encoderList[x].TrackBar = totalEncoderDelta * (16384 / totalEncoderSteps);
                    encoderList[x].Far = encoderList[x + 1].Near;
                    totalEncoderDelta = totalEncoderDelta + (encoderList[x + 1].Encoder - encoderList[x].Encoder);
                }

                encoderList[encoderList.Count - 1].Near = encoderList[encoderList.Count - 1].Dist;
                encoderList[encoderList.Count - 1].Far = encoderList[encoderList.Count - 1].Dist;
                encoderList[encoderList.Count - 1].TrackBar = totalEncoderDelta * (16384 / totalEncoderSteps);

                using (StreamWriter sw = new StreamWriter(lensDirectory + @"\encoderDictionary.txt"))
                {
                    foreach (EncoderItem ei in encoderList)
                    {
                        sw.WriteLine("e.Dist:" + ei.Dist + ", " + "e.DistStep:" + ei.DistStep + ", " + "e.Near:" + ei.Near + ", " + "e.Far:" + ei.Far + ", " + "e.Encoder:"
                            + ei.Encoder + ", " + "e.Trackbar:" + ei.TrackBar);
                    }
                }

                //Map focus positions to encoder values
                int encoderDelta = 0;
                double footStep = 0;
                double trackBarBuffer = 0;
                for (int x = 0; x < encoderList.Count - 1; x++)
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

                //Infinity max calculation
                encoderBuffer = new EncoderItem(encoderList[encoderList.Count - 1].Dist, encoderList[encoderList.Count - 1].DistStep, encoderList[encoderList.Count - 1].Near, encoderList[encoderList.Count - 1].Far, encoderList[encoderList.Count - 1].Encoder, encoderList[encoderList.Count - 1].TrackBar, 0, 0);
                encoderBuffer.Dist = encoderBuffer.Near;
                encoderBuffer.DistStep = footStep;
                encoderItems.EncoderItems.Add(new EncoderItem(encoderBuffer.Dist, encoderBuffer.DistStep, encoderBuffer.Near, encoderBuffer.Far, encoderBuffer.Encoder, encoderBuffer.TrackBar, 0, 0));

                stepCounter = 0;
                txtDistance.Text = string.Empty;
                txtStepCounter.Text = string.Empty;

                //Prepare for medium ratio calculation
                btnAddDistance.Text = "Calculate Medium Step Ratio";
                btnAddDistance.ForeColor = Color.Red;

                //Clean up remnants from small calibration step
                lblDistanceMarker.Visible = false;
                txtDistance.Visible = false;
                lblFt.Visible = false;
                btnInfinity.Visible = false;
                btnInfinityMax.Visible = false;
                btnAddDistance.Enabled = true;
                btnIncrease.Enabled = true;
                btnDecrease.Enabled = true;

                stepCounter = 0;

                //Set small and large step buttons enabled enabled property to false and medium buttons true. Disable Save button
                btnFar1.Enabled = false;
                btnNear1.Enabled = false;

                btnFar2.Enabled = true;
                btnNear2.Enabled = true;

                btnFar3.Enabled = false;
                btnNear3.Enabled = false;

                btnSave.Enabled = false;

                //Set small step calibration flag to true
                smallCalc = true;
                FindAndMoveMsgBox("Medium Step", this.Location.X + this.Height / 2, this.Location.Y + this.Width / 8);
                MessageBox.Show(this, "Manually rack lens to minimum focus and step to maximum using medium steps.", "Medium Step");

                //Medium radio button enabled and selected. All others not.
                rdoSmall.Enabled = false;
                rdoMedium.Enabled = true;
                rdoLarge.Enabled = false;
                rdoMedium.Checked = true;

                //Medium radio click event method manually called to set proper stepsize button groups and rack burst to default
                rdoMedium_Click(sender, e);
            }
            else if (smallCalc && !mediumCalc && !largeCalc)
            {
                //If this is the medium ratio calculation step, set the medium ratio property on all the items in the encoderitems collection
                //Change this in the future to sit at the parent level and change once rather than iterating
                foreach (EncoderItem ei in encoderItems.EncoderItems)
                {
                    ei.MediumRatio = mediumRatio;
                }

                //Change next button to a save button
                btnSave.Text = "Save";
                btnSave.ForeColor = Color.Black;

                //Reset step counter and clear the distance and step text boxes
                stepCounter = 0;
                txtDistance.Text = string.Empty;
                txtStepCounter.Text = string.Empty;

                //Prepare for large ratio calculation
                btnAddDistance.Text = "Calculate Large Step Ratio";
                btnAddDistance.ForeColor = Color.Red;

                //Clean up remnants from small calibration step. (Should have happened earlier. This is probably redundant)
                lblDistanceMarker.Visible = false;
                txtDistance.Visible = false;
                lblFt.Visible = false;
                btnInfinity.Visible = false;
                btnInfinityMax.Visible = false;
                btnAddDistance.Enabled = true;
                btnIncrease.Enabled = true;
                btnDecrease.Enabled = true;

                stepCounter = 0;

                //Set small and medium step buttons enabled property to false and large buttons true. Disable Save button
                btnFar1.Enabled = false;
                btnNear1.Enabled = false;

                btnFar2.Enabled = false;
                btnNear2.Enabled = false;

                btnFar3.Enabled = true;
                btnNear3.Enabled = true;

                btnSave.Enabled = false;

                //Set medium ratio calculated flag to true
                mediumCalc = true;
                FindAndMoveMsgBox("Large Step", this.Location.X + this.Height / 2, this.Location.Y + this.Width / 8);
                MessageBox.Show(this, "Manually rack lens to minimum focus and step to maximum using large steps.", "Large Step");

                //Large radio button enabled and selected. All others not.
                rdoLarge.Checked = true;
                rdoSmall.Enabled = false;
                rdoMedium.Enabled = false;
                rdoLarge.Enabled = true;

                //Large radio click event method manually called to set proper stepsize button groups and rack burst to default
                rdoLarge_Click(sender, e);
            }
            else if (smallCalc && mediumCalc && !largeCalc)
            {
                //Start wait button background thread to set the Save button text to wait and write the encoder item collection data to XML
                Thread waitButtonThread = new Thread(WaitButton);
                waitButtonThread.IsBackground = true;
                waitButtonThread.Start();
                Application.DoEvents();
            }
        }

        void writeLargeRatio()
        {
            //Writes the large ratio property for every item in the encoder collection
            //This should be changed later to a write once hier in the hierarchy
            foreach (EncoderItem ei in encoderItems.EncoderItems)
            {
                ei.LargeRatio = largeRatio;
            }

            //Another background thread is called to save encoder items to XML.
            Thread ExecuteSaveThread = new Thread(ExecuteSave);
            ExecuteSaveThread.IsBackground = true;
            ExecuteSaveThread.Start();
            Application.DoEvents();
        }

        public void GlobalSave()
        {
            if (this.InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    btnSave.ForeColor = Color.Red;
                    btnSave.Text = "Wait...";
                    btnSave.Enabled = false;

                    foreach (Control control in this.Controls)
                    {
                        if (control.GetType() == typeof(Button))
                        {
                            ((Button)(control)).Enabled = false;
                        }
                    }

                    Thread ExecuteSaveThread = new Thread(ExecuteSave);
                    ExecuteSaveThread.IsBackground = true;
                    ExecuteSaveThread.Start();
                    Application.DoEvents();
                });
            }
            else
            {
                btnSave.ForeColor = Color.Red;
                btnSave.Text = "Wait...";
                btnSave.Enabled = false;

                foreach (Control control in this.Controls)
                {
                    if (control.GetType() == typeof(Button))
                    {
                        ((Button)(control)).Enabled = false;
                    }
                }

                Thread ExecuteSaveThread = new Thread(ExecuteSave);
                ExecuteSaveThread.IsBackground = true;
                ExecuteSaveThread.Start();
                Application.DoEvents();
            }
        }

        public void ExecuteSave()
        {
            //Save encoder items to XML
            SaveEncoderDictionary(lensDirectory + "\\" + camera.LensName + ".xml");
            saved = true;
            Thread.Sleep(1500);

            //Update parent form lensname, dictionary and controls. Then close this form
            if (this.InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    parentForm.currentLensName = camera.LensName;

                    if (encoderItems.EncoderItems.Count > 1)
                    {
                        parentForm.encoderDictionaryLoaded = true;
                        parentForm.LoadEncoderDictionary(lensDirectory + "\\" + camera.LensName + ".xml");
                        parentForm.UpdateDslrControls();
                    }

                    largeCalc = true;

                    FindAndMoveMsgBox("Saved", this.Location.X + this.Height / 2, this.Location.Y + this.Width / 8);
                    if (MessageBox.Show(this, "Save Complete.", "Saved") == System.Windows.Forms.DialogResult.OK)
                    {
                        this.Close();
                    }
                });
            }
            else
            {
                parentForm.currentLensName = camera.LensName;
                parentForm.encoderDictionaryLoaded = true;
                parentForm.LoadEncoderDictionary(lensDirectory + "\\" + camera.LensName + ".xml");
                parentForm.UpdateDslrControls();

                largeCalc = true;

                FindAndMoveMsgBox("Saved", this.Location.X + this.Height / 2, this.Location.Y + this.Width / 8);
                if (MessageBox.Show(this, "Save Complete.", "Saved") == System.Windows.Forms.DialogResult.OK)
                {
                    this.Close();
                }
            }
        }

        void WaitButton()
        {
            //This runs after the large ratio is calculated and the user clicks Save. We set the Save button text to a red Wait, disable it, and set all buttons to disabled
            if (this.InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    btnSave.ForeColor = Color.Red;
                    btnSave.Text = "Wait...";
                    btnSave.Enabled = false;

                    foreach (Control control in this.Controls)
                    {
                        if (control.GetType() == typeof(Button))
                        {
                            ((Button)(control)).Enabled = false;
                        }
                    }
                });
            }
            else
            {
                btnSave.ForeColor = Color.Red;
                btnSave.Text = "Wait...";
                btnSave.Enabled = false;

                foreach (Control control in this.Controls)
                {
                    if (control.GetType() == typeof(Button))
                    {
                        ((Button)(control)).Enabled = false;
                    }
                }
            }

            //Another background thread is called to write the large ratio property for every item in the encoder collection and save to XML.
            //This should be changed later to a write once higher in the hierarchy
            Thread writeLargeRatioThread = new Thread(writeLargeRatio);
            writeLargeRatioThread.IsBackground = true;
            writeLargeRatioThread.Start();
            Application.DoEvents();
        }

        void SaveEncoderDictionary(string fileName)
        {
            //Serialize encoder items to XML
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

        private void btnReset_Click(object sender, EventArgs e)
        {
            //Reset form controls and variables to initial state
            //TODO: Test this again
            if (bgWorkerNear1.IsBusy)
            {
                bgWorkerNear1.CancelAsync();
                btnNear1.Text = "<";
                btnFar1.Enabled = true;
                btnNear1.ForeColor = Color.Black;
            }

            if (bgWorkerNear2.IsBusy)
            {
                bgWorkerNear2.CancelAsync();
                btnNear2.Text = "<<";
                btnFar2.Enabled = true;
                btnNear2.ForeColor = Color.Black;
            }

            if (bgWorkerNear3.IsBusy)
            {
                bgWorkerNear3.CancelAsync();
                btnNear3.Text = "<<<";
                btnFar3.Enabled = true;
                btnNear3.ForeColor = Color.Black;
            }

            if (bgWorkerFar1.IsBusy)
            {
                bgWorkerFar1.CancelAsync();
                btnFar1.Text = ">";
                btnNear1.Enabled = true;
                btnFar1.ForeColor = Color.Black;
            }

            if (bgWorkerFar2.IsBusy)
            {
                bgWorkerFar2.CancelAsync();
                btnFar2.Text = ">>";
                btnNear2.Enabled = true;
                btnFar2.ForeColor = Color.Black;
            }

            if (bgWorkerFar3.IsBusy)
            {
                bgWorkerFar3.CancelAsync();
                btnFar3.Text = ">>>";
                btnNear3.Enabled = true;
                btnFar3.ForeColor = Color.Black;
            }

            smallCalc = false;
            mediumCalc = false;
            largeCalc = false;

            btnAddDistance.Text = "Add Distance Mark";
            btnAddDistance.ForeColor = Color.Black;
            mediumRatio = 0;
            largeRatio = 0;

            lblDistanceMarker.Visible = true;
            txtDistance.Visible = true;
            lblFt.Visible = true;
            btnInfinity.Visible = true;
            btnInfinityMax.Visible = true;
            btnSave.Enabled = true;
            btnSave.Text = "Next";

            stepCounter = 0;
            smallStepCounter = 0;
            encoderItems.EncoderItems.Clear();
            encoderList.Clear();
            calibrationDictionary.Clear();
            txtDistance.Text = string.Empty;
            txtStepCounter.Text = string.Empty;
            trvDistanceMark.Nodes.Clear();

            btnInfinity.Enabled = true;
            btnInfinityMax.Enabled = false;
            btnAddDistance.Enabled = true;
            btnIncrease.Enabled = true;
            btnDecrease.Enabled = true;
            btnFar1.Enabled = true;
            btnNear1.Enabled = true;

            rdoSmall.Enabled = true;
            rdoSmall.Checked = true;
            rdoSmall_Click(sender, e);
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

        private void rdoSmall_Click(object sender, EventArgs e)
        {
            //Resets the step counter and enables the small step button group and disables the others and their radio buttons. Sets rackburst to default and calls its click method manually
            stepCounter = 0;

            btnFar1.Enabled = true;
            btnNear1.Enabled = true;

            rdoMedium.Enabled = false;
            btnFar2.Enabled = false;
            btnNear2.Enabled = false;

            rdoLarge.Enabled = false;
            btnFar3.Enabled = false;
            btnNear3.Enabled = false;

            rdoRackBurst.Checked = true;
            rdoRackBurst_Click(new object(), new EventArgs());
        }

        private void rdoMedium_Click(object sender, EventArgs e)
        {
            //Resets the step counter and enables the medium step button group and disables the others and their radio buttons. Sets rackburst to default and calls its click method manually
            stepCounter = 0;

            rdoSmall.Enabled = false;
            btnFar1.Enabled = false;
            btnNear1.Enabled = false;

            btnFar2.Enabled = true;
            btnNear2.Enabled = true;

            rdoLarge.Enabled = false;
            btnFar3.Enabled = false;
            btnNear3.Enabled = false;

            rdoRackBurst.Checked = true;
            rdoRackBurst_Click(new object(), new EventArgs());
        }

        private void rdoLarge_Click(object sender, EventArgs e)
        {
            //Resets the step counter and enables the large step button group and disables the others and their radio buttons. Sets rackburst to default and calls its click methoda manually
            stepCounter = 0;

            rdoSmall.Enabled = false;
            btnFar1.Enabled = false;
            btnNear1.Enabled = false;

            rdoMedium.Enabled = false;
            btnFar2.Enabled = false;
            btnNear2.Enabled = false;

            btnFar3.Enabled = true;
            btnNear3.Enabled = true;

            rdoRackBurst.Checked = true;
            rdoRackBurst_Click(new object(), new EventArgs());
        }

        private void rdoRackBurst_Click(object sender, EventArgs e)
        {
            //Set the singlestep flag to false so the rack burst condition logic is executed
            singleStep = false;
        }

        private void rdoSingleStep_Click(object sender, EventArgs e)
        {
            //Set the singlestep flag to false so the single step condition logic is executed
            singleStep = true;
        }

        private void btnIncrease_Click(object sender, EventArgs e)
        {
            //Allows the user to correct small over and undershoots when at the end of the lens movement. Modifies small step counter or regular step counter where appropriate
            if (smallStepCounter > 0)
            {
                smallStepCounter++;
            }
            else
            {
                stepCounter++;
            }
        }

        private void btnDecrease_Click(object sender, EventArgs e)
        {
            //Allows the user to correct small over and undershoots when at the end of the lens movement. Modifies small step counter or regular step counter where appropriate
            if (smallStepCounter > 0)
            {
                smallStepCounter--;
            }
            else
            {
                if (stepCounter > 0)
                    stepCounter--;
            }
        }

        private void btnInfinity_Click(object sender, EventArgs e)
        {
            //Clears the distance text box and puts in the infinity symbol
            txtDistance.Text = string.Empty;
            txtDistance.Text = "\u221E";
        }

        private void btnInfinityMax_Click(object sender, EventArgs e)
        {
            //Clears the distance text box and puts in the infinity symbol with max subtext
            txtDistance.Text = string.Empty;
            txtDistance.Text = "\u221E" + " " + "\u1DAC\u1D43\u02E3";
        }

        private void btnBenchmarks_Click(object sender, EventArgs e)
        {
            //Launches the benchmark tool dialog
            FindAndMoveMsgBox("Benchmark", this.Location.X + this.Height / 2, this.Location.Y + this.Width / 8);
            Benchmark benchmark = new Benchmark(this, camera, encoderItems);
            benchmark.ShowDialog(this);
        }

        private void DSLRCalibration_FormClosed(object sender, FormClosedEventArgs e)
        {
            //Restarts the parent form's lens play manager background thread
            if (!parentForm.lensPlayManagerThread.IsAlive && encoderItems.EncoderItems.Count > 1)
            {
                parentForm.lensPlayManagerThread = new Thread(parentForm.LensPlayManager);
                parentForm.lensPlayManagerThread.IsBackground = true;
                parentForm.lensPlayManagerThread.Start();
            }

            //Sets the trackbar to the beginning if calibration data is saved
            if (saved)
            {
                if (parentForm.InvokeRequired)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        parentForm.tbFocus.Value = parentForm.tbFocus.Minimum;
                    });
                }
                else
                {
                    parentForm.tbFocus.Value = parentForm.tbFocus.Minimum;
                }
            }
        }

        private void btnLensPlayComp_Click(object sender, EventArgs e)
        {
            //Launches the lens play compensation tool dialog
            FindAndMoveMsgBox("Lens Play Compensation", this.Location.X + this.Height / 2, this.Location.Y + this.Width / 8);
            LensPlayCompensation lensPlayCompensation = new LensPlayCompensation(parentForm, this);
            parentForm.tbFocus.Value = parentForm.tbFocus.Minimum;
            lensPlayCompensation.ShowDialog(this);
        }
    }
}
