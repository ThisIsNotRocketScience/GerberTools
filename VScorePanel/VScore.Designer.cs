namespace VScorePanel
{
    partial class VScore
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VScore));
            this.xbox = new System.Windows.Forms.NumericUpDown();
            this.ybox = new System.Windows.Forms.NumericUpDown();
            this.framebox = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.FrameTitle = new System.Windows.Forms.TextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.label5 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.xbox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ybox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.framebox)).BeginInit();
            this.SuspendLayout();
            // 
            // xbox
            // 
            this.xbox.Location = new System.Drawing.Point(116, 12);
            this.xbox.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.xbox.Name = "xbox";
            this.xbox.Size = new System.Drawing.Size(120, 26);
            this.xbox.TabIndex = 0;
            this.xbox.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // ybox
            // 
            this.ybox.Location = new System.Drawing.Point(116, 53);
            this.ybox.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.ybox.Name = "ybox";
            this.ybox.Size = new System.Drawing.Size(120, 26);
            this.ybox.TabIndex = 1;
            this.ybox.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // framebox
            // 
            this.framebox.Location = new System.Drawing.Point(116, 96);
            this.framebox.Maximum = new decimal(new int[] {
            40,
            0,
            0,
            0});
            this.framebox.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.framebox.Name = "framebox";
            this.framebox.Size = new System.Drawing.Size(120, 26);
            this.framebox.TabIndex = 2;
            this.framebox.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(87, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(20, 20);
            this.label1.TabIndex = 3;
            this.label1.Text = "X";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(87, 55);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(20, 20);
            this.label2.TabIndex = 4;
            this.label2.Text = "Y";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(22, 102);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(85, 20);
            this.label3.TabIndex = 5;
            this.label3.Text = "Frame MM";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(22, 148);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(88, 20);
            this.label4.TabIndex = 6;
            this.label4.Text = "Frame Title";
            // 
            // FrameTitle
            // 
            this.FrameTitle.Location = new System.Drawing.Point(116, 145);
            this.FrameTitle.Name = "FrameTitle";
            this.FrameTitle.Size = new System.Drawing.Size(137, 26);
            this.FrameTitle.TabIndex = 7;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 10;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(87, 269);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(132, 20);
            this.label5.TabIndex = 8;
            this.label5.Text = "Drop Folder Here";
            // 
            // VScore
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(516, 371);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.FrameTitle);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.framebox);
            this.Controls.Add(this.ybox);
            this.Controls.Add(this.xbox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "VScore";
            this.Text = "TiNRS VGrooveDrop";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.VScore_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.VScore_DragEnter);
            ((System.ComponentModel.ISupportInitialize)(this.xbox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ybox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.framebox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown xbox;
        private System.Windows.Forms.NumericUpDown ybox;
        private System.Windows.Forms.NumericUpDown framebox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox FrameTitle;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label label5;
    }
}

