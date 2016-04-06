namespace Birger_Rack_Focus_Assistant
{
    partial class RangeFinder
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
            this.btnActivate = new System.Windows.Forms.Button();
            this.txtMeasurement = new System.Windows.Forms.TextBox();
            this.btnFocusControl = new System.Windows.Forms.Button();
            this.txtFocusOffset = new System.Windows.Forms.TextBox();
            this.lblFocusOffset = new System.Windows.Forms.Label();
            this.rdoLaser = new System.Windows.Forms.RadioButton();
            this.rdoSonar = new System.Windows.Forms.RadioButton();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.lblIrisBarValue = new System.Windows.Forms.Label();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.lblTrackbarValue = new System.Windows.Forms.Label();
            this.tbIris = new System.Windows.Forms.TrackBar();
            this.tbFocus = new System.Windows.Forms.TrackBar();
            this.btnSetOffset = new System.Windows.Forms.Button();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbIris)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbFocus)).BeginInit();
            this.SuspendLayout();
            // 
            // btnActivate
            // 
            this.btnActivate.Location = new System.Drawing.Point(12, 35);
            this.btnActivate.Name = "btnActivate";
            this.btnActivate.Size = new System.Drawing.Size(100, 35);
            this.btnActivate.TabIndex = 0;
            this.btnActivate.Text = "Activate RangeFinder";
            this.btnActivate.UseVisualStyleBackColor = true;
            this.btnActivate.Click += new System.EventHandler(this.btnActivate_Click);
            // 
            // txtMeasurement
            // 
            this.txtMeasurement.Enabled = false;
            this.txtMeasurement.Location = new System.Drawing.Point(150, 43);
            this.txtMeasurement.Name = "txtMeasurement";
            this.txtMeasurement.Size = new System.Drawing.Size(100, 20);
            this.txtMeasurement.TabIndex = 1;
            // 
            // btnFocusControl
            // 
            this.btnFocusControl.Location = new System.Drawing.Point(12, 85);
            this.btnFocusControl.Name = "btnFocusControl";
            this.btnFocusControl.Size = new System.Drawing.Size(100, 35);
            this.btnFocusControl.TabIndex = 2;
            this.btnFocusControl.Text = "Start Focus Control";
            this.btnFocusControl.UseVisualStyleBackColor = true;
            this.btnFocusControl.Click += new System.EventHandler(this.btnFocusControl_Click);
            // 
            // txtFocusOffset
            // 
            this.txtFocusOffset.BackColor = System.Drawing.SystemColors.Control;
            this.txtFocusOffset.Location = new System.Drawing.Point(150, 100);
            this.txtFocusOffset.Name = "txtFocusOffset";
            this.txtFocusOffset.ReadOnly = true;
            this.txtFocusOffset.Size = new System.Drawing.Size(100, 20);
            this.txtFocusOffset.TabIndex = 3;
            // 
            // lblFocusOffset
            // 
            this.lblFocusOffset.AutoSize = true;
            this.lblFocusOffset.Location = new System.Drawing.Point(166, 85);
            this.lblFocusOffset.Name = "lblFocusOffset";
            this.lblFocusOffset.Size = new System.Drawing.Size(67, 13);
            this.lblFocusOffset.TabIndex = 4;
            this.lblFocusOffset.Text = "Focus Offset";
            // 
            // rdoLaser
            // 
            this.rdoLaser.AutoSize = true;
            this.rdoLaser.Checked = true;
            this.rdoLaser.Location = new System.Drawing.Point(12, 12);
            this.rdoLaser.Name = "rdoLaser";
            this.rdoLaser.Size = new System.Drawing.Size(51, 17);
            this.rdoLaser.TabIndex = 5;
            this.rdoLaser.TabStop = true;
            this.rdoLaser.Text = "Laser";
            this.rdoLaser.UseVisualStyleBackColor = true;
            this.rdoLaser.CheckedChanged += new System.EventHandler(this.rdoLaser_CheckedChanged);
            // 
            // rdoSonar
            // 
            this.rdoSonar.AutoSize = true;
            this.rdoSonar.Location = new System.Drawing.Point(80, 12);
            this.rdoSonar.Name = "rdoSonar";
            this.rdoSonar.Size = new System.Drawing.Size(53, 17);
            this.rdoSonar.TabIndex = 6;
            this.rdoSonar.Text = "Sonar";
            this.rdoSonar.UseVisualStyleBackColor = true;
            this.rdoSonar.CheckedChanged += new System.EventHandler(this.rdoSonar_CheckedChanged);
            // 
            // label8
            // 
            this.label8.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.label8.Location = new System.Drawing.Point(-557, -36);
            this.label8.Margin = new System.Windows.Forms.Padding(0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(29, 13);
            this.label8.TabIndex = 82;
            this.label8.Text = "Iris:  ";
            this.label8.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label7
            // 
            this.label7.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.label7.Location = new System.Drawing.Point(-477, -36);
            this.label7.Margin = new System.Windows.Forms.Padding(0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(58, 13);
            this.label7.TabIndex = 81;
            this.label7.Text = "Distance:  ";
            this.label7.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel3.Controls.Add(this.lblIrisBarValue, 0, 0);
            this.tableLayoutPanel3.Location = new System.Drawing.Point(21, 139);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 21F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(72, 21);
            this.tableLayoutPanel3.TabIndex = 80;
            // 
            // lblIrisBarValue
            // 
            this.lblIrisBarValue.AutoSize = true;
            this.lblIrisBarValue.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblIrisBarValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.lblIrisBarValue.Location = new System.Drawing.Point(3, 0);
            this.lblIrisBarValue.Name = "lblIrisBarValue";
            this.lblIrisBarValue.Size = new System.Drawing.Size(23, 21);
            this.lblIrisBarValue.TabIndex = 69;
            this.lblIrisBarValue.Text = "0";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Controls.Add(this.lblTrackbarValue, 0, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(169, 142);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 21F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(149, 21);
            this.tableLayoutPanel2.TabIndex = 79;
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
            // tbIris
            // 
            this.tbIris.Location = new System.Drawing.Point(14, 217);
            this.tbIris.Maximum = 1;
            this.tbIris.Name = "tbIris";
            this.tbIris.Size = new System.Drawing.Size(315, 45);
            this.tbIris.SmallChange = 3;
            this.tbIris.TabIndex = 77;
            this.tbIris.TickFrequency = 0;
            this.tbIris.ValueChanged += new System.EventHandler(this.tbIris_ValueChanged);
            // 
            // tbFocus
            // 
            this.tbFocus.Location = new System.Drawing.Point(12, 166);
            this.tbFocus.Maximum = 16383;
            this.tbFocus.Name = "tbFocus";
            this.tbFocus.Size = new System.Drawing.Size(317, 45);
            this.tbFocus.SmallChange = 3;
            this.tbFocus.TabIndex = 75;
            this.tbFocus.TickFrequency = 0;
            this.tbFocus.ValueChanged += new System.EventHandler(this.tbFocus_ValueChanged);
            this.tbFocus.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbFocus_KeyDown);
            this.tbFocus.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tbFocus_MouseDown);
            // 
            // btnSetOffset
            // 
            this.btnSetOffset.Enabled = false;
            this.btnSetOffset.Location = new System.Drawing.Point(257, 96);
            this.btnSetOffset.Name = "btnSetOffset";
            this.btnSetOffset.Size = new System.Drawing.Size(75, 23);
            this.btnSetOffset.TabIndex = 83;
            this.btnSetOffset.Text = "Set Offset";
            this.btnSetOffset.UseVisualStyleBackColor = true;
            this.btnSetOffset.Click += new System.EventHandler(this.btnSetOffset_Click);
            // 
            // RangeFinder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(341, 266);
            this.Controls.Add(this.btnSetOffset);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.tableLayoutPanel3);
            this.Controls.Add(this.tableLayoutPanel2);
            this.Controls.Add(this.tbIris);
            this.Controls.Add(this.tbFocus);
            this.Controls.Add(this.rdoSonar);
            this.Controls.Add(this.rdoLaser);
            this.Controls.Add(this.lblFocusOffset);
            this.Controls.Add(this.txtFocusOffset);
            this.Controls.Add(this.btnFocusControl);
            this.Controls.Add(this.txtMeasurement);
            this.Controls.Add(this.btnActivate);
            this.Name = "RangeFinder";
            this.Text = "RangeFinder";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RangeFinder_FormClosing);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbIris)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbFocus)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnActivate;
        private System.Windows.Forms.TextBox txtMeasurement;
        private System.Windows.Forms.Button btnFocusControl;
        private System.Windows.Forms.TextBox txtFocusOffset;
        private System.Windows.Forms.Label lblFocusOffset;
        private System.Windows.Forms.RadioButton rdoLaser;
        private System.Windows.Forms.RadioButton rdoSonar;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Label lblIrisBarValue;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label lblTrackbarValue;
        private System.Windows.Forms.TrackBar tbIris;
        private System.Windows.Forms.TrackBar tbFocus;
        private System.Windows.Forms.Button btnSetOffset;
    }
}