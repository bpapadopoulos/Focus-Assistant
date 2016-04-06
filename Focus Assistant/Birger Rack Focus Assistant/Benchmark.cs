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
    public partial class Benchmark : Form
    {
        bool benchMarking = false;
        int smallDelay;
        int medDelay;
        int lrgDelay;
        Camera camera;
        DSLRCalibration parentForm;
        EncoderItemList encoderItems = new EncoderItemList();

        BackgroundWorker bgWorkerSmall = new BackgroundWorker();
        BackgroundWorker bgWorkerMed = new BackgroundWorker();
        BackgroundWorker bgWorkerLrg = new BackgroundWorker();

        public Benchmark(DSLRCalibration parentFormIn, Camera cameraIn, EncoderItemList encoderItemListIn)
        {
            InitializeComponent();

            parentForm = parentFormIn;
            camera = cameraIn;
            encoderItems = encoderItemListIn;

            camera.OnPropertyChanged += new CanonCameraAppLib.PropertyChangedEventHandler(camera_OnPropertyChanged);
            camera.OnShutdown += new ShutdownEventHandler(camera_OnShutdown);

            //Step size benchmark background workers
            bgWorkerLrg.DoWork += new DoWorkEventHandler(bgWorkerLrg_DoWork);
            bgWorkerLrg.WorkerSupportsCancellation = true;

            bgWorkerMed.DoWork += new DoWorkEventHandler(bgWorkerMed_DoWork);
            bgWorkerMed.WorkerSupportsCancellation = true;

            bgWorkerSmall.DoWork += new DoWorkEventHandler(bgWorkerSmall_DoWork);
            bgWorkerSmall.WorkerSupportsCancellation = true;

            //Populate controls to reflect current delay settings
            tbDelay.Value = encoderItems.SmallDelay;
            txtSmallDelay.Text = encoderItems.SmallDelay.ToString();
            txtMedDelay.Text = encoderItems.MedDelay.ToString();
            txtLrgDelay.Text = encoderItems.LrgDelay.ToString();

            //Start thread that disables controls if benchmarking is taking place
            Thread disableControlsThread = new Thread(DisableControls);
            disableControlsThread.IsBackground = true;
            disableControlsThread.Start();

            //Start thread that enables controls if benchmarking is not taking place
            Thread enableControlsThread = new Thread(EnableControls);
            enableControlsThread.IsBackground = true;
            enableControlsThread.Start();
        }

        void camera_OnShutdown(Camera sender, ShutdownEventArgs e)
        {
            //Detects if camera is shut down
            this.Close();
            parentForm.camera_OnShutdown(sender, e);
        }

        void camera_OnPropertyChanged(Camera sender, CanonCameraAppLib.PropertyChangedEventArgs e)
        {
            //Detects if liveview is lost
            if (!camera.IsInLiveViewMode)
            {
                this.Close();
                parentForm.camera_OnPropertyChanged(sender, e);
            }
        }

        void bgWorkerSmall_DoWork(object sender, DoWorkEventArgs e)
        {
            //Sets benchmarking flag to true and drives lens far one small step every two seconds
            benchMarking = true;

            for (int x = 0; x < 95; x++)
            {
                uint result;
                if (camera != null)
                {
                    result = camera.DriveLensFarOne();
                }

                Thread.Sleep(2000);
            }

            //Drives lens back toward starting position one small step every preset delay interval
            for (int x = 0; x < 95; x++)
            {
                uint result;
                if (camera != null)
                {
                    result = camera.DriveLensNearOne();
                }

                Thread.Sleep(smallDelay);
            }

            //Cancels this worker
            bgWorkerSmall.CancelAsync();
            //Sets benchmarking flag to false
            benchMarking = false;

            if (bgWorkerSmall.CancellationPending)
            {
                e.Cancel = true;
            }

            //Manually calls small radio click event
            rdoSmall_Click(sender, e);
        }

        void bgWorkerMed_DoWork(object sender, DoWorkEventArgs e)
        {
            //Sets benchmarking flag to true and drives lens far one medium step every two seconds
            benchMarking = true;

            for (int x = 0; x < 25; x++)
            {
                uint result;
                if (camera != null)
                {
                    result = camera.DriveLensFarTwo();
                }

                Thread.Sleep(2000);
            }

            //Drives lens back toward starting position one medium step every preset delay interval
            for (int x = 0; x < 25; x++)
            {
                uint result;
                if (camera != null)
                {
                    result = camera.DriveLensNearTwo();
                }

                Thread.Sleep(medDelay);
            }

            //Cancels this worker
            bgWorkerMed.CancelAsync();
            //Sets benchmarking flag to false
            benchMarking = false;

            if (bgWorkerMed.CancellationPending)
            {
                e.Cancel = true;
            }

            //Manually calls medium radio click event
            rdoMedium_Click(sender, e);
        }

        void bgWorkerLrg_DoWork(object sender, DoWorkEventArgs e)
        {
            //Sets benchmarking flag to true and drives lens far one large step every two seconds
            benchMarking = true;

            for (int x = 0; x < 5; x++)
            {
                uint result;
                if (camera != null)
                {
                    result = camera.DriveLensFarThree();
                }

                Thread.Sleep(2000);
            }

            //Drives lens back toward starting position one large step every preset delay interval
            for (int x = 0; x < 5; x++)
            {
                uint result;
                if (camera != null)
                {
                    result = camera.DriveLensNearThree();
                }

                Thread.Sleep(lrgDelay);
            }

            //Cancels this worker
            bgWorkerLrg.CancelAsync();
            //Sets benchmarking flag to false
            benchMarking = false;

            if (bgWorkerLrg.CancellationPending)
            {
                e.Cancel = true;
            }

            //Manually calls large radio click event
            rdoLarge_Click(sender, e);
        }

        void EnableControls()
        {
            while (1 == 1)
            {
                //Infinite loop that checks to see if benchmark is not taking place. 
                //If not, it checks the radio buttons and calls the click method on the currently selected one. Then rests for 2 seconds
                if (!benchMarking)
                {
                    if (rdoLarge.Checked)
                    {
                        rdoLarge_Click(new object(), new EventArgs());
                    }
                    else if (rdoMedium.Checked)
                    {
                        rdoMedium_Click(new object(), new EventArgs());
                    }
                    else if (rdoSmall.Checked)
                    {
                        rdoSmall_Click(new object(), new EventArgs());
                    }

                    Thread.Sleep(2000);
                }
            }
        }

        void DisableControls()
        {
            while (1 == 1)
            {
                //Infinite loop that checks to see if benchmark is taking place. 
                //If so, it disables all buttons. Then rests for a tenth of a second
                if (benchMarking)
                {
                    if (!this.InvokeRequired)
                    {
                        grpStepSize.Enabled = false;

                        foreach (Control control in this.Controls)
                        {
                            if (control.GetType() == typeof(Button))
                            {
                                ((Button)control).Enabled = false;
                            }
                        }
                    }
                    else
                    {
                        BeginInvoke((MethodInvoker)delegate
                        {
                            grpStepSize.Enabled = false;

                            foreach (Control control in this.Controls)
                            {
                                if (control.GetType() == typeof(Button))
                                {
                                    ((Button)control).Enabled = false;
                                }
                            }
                        });
                    }
                }

                Thread.Sleep(125);
            }
        }

        private void rdoMedium_Click(object sender, EventArgs e)
        {
            //Sets the trackbar to reflect the value in the med delay text box and disables the non medium benchmark buttons
            //If benchmarking in progress, button text gets Wait message
            if (this.InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    tbDelay.Value = int.Parse(txtMedDelay.Text);

                    btnLrgBench.Enabled = false;
                    btnMedBench.Enabled = true;
                    btnSmallBench.Enabled = false;

                    grpStepSize.Enabled = true;

                    if (benchMarking)
                    {
                        tbDelay.Enabled = false;
                        btnMedBench.Text = "Wait...";
                    }
                    else
                    {
                        tbDelay.Enabled = true;
                        btnMedBench.Text = "Start Medium Step Benchmark";
                    }

                    btnSave.Enabled = true;
                });
            }
            else
            {
                tbDelay.Value = int.Parse(txtMedDelay.Text);

                btnLrgBench.Enabled = false;
                btnMedBench.Enabled = true;
                btnSmallBench.Enabled = false;

                grpStepSize.Enabled = true;

                if (benchMarking)
                {
                    tbDelay.Enabled = false;
                    btnMedBench.Text = "Wait...";
                }
                else
                {
                    tbDelay.Enabled = true;
                    btnMedBench.Text = "Start Medium Step Benchmark";
                }

                btnSave.Enabled = true;
            }
        }

        private void rdoLarge_Click(object sender, EventArgs e)
        {
            //Sets the trackbar to reflect the value in the large delay text box and disables the non large benchmark buttons
            //If benchmarking in progress, button text gets Wait message
            if (this.InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    tbDelay.Value = int.Parse(txtLrgDelay.Text);

                    btnLrgBench.Enabled = true;
                    btnMedBench.Enabled = false;
                    btnSmallBench.Enabled = false;

                    grpStepSize.Enabled = true;

                    if (benchMarking)
                    {
                        tbDelay.Enabled = false;
                        btnLrgBench.Text = "Wait...";
                    }
                    else
                    {
                        tbDelay.Enabled = true;
                        btnLrgBench.Text = "Start Large Step Benchmark";
                    }
                    btnSave.Enabled = true;
                });
            }
            else
            {
                tbDelay.Value = int.Parse(txtLrgDelay.Text);

                btnLrgBench.Enabled = true;
                btnMedBench.Enabled = false;
                btnSmallBench.Enabled = false;

                grpStepSize.Enabled = true;

                if (benchMarking)
                {
                    tbDelay.Enabled = false;
                    btnLrgBench.Text = "Wait...";
                }
                else
                {
                    tbDelay.Enabled = true;
                    btnLrgBench.Text = "Start Large Step Benchmark";
                }
                btnSave.Enabled = true;
            }
        }

        private void rdoSmall_Click(object sender, EventArgs e)
        {
            //Sets the trackbar to reflect the value in the small delay text box and disables the non small benchmark buttons
            //If benchmarking in progress, button text gets Wait message
            if (this.InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    tbDelay.Value = int.Parse(txtSmallDelay.Text);

                    btnLrgBench.Enabled = false;
                    btnMedBench.Enabled = false;
                    btnSmallBench.Enabled = true;

                    grpStepSize.Enabled = true;

                    if (benchMarking)
                    {
                        tbDelay.Enabled = false;
                        btnSmallBench.Text = "Wait...";
                    }
                    else
                    {
                        tbDelay.Enabled = true;
                        btnSmallBench.Text = "Start Small Step Benchmark";
                    }
                    btnSave.Enabled = true;

                    Application.DoEvents();
                });
            }
            else
            {
                tbDelay.Value = int.Parse(txtSmallDelay.Text);

                btnLrgBench.Enabled = false;
                btnMedBench.Enabled = false;
                btnSmallBench.Enabled = true;

                grpStepSize.Enabled = true;

                if (benchMarking)
                {
                    tbDelay.Enabled = false;
                    btnSmallBench.Text = "Wait...";
                }
                else
                {
                    tbDelay.Enabled = true;
                    btnSmallBench.Text = "Start Small Step Benchmark";
                }
                btnSave.Enabled = true;
            }
        }

        private void btnSmallBench_Click(object sender, EventArgs e)
        {
            //Lauches the small benchmark background worker and sets the controls to busy state
            if (!bgWorkerSmall.IsBusy)
            {
                tbDelay.Enabled = false;
                smallDelay = int.Parse(txtSmallDelay.Text);
                btnSmallBench.Text = "Wait...";
                bgWorkerSmall.RunWorkerAsync();
            }
        }

        private void btnMedBench_Click(object sender, EventArgs e)
        {
            //Lauches the medium benchmark background worker and sets the controls to busy state
            if (!bgWorkerMed.IsBusy)
            {
                tbDelay.Enabled = false;
                medDelay = int.Parse(txtMedDelay.Text);
                btnMedBench.Text = "Wait...";
                bgWorkerMed.RunWorkerAsync();
            }
        }

        private void btnLrgBench_Click(object sender, EventArgs e)
        {
            //Lauches the large benchmark background worker and sets the controls to busy state
            if (!bgWorkerLrg.IsBusy)
            {
                tbDelay.Enabled = false;
                lrgDelay = int.Parse(txtLrgDelay.Text);
                btnLrgBench.Text = "Wait...";
                bgWorkerLrg.RunWorkerAsync();
            }
        }

        private void tbDelay_Scroll(object sender, EventArgs e)
        {
            //Updates the appropriate delay textbox with the new trackbar value
            if (rdoLarge.Checked)
            {
                txtLrgDelay.Text = tbDelay.Value.ToString();
            }
            else if (rdoMedium.Checked)
            {
                txtMedDelay.Text = tbDelay.Value.ToString();
            }
            else
            {
                txtSmallDelay.Text = tbDelay.Value.ToString();
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            //Updates the small med and large delay properties for the current encoderitems collection in this and parent forms
            //Sets Save button text to busy state. Launches background thread to save XML
            btnSave.Text = "Wait....";

            encoderItems.LrgDelay = int.Parse(txtLrgDelay.Text);
            encoderItems.MedDelay = int.Parse(txtMedDelay.Text);
            encoderItems.SmallDelay = int.Parse(txtSmallDelay.Text);

            parentForm.encoderItems.LrgDelay = encoderItems.LrgDelay;
            parentForm.encoderItems.MedDelay = encoderItems.MedDelay;
            parentForm.encoderItems.SmallDelay = encoderItems.SmallDelay;

            Thread saveThread = new Thread(Save);
            saveThread.IsBackground = true;
            saveThread.Start();

            this.Close();
        }

        void Save()
        {
            //Calls parent global save method which disables necessary buttons, saves xml, and performs necessary operations in grandparent form 
            parentForm.GlobalSave();
        }

        private void Benchmark_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Updates parent form delay values on closing
            parentForm.encoderItems.LrgDelay = encoderItems.LrgDelay;
            parentForm.encoderItems.MedDelay = encoderItems.MedDelay;
            parentForm.encoderItems.SmallDelay = encoderItems.SmallDelay;
        }
    }
}
