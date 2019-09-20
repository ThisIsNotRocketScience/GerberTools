namespace GerberCombinerBuilder
{
    partial class PanelProperties
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PanelProperties));
            this.WidthBox = new System.Windows.Forms.NumericUpDown();
            this.HeightBox = new System.Windows.Forms.NumericUpDown();
            this.MarginBox = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.FillEmpty = new System.Windows.Forms.CheckBox();
            this.filloffsetbox = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.smoothoffsetbox = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.ExtraTabDrillDistance = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.ClipToOutlines = new System.Windows.Forms.CheckBox();
            this.noMouseBites = new System.Windows.Forms.CheckBox();
            this.mergebyfiletypebox = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.WidthBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.HeightBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MarginBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.filloffsetbox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.smoothoffsetbox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ExtraTabDrillDistance)).BeginInit();
            this.SuspendLayout();
            // 
            // WidthBox
            // 
            this.WidthBox.DecimalPlaces = 2;
            this.WidthBox.Location = new System.Drawing.Point(374, 37);
            this.WidthBox.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.WidthBox.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.WidthBox.Name = "WidthBox";
            this.WidthBox.Size = new System.Drawing.Size(240, 38);
            this.WidthBox.TabIndex = 0;
            this.WidthBox.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // HeightBox
            // 
            this.HeightBox.DecimalPlaces = 2;
            this.HeightBox.Location = new System.Drawing.Point(374, 91);
            this.HeightBox.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.HeightBox.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.HeightBox.Name = "HeightBox";
            this.HeightBox.Size = new System.Drawing.Size(240, 38);
            this.HeightBox.TabIndex = 1;
            this.HeightBox.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // MarginBox
            // 
            this.MarginBox.DecimalPlaces = 2;
            this.MarginBox.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.MarginBox.Location = new System.Drawing.Point(374, 145);
            this.MarginBox.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.MarginBox.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.MarginBox.Name = "MarginBox";
            this.MarginBox.Size = new System.Drawing.Size(240, 38);
            this.MarginBox.TabIndex = 2;
            this.MarginBox.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(270, 37);
            this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 32);
            this.label1.TabIndex = 3;
            this.label1.Text = "Width";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(274, 91);
            this.label2.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(98, 32);
            this.label2.TabIndex = 4;
            this.label2.Text = "Height";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(50, 149);
            this.label3.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(311, 32);
            this.label3.TabIndex = 5;
            this.label3.Text = "Margin between boards";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Location = new System.Drawing.Point(415, 780);
            this.button1.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(182, 54);
            this.button1.TabIndex = 6;
            this.button1.Text = "Ok";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.OkButton);
            // 
            // button2
            // 
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(65, 780);
            this.button2.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(182, 54);
            this.button2.TabIndex = 7;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.CancelButtonPress);
            // 
            // FillEmpty
            // 
            this.FillEmpty.AutoSize = true;
            this.FillEmpty.Location = new System.Drawing.Point(374, 215);
            this.FillEmpty.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.FillEmpty.Name = "FillEmpty";
            this.FillEmpty.Size = new System.Drawing.Size(223, 36);
            this.FillEmpty.TabIndex = 9;
            this.FillEmpty.Text = "Fill empty area";
            this.FillEmpty.UseVisualStyleBackColor = true;
            // 
            // filloffsetbox
            // 
            this.filloffsetbox.DecimalPlaces = 2;
            this.filloffsetbox.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.filloffsetbox.Location = new System.Drawing.Point(374, 275);
            this.filloffsetbox.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.filloffsetbox.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.filloffsetbox.Name = "filloffsetbox";
            this.filloffsetbox.Size = new System.Drawing.Size(240, 38);
            this.filloffsetbox.TabIndex = 10;
            this.filloffsetbox.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(42, 279);
            this.label4.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(318, 32);
            this.label4.TabIndex = 11;
            this.label4.Text = "Fill offset for empty area";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(42, 333);
            this.label5.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(298, 32);
            this.label5.TabIndex = 13;
            this.label5.Text = "Empty area smoothing";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // smoothoffsetbox
            // 
            this.smoothoffsetbox.DecimalPlaces = 2;
            this.smoothoffsetbox.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.smoothoffsetbox.Location = new System.Drawing.Point(374, 329);
            this.smoothoffsetbox.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.smoothoffsetbox.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.smoothoffsetbox.Name = "smoothoffsetbox";
            this.smoothoffsetbox.Size = new System.Drawing.Size(240, 38);
            this.smoothoffsetbox.TabIndex = 12;
            this.smoothoffsetbox.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(20, 426);
            this.label6.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(342, 32);
            this.label6.TabIndex = 15;
            this.label6.Text = "Extra distance for tabdrills";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ExtraTabDrillDistance
            // 
            this.ExtraTabDrillDistance.DecimalPlaces = 3;
            this.ExtraTabDrillDistance.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.ExtraTabDrillDistance.Location = new System.Drawing.Point(374, 422);
            this.ExtraTabDrillDistance.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ExtraTabDrillDistance.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.ExtraTabDrillDistance.Name = "ExtraTabDrillDistance";
            this.ExtraTabDrillDistance.Size = new System.Drawing.Size(240, 38);
            this.ExtraTabDrillDistance.TabIndex = 14;
            this.ExtraTabDrillDistance.Value = new decimal(new int[] {
            2,
            0,
            0,
            65536});
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(628, 46);
            this.label7.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(61, 32);
            this.label7.TabIndex = 18;
            this.label7.Text = "mm";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(628, 101);
            this.label8.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(61, 32);
            this.label8.TabIndex = 19;
            this.label8.Text = "mm";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(628, 155);
            this.label9.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(61, 32);
            this.label9.TabIndex = 20;
            this.label9.Text = "mm";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(628, 339);
            this.label10.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(61, 32);
            this.label10.TabIndex = 21;
            this.label10.Text = "mm";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(628, 285);
            this.label11.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(61, 32);
            this.label11.TabIndex = 22;
            this.label11.Text = "mm";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(628, 432);
            this.label12.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(61, 32);
            this.label12.TabIndex = 23;
            this.label12.Text = "mm";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ClipToOutlines
            // 
            this.ClipToOutlines.AutoSize = true;
            this.ClipToOutlines.Location = new System.Drawing.Point(39, 520);
            this.ClipToOutlines.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.ClipToOutlines.Name = "ClipToOutlines";
            this.ClipToOutlines.Size = new System.Drawing.Size(305, 36);
            this.ClipToOutlines.TabIndex = 24;
            this.ClipToOutlines.Text = "Clip to board outlines";
            this.ClipToOutlines.UseVisualStyleBackColor = true;
            // 
            // noMouseBites
            // 
            this.noMouseBites.AutoSize = true;
            this.noMouseBites.Location = new System.Drawing.Point(39, 574);
            this.noMouseBites.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.noMouseBites.Name = "noMouseBites";
            this.noMouseBites.Size = new System.Drawing.Size(393, 36);
            this.noMouseBites.TabIndex = 25;
            this.noMouseBites.Text = "Do not generate mousebites";
            this.noMouseBites.UseVisualStyleBackColor = true;
            this.noMouseBites.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // mergebyfiletypebox
            // 
            this.mergebyfiletypebox.AutoSize = true;
            this.mergebyfiletypebox.Location = new System.Drawing.Point(39, 628);
            this.mergebyfiletypebox.Margin = new System.Windows.Forms.Padding(6);
            this.mergebyfiletypebox.Name = "mergebyfiletypebox";
            this.mergebyfiletypebox.Size = new System.Drawing.Size(528, 36);
            this.mergebyfiletypebox.TabIndex = 26;
            this.mergebyfiletypebox.Text = "Merge by inferred filetype, not filename";
            this.mergebyfiletypebox.UseVisualStyleBackColor = true;
            this.mergebyfiletypebox.CheckedChanged += new System.EventHandler(this.CheckBox1_CheckedChanged_1);
            // 
            // PanelProperties
            // 
            this.AcceptButton = this.button1;
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button2;
            this.ClientSize = new System.Drawing.Size(750, 868);
            this.Controls.Add(this.mergebyfiletypebox);
            this.Controls.Add(this.noMouseBites);
            this.Controls.Add(this.ClipToOutlines);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.ExtraTabDrillDistance);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.smoothoffsetbox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.filloffsetbox);
            this.Controls.Add(this.FillEmpty);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.MarginBox);
            this.Controls.Add(this.HeightBox);
            this.Controls.Add(this.WidthBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Name = "PanelProperties";
            this.Text = "Panel Properties";
            ((System.ComponentModel.ISupportInitialize)(this.WidthBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.HeightBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MarginBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.filloffsetbox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.smoothoffsetbox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ExtraTabDrillDistance)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown WidthBox;
        private System.Windows.Forms.NumericUpDown HeightBox;
        private System.Windows.Forms.NumericUpDown MarginBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.CheckBox FillEmpty;
        private System.Windows.Forms.NumericUpDown filloffsetbox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown smoothoffsetbox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown ExtraTabDrillDistance;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.CheckBox ClipToOutlines;
        private System.Windows.Forms.CheckBox noMouseBites;
        private System.Windows.Forms.CheckBox mergebyfiletypebox;
    }
}