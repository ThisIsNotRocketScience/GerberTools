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
            this.pnpbox = new System.Windows.Forms.TextBox();
            this.stockbox = new System.Windows.Forms.TextBox();
            this.bombox = new System.Windows.Forms.TextBox();
            this.selectpnp = new System.Windows.Forms.Button();
            this.selectstock = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.gerberzipbutton = new System.Windows.Forms.Button();
            this.gerberzipbox = new System.Windows.Forms.TextBox();
            this.ProcessButton = new System.Windows.Forms.Button();
            this.topsilk = new System.Windows.Forms.CheckBox();
            this.bottomsilk = new System.Windows.Forms.CheckBox();
            this.FlipModeBox = new System.Windows.Forms.ListBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button1 = new System.Windows.Forms.Button();
            this.diprotatefile = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // selectbom
            // 
            this.selectbom.AutoSize = true;
            this.selectbom.Location = new System.Drawing.Point(336, 58);
            this.selectbom.Name = "selectbom";
            this.selectbom.Size = new System.Drawing.Size(171, 30);
            this.selectbom.TabIndex = 0;
            this.selectbom.Text = "BOM File";
            this.selectbom.UseVisualStyleBackColor = true;
            this.selectbom.Click += new System.EventHandler(this.button1_Click);
            // 
            // pnpbox
            // 
            this.pnpbox.Location = new System.Drawing.Point(14, 90);
            this.pnpbox.Name = "pnpbox";
            this.pnpbox.Size = new System.Drawing.Size(316, 26);
            this.pnpbox.TabIndex = 2;
            this.pnpbox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.pnpbox.TextChanged += new System.EventHandler(this.pnpbox_TextChanged);
            // 
            // stockbox
            // 
            this.stockbox.Location = new System.Drawing.Point(14, 136);
            this.stockbox.Name = "stockbox";
            this.stockbox.Size = new System.Drawing.Size(316, 26);
            this.stockbox.TabIndex = 3;
            this.stockbox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.stockbox.TextChanged += new System.EventHandler(this.stockbox_TextChanged);
            // 
            // bombox
            // 
            this.bombox.Location = new System.Drawing.Point(14, 58);
            this.bombox.Name = "bombox";
            this.bombox.Size = new System.Drawing.Size(316, 26);
            this.bombox.TabIndex = 5;
            this.bombox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.bombox.TextChanged += new System.EventHandler(this.bombox_TextChanged);
            // 
            // selectpnp
            // 
            this.selectpnp.AutoSize = true;
            this.selectpnp.Location = new System.Drawing.Point(336, 94);
            this.selectpnp.Name = "selectpnp";
            this.selectpnp.Size = new System.Drawing.Size(171, 30);
            this.selectpnp.TabIndex = 6;
            this.selectpnp.Text = "PNP File";
            this.selectpnp.UseVisualStyleBackColor = true;
            this.selectpnp.Click += new System.EventHandler(this.selectpnp_Click);
            // 
            // selectstock
            // 
            this.selectstock.AutoSize = true;
            this.selectstock.Location = new System.Drawing.Point(336, 136);
            this.selectstock.Name = "selectstock";
            this.selectstock.Size = new System.Drawing.Size(171, 30);
            this.selectstock.TabIndex = 7;
            this.selectstock.Text = "Stock File";
            this.selectstock.UseVisualStyleBackColor = true;
            this.selectstock.Click += new System.EventHandler(this.selectstock_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "All Files|*.*";
            // 
            // gerberzipbutton
            // 
            this.gerberzipbutton.AutoSize = true;
            this.gerberzipbutton.Location = new System.Drawing.Point(336, 22);
            this.gerberzipbutton.Name = "gerberzipbutton";
            this.gerberzipbutton.Size = new System.Drawing.Size(171, 30);
            this.gerberzipbutton.TabIndex = 12;
            this.gerberzipbutton.Text = "Gerber Zip File";
            this.gerberzipbutton.UseVisualStyleBackColor = true;
            this.gerberzipbutton.Click += new System.EventHandler(this.gerberzipbutton_Click);
            // 
            // gerberzipbox
            // 
            this.gerberzipbox.Location = new System.Drawing.Point(14, 26);
            this.gerberzipbox.Name = "gerberzipbox";
            this.gerberzipbox.Size = new System.Drawing.Size(316, 26);
            this.gerberzipbox.TabIndex = 11;
            this.gerberzipbox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.gerberzipbox.TextChanged += new System.EventHandler(this.gerberzipbox_TextChanged);
            // 
            // ProcessButton
            // 
            this.ProcessButton.Location = new System.Drawing.Point(717, 136);
            this.ProcessButton.Name = "ProcessButton";
            this.ProcessButton.Size = new System.Drawing.Size(141, 26);
            this.ProcessButton.TabIndex = 13;
            this.ProcessButton.Text = "Process!";
            this.ProcessButton.UseVisualStyleBackColor = true;
            this.ProcessButton.Click += new System.EventHandler(this.ProcessButton_Click);
            // 
            // topsilk
            // 
            this.topsilk.AutoSize = true;
            this.topsilk.Location = new System.Drawing.Point(547, 94);
            this.topsilk.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.topsilk.Name = "topsilk";
            this.topsilk.Size = new System.Drawing.Size(91, 24);
            this.topsilk.TabIndex = 14;
            this.topsilk.Text = "Top Silk";
            this.topsilk.UseVisualStyleBackColor = true;
            this.topsilk.CheckedChanged += new System.EventHandler(this.topsilk_CheckedChanged);
            // 
            // bottomsilk
            // 
            this.bottomsilk.AutoSize = true;
            this.bottomsilk.Location = new System.Drawing.Point(547, 62);
            this.bottomsilk.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.bottomsilk.Name = "bottomsilk";
            this.bottomsilk.Size = new System.Drawing.Size(116, 24);
            this.bottomsilk.TabIndex = 15;
            this.bottomsilk.Text = "Bottom Silk";
            this.bottomsilk.UseVisualStyleBackColor = true;
            this.bottomsilk.CheckedChanged += new System.EventHandler(this.bottomsilk_CheckedChanged);
            // 
            // FlipModeBox
            // 
            this.FlipModeBox.FormattingEnabled = true;
            this.FlipModeBox.ItemHeight = 20;
            this.FlipModeBox.Items.AddRange(new object[] {
            "None",
            "Diagonal",
            "Horizontal"});
            this.FlipModeBox.Location = new System.Drawing.Point(756, 22);
            this.FlipModeBox.Name = "FlipModeBox";
            this.FlipModeBox.Size = new System.Drawing.Size(120, 84);
            this.FlipModeBox.TabIndex = 18;
            this.FlipModeBox.SelectedIndexChanged += new System.EventHandler(this.FlipModeBox_SelectedIndexChanged);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(1566, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(194, 766);
            this.pictureBox1.TabIndex = 19;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox1_Paint);
            this.pictureBox1.Resize += new System.EventHandler(this.pictureBox1_Resize);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel1.Controls.Add(this.pictureBox2, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.pictureBox1, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1763, 772);
            this.tableLayoutPanel1.TabIndex = 20;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox2.Location = new System.Drawing.Point(1366, 3);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(194, 766);
            this.pictureBox2.TabIndex = 21;
            this.pictureBox2.TabStop = false;
            this.pictureBox2.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox2_Paint);
            this.pictureBox2.Resize += new System.EventHandler(this.pictureBox2_Resize);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.diprotatefile);
            this.groupBox1.Controls.Add(this.bombox);
            this.groupBox1.Controls.Add(this.FlipModeBox);
            this.groupBox1.Controls.Add(this.selectstock);
            this.groupBox1.Controls.Add(this.selectbom);
            this.groupBox1.Controls.Add(this.pnpbox);
            this.groupBox1.Controls.Add(this.selectpnp);
            this.groupBox1.Controls.Add(this.gerberzipbox);
            this.groupBox1.Controls.Add(this.stockbox);
            this.groupBox1.Controls.Add(this.gerberzipbutton);
            this.groupBox1.Controls.Add(this.topsilk);
            this.groupBox1.Controls.Add(this.ProcessButton);
            this.groupBox1.Controls.Add(this.bottomsilk);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(882, 230);
            this.groupBox1.TabIndex = 22;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Things";
            // 
            // button1
            // 
            this.button1.AutoSize = true;
            this.button1.Location = new System.Drawing.Point(336, 172);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(171, 30);
            this.button1.TabIndex = 20;
            this.button1.Text = "diprotate file";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.diprotatebutton_click);
            // 
            // diprotatefile
            // 
            this.diprotatefile.Location = new System.Drawing.Point(14, 172);
            this.diprotatefile.Name = "diprotatefile";
            this.diprotatefile.Size = new System.Drawing.Size(316, 26);
            this.diprotatefile.TabIndex = 19;
            this.diprotatefile.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // Actions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ClientSize = new System.Drawing.Size(1763, 772);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "Actions";
            this.Text = "Actions";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button selectbom;
        private System.Windows.Forms.TextBox pnpbox;
        private System.Windows.Forms.TextBox stockbox;
        private System.Windows.Forms.TextBox bombox;
        private System.Windows.Forms.Button selectpnp;
        private System.Windows.Forms.Button selectstock;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button gerberzipbutton;
        private System.Windows.Forms.TextBox gerberzipbox;
        private System.Windows.Forms.Button ProcessButton;
        private System.Windows.Forms.CheckBox topsilk;
        private System.Windows.Forms.CheckBox bottomsilk;
        private System.Windows.Forms.ListBox FlipModeBox;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox diprotatefile;
    }
}