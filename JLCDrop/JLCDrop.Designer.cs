namespace JLCDrop
{
    partial class JLCDropForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(JLCDropForm));
            this.label1 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.MakeFrameBox = new System.Windows.Forms.CheckBox();
            this.FrameMM = new System.Windows.Forms.NumericUpDown();
            this.FrameName = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.FrameMM)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Snow;
            this.label1.Location = new System.Drawing.Point(43, 51);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(233, 46);
            this.label1.TabIndex = 0;
            this.label1.Text = "DROP BRD";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // MakeFrameBox
            // 
            this.MakeFrameBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MakeFrameBox.AutoSize = true;
            this.MakeFrameBox.Checked = true;
            this.MakeFrameBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.MakeFrameBox.ForeColor = System.Drawing.Color.White;
            this.MakeFrameBox.Location = new System.Drawing.Point(86, 540);
            this.MakeFrameBox.Name = "MakeFrameBox";
            this.MakeFrameBox.Size = new System.Drawing.Size(124, 24);
            this.MakeFrameBox.TabIndex = 1;
            this.MakeFrameBox.Text = "Make Frame";
            this.MakeFrameBox.UseVisualStyleBackColor = true;
            // 
            // FrameMM
            // 
            this.FrameMM.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FrameMM.Location = new System.Drawing.Point(68, 482);
            this.FrameMM.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.FrameMM.Minimum = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.FrameMM.Name = "FrameMM";
            this.FrameMM.Size = new System.Drawing.Size(434, 26);
            this.FrameMM.TabIndex = 2;
            this.FrameMM.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // FrameName
            // 
            this.FrameName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FrameName.Location = new System.Drawing.Point(33, 241);
            this.FrameName.Name = "FrameName";
            this.FrameName.Size = new System.Drawing.Size(512, 26);
            this.FrameName.TabIndex = 3;
            this.FrameName.Text = "Frame";
            // 
            // JLCDropForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(593, 627);
            this.Controls.Add(this.FrameName);
            this.Controls.Add(this.FrameMM);
            this.Controls.Add(this.MakeFrameBox);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "JLCDropForm";
            this.Text = "JLCDrop";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.Form1_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.Form1_DragEnter);
            ((System.ComponentModel.ISupportInitialize)(this.FrameMM)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.CheckBox MakeFrameBox;
        private System.Windows.Forms.NumericUpDown FrameMM;
        private System.Windows.Forms.TextBox FrameName;
    }
}

