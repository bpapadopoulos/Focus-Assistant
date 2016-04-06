namespace Birger_Rack_Focus_Assistant
{
    partial class Benchmark
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
            this.txtLrgDelay = new System.Windows.Forms.TextBox();
            this.txtMedDelay = new System.Windows.Forms.TextBox();
            this.txtSmallDelay = new System.Windows.Forms.TextBox();
            this.rdoLarge = new System.Windows.Forms.RadioButton();
            this.rdoMedium = new System.Windows.Forms.RadioButton();
            this.rdoSmall = new System.Windows.Forms.RadioButton();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnLrgBench = new System.Windows.Forms.Button();
            this.btnMedBench = new System.Windows.Forms.Button();
            this.btnSmallBench = new System.Windows.Forms.Button();
            this.tbDelay = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.grpStepSize = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.tbDelay)).BeginInit();
            this.grpStepSize.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtLrgDelay
            // 
            this.txtLrgDelay.Enabled = false;
            this.txtLrgDelay.Location = new System.Drawing.Point(284, 88);
            this.txtLrgDelay.Name = "txtLrgDelay";
            this.txtLrgDelay.Size = new System.Drawing.Size(42, 20);
            this.txtLrgDelay.TabIndex = 64;
            // 
            // txtMedDelay
            // 
            this.txtMedDelay.Enabled = false;
            this.txtMedDelay.Location = new System.Drawing.Point(284, 59);
            this.txtMedDelay.Name = "txtMedDelay";
            this.txtMedDelay.Size = new System.Drawing.Size(42, 20);
            this.txtMedDelay.TabIndex = 63;
            // 
            // txtSmallDelay
            // 
            this.txtSmallDelay.Enabled = false;
            this.txtSmallDelay.Location = new System.Drawing.Point(284, 30);
            this.txtSmallDelay.Name = "txtSmallDelay";
            this.txtSmallDelay.Size = new System.Drawing.Size(42, 20);
            this.txtSmallDelay.TabIndex = 62;
            // 
            // rdoLarge
            // 
            this.rdoLarge.AutoSize = true;
            this.rdoLarge.Location = new System.Drawing.Point(6, 76);
            this.rdoLarge.Name = "rdoLarge";
            this.rdoLarge.Size = new System.Drawing.Size(52, 17);
            this.rdoLarge.TabIndex = 54;
            this.rdoLarge.Text = "Large";
            this.rdoLarge.UseVisualStyleBackColor = true;
            this.rdoLarge.Click += new System.EventHandler(this.rdoLarge_Click);
            // 
            // rdoMedium
            // 
            this.rdoMedium.AutoSize = true;
            this.rdoMedium.Location = new System.Drawing.Point(6, 47);
            this.rdoMedium.Name = "rdoMedium";
            this.rdoMedium.Size = new System.Drawing.Size(62, 17);
            this.rdoMedium.TabIndex = 53;
            this.rdoMedium.Text = "Medium";
            this.rdoMedium.UseVisualStyleBackColor = true;
            this.rdoMedium.Click += new System.EventHandler(this.rdoMedium_Click);
            // 
            // rdoSmall
            // 
            this.rdoSmall.AutoSize = true;
            this.rdoSmall.Checked = true;
            this.rdoSmall.Location = new System.Drawing.Point(6, 18);
            this.rdoSmall.Name = "rdoSmall";
            this.rdoSmall.Size = new System.Drawing.Size(50, 17);
            this.rdoSmall.TabIndex = 52;
            this.rdoSmall.TabStop = true;
            this.rdoSmall.Text = "Small";
            this.rdoSmall.UseVisualStyleBackColor = true;
            this.rdoSmall.Click += new System.EventHandler(this.rdoSmall_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(323, 123);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 51;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnLrgBench
            // 
            this.btnLrgBench.Enabled = false;
            this.btnLrgBench.Location = new System.Drawing.Point(93, 86);
            this.btnLrgBench.Name = "btnLrgBench";
            this.btnLrgBench.Size = new System.Drawing.Size(184, 23);
            this.btnLrgBench.TabIndex = 43;
            this.btnLrgBench.Text = "Start Large Step Benchmark";
            this.btnLrgBench.UseVisualStyleBackColor = true;
            this.btnLrgBench.Click += new System.EventHandler(this.btnLrgBench_Click);
            // 
            // btnMedBench
            // 
            this.btnMedBench.Enabled = false;
            this.btnMedBench.Location = new System.Drawing.Point(92, 57);
            this.btnMedBench.Name = "btnMedBench";
            this.btnMedBench.Size = new System.Drawing.Size(185, 23);
            this.btnMedBench.TabIndex = 41;
            this.btnMedBench.Text = "Start Medium Step Benchmark";
            this.btnMedBench.UseVisualStyleBackColor = true;
            this.btnMedBench.Click += new System.EventHandler(this.btnMedBench_Click);
            // 
            // btnSmallBench
            // 
            this.btnSmallBench.Location = new System.Drawing.Point(92, 28);
            this.btnSmallBench.Name = "btnSmallBench";
            this.btnSmallBench.Size = new System.Drawing.Size(185, 23);
            this.btnSmallBench.TabIndex = 39;
            this.btnSmallBench.Text = "Start Small Step Benchmark";
            this.btnSmallBench.UseVisualStyleBackColor = true;
            this.btnSmallBench.Click += new System.EventHandler(this.btnSmallBench_Click);
            // 
            // tbDelay
            // 
            this.tbDelay.Location = new System.Drawing.Point(357, 24);
            this.tbDelay.Maximum = 2000;
            this.tbDelay.Name = "tbDelay";
            this.tbDelay.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.tbDelay.Size = new System.Drawing.Size(45, 88);
            this.tbDelay.SmallChange = 3;
            this.tbDelay.TabIndex = 65;
            this.tbDelay.TickFrequency = 0;
            this.tbDelay.Value = 2000;
            this.tbDelay.Scroll += new System.EventHandler(this.tbDelay_Scroll);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(331, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(20, 13);
            this.label1.TabIndex = 66;
            this.label1.Text = "ms";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(331, 66);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(20, 13);
            this.label2.TabIndex = 67;
            this.label2.Text = "ms";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(331, 96);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(20, 13);
            this.label3.TabIndex = 68;
            this.label3.Text = "ms";
            // 
            // grpStepSize
            // 
            this.grpStepSize.Controls.Add(this.rdoSmall);
            this.grpStepSize.Controls.Add(this.rdoMedium);
            this.grpStepSize.Controls.Add(this.rdoLarge);
            this.grpStepSize.Location = new System.Drawing.Point(12, 16);
            this.grpStepSize.Name = "grpStepSize";
            this.grpStepSize.Size = new System.Drawing.Size(75, 100);
            this.grpStepSize.TabIndex = 69;
            this.grpStepSize.TabStop = false;
            this.grpStepSize.Text = "Step Size";
            // 
            // Benchmark
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(410, 167);
            this.Controls.Add(this.grpStepSize);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbDelay);
            this.Controls.Add(this.txtLrgDelay);
            this.Controls.Add(this.txtMedDelay);
            this.Controls.Add(this.txtSmallDelay);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnLrgBench);
            this.Controls.Add(this.btnMedBench);
            this.Controls.Add(this.btnSmallBench);
            this.Name = "Benchmark";
            this.Text = "Benchmark";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Benchmark_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.tbDelay)).EndInit();
            this.grpStepSize.ResumeLayout(false);
            this.grpStepSize.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtLrgDelay;
        private System.Windows.Forms.TextBox txtMedDelay;
        private System.Windows.Forms.TextBox txtSmallDelay;
        private System.Windows.Forms.RadioButton rdoLarge;
        private System.Windows.Forms.RadioButton rdoMedium;
        private System.Windows.Forms.RadioButton rdoSmall;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnLrgBench;
        private System.Windows.Forms.Button btnMedBench;
        private System.Windows.Forms.Button btnSmallBench;
        private System.Windows.Forms.TrackBar tbDelay;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox grpStepSize;
    }
}