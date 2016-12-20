namespace CaseBuilder
{
    partial class CaseBuilder
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CaseBuilder));
            this.offsetbox = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.holediamBox = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.offsetbox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.holediamBox)).BeginInit();
            this.SuspendLayout();
            // 
            // offsetbox
            // 
            this.offsetbox.DecimalPlaces = 2;
            this.offsetbox.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.offsetbox.Location = new System.Drawing.Point(343, 81);
            this.offsetbox.Name = "offsetbox";
            this.offsetbox.Size = new System.Drawing.Size(120, 22);
            this.offsetbox.TabIndex = 0;
            this.offsetbox.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 19.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(27, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(404, 38);
            this.label1.TabIndex = 1;
            this.label1.Text = "Drop Folder/Zip/Files here!";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(31, 83);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(306, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "edge-offset (how much bigger than the board?)";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(159, 111);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(178, 17);
            this.label3.TabIndex = 4;
            this.label3.Text = "mount hole size to scan for";
            // 
            // holediamBox
            // 
            this.holediamBox.DecimalPlaces = 2;
            this.holediamBox.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.holediamBox.Location = new System.Drawing.Point(343, 111);
            this.holediamBox.Name = "holediamBox";
            this.holediamBox.Size = new System.Drawing.Size(120, 22);
            this.holediamBox.TabIndex = 3;
            this.holediamBox.Value = new decimal(new int[] {
            32,
            0,
            0,
            65536});
            // 
            // CaseBuilder
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Beige;
            this.ClientSize = new System.Drawing.Size(475, 144);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.holediamBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.offsetbox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "CaseBuilder";
            this.Text = "SickOfBeige Case Builder";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.Form1_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.Form1_DragEnter);
            ((System.ComponentModel.ISupportInitialize)(this.offsetbox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.holediamBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown offsetbox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown holediamBox;
    }
}

