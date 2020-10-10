namespace PnP_Processor
{
    partial class Actions
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
            this.selectbom = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.pnpbox = new System.Windows.Forms.TextBox();
            this.stockbox = new System.Windows.Forms.TextBox();
            this.outlinebox = new System.Windows.Forms.TextBox();
            this.bombox = new System.Windows.Forms.TextBox();
            this.selectpnp = new System.Windows.Forms.Button();
            this.selectstock = new System.Windows.Forms.Button();
            this.selectoutline = new System.Windows.Forms.Button();
            this.selectsilk = new System.Windows.Forms.Button();
            this.silkbox = new System.Windows.Forms.TextBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.gerberzipbutton = new System.Windows.Forms.Button();
            this.gerberzipbox = new System.Windows.Forms.TextBox();
            this.ProcessButton = new System.Windows.Forms.Button();
            this.topsilk = new System.Windows.Forms.CheckBox();
            this.bottomsilk = new System.Windows.Forms.CheckBox();
            this.flipDiag = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // selectbom
            // 
            this.selectbom.AutoSize = true;
            this.selectbom.Location = new System.Drawing.Point(411, 29);
            this.selectbom.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.selectbom.Name = "selectbom";
            this.selectbom.Size = new System.Drawing.Size(154, 23);
            this.selectbom.TabIndex = 0;
            this.selectbom.Text = "BOM File";
            this.selectbom.UseVisualStyleBackColor = true;
            this.selectbom.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(1, 1);
            this.button2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(92, 27);
            this.button2.TabIndex = 1;
            this.button2.Text = "Rotate 90";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(97, 1);
            this.button3.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(221, 27);
            this.button3.TabIndex = 2;
            this.button3.Text = "Switch Sides";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // pnpbox
            // 
            this.pnpbox.Location = new System.Drawing.Point(1, 52);
            this.pnpbox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.pnpbox.Name = "pnpbox";
            this.pnpbox.Size = new System.Drawing.Size(399, 20);
            this.pnpbox.TabIndex = 2;
            this.pnpbox.TextChanged += new System.EventHandler(this.pnpbox_TextChanged);
            // 
            // stockbox
            // 
            this.stockbox.Location = new System.Drawing.Point(1, 73);
            this.stockbox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.stockbox.Name = "stockbox";
            this.stockbox.Size = new System.Drawing.Size(399, 20);
            this.stockbox.TabIndex = 3;
            this.stockbox.TextChanged += new System.EventHandler(this.stockbox_TextChanged);
            // 
            // outlinebox
            // 
            this.outlinebox.Location = new System.Drawing.Point(1, 94);
            this.outlinebox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.outlinebox.Name = "outlinebox";
            this.outlinebox.Size = new System.Drawing.Size(399, 20);
            this.outlinebox.TabIndex = 4;
            this.outlinebox.TextChanged += new System.EventHandler(this.outlinebox_TextChanged);
            // 
            // bombox
            // 
            this.bombox.Location = new System.Drawing.Point(1, 31);
            this.bombox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.bombox.Name = "bombox";
            this.bombox.Size = new System.Drawing.Size(399, 20);
            this.bombox.TabIndex = 5;
            this.bombox.TextChanged += new System.EventHandler(this.bombox_TextChanged);
            // 
            // selectpnp
            // 
            this.selectpnp.AutoSize = true;
            this.selectpnp.Location = new System.Drawing.Point(411, 49);
            this.selectpnp.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.selectpnp.Name = "selectpnp";
            this.selectpnp.Size = new System.Drawing.Size(154, 23);
            this.selectpnp.TabIndex = 6;
            this.selectpnp.Text = "PNP File";
            this.selectpnp.UseVisualStyleBackColor = true;
            this.selectpnp.Click += new System.EventHandler(this.selectpnp_Click);
            // 
            // selectstock
            // 
            this.selectstock.AutoSize = true;
            this.selectstock.Location = new System.Drawing.Point(411, 70);
            this.selectstock.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.selectstock.Name = "selectstock";
            this.selectstock.Size = new System.Drawing.Size(154, 23);
            this.selectstock.TabIndex = 7;
            this.selectstock.Text = "Stock File";
            this.selectstock.UseVisualStyleBackColor = true;
            this.selectstock.Click += new System.EventHandler(this.selectstock_Click);
            // 
            // selectoutline
            // 
            this.selectoutline.AutoSize = true;
            this.selectoutline.Location = new System.Drawing.Point(411, 91);
            this.selectoutline.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.selectoutline.Name = "selectoutline";
            this.selectoutline.Size = new System.Drawing.Size(154, 23);
            this.selectoutline.TabIndex = 8;
            this.selectoutline.Text = "Outline File";
            this.selectoutline.UseVisualStyleBackColor = true;
            this.selectoutline.Click += new System.EventHandler(this.selectoutline_Click);
            // 
            // selectsilk
            // 
            this.selectsilk.AutoSize = true;
            this.selectsilk.Location = new System.Drawing.Point(411, 112);
            this.selectsilk.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.selectsilk.Name = "selectsilk";
            this.selectsilk.Size = new System.Drawing.Size(154, 23);
            this.selectsilk.TabIndex = 10;
            this.selectsilk.Text = "Silk File";
            this.selectsilk.UseVisualStyleBackColor = true;
            this.selectsilk.Click += new System.EventHandler(this.selectsilk_Click);
            // 
            // silkbox
            // 
            this.silkbox.Location = new System.Drawing.Point(1, 114);
            this.silkbox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.silkbox.Name = "silkbox";
            this.silkbox.Size = new System.Drawing.Size(399, 20);
            this.silkbox.TabIndex = 9;
            this.silkbox.TextChanged += new System.EventHandler(this.silkbox_TextChanged);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "All Files|*.*";
            // 
            // gerberzipbutton
            // 
            this.gerberzipbutton.AutoSize = true;
            this.gerberzipbutton.Location = new System.Drawing.Point(411, 133);
            this.gerberzipbutton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.gerberzipbutton.Name = "gerberzipbutton";
            this.gerberzipbutton.Size = new System.Drawing.Size(154, 23);
            this.gerberzipbutton.TabIndex = 12;
            this.gerberzipbutton.Text = "Gerber Zip File";
            this.gerberzipbutton.UseVisualStyleBackColor = true;
            this.gerberzipbutton.Click += new System.EventHandler(this.gerberzipbutton_Click);
            // 
            // gerberzipbox
            // 
            this.gerberzipbox.Location = new System.Drawing.Point(1, 135);
            this.gerberzipbox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.gerberzipbox.Name = "gerberzipbox";
            this.gerberzipbox.Size = new System.Drawing.Size(399, 20);
            this.gerberzipbox.TabIndex = 11;
            this.gerberzipbox.TextChanged += new System.EventHandler(this.gerberzipbox_TextChanged);
            // 
            // ProcessButton
            // 
            this.ProcessButton.Location = new System.Drawing.Point(322, 1);
            this.ProcessButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.ProcessButton.Name = "ProcessButton";
            this.ProcessButton.Size = new System.Drawing.Size(221, 27);
            this.ProcessButton.TabIndex = 13;
            this.ProcessButton.Text = "Process!";
            this.ProcessButton.UseVisualStyleBackColor = true;
            this.ProcessButton.Click += new System.EventHandler(this.ProcessButton_Click);
            // 
            // topsilk
            // 
            this.topsilk.AutoSize = true;
            this.topsilk.Location = new System.Drawing.Point(650, 54);
            this.topsilk.Name = "topsilk";
            this.topsilk.Size = new System.Drawing.Size(65, 17);
            this.topsilk.TabIndex = 14;
            this.topsilk.Text = "Top Silk";
            this.topsilk.UseVisualStyleBackColor = true;
            this.topsilk.CheckedChanged += new System.EventHandler(this.topsilk_CheckedChanged);
            // 
            // bottomsilk
            // 
            this.bottomsilk.AutoSize = true;
            this.bottomsilk.Location = new System.Drawing.Point(650, 77);
            this.bottomsilk.Name = "bottomsilk";
            this.bottomsilk.Size = new System.Drawing.Size(79, 17);
            this.bottomsilk.TabIndex = 15;
            this.bottomsilk.Text = "Bottom Silk";
            this.bottomsilk.UseVisualStyleBackColor = true;
            this.bottomsilk.CheckedChanged += new System.EventHandler(this.bottomsilk_CheckedChanged);
            // 
            // flipDiag
            // 
            this.flipDiag.AutoSize = true;
            this.flipDiag.Location = new System.Drawing.Point(650, 112);
            this.flipDiag.Name = "flipDiag";
            this.flipDiag.Size = new System.Drawing.Size(87, 17);
            this.flipDiag.TabIndex = 16;
            this.flipDiag.Text = "Flip Diagonal";
            this.flipDiag.UseVisualStyleBackColor = true;
            this.flipDiag.CheckedChanged += new System.EventHandler(this.flipDiag_CheckedChanged);
            // 
            // Actions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ClientSize = new System.Drawing.Size(880, 162);
            this.Controls.Add(this.flipDiag);
            this.Controls.Add(this.bottomsilk);
            this.Controls.Add(this.topsilk);
            this.Controls.Add(this.ProcessButton);
            this.Controls.Add(this.gerberzipbutton);
            this.Controls.Add(this.gerberzipbox);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.selectsilk);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.silkbox);
            this.Controls.Add(this.selectoutline);
            this.Controls.Add(this.selectstock);
            this.Controls.Add(this.selectpnp);
            this.Controls.Add(this.selectbom);
            this.Controls.Add(this.bombox);
            this.Controls.Add(this.outlinebox);
            this.Controls.Add(this.stockbox);
            this.Controls.Add(this.pnpbox);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "Actions";
            this.Text = "Actions";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button selectbom;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.TextBox pnpbox;
        private System.Windows.Forms.TextBox stockbox;
        private System.Windows.Forms.TextBox outlinebox;
        private System.Windows.Forms.TextBox bombox;
        private System.Windows.Forms.Button selectpnp;
        private System.Windows.Forms.Button selectstock;
        private System.Windows.Forms.Button selectoutline;
        private System.Windows.Forms.Button selectsilk;
        private System.Windows.Forms.TextBox silkbox;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button gerberzipbutton;
        private System.Windows.Forms.TextBox gerberzipbox;
        private System.Windows.Forms.Button ProcessButton;
        private System.Windows.Forms.CheckBox topsilk;
        private System.Windows.Forms.CheckBox bottomsilk;
        private System.Windows.Forms.CheckBox flipDiag;
    }
}