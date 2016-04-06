namespace Birger_Rack_Focus_Assistant
{
    partial class LensPlayCompensation
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
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.lblTrackbarValue = new System.Windows.Forms.Label();
            this.tbFocus = new System.Windows.Forms.TrackBar();
            this.btnSetStartFocus = new System.Windows.Forms.Button();
            this.btnSetEndFocus = new System.Windows.Forms.Button();
            this.btnSetDriftedFocus = new System.Windows.Forms.Button();
            this.txtSetEndFocus = new System.Windows.Forms.TextBox();
            this.txtSetStartFocus = new System.Windows.Forms.TextBox();
            this.txtSetStartFocusDrift = new System.Windows.Forms.TextBox();
            this.btnStartLensPlaySimulation = new System.Windows.Forms.Button();
            this.btnCompensationRatio = new System.Windows.Forms.Button();
            this.txtCompensationRatio = new System.Windows.Forms.TextBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnJumpToEnd = new System.Windows.Forms.Button();
            this.btnJumpToStart = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.lblRackNum = new System.Windows.Forms.Label();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnBeginSetDrifted = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rdoEnabled = new System.Windows.Forms.RadioButton();
            this.rdoDisabled = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.lblEncoderDelta = new System.Windows.Forms.Label();
            this.lblEncoderDrift = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.btnStep1 = new System.Windows.Forms.Button();
            this.btnStep2 = new System.Windows.Forms.Button();
            this.btnStep3 = new System.Windows.Forms.Button();
            this.btnStep4 = new System.Windows.Forms.Button();
            this.chkUseSavedCompensation = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbFocus)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Controls.Add(this.lblTrackbarValue, 0, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(305, 7);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 21F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(149, 21);
            this.tableLayoutPanel2.TabIndex = 82;
            // 
            // lblTrackbarValue
            // 
            this.lblTrackbarValue.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblTrackbarValue.AutoSize = true;
            this.lblTrackbarValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.lblTrackbarValue.Location = new System.Drawing.Point(44, 0);
            this.lblTrackbarValue.Margin = new System.Windows.Forms.Padding(0);
            this.lblTrackbarValue.Name = "lblTrackbarValue";
            this.lblTrackbarValue.Size = new System.Drawing.Size(60, 21);
            this.lblTrackbarValue.TabIndex = 70;
            this.lblTrackbarValue.Text = "0.00ft";
            this.lblTrackbarValue.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // tbFocus
            // 
            this.tbFocus.Location = new System.Drawing.Point(12, 31);
            this.tbFocus.Maximum = 16383;
            this.tbFocus.Name = "tbFocus";
            this.tbFocus.Size = new System.Drawing.Size(446, 45);
            this.tbFocus.SmallChange = 3;
            this.tbFocus.TabIndex = 81;
            this.tbFocus.TickFrequency = 0;
            this.tbFocus.ValueChanged += new System.EventHandler(this.tbFocus_ValueChanged);
            this.tbFocus.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbFocus_KeyDown);
            this.tbFocus.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tbFocus_MouseDown);
            // 
            // btnSetStartFocus
            // 
            this.btnSetStartFocus.Location = new System.Drawing.Point(12, 147);
            this.btnSetStartFocus.Name = "btnSetStartFocus";
            this.btnSetStartFocus.Size = new System.Drawing.Size(118, 23);
            this.btnSetStartFocus.TabIndex = 83;
            this.btnSetStartFocus.Text = "Set Starting Focus";
            this.btnSetStartFocus.UseVisualStyleBackColor = true;
            this.btnSetStartFocus.Click += new System.EventHandler(this.btnSetStartFocus_Click);
            // 
            // btnSetEndFocus
            // 
            this.btnSetEndFocus.Enabled = false;
            this.btnSetEndFocus.Location = new System.Drawing.Point(12, 176);
            this.btnSetEndFocus.Name = "btnSetEndFocus";
            this.btnSetEndFocus.Size = new System.Drawing.Size(118, 23);
            this.btnSetEndFocus.TabIndex = 84;
            this.btnSetEndFocus.Text = "Set Ending Focus";
            this.btnSetEndFocus.UseVisualStyleBackColor = true;
            this.btnSetEndFocus.Click += new System.EventHandler(this.btnSetEndFocus_Click);
            // 
            // btnSetDriftedFocus
            // 
            this.btnSetDriftedFocus.Enabled = false;
            this.btnSetDriftedFocus.Location = new System.Drawing.Point(12, 205);
            this.btnSetDriftedFocus.Name = "btnSetDriftedFocus";
            this.btnSetDriftedFocus.Size = new System.Drawing.Size(118, 23);
            this.btnSetDriftedFocus.TabIndex = 85;
            this.btnSetDriftedFocus.Text = "Set Drifted Focus";
            this.btnSetDriftedFocus.UseVisualStyleBackColor = true;
            this.btnSetDriftedFocus.Click += new System.EventHandler(this.btnSetDriftedFocus_Click);
            // 
            // txtSetEndFocus
            // 
            this.txtSetEndFocus.BackColor = System.Drawing.SystemColors.Control;
            this.txtSetEndFocus.Enabled = false;
            this.txtSetEndFocus.Location = new System.Drawing.Point(171, 178);
            this.txtSetEndFocus.Name = "txtSetEndFocus";
            this.txtSetEndFocus.ReadOnly = true;
            this.txtSetEndFocus.Size = new System.Drawing.Size(54, 20);
            this.txtSetEndFocus.TabIndex = 87;
            // 
            // txtSetStartFocus
            // 
            this.txtSetStartFocus.BackColor = System.Drawing.SystemColors.Control;
            this.txtSetStartFocus.Enabled = false;
            this.txtSetStartFocus.Location = new System.Drawing.Point(171, 149);
            this.txtSetStartFocus.Name = "txtSetStartFocus";
            this.txtSetStartFocus.ReadOnly = true;
            this.txtSetStartFocus.Size = new System.Drawing.Size(54, 20);
            this.txtSetStartFocus.TabIndex = 86;
            // 
            // txtSetStartFocusDrift
            // 
            this.txtSetStartFocusDrift.BackColor = System.Drawing.SystemColors.Control;
            this.txtSetStartFocusDrift.Enabled = false;
            this.txtSetStartFocusDrift.Location = new System.Drawing.Point(171, 207);
            this.txtSetStartFocusDrift.Name = "txtSetStartFocusDrift";
            this.txtSetStartFocusDrift.ReadOnly = true;
            this.txtSetStartFocusDrift.Size = new System.Drawing.Size(54, 20);
            this.txtSetStartFocusDrift.TabIndex = 88;
            // 
            // btnStartLensPlaySimulation
            // 
            this.btnStartLensPlaySimulation.Enabled = false;
            this.btnStartLensPlaySimulation.Location = new System.Drawing.Point(326, 146);
            this.btnStartLensPlaySimulation.Name = "btnStartLensPlaySimulation";
            this.btnStartLensPlaySimulation.Size = new System.Drawing.Size(132, 55);
            this.btnStartLensPlaySimulation.TabIndex = 89;
            this.btnStartLensPlaySimulation.Text = "Start Lens Play Simulation";
            this.btnStartLensPlaySimulation.UseVisualStyleBackColor = true;
            this.btnStartLensPlaySimulation.Click += new System.EventHandler(this.btnStartLensPlaySimulation_Click);
            // 
            // btnCompensationRatio
            // 
            this.btnCompensationRatio.Enabled = false;
            this.btnCompensationRatio.Location = new System.Drawing.Point(12, 234);
            this.btnCompensationRatio.Name = "btnCompensationRatio";
            this.btnCompensationRatio.Size = new System.Drawing.Size(130, 44);
            this.btnCompensationRatio.TabIndex = 90;
            this.btnCompensationRatio.Text = "Calculate Compensation Ratio";
            this.btnCompensationRatio.UseVisualStyleBackColor = true;
            this.btnCompensationRatio.Click += new System.EventHandler(this.btnCompensationRatio_Click);
            // 
            // txtCompensationRatio
            // 
            this.txtCompensationRatio.BackColor = System.Drawing.SystemColors.Control;
            this.txtCompensationRatio.Location = new System.Drawing.Point(171, 247);
            this.txtCompensationRatio.Name = "txtCompensationRatio";
            this.txtCompensationRatio.Size = new System.Drawing.Size(54, 20);
            this.txtCompensationRatio.TabIndex = 91;
            // 
            // btnSave
            // 
            this.btnSave.Enabled = false;
            this.btnSave.Location = new System.Drawing.Point(383, 255);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 92;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnJumpToEnd
            // 
            this.btnJumpToEnd.Enabled = false;
            this.btnJumpToEnd.Location = new System.Drawing.Point(231, 178);
            this.btnJumpToEnd.Name = "btnJumpToEnd";
            this.btnJumpToEnd.Size = new System.Drawing.Size(85, 23);
            this.btnJumpToEnd.TabIndex = 94;
            this.btnJumpToEnd.Text = "Jump To End";
            this.btnJumpToEnd.UseVisualStyleBackColor = true;
            this.btnJumpToEnd.Click += new System.EventHandler(this.btnJumpToEnd_Click);
            // 
            // btnJumpToStart
            // 
            this.btnJumpToStart.Enabled = false;
            this.btnJumpToStart.Location = new System.Drawing.Point(231, 146);
            this.btnJumpToStart.Name = "btnJumpToStart";
            this.btnJumpToStart.Size = new System.Drawing.Size(85, 23);
            this.btnJumpToStart.TabIndex = 93;
            this.btnJumpToStart.Text = "Jump To Start";
            this.btnJumpToStart.UseVisualStyleBackColor = true;
            this.btnJumpToStart.Click += new System.EventHandler(this.btnJumpToStart_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(326, 236);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 13);
            this.label1.TabIndex = 95;
            this.label1.Text = "Rack #:";
            // 
            // lblRackNum
            // 
            this.lblRackNum.AutoSize = true;
            this.lblRackNum.Location = new System.Drawing.Point(374, 236);
            this.lblRackNum.Name = "lblRackNum";
            this.lblRackNum.Size = new System.Drawing.Size(0, 13);
            this.lblRackNum.TabIndex = 96;
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(302, 255);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(75, 23);
            this.btnDelete.TabIndex = 97;
            this.btnDelete.Text = "Delete Ratio";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnBeginSetDrifted
            // 
            this.btnBeginSetDrifted.Enabled = false;
            this.btnBeginSetDrifted.Location = new System.Drawing.Point(326, 205);
            this.btnBeginSetDrifted.Name = "btnBeginSetDrifted";
            this.btnBeginSetDrifted.Size = new System.Drawing.Size(132, 23);
            this.btnBeginSetDrifted.TabIndex = 98;
            this.btnBeginSetDrifted.Text = "Begin Setting Drifted";
            this.btnBeginSetDrifted.UseVisualStyleBackColor = true;
            this.btnBeginSetDrifted.Click += new System.EventHandler(this.btnBeginSetDrifted_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rdoEnabled);
            this.groupBox1.Controls.Add(this.rdoDisabled);
            this.groupBox1.Location = new System.Drawing.Point(326, 82);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(132, 58);
            this.groupBox1.TabIndex = 99;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Compensation";
            // 
            // rdoEnabled
            // 
            this.rdoEnabled.AutoSize = true;
            this.rdoEnabled.Checked = true;
            this.rdoEnabled.Location = new System.Drawing.Point(6, 16);
            this.rdoEnabled.Name = "rdoEnabled";
            this.rdoEnabled.Size = new System.Drawing.Size(64, 17);
            this.rdoEnabled.TabIndex = 29;
            this.rdoEnabled.TabStop = true;
            this.rdoEnabled.Text = "Enabled";
            this.rdoEnabled.UseVisualStyleBackColor = true;
            this.rdoEnabled.CheckedChanged += new System.EventHandler(this.rdoEnabled_CheckedChanged);
            // 
            // rdoDisabled
            // 
            this.rdoDisabled.AutoSize = true;
            this.rdoDisabled.Location = new System.Drawing.Point(6, 36);
            this.rdoDisabled.Name = "rdoDisabled";
            this.rdoDisabled.Size = new System.Drawing.Size(66, 17);
            this.rdoDisabled.TabIndex = 30;
            this.rdoDisabled.Text = "Disabled";
            this.rdoDisabled.UseVisualStyleBackColor = true;
            this.rdoDisabled.CheckedChanged += new System.EventHandler(this.rdoDisabled_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 118);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(78, 13);
            this.label2.TabIndex = 100;
            this.label2.Text = "Encoder Delta:";
            // 
            // lblEncoderDelta
            // 
            this.lblEncoderDelta.AutoSize = true;
            this.lblEncoderDelta.Location = new System.Drawing.Point(98, 117);
            this.lblEncoderDelta.Name = "lblEncoderDelta";
            this.lblEncoderDelta.Size = new System.Drawing.Size(0, 13);
            this.lblEncoderDelta.TabIndex = 101;
            // 
            // lblEncoderDrift
            // 
            this.lblEncoderDrift.AutoSize = true;
            this.lblEncoderDrift.Location = new System.Drawing.Point(247, 117);
            this.lblEncoderDrift.Name = "lblEncoderDrift";
            this.lblEncoderDrift.Size = new System.Drawing.Size(0, 13);
            this.lblEncoderDrift.TabIndex = 103;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(168, 118);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(72, 13);
            this.label4.TabIndex = 102;
            this.label4.Text = "Encoder Drift:";
            // 
            // btnStep1
            // 
            this.btnStep1.Location = new System.Drawing.Point(3, 82);
            this.btnStep1.Name = "btnStep1";
            this.btnStep1.Size = new System.Drawing.Size(75, 23);
            this.btnStep1.TabIndex = 104;
            this.btnStep1.Text = "Step 1";
            this.btnStep1.UseVisualStyleBackColor = true;
            this.btnStep1.Click += new System.EventHandler(this.btnStep1_Click);
            // 
            // btnStep2
            // 
            this.btnStep2.Location = new System.Drawing.Point(84, 82);
            this.btnStep2.Name = "btnStep2";
            this.btnStep2.Size = new System.Drawing.Size(75, 23);
            this.btnStep2.TabIndex = 105;
            this.btnStep2.Text = "Step 2";
            this.btnStep2.UseVisualStyleBackColor = true;
            this.btnStep2.Click += new System.EventHandler(this.btnStep2_Click);
            // 
            // btnStep3
            // 
            this.btnStep3.Location = new System.Drawing.Point(165, 82);
            this.btnStep3.Name = "btnStep3";
            this.btnStep3.Size = new System.Drawing.Size(75, 23);
            this.btnStep3.TabIndex = 106;
            this.btnStep3.Text = "Step 3";
            this.btnStep3.UseVisualStyleBackColor = true;
            this.btnStep3.Click += new System.EventHandler(this.btnStep3_Click);
            // 
            // btnStep4
            // 
            this.btnStep4.Location = new System.Drawing.Point(246, 82);
            this.btnStep4.Name = "btnStep4";
            this.btnStep4.Size = new System.Drawing.Size(75, 23);
            this.btnStep4.TabIndex = 107;
            this.btnStep4.Text = "Step 4";
            this.btnStep4.UseVisualStyleBackColor = true;
            this.btnStep4.Click += new System.EventHandler(this.btnStep4_Click);
            // 
            // chkUseSavedCompensation
            // 
            this.chkUseSavedCompensation.AutoSize = true;
            this.chkUseSavedCompensation.Location = new System.Drawing.Point(16, 7);
            this.chkUseSavedCompensation.Name = "chkUseSavedCompensation";
            this.chkUseSavedCompensation.Size = new System.Drawing.Size(190, 17);
            this.chkUseSavedCompensation.TabIndex = 108;
            this.chkUseSavedCompensation.Text = "Use Saved Compensation Settings";
            this.chkUseSavedCompensation.UseVisualStyleBackColor = true;
            this.chkUseSavedCompensation.CheckedChanged += new System.EventHandler(this.chkUseSavedCompensation_CheckedChanged);
            // 
            // LensPlayCompensation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(470, 291);
            this.Controls.Add(this.chkUseSavedCompensation);
            this.Controls.Add(this.btnStep4);
            this.Controls.Add(this.btnStep3);
            this.Controls.Add(this.btnStep2);
            this.Controls.Add(this.btnStep1);
            this.Controls.Add(this.lblEncoderDrift);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lblEncoderDelta);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnBeginSetDrifted);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.lblRackNum);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnJumpToEnd);
            this.Controls.Add(this.btnJumpToStart);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.txtCompensationRatio);
            this.Controls.Add(this.btnCompensationRatio);
            this.Controls.Add(this.btnStartLensPlaySimulation);
            this.Controls.Add(this.txtSetStartFocusDrift);
            this.Controls.Add(this.txtSetEndFocus);
            this.Controls.Add(this.txtSetStartFocus);
            this.Controls.Add(this.btnSetDriftedFocus);
            this.Controls.Add(this.btnSetEndFocus);
            this.Controls.Add(this.btnSetStartFocus);
            this.Controls.Add(this.tableLayoutPanel2);
            this.Controls.Add(this.tbFocus);
            this.Name = "LensPlayCompensation";
            this.Text = "Lens Play Compensation";
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbFocus)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label lblTrackbarValue;
        private System.Windows.Forms.TrackBar tbFocus;
        private System.Windows.Forms.Button btnSetStartFocus;
        private System.Windows.Forms.Button btnSetEndFocus;
        private System.Windows.Forms.Button btnSetDriftedFocus;
        private System.Windows.Forms.TextBox txtSetEndFocus;
        private System.Windows.Forms.TextBox txtSetStartFocus;
        private System.Windows.Forms.TextBox txtSetStartFocusDrift;
        private System.Windows.Forms.Button btnStartLensPlaySimulation;
        private System.Windows.Forms.Button btnCompensationRatio;
        private System.Windows.Forms.TextBox txtCompensationRatio;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnJumpToEnd;
        private System.Windows.Forms.Button btnJumpToStart;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblRackNum;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnBeginSetDrifted;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rdoEnabled;
        private System.Windows.Forms.RadioButton rdoDisabled;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblEncoderDelta;
        private System.Windows.Forms.Label lblEncoderDrift;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnStep1;
        private System.Windows.Forms.Button btnStep2;
        private System.Windows.Forms.Button btnStep3;
        private System.Windows.Forms.Button btnStep4;
        private System.Windows.Forms.CheckBox chkUseSavedCompensation;
    }
}