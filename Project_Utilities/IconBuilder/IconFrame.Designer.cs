namespace IconBuilder
{
    partial class IconFrame
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IconFrame));
            this.pb32 = new System.Windows.Forms.PictureBox();
            this.pb48 = new System.Windows.Forms.PictureBox();
            this.pb64 = new System.Windows.Forms.PictureBox();
            this.pb96 = new System.Windows.Forms.PictureBox();
            this.pb128 = new System.Windows.Forms.PictureBox();
            this.Letter = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pb32)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pb48)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pb64)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pb96)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pb128)).BeginInit();
            this.SuspendLayout();
            // 
            // pb32
            // 
            this.pb32.Location = new System.Drawing.Point(12, 32);
            this.pb32.Name = "pb32";
            this.pb32.Size = new System.Drawing.Size(32, 32);
            this.pb32.TabIndex = 0;
            this.pb32.TabStop = false;
            this.pb32.Paint += new System.Windows.Forms.PaintEventHandler(this.pb32_Paint);
            // 
            // pb48
            // 
            this.pb48.Location = new System.Drawing.Point(61, 32);
            this.pb48.Name = "pb48";
            this.pb48.Size = new System.Drawing.Size(48, 48);
            this.pb48.TabIndex = 1;
            this.pb48.TabStop = false;
            this.pb48.Paint += new System.Windows.Forms.PaintEventHandler(this.pb48_Paint);
            // 
            // pb64
            // 
            this.pb64.Location = new System.Drawing.Point(129, 32);
            this.pb64.Name = "pb64";
            this.pb64.Size = new System.Drawing.Size(64, 64);
            this.pb64.TabIndex = 2;
            this.pb64.TabStop = false;
            this.pb64.Paint += new System.Windows.Forms.PaintEventHandler(this.pb64_Paint);
            // 
            // pb96
            // 
            this.pb96.Location = new System.Drawing.Point(216, 32);
            this.pb96.Name = "pb96";
            this.pb96.Size = new System.Drawing.Size(96, 96);
            this.pb96.TabIndex = 3;
            this.pb96.TabStop = false;
            this.pb96.Paint += new System.Windows.Forms.PaintEventHandler(this.pb96_Paint);
            // 
            // pb128
            // 
            this.pb128.Location = new System.Drawing.Point(339, 32);
            this.pb128.Name = "pb128";
            this.pb128.Size = new System.Drawing.Size(128, 128);
            this.pb128.TabIndex = 4;
            this.pb128.TabStop = false;
            this.pb128.Paint += new System.Windows.Forms.PaintEventHandler(this.pb128_Paint);
            // 
            // Letter
            // 
            this.Letter.Location = new System.Drawing.Point(9, 167);
            this.Letter.Name = "Letter";
            this.Letter.Size = new System.Drawing.Size(100, 22);
            this.Letter.TabIndex = 5;
            this.Letter.TextChanged += new System.EventHandler(this.Letter_TextChanged);
            // 
            // IconFrame
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(473, 201);
            this.Controls.Add(this.Letter);
            this.Controls.Add(this.pb128);
            this.Controls.Add(this.pb96);
            this.Controls.Add(this.pb64);
            this.Controls.Add(this.pb48);
            this.Controls.Add(this.pb32);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "IconFrame";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.pb32)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pb48)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pb64)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pb96)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pb128)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pb32;
        private System.Windows.Forms.PictureBox pb48;
        private System.Windows.Forms.PictureBox pb64;
        private System.Windows.Forms.PictureBox pb96;
        private System.Windows.Forms.PictureBox pb128;
        private System.Windows.Forms.TextBox Letter;
    }
}

