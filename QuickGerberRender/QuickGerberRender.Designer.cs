namespace WindowsFormsApplication1
{
    partial class QuickGerberRender
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QuickGerberRender));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.TracesBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.CopperColor = new System.Windows.Forms.ComboBox();
            this.SilkScreenColor = new System.Windows.Forms.ComboBox();
            this.SolderMaskColor = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.SystemColors.Control;
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.Controls.Add(this.TracesBox, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.CopperColor, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.SilkScreenColor, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.SolderMaskColor, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label4, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.label3, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 393);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(5);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 88F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 72F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 95F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1213, 322);
            this.tableLayoutPanel1.TabIndex = 5;
            // 
            // TracesBox
            // 
            this.TracesBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TracesBox.FormattingEnabled = true;
            this.TracesBox.Items.AddRange(new object[] {
            "Auto"});
            this.TracesBox.Location = new System.Drawing.Point(16, 241);
            this.TracesBox.Margin = new System.Windows.Forms.Padding(16, 14, 16, 14);
            this.TracesBox.Name = "TracesBox";
            this.TracesBox.Size = new System.Drawing.Size(372, 39);
            this.TracesBox.TabIndex = 12;
            this.TracesBox.Text = "Auto";
            this.TracesBox.SelectedIndexChanged += new System.EventHandler(this.TracesBox_SelectedIndexChanged);
            this.TracesBox.TextUpdate += new System.EventHandler(this.TracesBox_TextUpdate);
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(16, 169);
            this.label1.Margin = new System.Windows.Forms.Padding(16, 14, 16, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(372, 44);
            this.label1.TabIndex = 11;
            this.label1.Text = "Traces";
            // 
            // CopperColor
            // 
            this.CopperColor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CopperColor.FormattingEnabled = true;
            this.CopperColor.Location = new System.Drawing.Point(824, 81);
            this.CopperColor.Margin = new System.Windows.Forms.Padding(16, 14, 16, 14);
            this.CopperColor.Name = "CopperColor";
            this.CopperColor.Size = new System.Drawing.Size(373, 39);
            this.CopperColor.TabIndex = 9;
            this.CopperColor.Text = "Gold";
            this.CopperColor.SelectedIndexChanged += new System.EventHandler(this.CopperColor_SelectedIndexChanged);
            this.CopperColor.TextUpdate += new System.EventHandler(this.CopperColor_TextUpdate);
            // 
            // SilkScreenColor
            // 
            this.SilkScreenColor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SilkScreenColor.FormattingEnabled = true;
            this.SilkScreenColor.Location = new System.Drawing.Point(420, 81);
            this.SilkScreenColor.Margin = new System.Windows.Forms.Padding(16, 14, 16, 14);
            this.SilkScreenColor.Name = "SilkScreenColor";
            this.SilkScreenColor.Size = new System.Drawing.Size(372, 39);
            this.SilkScreenColor.TabIndex = 8;
            this.SilkScreenColor.Text = "White";
            this.SilkScreenColor.SelectedIndexChanged += new System.EventHandler(this.SilkScreenColor_SelectedIndexChanged);
            this.SilkScreenColor.TextUpdate += new System.EventHandler(this.SilkScreenColor_TextUpdate);
            // 
            // SolderMaskColor
            // 
            this.SolderMaskColor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SolderMaskColor.FormattingEnabled = true;
            this.SolderMaskColor.Location = new System.Drawing.Point(16, 81);
            this.SolderMaskColor.Margin = new System.Windows.Forms.Padding(16, 14, 16, 14);
            this.SolderMaskColor.Name = "SolderMaskColor";
            this.SolderMaskColor.Size = new System.Drawing.Size(372, 39);
            this.SolderMaskColor.TabIndex = 7;
            this.SolderMaskColor.Text = "Green";
            this.SolderMaskColor.SelectedIndexChanged += new System.EventHandler(this.SolderMaskColor_SelectedIndexChanged);
            this.SolderMaskColor.TextUpdate += new System.EventHandler(this.SolderMaskColor_TextUpdate);
            // 
            // label4
            // 
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Location = new System.Drawing.Point(824, 14);
            this.label4.Margin = new System.Windows.Forms.Padding(16, 14, 16, 14);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(373, 39);
            this.label4.TabIndex = 5;
            this.label4.Text = "Pads";
            this.label4.Click += new System.EventHandler(this.label4_Click);
            // 
            // label3
            // 
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(420, 14);
            this.label3.Margin = new System.Windows.Forms.Padding(16, 14, 16, 14);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(372, 39);
            this.label3.TabIndex = 4;
            this.label3.Text = "Silk";
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(16, 14);
            this.label2.Margin = new System.Windows.Forms.Padding(16, 14, 16, 14);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(372, 39);
            this.label2.TabIndex = 3;
            this.label2.Text = "Soldermask";
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(5);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(1213, 334);
            this.pictureBox1.TabIndex = 6;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox1_Paint);
            this.pictureBox1.Resize += new System.EventHandler(this.pictureBox1_Resize);
            // 
            // splitter1
            // 
            this.splitter1.Cursor = System.Windows.Forms.Cursors.HSplit;
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitter1.Location = new System.Drawing.Point(0, 369);
            this.splitter1.Margin = new System.Windows.Forms.Padding(5);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(1213, 24);
            this.splitter1.TabIndex = 7;
            this.splitter1.TabStop = false;
            // 
            // QuickGerberRender
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(1213, 715);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.pictureBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "QuickGerberRender";
            this.Text = "TINRS - Quick Gerber Renderer";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.Form1_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.Form1_DragEnter);
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ComboBox SolderMaskColor;
        private System.Windows.Forms.ComboBox CopperColor;
        private System.Windows.Forms.ComboBox SilkScreenColor;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.ComboBox TracesBox;
        private System.Windows.Forms.Label label1;
    }
}

