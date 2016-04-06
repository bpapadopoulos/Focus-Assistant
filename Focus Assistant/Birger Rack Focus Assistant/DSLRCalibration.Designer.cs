namespace Birger_Rack_Focus_Assistant
{
    partial class DSLRCalibration
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
            this.btnFar3 = new System.Windows.Forms.Button();
            this.btnNear3 = new System.Windows.Forms.Button();
            this.btnFar2 = new System.Windows.Forms.Button();
            this.btnNear2 = new System.Windows.Forms.Button();
            this.btnFar1 = new System.Windows.Forms.Button();
            this.btnNear1 = new System.Windows.Forms.Button();
            this.txtDistance = new System.Windows.Forms.TextBox();
            this.lblStepCounter = new System.Windows.Forms.Label();
            this.btnAddDistance = new System.Windows.Forms.Button();
            this.lblDistanceMarker = new System.Windows.Forms.Label();
            this.txtStepCounter = new System.Windows.Forms.TextBox();
            this.trvDistanceMark = new System.Windows.Forms.TreeView();
            this.btnSave = new System.Windows.Forms.Button();
            this.rdoSmall = new System.Windows.Forms.RadioButton();
            this.rdoMedium = new System.Windows.Forms.RadioButton();
            this.rdoLarge = new System.Windows.Forms.RadioButton();
            this.lblFt = new System.Windows.Forms.Label();
            this.btnClear = new System.Windows.Forms.Button();
            this.rdoRackBurst = new System.Windows.Forms.RadioButton();
            this.rdoSingleStep = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnIncrease = new System.Windows.Forms.Button();
            this.btnDecrease = new System.Windows.Forms.Button();
            this.btnInfinity = new System.Windows.Forms.Button();
            this.btnInfinityMax = new System.Windows.Forms.Button();
            this.btnBenchmarks = new System.Windows.Forms.Button();
            this.btnLensPlayComp = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnFar3
            // 
            this.btnFar3.Enabled = false;
            this.btnFar3.Location = new System.Drawing.Point(235, 160);
            this.btnFar3.Name = "btnFar3";
            this.btnFar3.Size = new System.Drawing.Size(75, 23);
            this.btnFar3.TabIndex = 16;
            this.btnFar3.Text = ">>>";
            this.btnFar3.UseVisualStyleBackColor = true;
            this.btnFar3.Click += new System.EventHandler(this.btnFar3_Click);
            // 
            // btnNear3
            // 
            this.btnNear3.Enabled = false;
            this.btnNear3.Location = new System.Drawing.Point(89, 160);
            this.btnNear3.Name = "btnNear3";
            this.btnNear3.Size = new System.Drawing.Size(75, 23);
            this.btnNear3.TabIndex = 15;
            this.btnNear3.Text = "<<<";
            this.btnNear3.UseVisualStyleBackColor = true;
            this.btnNear3.Click += new System.EventHandler(this.btnNear3_Click);
            // 
            // btnFar2
            // 
            this.btnFar2.Enabled = false;
            this.btnFar2.Location = new System.Drawing.Point(235, 131);
            this.btnFar2.Name = "btnFar2";
            this.btnFar2.Size = new System.Drawing.Size(75, 23);
            this.btnFar2.TabIndex = 14;
            this.btnFar2.Text = ">>";
            this.btnFar2.UseVisualStyleBackColor = true;
            this.btnFar2.Click += new System.EventHandler(this.btnFar2_Click);
            // 
            // btnNear2
            // 
            this.btnNear2.Enabled = false;
            this.btnNear2.Location = new System.Drawing.Point(88, 131);
            this.btnNear2.Name = "btnNear2";
            this.btnNear2.Size = new System.Drawing.Size(75, 23);
            this.btnNear2.TabIndex = 13;
            this.btnNear2.Text = "<<";
            this.btnNear2.UseVisualStyleBackColor = true;
            this.btnNear2.Click += new System.EventHandler(this.btnNear2_Click);
            // 
            // btnFar1
            // 
            this.btnFar1.Location = new System.Drawing.Point(235, 102);
            this.btnFar1.Name = "btnFar1";
            this.btnFar1.Size = new System.Drawing.Size(75, 23);
            this.btnFar1.TabIndex = 12;
            this.btnFar1.Text = ">";
            this.btnFar1.UseVisualStyleBackColor = true;
            this.btnFar1.Click += new System.EventHandler(this.btnFar1_Click);
            // 
            // btnNear1
            // 
            this.btnNear1.Location = new System.Drawing.Point(88, 102);
            this.btnNear1.Name = "btnNear1";
            this.btnNear1.Size = new System.Drawing.Size(75, 23);
            this.btnNear1.TabIndex = 11;
            this.btnNear1.Text = "<";
            this.btnNear1.UseVisualStyleBackColor = true;
            this.btnNear1.Click += new System.EventHandler(this.btnNear1_Click);
            // 
            // txtDistance
            // 
            this.txtDistance.Font = new System.Drawing.Font("Meiryo", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDistance.Location = new System.Drawing.Point(64, 66);
            this.txtDistance.Margin = new System.Windows.Forms.Padding(0);
            this.txtDistance.MaximumSize = new System.Drawing.Size(80, 20);
            this.txtDistance.Name = "txtDistance";
            this.txtDistance.Size = new System.Drawing.Size(80, 24);
            this.txtDistance.TabIndex = 17;
            this.txtDistance.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // lblStepCounter
            // 
            this.lblStepCounter.AutoSize = true;
            this.lblStepCounter.Location = new System.Drawing.Point(61, 10);
            this.lblStepCounter.Name = "lblStepCounter";
            this.lblStepCounter.Size = new System.Drawing.Size(72, 13);
            this.lblStepCounter.TabIndex = 18;
            this.lblStepCounter.Text = "Step Counter:";
            // 
            // btnAddDistance
            // 
            this.btnAddDistance.Location = new System.Drawing.Point(202, 65);
            this.btnAddDistance.Name = "btnAddDistance";
            this.btnAddDistance.Size = new System.Drawing.Size(108, 23);
            this.btnAddDistance.TabIndex = 19;
            this.btnAddDistance.Text = "Add Distance Mark";
            this.btnAddDistance.UseVisualStyleBackColor = true;
            this.btnAddDistance.Click += new System.EventHandler(this.btnAddDistance_Click);
            // 
            // lblDistanceMarker
            // 
            this.lblDistanceMarker.AutoSize = true;
            this.lblDistanceMarker.Location = new System.Drawing.Point(62, 50);
            this.lblDistanceMarker.Name = "lblDistanceMarker";
            this.lblDistanceMarker.Size = new System.Drawing.Size(85, 13);
            this.lblDistanceMarker.TabIndex = 20;
            this.lblDistanceMarker.Text = "Distance Marker";
            // 
            // txtStepCounter
            // 
            this.txtStepCounter.Enabled = false;
            this.txtStepCounter.Location = new System.Drawing.Point(64, 27);
            this.txtStepCounter.Name = "txtStepCounter";
            this.txtStepCounter.Size = new System.Drawing.Size(100, 20);
            this.txtStepCounter.TabIndex = 21;
            // 
            // trvDistanceMark
            // 
            this.trvDistanceMark.Location = new System.Drawing.Point(332, 9);
            this.trvDistanceMark.Name = "trvDistanceMark";
            this.trvDistanceMark.Size = new System.Drawing.Size(121, 174);
            this.trvDistanceMark.TabIndex = 22;
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(378, 189);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 23;
            this.btnSave.Text = "Next";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // rdoSmall
            // 
            this.rdoSmall.AutoSize = true;
            this.rdoSmall.Checked = true;
            this.rdoSmall.Location = new System.Drawing.Point(10, 108);
            this.rdoSmall.Name = "rdoSmall";
            this.rdoSmall.Size = new System.Drawing.Size(50, 17);
            this.rdoSmall.TabIndex = 24;
            this.rdoSmall.TabStop = true;
            this.rdoSmall.Text = "Small";
            this.rdoSmall.UseVisualStyleBackColor = true;
            this.rdoSmall.Click += new System.EventHandler(this.rdoSmall_Click);
            // 
            // rdoMedium
            // 
            this.rdoMedium.AutoSize = true;
            this.rdoMedium.Enabled = false;
            this.rdoMedium.Location = new System.Drawing.Point(10, 137);
            this.rdoMedium.Name = "rdoMedium";
            this.rdoMedium.Size = new System.Drawing.Size(62, 17);
            this.rdoMedium.TabIndex = 25;
            this.rdoMedium.Text = "Medium";
            this.rdoMedium.UseVisualStyleBackColor = true;
            this.rdoMedium.Click += new System.EventHandler(this.rdoMedium_Click);
            // 
            // rdoLarge
            // 
            this.rdoLarge.AutoSize = true;
            this.rdoLarge.Enabled = false;
            this.rdoLarge.Location = new System.Drawing.Point(10, 166);
            this.rdoLarge.Name = "rdoLarge";
            this.rdoLarge.Size = new System.Drawing.Size(52, 17);
            this.rdoLarge.TabIndex = 26;
            this.rdoLarge.Text = "Large";
            this.rdoLarge.UseVisualStyleBackColor = true;
            this.rdoLarge.Click += new System.EventHandler(this.rdoLarge_Click);
            // 
            // lblFt
            // 
            this.lblFt.AutoSize = true;
            this.lblFt.Location = new System.Drawing.Point(148, 66);
            this.lblFt.Name = "lblFt";
            this.lblFt.Size = new System.Drawing.Size(16, 13);
            this.lblFt.TabIndex = 27;
            this.lblFt.Text = "ft.";
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(297, 189);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(75, 23);
            this.btnClear.TabIndex = 28;
            this.btnClear.Text = "Reset";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // rdoRackBurst
            // 
            this.rdoRackBurst.AutoSize = true;
            this.rdoRackBurst.Checked = true;
            this.rdoRackBurst.Location = new System.Drawing.Point(6, 16);
            this.rdoRackBurst.Name = "rdoRackBurst";
            this.rdoRackBurst.Size = new System.Drawing.Size(78, 17);
            this.rdoRackBurst.TabIndex = 29;
            this.rdoRackBurst.TabStop = true;
            this.rdoRackBurst.Text = "Rack Burst";
            this.rdoRackBurst.UseVisualStyleBackColor = true;
            this.rdoRackBurst.Click += new System.EventHandler(this.rdoRackBurst_Click);
            // 
            // rdoSingleStep
            // 
            this.rdoSingleStep.AutoSize = true;
            this.rdoSingleStep.Location = new System.Drawing.Point(6, 36);
            this.rdoSingleStep.Name = "rdoSingleStep";
            this.rdoSingleStep.Size = new System.Drawing.Size(79, 17);
            this.rdoSingleStep.TabIndex = 30;
            this.rdoSingleStep.Text = "Single Step";
            this.rdoSingleStep.UseVisualStyleBackColor = true;
            this.rdoSingleStep.Click += new System.EventHandler(this.rdoSingleStep_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rdoRackBurst);
            this.groupBox1.Controls.Add(this.rdoSingleStep);
            this.groupBox1.Location = new System.Drawing.Point(202, 1);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(112, 58);
            this.groupBox1.TabIndex = 31;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Mode";
            // 
            // btnIncrease
            // 
            this.btnIncrease.Font = new System.Drawing.Font("Microsoft Sans Serif", 5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnIncrease.Location = new System.Drawing.Point(10, 12);
            this.btnIncrease.Name = "btnIncrease";
            this.btnIncrease.Size = new System.Drawing.Size(45, 14);
            this.btnIncrease.TabIndex = 32;
            this.btnIncrease.Text = "Increase";
            this.btnIncrease.UseVisualStyleBackColor = true;
            this.btnIncrease.Click += new System.EventHandler(this.btnIncrease_Click);
            // 
            // btnDecrease
            // 
            this.btnDecrease.Font = new System.Drawing.Font("Microsoft Sans Serif", 5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDecrease.Location = new System.Drawing.Point(10, 31);
            this.btnDecrease.Name = "btnDecrease";
            this.btnDecrease.Size = new System.Drawing.Size(45, 14);
            this.btnDecrease.TabIndex = 33;
            this.btnDecrease.Text = "Decrease";
            this.btnDecrease.UseVisualStyleBackColor = true;
            this.btnDecrease.Click += new System.EventHandler(this.btnDecrease_Click);
            // 
            // btnInfinity
            // 
            this.btnInfinity.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnInfinity.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnInfinity.Location = new System.Drawing.Point(10, 51);
            this.btnInfinity.Name = "btnInfinity";
            this.btnInfinity.Size = new System.Drawing.Size(45, 15);
            this.btnInfinity.TabIndex = 34;
            this.btnInfinity.Text = "∞";
            this.btnInfinity.UseVisualStyleBackColor = true;
            this.btnInfinity.Click += new System.EventHandler(this.btnInfinity_Click);
            // 
            // btnInfinityMax
            // 
            this.btnInfinityMax.Enabled = false;
            this.btnInfinityMax.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnInfinityMax.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnInfinityMax.Location = new System.Drawing.Point(10, 72);
            this.btnInfinityMax.Name = "btnInfinityMax";
            this.btnInfinityMax.Size = new System.Drawing.Size(45, 15);
            this.btnInfinityMax.TabIndex = 35;
            this.btnInfinityMax.Text = "∞";
            this.btnInfinityMax.UseVisualStyleBackColor = true;
            this.btnInfinityMax.Click += new System.EventHandler(this.btnInfinityMax_Click);
            // 
            // btnBenchmarks
            // 
            this.btnBenchmarks.Location = new System.Drawing.Point(12, 190);
            this.btnBenchmarks.Name = "btnBenchmarks";
            this.btnBenchmarks.Size = new System.Drawing.Size(75, 23);
            this.btnBenchmarks.TabIndex = 39;
            this.btnBenchmarks.Text = "Benchmarks";
            this.btnBenchmarks.UseVisualStyleBackColor = true;
            this.btnBenchmarks.Click += new System.EventHandler(this.btnBenchmarks_Click);
            // 
            // btnLensPlayComp
            // 
            this.btnLensPlayComp.Enabled = false;
            this.btnLensPlayComp.Location = new System.Drawing.Point(93, 190);
            this.btnLensPlayComp.Name = "btnLensPlayComp";
            this.btnLensPlayComp.Size = new System.Drawing.Size(135, 23);
            this.btnLensPlayComp.TabIndex = 40;
            this.btnLensPlayComp.Text = "Lens Play Compensation";
            this.btnLensPlayComp.UseVisualStyleBackColor = true;
            this.btnLensPlayComp.Click += new System.EventHandler(this.btnLensPlayComp_Click);
            // 
            // DSLRCalibration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(470, 225);
            this.Controls.Add(this.btnLensPlayComp);
            this.Controls.Add(this.btnBenchmarks);
            this.Controls.Add(this.btnInfinityMax);
            this.Controls.Add(this.btnInfinity);
            this.Controls.Add(this.btnDecrease);
            this.Controls.Add(this.btnIncrease);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.lblFt);
            this.Controls.Add(this.rdoLarge);
            this.Controls.Add(this.rdoMedium);
            this.Controls.Add(this.rdoSmall);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.trvDistanceMark);
            this.Controls.Add(this.txtStepCounter);
            this.Controls.Add(this.lblDistanceMarker);
            this.Controls.Add(this.btnAddDistance);
            this.Controls.Add(this.lblStepCounter);
            this.Controls.Add(this.txtDistance);
            this.Controls.Add(this.btnFar3);
            this.Controls.Add(this.btnNear3);
            this.Controls.Add(this.btnFar2);
            this.Controls.Add(this.btnNear2);
            this.Controls.Add(this.btnFar1);
            this.Controls.Add(this.btnNear1);
            this.Name = "DSLRCalibration";
            this.Text = "Lens Calibration";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DSLRCalibration_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.DSLRCalibration_FormClosed);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnFar3;
        private System.Windows.Forms.Button btnNear3;
        private System.Windows.Forms.Button btnFar2;
        private System.Windows.Forms.Button btnNear2;
        private System.Windows.Forms.Button btnFar1;
        private System.Windows.Forms.Button btnNear1;
        private System.Windows.Forms.TextBox txtDistance;
        private System.Windows.Forms.Label lblStepCounter;
        private System.Windows.Forms.Button btnAddDistance;
        private System.Windows.Forms.Label lblDistanceMarker;
        private System.Windows.Forms.TextBox txtStepCounter;
        private System.Windows.Forms.TreeView trvDistanceMark;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.RadioButton rdoSmall;
        private System.Windows.Forms.RadioButton rdoMedium;
        private System.Windows.Forms.RadioButton rdoLarge;
        private System.Windows.Forms.Label lblFt;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.RadioButton rdoRackBurst;
        private System.Windows.Forms.RadioButton rdoSingleStep;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnIncrease;
        private System.Windows.Forms.Button btnDecrease;
        private System.Windows.Forms.Button btnInfinity;
        private System.Windows.Forms.Button btnInfinityMax;
        private System.Windows.Forms.Button btnBenchmarks;
        private System.Windows.Forms.Button btnLensPlayComp;
    }
}