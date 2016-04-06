using System;
using ManagedWinapi;

namespace Birger_Rack_Focus_Assistant
{
    partial class frmFocusPuller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.btnSetStartFocus = new System.Windows.Forms.Button();
            this.btnSetEndFocus = new System.Windows.Forms.Button();
            this.txtSetStartFocus = new System.Windows.Forms.TextBox();
            this.txtSetEndFocus = new System.Windows.Forms.TextBox();
            this.btnRackFocus = new System.Windows.Forms.Button();
            this.txtStartingThumb = new System.Windows.Forms.TextBox();
            this.txtEndingThumb = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtStartingThumbActual = new System.Windows.Forms.TextBox();
            this.txtEndingThumbActual = new System.Windows.Forms.TextBox();
            this.btnTimeFocusMove = new System.Windows.Forms.Button();
            this.txtFocusTime = new System.Windows.Forms.TextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.txtSetFocusDuration = new System.Windows.Forms.TextBox();
            this.btnSetFocusDuration = new System.Windows.Forms.Button();
            this.txtFocusDuration = new System.Windows.Forms.TextBox();
            this.lblFormattedTimespan = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.btnAddToBatch = new System.Windows.Forms.Button();
            this.btnJumpToStart = new System.Windows.Forms.Button();
            this.btnJumpToEnd = new System.Windows.Forms.Button();
            this.btnSwap = new System.Windows.Forms.Button();
            this.chkEnableBeeps = new System.Windows.Forms.CheckBox();
            this.msMenuStrip = new System.Windows.Forms.MenuStrip();
            this.miFile = new System.Windows.Forms.ToolStripMenuItem();
            this.miOpenBatch = new System.Windows.Forms.ToolStripMenuItem();
            this.miSaveBatch = new System.Windows.Forms.ToolStripMenuItem();
            this.miNewBatch = new System.Windows.Forms.ToolStripMenuItem();
            this.dlgSave = new System.Windows.Forms.SaveFileDialog();
            this.dlgOpen = new System.Windows.Forms.OpenFileDialog();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnUp = new System.Windows.Forms.Button();
            this.btnDown = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCopy = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnBatchRackFocus = new System.Windows.Forms.Button();
            this.btnRecord = new System.Windows.Forms.Button();
            this.recordTimer = new System.Windows.Forms.Timer(this.components);
            this.btnSwapIris = new System.Windows.Forms.Button();
            this.btnJumpToEndIris = new System.Windows.Forms.Button();
            this.btnJumpToStartIris = new System.Windows.Forms.Button();
            this.txtEndingThumbIris = new System.Windows.Forms.TextBox();
            this.txtStartingThumbIris = new System.Windows.Forms.TextBox();
            this.txtSetEndIris = new System.Windows.Forms.TextBox();
            this.txtSetStartIris = new System.Windows.Forms.TextBox();
            this.btnSetEndIris = new System.Windows.Forms.Button();
            this.btnSetStartIris = new System.Windows.Forms.Button();
            this.btnRangefinder = new System.Windows.Forms.Button();
            this.tbFocus = new System.Windows.Forms.TrackBar();
            this.btnConnect = new System.Windows.Forms.Button();
            this.txtTrackbarValue = new System.Windows.Forms.TextBox();
            this.tbIris = new System.Windows.Forms.TrackBar();
            this.txtIrisBarValue = new System.Windows.Forms.TextBox();
            this.btnLearnFocusStops = new System.Windows.Forms.Button();
            this.lblIrisBarValue = new System.Windows.Forms.Label();
            this.lblTrackbarValue = new System.Windows.Forms.Label();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.txtStepCounter = new System.Windows.Forms.TextBox();
            this.lblStepCounter = new System.Windows.Forms.Label();
            this.txtStepCounterEx = new System.Windows.Forms.TextBox();
            this.lblStepCounterEx = new System.Windows.Forms.Label();
            this.prgBarFocus = new System.Windows.Forms.ProgressBar();
            this.txtCurrentThumb = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.msMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbFocus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbIris)).BeginInit();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnSetStartFocus
            // 
            this.btnSetStartFocus.Enabled = false;
            this.btnSetStartFocus.Location = new System.Drawing.Point(5, 104);
            this.btnSetStartFocus.Name = "btnSetStartFocus";
            this.btnSetStartFocus.Size = new System.Drawing.Size(118, 23);
            this.btnSetStartFocus.TabIndex = 11;
            this.btnSetStartFocus.Text = "Set Starting Focus";
            this.btnSetStartFocus.UseVisualStyleBackColor = true;
            this.btnSetStartFocus.Click += new System.EventHandler(this.btnSetStartFocus_Click);
            // 
            // btnSetEndFocus
            // 
            this.btnSetEndFocus.Enabled = false;
            this.btnSetEndFocus.Location = new System.Drawing.Point(5, 163);
            this.btnSetEndFocus.Name = "btnSetEndFocus";
            this.btnSetEndFocus.Size = new System.Drawing.Size(118, 23);
            this.btnSetEndFocus.TabIndex = 12;
            this.btnSetEndFocus.Text = "Set Ending Focus";
            this.btnSetEndFocus.UseVisualStyleBackColor = true;
            this.btnSetEndFocus.Click += new System.EventHandler(this.btnSetEndFocus_Click);
            // 
            // txtSetStartFocus
            // 
            this.txtSetStartFocus.Enabled = false;
            this.txtSetStartFocus.Location = new System.Drawing.Point(129, 104);
            this.txtSetStartFocus.Name = "txtSetStartFocus";
            this.txtSetStartFocus.Size = new System.Drawing.Size(54, 20);
            this.txtSetStartFocus.TabIndex = 13;
            // 
            // txtSetEndFocus
            // 
            this.txtSetEndFocus.Enabled = false;
            this.txtSetEndFocus.Location = new System.Drawing.Point(129, 165);
            this.txtSetEndFocus.Name = "txtSetEndFocus";
            this.txtSetEndFocus.Size = new System.Drawing.Size(54, 20);
            this.txtSetEndFocus.TabIndex = 14;
            // 
            // btnRackFocus
            // 
            this.btnRackFocus.Enabled = false;
            this.btnRackFocus.Location = new System.Drawing.Point(531, 396);
            this.btnRackFocus.Name = "btnRackFocus";
            this.btnRackFocus.Size = new System.Drawing.Size(108, 23);
            this.btnRackFocus.TabIndex = 15;
            this.btnRackFocus.Text = "Rack Focus";
            this.btnRackFocus.UseVisualStyleBackColor = true;
            this.btnRackFocus.Click += new System.EventHandler(this.btnRackFocus_Click);
            // 
            // txtStartingThumb
            // 
            this.txtStartingThumb.Enabled = false;
            this.txtStartingThumb.Location = new System.Drawing.Point(189, 104);
            this.txtStartingThumb.Name = "txtStartingThumb";
            this.txtStartingThumb.Size = new System.Drawing.Size(40, 20);
            this.txtStartingThumb.TabIndex = 16;
            // 
            // txtEndingThumb
            // 
            this.txtEndingThumb.Enabled = false;
            this.txtEndingThumb.Location = new System.Drawing.Point(189, 165);
            this.txtEndingThumb.Name = "txtEndingThumb";
            this.txtEndingThumb.Size = new System.Drawing.Size(40, 20);
            this.txtEndingThumb.TabIndex = 17;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(140, 373);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(112, 13);
            this.label1.TabIndex = 18;
            this.label1.Text = "Actual Starting Thumb";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(140, 399);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(109, 13);
            this.label2.TabIndex = 19;
            this.label2.Text = "Actual Ending Thumb";
            // 
            // txtStartingThumbActual
            // 
            this.txtStartingThumbActual.Enabled = false;
            this.txtStartingThumbActual.Location = new System.Drawing.Point(253, 370);
            this.txtStartingThumbActual.Name = "txtStartingThumbActual";
            this.txtStartingThumbActual.Size = new System.Drawing.Size(100, 20);
            this.txtStartingThumbActual.TabIndex = 20;
            // 
            // txtEndingThumbActual
            // 
            this.txtEndingThumbActual.Enabled = false;
            this.txtEndingThumbActual.Location = new System.Drawing.Point(253, 396);
            this.txtEndingThumbActual.Name = "txtEndingThumbActual";
            this.txtEndingThumbActual.Size = new System.Drawing.Size(100, 20);
            this.txtEndingThumbActual.TabIndex = 21;
            // 
            // btnTimeFocusMove
            // 
            this.btnTimeFocusMove.Location = new System.Drawing.Point(5, 32);
            this.btnTimeFocusMove.Name = "btnTimeFocusMove";
            this.btnTimeFocusMove.Size = new System.Drawing.Size(120, 23);
            this.btnTimeFocusMove.TabIndex = 30;
            this.btnTimeFocusMove.Text = "Time Focus Move";
            this.btnTimeFocusMove.UseVisualStyleBackColor = true;
            this.btnTimeFocusMove.Click += new System.EventHandler(this.btnTimeFocusMove_Click);
            // 
            // txtFocusTime
            // 
            this.txtFocusTime.Enabled = false;
            this.txtFocusTime.Location = new System.Drawing.Point(131, 34);
            this.txtFocusTime.Name = "txtFocusTime";
            this.txtFocusTime.Size = new System.Drawing.Size(100, 20);
            this.txtFocusTime.TabIndex = 31;
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_tick);
            // 
            // txtSetFocusDuration
            // 
            this.txtSetFocusDuration.Location = new System.Drawing.Point(131, 63);
            this.txtSetFocusDuration.Name = "txtSetFocusDuration";
            this.txtSetFocusDuration.Size = new System.Drawing.Size(100, 20);
            this.txtSetFocusDuration.TabIndex = 33;
            // 
            // btnSetFocusDuration
            // 
            this.btnSetFocusDuration.Location = new System.Drawing.Point(5, 61);
            this.btnSetFocusDuration.Name = "btnSetFocusDuration";
            this.btnSetFocusDuration.Size = new System.Drawing.Size(120, 23);
            this.btnSetFocusDuration.TabIndex = 32;
            this.btnSetFocusDuration.Text = "Set Focus Duration";
            this.btnSetFocusDuration.UseVisualStyleBackColor = true;
            this.btnSetFocusDuration.Click += new System.EventHandler(this.btnSetFocusDuration_Click);
            // 
            // txtFocusDuration
            // 
            this.txtFocusDuration.Enabled = false;
            this.txtFocusDuration.Location = new System.Drawing.Point(240, 63);
            this.txtFocusDuration.Name = "txtFocusDuration";
            this.txtFocusDuration.Size = new System.Drawing.Size(100, 20);
            this.txtFocusDuration.TabIndex = 34;
            // 
            // lblFormattedTimespan
            // 
            this.lblFormattedTimespan.AutoSize = true;
            this.lblFormattedTimespan.Location = new System.Drawing.Point(3, 0);
            this.lblFormattedTimespan.Name = "lblFormattedTimespan";
            this.lblFormattedTimespan.Size = new System.Drawing.Size(0, 13);
            this.lblFormattedTimespan.TabIndex = 35;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Controls.Add(this.lblFormattedTimespan, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(473, 398);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 21F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(52, 21);
            this.tableLayoutPanel1.TabIndex = 36;
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(408, 90);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(167, 277);
            this.listBox1.TabIndex = 37;
            // 
            // btnAddToBatch
            // 
            this.btnAddToBatch.Enabled = false;
            this.btnAddToBatch.Location = new System.Drawing.Point(7, 303);
            this.btnAddToBatch.Name = "btnAddToBatch";
            this.btnAddToBatch.Size = new System.Drawing.Size(132, 23);
            this.btnAddToBatch.TabIndex = 38;
            this.btnAddToBatch.Text = "Add To Batch";
            this.btnAddToBatch.UseVisualStyleBackColor = true;
            this.btnAddToBatch.Click += new System.EventHandler(this.btnAddToBatch_Click);
            // 
            // btnJumpToStart
            // 
            this.btnJumpToStart.Enabled = false;
            this.btnJumpToStart.Location = new System.Drawing.Point(238, 103);
            this.btnJumpToStart.Name = "btnJumpToStart";
            this.btnJumpToStart.Size = new System.Drawing.Size(85, 23);
            this.btnJumpToStart.TabIndex = 39;
            this.btnJumpToStart.Text = "Jump To Start";
            this.btnJumpToStart.UseVisualStyleBackColor = true;
            this.btnJumpToStart.Click += new System.EventHandler(this.btnJumpToStart_Click);
            // 
            // btnJumpToEnd
            // 
            this.btnJumpToEnd.Enabled = false;
            this.btnJumpToEnd.Location = new System.Drawing.Point(238, 162);
            this.btnJumpToEnd.Name = "btnJumpToEnd";
            this.btnJumpToEnd.Size = new System.Drawing.Size(85, 23);
            this.btnJumpToEnd.TabIndex = 40;
            this.btnJumpToEnd.Text = "Jump To End";
            this.btnJumpToEnd.UseVisualStyleBackColor = true;
            this.btnJumpToEnd.Click += new System.EventHandler(this.btnJumpToEnd_Click);
            // 
            // btnSwap
            // 
            this.btnSwap.Enabled = false;
            this.btnSwap.Location = new System.Drawing.Point(5, 134);
            this.btnSwap.Name = "btnSwap";
            this.btnSwap.Size = new System.Drawing.Size(85, 23);
            this.btnSwap.TabIndex = 41;
            this.btnSwap.Text = "Swap";
            this.btnSwap.UseVisualStyleBackColor = true;
            this.btnSwap.Click += new System.EventHandler(this.btnSwap_Click);
            // 
            // chkEnableBeeps
            // 
            this.chkEnableBeeps.AutoSize = true;
            this.chkEnableBeeps.Location = new System.Drawing.Point(408, 375);
            this.chkEnableBeeps.Name = "chkEnableBeeps";
            this.chkEnableBeeps.Size = new System.Drawing.Size(117, 17);
            this.chkEnableBeeps.TabIndex = 42;
            this.chkEnableBeeps.Text = "Enable Beep Assist";
            this.chkEnableBeeps.UseVisualStyleBackColor = true;
            this.chkEnableBeeps.CheckedChanged += new System.EventHandler(this.chkEnableBeeps_CheckedChanged);
            // 
            // msMenuStrip
            // 
            this.msMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miFile});
            this.msMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.msMenuStrip.Name = "msMenuStrip";
            this.msMenuStrip.Size = new System.Drawing.Size(651, 24);
            this.msMenuStrip.TabIndex = 43;
            this.msMenuStrip.Text = "Menu";
            // 
            // miFile
            // 
            this.miFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miOpenBatch,
            this.miSaveBatch,
            this.miNewBatch});
            this.miFile.Name = "miFile";
            this.miFile.Size = new System.Drawing.Size(37, 20);
            this.miFile.Text = "File";
            // 
            // miOpenBatch
            // 
            this.miOpenBatch.Name = "miOpenBatch";
            this.miOpenBatch.Size = new System.Drawing.Size(136, 22);
            this.miOpenBatch.Text = "Open Batch";
            this.miOpenBatch.Click += new System.EventHandler(this.miOpenBatch_Click);
            // 
            // miSaveBatch
            // 
            this.miSaveBatch.Name = "miSaveBatch";
            this.miSaveBatch.Size = new System.Drawing.Size(136, 22);
            this.miSaveBatch.Text = "Save Batch";
            this.miSaveBatch.Click += new System.EventHandler(this.miSaveBatch_Click);
            // 
            // miNewBatch
            // 
            this.miNewBatch.Name = "miNewBatch";
            this.miNewBatch.Size = new System.Drawing.Size(136, 22);
            this.miNewBatch.Text = "New Batch";
            this.miNewBatch.Click += new System.EventHandler(this.miNewBatch_Click);
            // 
            // dlgSave
            // 
            this.dlgSave.FileOk += new System.ComponentModel.CancelEventHandler(this.dlgSave_FileOk);
            // 
            // dlgOpen
            // 
            this.dlgOpen.FileOk += new System.ComponentModel.CancelEventHandler(this.dlgOpen_FileOk);
            // 
            // btnDelete
            // 
            this.btnDelete.Enabled = false;
            this.btnDelete.Location = new System.Drawing.Point(587, 34);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(52, 23);
            this.btnDelete.TabIndex = 44;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnEdit
            // 
            this.btnEdit.Enabled = false;
            this.btnEdit.Location = new System.Drawing.Point(587, 60);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(52, 23);
            this.btnEdit.TabIndex = 45;
            this.btnEdit.Text = "Edit";
            this.btnEdit.UseVisualStyleBackColor = true;
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            // 
            // btnUp
            // 
            this.btnUp.Location = new System.Drawing.Point(587, 90);
            this.btnUp.Name = "btnUp";
            this.btnUp.Size = new System.Drawing.Size(52, 37);
            this.btnUp.TabIndex = 46;
            this.btnUp.Text = "Move Up";
            this.btnUp.UseVisualStyleBackColor = true;
            this.btnUp.Click += new System.EventHandler(this.btnUp_Click);
            // 
            // btnDown
            // 
            this.btnDown.Location = new System.Drawing.Point(587, 330);
            this.btnDown.Name = "btnDown";
            this.btnDown.Size = new System.Drawing.Size(52, 37);
            this.btnDown.TabIndex = 47;
            this.btnDown.Text = "Move Down";
            this.btnDown.UseVisualStyleBackColor = true;
            this.btnDown.Click += new System.EventHandler(this.btnDown_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(88, 332);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 48;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Visible = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCopy
            // 
            this.btnCopy.Location = new System.Drawing.Point(169, 332);
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(75, 23);
            this.btnCopy.TabIndex = 49;
            this.btnCopy.Text = "Copy";
            this.btnCopy.UseVisualStyleBackColor = true;
            this.btnCopy.Visible = false;
            this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(7, 332);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 50;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnBatchRackFocus
            // 
            this.btnBatchRackFocus.Enabled = false;
            this.btnBatchRackFocus.Location = new System.Drawing.Point(531, 371);
            this.btnBatchRackFocus.Name = "btnBatchRackFocus";
            this.btnBatchRackFocus.Size = new System.Drawing.Size(108, 23);
            this.btnBatchRackFocus.TabIndex = 51;
            this.btnBatchRackFocus.Text = "Batch Rack Focus";
            this.btnBatchRackFocus.UseVisualStyleBackColor = true;
            this.btnBatchRackFocus.Click += new System.EventHandler(this.btnBatchRackFocus_Click);
            // 
            // btnRecord
            // 
            this.btnRecord.Enabled = false;
            this.btnRecord.Location = new System.Drawing.Point(495, 34);
            this.btnRecord.Name = "btnRecord";
            this.btnRecord.Size = new System.Drawing.Size(80, 51);
            this.btnRecord.TabIndex = 52;
            this.btnRecord.Text = "Begin Realtime Recording";
            this.btnRecord.UseVisualStyleBackColor = true;
            this.btnRecord.Click += new System.EventHandler(this.btnRecord_Click);
            // 
            // recordTimer
            // 
            this.recordTimer.Tick += new System.EventHandler(this.recordTimer_Tick);
            // 
            // btnSwapIris
            // 
            this.btnSwapIris.Enabled = false;
            this.btnSwapIris.Location = new System.Drawing.Point(7, 234);
            this.btnSwapIris.Name = "btnSwapIris";
            this.btnSwapIris.Size = new System.Drawing.Size(85, 23);
            this.btnSwapIris.TabIndex = 61;
            this.btnSwapIris.Text = "Swap";
            this.btnSwapIris.UseVisualStyleBackColor = true;
            this.btnSwapIris.Click += new System.EventHandler(this.btnSwapIris_Click);
            // 
            // btnJumpToEndIris
            // 
            this.btnJumpToEndIris.Enabled = false;
            this.btnJumpToEndIris.Location = new System.Drawing.Point(240, 262);
            this.btnJumpToEndIris.Name = "btnJumpToEndIris";
            this.btnJumpToEndIris.Size = new System.Drawing.Size(85, 23);
            this.btnJumpToEndIris.TabIndex = 60;
            this.btnJumpToEndIris.Text = "Jump To End";
            this.btnJumpToEndIris.UseVisualStyleBackColor = true;
            this.btnJumpToEndIris.Click += new System.EventHandler(this.btnJumpToEndIris_Click);
            // 
            // btnJumpToStartIris
            // 
            this.btnJumpToStartIris.Enabled = false;
            this.btnJumpToStartIris.Location = new System.Drawing.Point(240, 203);
            this.btnJumpToStartIris.Name = "btnJumpToStartIris";
            this.btnJumpToStartIris.Size = new System.Drawing.Size(85, 23);
            this.btnJumpToStartIris.TabIndex = 59;
            this.btnJumpToStartIris.Text = "Jump To Start";
            this.btnJumpToStartIris.UseVisualStyleBackColor = true;
            this.btnJumpToStartIris.Click += new System.EventHandler(this.btnJumpToStartIris_Click);
            // 
            // txtEndingThumbIris
            // 
            this.txtEndingThumbIris.Enabled = false;
            this.txtEndingThumbIris.Location = new System.Drawing.Point(191, 265);
            this.txtEndingThumbIris.Name = "txtEndingThumbIris";
            this.txtEndingThumbIris.Size = new System.Drawing.Size(40, 20);
            this.txtEndingThumbIris.TabIndex = 58;
            // 
            // txtStartingThumbIris
            // 
            this.txtStartingThumbIris.Enabled = false;
            this.txtStartingThumbIris.Location = new System.Drawing.Point(191, 204);
            this.txtStartingThumbIris.Name = "txtStartingThumbIris";
            this.txtStartingThumbIris.Size = new System.Drawing.Size(40, 20);
            this.txtStartingThumbIris.TabIndex = 57;
            // 
            // txtSetEndIris
            // 
            this.txtSetEndIris.Enabled = false;
            this.txtSetEndIris.Location = new System.Drawing.Point(131, 265);
            this.txtSetEndIris.Name = "txtSetEndIris";
            this.txtSetEndIris.Size = new System.Drawing.Size(52, 20);
            this.txtSetEndIris.TabIndex = 56;
            // 
            // txtSetStartIris
            // 
            this.txtSetStartIris.Enabled = false;
            this.txtSetStartIris.Location = new System.Drawing.Point(131, 204);
            this.txtSetStartIris.Name = "txtSetStartIris";
            this.txtSetStartIris.Size = new System.Drawing.Size(52, 20);
            this.txtSetStartIris.TabIndex = 55;
            // 
            // btnSetEndIris
            // 
            this.btnSetEndIris.Enabled = false;
            this.btnSetEndIris.Location = new System.Drawing.Point(7, 263);
            this.btnSetEndIris.Name = "btnSetEndIris";
            this.btnSetEndIris.Size = new System.Drawing.Size(118, 23);
            this.btnSetEndIris.TabIndex = 54;
            this.btnSetEndIris.Text = "Set Ending Iris";
            this.btnSetEndIris.UseVisualStyleBackColor = true;
            this.btnSetEndIris.Click += new System.EventHandler(this.btnSetEndIris_Click);
            // 
            // btnSetStartIris
            // 
            this.btnSetStartIris.Enabled = false;
            this.btnSetStartIris.Location = new System.Drawing.Point(7, 204);
            this.btnSetStartIris.Name = "btnSetStartIris";
            this.btnSetStartIris.Size = new System.Drawing.Size(118, 23);
            this.btnSetStartIris.TabIndex = 53;
            this.btnSetStartIris.Text = "Set Starting Iris";
            this.btnSetStartIris.UseVisualStyleBackColor = true;
            this.btnSetStartIris.Click += new System.EventHandler(this.btnSetStartIris_Click);
            // 
            // btnRangefinder
            // 
            this.btnRangefinder.Enabled = false;
            this.btnRangefinder.Location = new System.Drawing.Point(408, 34);
            this.btnRangefinder.Name = "btnRangefinder";
            this.btnRangefinder.Size = new System.Drawing.Size(80, 51);
            this.btnRangefinder.TabIndex = 62;
            this.btnRangefinder.Text = "Launch RangeFinder";
            this.btnRangefinder.UseVisualStyleBackColor = true;
            this.btnRangefinder.Click += new System.EventHandler(this.btnRangefinder_Click);
            // 
            // tbFocus
            // 
            this.tbFocus.Enabled = false;
            this.tbFocus.Location = new System.Drawing.Point(5, 488);
            this.tbFocus.Maximum = 16383;
            this.tbFocus.Name = "tbFocus";
            this.tbFocus.Size = new System.Drawing.Size(632, 45);
            this.tbFocus.SmallChange = 3;
            this.tbFocus.TabIndex = 63;
            this.tbFocus.TickFrequency = 0;
            this.tbFocus.ValueChanged += new System.EventHandler(this.tbFocus_ValueChanged);
            this.tbFocus.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbFocus_KeyDown);
            this.tbFocus.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tbFocus_MouseDown);
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(7, 438);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(104, 23);
            this.btnConnect.TabIndex = 64;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // txtTrackbarValue
            // 
            this.txtTrackbarValue.Location = new System.Drawing.Point(251, 306);
            this.txtTrackbarValue.Name = "txtTrackbarValue";
            this.txtTrackbarValue.Size = new System.Drawing.Size(100, 20);
            this.txtTrackbarValue.TabIndex = 65;
            // 
            // tbIris
            // 
            this.tbIris.Enabled = false;
            this.tbIris.Location = new System.Drawing.Point(7, 549);
            this.tbIris.Maximum = 1;
            this.tbIris.Name = "tbIris";
            this.tbIris.Size = new System.Drawing.Size(632, 45);
            this.tbIris.SmallChange = 3;
            this.tbIris.TabIndex = 66;
            this.tbIris.TickFrequency = 0;
            this.tbIris.ValueChanged += new System.EventHandler(this.tbIris_ValueChanged);
            // 
            // txtIrisBarValue
            // 
            this.txtIrisBarValue.Location = new System.Drawing.Point(145, 306);
            this.txtIrisBarValue.Name = "txtIrisBarValue";
            this.txtIrisBarValue.Size = new System.Drawing.Size(100, 20);
            this.txtIrisBarValue.TabIndex = 67;
            // 
            // btnLearnFocusStops
            // 
            this.btnLearnFocusStops.Enabled = false;
            this.btnLearnFocusStops.Location = new System.Drawing.Point(115, 438);
            this.btnLearnFocusStops.Name = "btnLearnFocusStops";
            this.btnLearnFocusStops.Size = new System.Drawing.Size(156, 23);
            this.btnLearnFocusStops.TabIndex = 68;
            this.btnLearnFocusStops.Text = "Learn Focus Scale";
            this.btnLearnFocusStops.UseVisualStyleBackColor = true;
            this.btnLearnFocusStops.Click += new System.EventHandler(this.btnLearnFocusStops_Click);
            // 
            // lblIrisBarValue
            // 
            this.lblIrisBarValue.AutoSize = true;
            this.lblIrisBarValue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblIrisBarValue.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblIrisBarValue.Font = new System.Drawing.Font("Verdana", 15F);
            this.lblIrisBarValue.Location = new System.Drawing.Point(3, 0);
            this.lblIrisBarValue.Name = "lblIrisBarValue";
            this.lblIrisBarValue.Size = new System.Drawing.Size(27, 30);
            this.lblIrisBarValue.TabIndex = 69;
            this.lblIrisBarValue.Text = "0";
            // 
            // lblTrackbarValue
            // 
            this.lblTrackbarValue.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblTrackbarValue.AutoSize = true;
            this.lblTrackbarValue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblTrackbarValue.Font = new System.Drawing.Font("Verdana", 15F);
            this.lblTrackbarValue.Location = new System.Drawing.Point(37, 1);
            this.lblTrackbarValue.Margin = new System.Windows.Forms.Padding(0);
            this.lblTrackbarValue.Name = "lblTrackbarValue";
            this.lblTrackbarValue.Size = new System.Drawing.Size(75, 27);
            this.lblTrackbarValue.TabIndex = 70;
            this.lblTrackbarValue.Text = "0.00ft";
            this.lblTrackbarValue.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Controls.Add(this.lblTrackbarValue, 0, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(488, 447);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(149, 30);
            this.tableLayoutPanel2.TabIndex = 71;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel3.Controls.Add(this.lblIrisBarValue, 0, 0);
            this.tableLayoutPanel3.Location = new System.Drawing.Point(363, 447);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(109, 30);
            this.tableLayoutPanel3.TabIndex = 72;
            // 
            // label7
            // 
            this.label7.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.label7.Location = new System.Drawing.Point(485, 434);
            this.label7.Margin = new System.Windows.Forms.Padding(0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(58, 13);
            this.label7.TabIndex = 73;
            this.label7.Text = "Distance:  ";
            this.label7.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label8
            // 
            this.label8.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.label8.Location = new System.Drawing.Point(360, 434);
            this.label8.Margin = new System.Windows.Forms.Padding(0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(29, 13);
            this.label8.TabIndex = 74;
            this.label8.Text = "Iris:  ";
            this.label8.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // txtStepCounter
            // 
            this.txtStepCounter.Enabled = false;
            this.txtStepCounter.Location = new System.Drawing.Point(118, 455);
            this.txtStepCounter.Name = "txtStepCounter";
            this.txtStepCounter.Size = new System.Drawing.Size(100, 20);
            this.txtStepCounter.TabIndex = 76;
            this.txtStepCounter.Visible = false;
            // 
            // lblStepCounter
            // 
            this.lblStepCounter.AutoSize = true;
            this.lblStepCounter.Location = new System.Drawing.Point(115, 438);
            this.lblStepCounter.Name = "lblStepCounter";
            this.lblStepCounter.Size = new System.Drawing.Size(72, 13);
            this.lblStepCounter.TabIndex = 75;
            this.lblStepCounter.Text = "Step Counter:";
            this.lblStepCounter.Visible = false;
            // 
            // txtStepCounterEx
            // 
            this.txtStepCounterEx.Enabled = false;
            this.txtStepCounterEx.Location = new System.Drawing.Point(226, 455);
            this.txtStepCounterEx.Name = "txtStepCounterEx";
            this.txtStepCounterEx.Size = new System.Drawing.Size(100, 20);
            this.txtStepCounterEx.TabIndex = 78;
            this.txtStepCounterEx.Visible = false;
            // 
            // lblStepCounterEx
            // 
            this.lblStepCounterEx.AutoSize = true;
            this.lblStepCounterEx.Location = new System.Drawing.Point(223, 438);
            this.lblStepCounterEx.Name = "lblStepCounterEx";
            this.lblStepCounterEx.Size = new System.Drawing.Size(87, 13);
            this.lblStepCounterEx.TabIndex = 77;
            this.lblStepCounterEx.Text = "Step Counter Ex:";
            this.lblStepCounterEx.Visible = false;
            // 
            // prgBarFocus
            // 
            this.prgBarFocus.Location = new System.Drawing.Point(8, 463);
            this.prgBarFocus.Name = "prgBarFocus";
            this.prgBarFocus.Size = new System.Drawing.Size(102, 14);
            this.prgBarFocus.TabIndex = 79;
            this.prgBarFocus.Visible = false;
            // 
            // txtCurrentThumb
            // 
            this.txtCurrentThumb.Enabled = false;
            this.txtCurrentThumb.Location = new System.Drawing.Point(8, 395);
            this.txtCurrentThumb.Name = "txtCurrentThumb";
            this.txtCurrentThumb.Size = new System.Drawing.Size(100, 20);
            this.txtCurrentThumb.TabIndex = 81;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 373);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 13);
            this.label3.TabIndex = 80;
            this.label3.Text = "Current Thumb";
            // 
            // frmFocusPuller
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(651, 606);
            this.Controls.Add(this.txtCurrentThumb);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.prgBarFocus);
            this.Controls.Add(this.txtStepCounterEx);
            this.Controls.Add(this.lblStepCounterEx);
            this.Controls.Add(this.txtStepCounter);
            this.Controls.Add(this.lblStepCounter);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.tableLayoutPanel3);
            this.Controls.Add(this.tableLayoutPanel2);
            this.Controls.Add(this.btnLearnFocusStops);
            this.Controls.Add(this.txtIrisBarValue);
            this.Controls.Add(this.tbIris);
            this.Controls.Add(this.txtTrackbarValue);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.tbFocus);
            this.Controls.Add(this.btnRangefinder);
            this.Controls.Add(this.btnSwapIris);
            this.Controls.Add(this.btnJumpToEndIris);
            this.Controls.Add(this.btnJumpToStartIris);
            this.Controls.Add(this.txtEndingThumbIris);
            this.Controls.Add(this.txtStartingThumbIris);
            this.Controls.Add(this.txtSetEndIris);
            this.Controls.Add(this.txtSetStartIris);
            this.Controls.Add(this.btnSetEndIris);
            this.Controls.Add(this.btnSetStartIris);
            this.Controls.Add(this.btnRecord);
            this.Controls.Add(this.btnBatchRackFocus);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnCopy);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnDown);
            this.Controls.Add(this.btnUp);
            this.Controls.Add(this.btnEdit);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.chkEnableBeeps);
            this.Controls.Add(this.btnSwap);
            this.Controls.Add(this.btnJumpToEnd);
            this.Controls.Add(this.btnJumpToStart);
            this.Controls.Add(this.btnAddToBatch);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.txtFocusDuration);
            this.Controls.Add(this.txtSetFocusDuration);
            this.Controls.Add(this.btnSetFocusDuration);
            this.Controls.Add(this.txtFocusTime);
            this.Controls.Add(this.btnTimeFocusMove);
            this.Controls.Add(this.txtEndingThumbActual);
            this.Controls.Add(this.txtStartingThumbActual);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtEndingThumb);
            this.Controls.Add(this.txtStartingThumb);
            this.Controls.Add(this.btnRackFocus);
            this.Controls.Add(this.txtSetEndFocus);
            this.Controls.Add(this.txtSetStartFocus);
            this.Controls.Add(this.btnSetEndFocus);
            this.Controls.Add(this.btnSetStartFocus);
            this.Controls.Add(this.msMenuStrip);
            this.MainMenuStrip = this.msMenuStrip;
            this.Name = "frmFocusPuller";
            this.Text = "Focus Assistant";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmFocusPuller_FormClosing);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.msMenuStrip.ResumeLayout(false);
            this.msMenuStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbFocus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbIris)).EndInit();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSetStartFocus;
        private System.Windows.Forms.Button btnSetEndFocus;
        private System.Windows.Forms.TextBox txtSetStartFocus;
        private System.Windows.Forms.TextBox txtSetEndFocus;
        private System.Windows.Forms.Button btnRackFocus;
        private System.Windows.Forms.TextBox txtStartingThumb;
        private System.Windows.Forms.TextBox txtEndingThumb;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtStartingThumbActual;
        private System.Windows.Forms.TextBox txtEndingThumbActual;
        private System.Windows.Forms.Button btnTimeFocusMove;
        private System.Windows.Forms.TextBox txtFocusTime;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.TextBox txtSetFocusDuration;
        private System.Windows.Forms.Button btnSetFocusDuration;
        private System.Windows.Forms.TextBox txtFocusDuration;
        private System.Windows.Forms.Label lblFormattedTimespan;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Button btnAddToBatch;
        private System.Windows.Forms.Button btnJumpToStart;
        private System.Windows.Forms.Button btnJumpToEnd;
        private System.Windows.Forms.Button btnSwap;
        private System.Windows.Forms.CheckBox chkEnableBeeps;
        private System.Windows.Forms.MenuStrip msMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem miFile;
        private System.Windows.Forms.ToolStripMenuItem miSaveBatch;
        private System.Windows.Forms.ToolStripMenuItem miOpenBatch;
        private System.Windows.Forms.SaveFileDialog dlgSave;
        private System.Windows.Forms.OpenFileDialog dlgOpen;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Button btnUp;
        private System.Windows.Forms.Button btnDown;
        private System.Windows.Forms.ToolStripMenuItem miNewBatch;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCopy;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnBatchRackFocus;
        private System.Windows.Forms.Button btnRecord;
        private System.Windows.Forms.Timer recordTimer;
        private System.Windows.Forms.Button btnSwapIris;
        private System.Windows.Forms.Button btnJumpToEndIris;
        private System.Windows.Forms.Button btnJumpToStartIris;
        private System.Windows.Forms.TextBox txtEndingThumbIris;
        private System.Windows.Forms.TextBox txtStartingThumbIris;
        private System.Windows.Forms.TextBox txtSetEndIris;
        private System.Windows.Forms.TextBox txtSetStartIris;
        private System.Windows.Forms.Button btnSetEndIris;
        private System.Windows.Forms.Button btnSetStartIris;
        private System.Windows.Forms.Button btnRangefinder;
        public System.Windows.Forms.TrackBar tbFocus;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.TextBox txtTrackbarValue;
        public System.Windows.Forms.TrackBar tbIris;
        private System.Windows.Forms.TextBox txtIrisBarValue;
        private System.Windows.Forms.Button btnLearnFocusStops;
        public System.Windows.Forms.Label lblIrisBarValue;
        private System.Windows.Forms.Label lblTrackbarValue;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtStepCounter;
        private System.Windows.Forms.Label lblStepCounter;
        private System.Windows.Forms.TextBox txtStepCounterEx;
        private System.Windows.Forms.Label lblStepCounterEx;
        private System.Windows.Forms.ProgressBar prgBarFocus;
        private System.Windows.Forms.TextBox txtCurrentThumb;
        private System.Windows.Forms.Label label3;
    }
}

