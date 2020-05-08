namespace OpampCalculator
{
    partial class OpampCalculator
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OpampCalculator));
            this.InVoltMax = new System.Windows.Forms.TextBox();
            this.InVoltMin = new System.Windows.Forms.TextBox();
            this.OutVoltMin = new System.Windows.Forms.TextBox();
            this.OutvoltMax = new System.Windows.Forms.TextBox();
            this.SafetyMargin = new System.Windows.Forms.TextBox();
            this.OutVoltActualMax = new System.Windows.Forms.TextBox();
            this.OutVoltActualMin = new System.Windows.Forms.TextBox();
            this.OffsetVoltage = new System.Windows.Forms.TextBox();
            this.OffsetResistor = new System.Windows.Forms.TextBox();
            this.InputResistor = new System.Windows.Forms.TextBox();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.IN = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.Scale = new System.Windows.Forms.TextBox();
            this.Offset = new System.Windows.Forms.TextBox();
            this.FeedbackResistor = new System.Windows.Forms.TextBox();
            this.FeedbackResistorLabel = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.InputCombo = new System.Windows.Forms.ComboBox();
            this.FeedbackCombo = new System.Windows.Forms.ComboBox();
            this.OffsetCombo = new System.Windows.Forms.ComboBox();
            this.label17 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.SelectedMinBox = new System.Windows.Forms.TextBox();
            this.SelectedMaxBox = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label20 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.label23 = new System.Windows.Forms.Label();
            this.FeedbackInputCombo = new System.Windows.Forms.ComboBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // InVoltMax
            // 
            this.InVoltMax.Location = new System.Drawing.Point(149, 121);
            this.InVoltMax.Name = "InVoltMax";
            this.InVoltMax.Size = new System.Drawing.Size(100, 22);
            this.InVoltMax.TabIndex = 0;
            this.InVoltMax.Text = "5";
            // 
            // InVoltMin
            // 
            this.InVoltMin.Location = new System.Drawing.Point(149, 166);
            this.InVoltMin.Name = "InVoltMin";
            this.InVoltMin.Size = new System.Drawing.Size(100, 22);
            this.InVoltMin.TabIndex = 1;
            this.InVoltMin.Text = "-5";
            // 
            // OutVoltMin
            // 
            this.OutVoltMin.Location = new System.Drawing.Point(386, 166);
            this.OutVoltMin.Name = "OutVoltMin";
            this.OutVoltMin.Size = new System.Drawing.Size(100, 22);
            this.OutVoltMin.TabIndex = 3;
            this.OutVoltMin.Text = "0";
            // 
            // OutvoltMax
            // 
            this.OutvoltMax.Location = new System.Drawing.Point(386, 121);
            this.OutvoltMax.Name = "OutvoltMax";
            this.OutvoltMax.Size = new System.Drawing.Size(100, 22);
            this.OutvoltMax.TabIndex = 2;
            this.OutvoltMax.Text = "2.048";
            // 
            // SafetyMargin
            // 
            this.SafetyMargin.Location = new System.Drawing.Point(262, 251);
            this.SafetyMargin.Name = "SafetyMargin";
            this.SafetyMargin.Size = new System.Drawing.Size(100, 22);
            this.SafetyMargin.TabIndex = 4;
            this.SafetyMargin.Text = "0.2";
            // 
            // OutVoltActualMax
            // 
            this.OutVoltActualMax.Enabled = false;
            this.OutVoltActualMax.Location = new System.Drawing.Point(673, 121);
            this.OutVoltActualMax.Name = "OutVoltActualMax";
            this.OutVoltActualMax.Size = new System.Drawing.Size(153, 22);
            this.OutVoltActualMax.TabIndex = 5;
            // 
            // OutVoltActualMin
            // 
            this.OutVoltActualMin.Enabled = false;
            this.OutVoltActualMin.Location = new System.Drawing.Point(673, 166);
            this.OutVoltActualMin.Name = "OutVoltActualMin";
            this.OutVoltActualMin.Size = new System.Drawing.Size(153, 22);
            this.OutVoltActualMin.TabIndex = 6;
            // 
            // OffsetVoltage
            // 
            this.OffsetVoltage.Location = new System.Drawing.Point(78, 251);
            this.OffsetVoltage.Name = "OffsetVoltage";
            this.OffsetVoltage.Size = new System.Drawing.Size(100, 22);
            this.OffsetVoltage.TabIndex = 7;
            this.OffsetVoltage.Text = "-5";
            // 
            // OffsetResistor
            // 
            this.OffsetResistor.Enabled = false;
            this.OffsetResistor.Location = new System.Drawing.Point(575, 308);
            this.OffsetResistor.Name = "OffsetResistor";
            this.OffsetResistor.Size = new System.Drawing.Size(100, 22);
            this.OffsetResistor.TabIndex = 8;
            // 
            // InputResistor
            // 
            this.InputResistor.Enabled = false;
            this.InputResistor.Location = new System.Drawing.Point(575, 251);
            this.InputResistor.Name = "InputResistor";
            this.InputResistor.Size = new System.Drawing.Size(100, 22);
            this.InputResistor.TabIndex = 9;
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(24, 26);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(47, 21);
            this.radioButton1.TabIndex = 10;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "Up";
            this.radioButton1.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioButton2);
            this.groupBox1.Controls.Add(this.radioButton1);
            this.groupBox1.Location = new System.Drawing.Point(411, 231);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(118, 112);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Rounding";
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(24, 69);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(64, 21);
            this.radioButton2.TabIndex = 11;
            this.radioButton2.TabStop = true;
            this.radioButton2.Text = "Down";
            this.radioButton2.UseVisualStyleBackColor = true;
            // 
            // IN
            // 
            this.IN.AutoSize = true;
            this.IN.Location = new System.Drawing.Point(182, 101);
            this.IN.Name = "IN";
            this.IN.Size = new System.Drawing.Size(19, 17);
            this.IN.TabIndex = 12;
            this.IN.Text = "In";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(392, 101);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 17);
            this.label1.TabIndex = 13;
            this.label1.Text = "Desired Out";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(716, 101);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(31, 17);
            this.label2.TabIndex = 14;
            this.label2.Text = "Out";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(75, 231);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(159, 17);
            this.label3.TabIndex = 15;
            this.label3.Text = "Available Offset Voltage";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(278, 231);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(91, 17);
            this.label4.TabIndex = 16;
            this.label4.Text = "Safetymargin";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(572, 288);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(98, 17);
            this.label5.TabIndex = 17;
            this.label5.Text = "OffsetResistor";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(572, 231);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(91, 17);
            this.label6.TabIndex = 18;
            this.label6.Text = "InputResistor";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(184, 254);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(17, 17);
            this.label7.TabIndex = 19;
            this.label7.Text = "V";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(368, 254);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(17, 17);
            this.label8.TabIndex = 20;
            this.label8.Text = "V";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(255, 124);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(17, 17);
            this.label9.TabIndex = 21;
            this.label9.Text = "V";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(255, 169);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(17, 17);
            this.label10.TabIndex = 22;
            this.label10.Text = "V";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(492, 124);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(17, 17);
            this.label11.TabIndex = 23;
            this.label11.Text = "V";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(492, 169);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(17, 17);
            this.label12.TabIndex = 24;
            this.label12.Text = "V";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(832, 124);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(17, 17);
            this.label13.TabIndex = 25;
            this.label13.Text = "V";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(832, 171);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(17, 17);
            this.label14.TabIndex = 26;
            this.label14.Text = "V";
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // Scale
            // 
            this.Scale.Enabled = false;
            this.Scale.Location = new System.Drawing.Point(534, 119);
            this.Scale.Name = "Scale";
            this.Scale.Size = new System.Drawing.Size(100, 22);
            this.Scale.TabIndex = 27;
            this.Scale.Text = "0";
            // 
            // Offset
            // 
            this.Offset.Enabled = false;
            this.Offset.Location = new System.Drawing.Point(534, 164);
            this.Offset.Name = "Offset";
            this.Offset.Size = new System.Drawing.Size(100, 22);
            this.Offset.TabIndex = 28;
            this.Offset.Text = "0";
            // 
            // FeedbackResistor
            // 
            this.FeedbackResistor.Enabled = false;
            this.FeedbackResistor.Location = new System.Drawing.Point(737, 278);
            this.FeedbackResistor.Name = "FeedbackResistor";
            this.FeedbackResistor.Size = new System.Drawing.Size(100, 22);
            this.FeedbackResistor.TabIndex = 29;
            // 
            // FeedbackResistorLabel
            // 
            this.FeedbackResistorLabel.AutoSize = true;
            this.FeedbackResistorLabel.Location = new System.Drawing.Point(734, 258);
            this.FeedbackResistorLabel.Name = "FeedbackResistorLabel";
            this.FeedbackResistorLabel.Size = new System.Drawing.Size(122, 17);
            this.FeedbackResistorLabel.TabIndex = 30;
            this.FeedbackResistorLabel.Text = "FeedbackResistor";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(534, 101);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(118, 17);
            this.label15.TabIndex = 31;
            this.label15.Text = "Calculated Scalar";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(536, 144);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(116, 17);
            this.label16.TabIndex = 32;
            this.label16.Text = "Calculated Offset";
            // 
            // InputCombo
            // 
            this.InputCombo.FormattingEnabled = true;
            this.InputCombo.Location = new System.Drawing.Point(395, 462);
            this.InputCombo.Name = "InputCombo";
            this.InputCombo.Size = new System.Drawing.Size(121, 24);
            this.InputCombo.TabIndex = 33;
            // 
            // FeedbackCombo
            // 
            this.FeedbackCombo.FormattingEnabled = true;
            this.FeedbackCombo.Location = new System.Drawing.Point(575, 489);
            this.FeedbackCombo.Name = "FeedbackCombo";
            this.FeedbackCombo.Size = new System.Drawing.Size(121, 24);
            this.FeedbackCombo.TabIndex = 34;
            // 
            // OffsetCombo
            // 
            this.OffsetCombo.FormattingEnabled = true;
            this.OffsetCombo.Location = new System.Drawing.Point(395, 514);
            this.OffsetCombo.Name = "OffsetCombo";
            this.OffsetCombo.Size = new System.Drawing.Size(121, 24);
            this.OffsetCombo.TabIndex = 35;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(922, 512);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(17, 17);
            this.label17.TabIndex = 40;
            this.label17.Text = "V";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(922, 465);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(17, 17);
            this.label18.TabIndex = 39;
            this.label18.Text = "V";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(806, 442);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(31, 17);
            this.label19.TabIndex = 38;
            this.label19.Text = "Out";
            // 
            // SelectedMinBox
            // 
            this.SelectedMinBox.Enabled = false;
            this.SelectedMinBox.Location = new System.Drawing.Point(763, 507);
            this.SelectedMinBox.Name = "SelectedMinBox";
            this.SelectedMinBox.Size = new System.Drawing.Size(153, 22);
            this.SelectedMinBox.TabIndex = 37;
            // 
            // SelectedMaxBox
            // 
            this.SelectedMaxBox.Enabled = false;
            this.SelectedMaxBox.Location = new System.Drawing.Point(763, 462);
            this.SelectedMaxBox.Name = "SelectedMaxBox";
            this.SelectedMaxBox.Size = new System.Drawing.Size(153, 22);
            this.SelectedMaxBox.TabIndex = 36;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(135, 462);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(182, 76);
            this.button1.TabIndex = 41;
            this.button1.Text = "Transfer -> ";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(574, 463);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(122, 17);
            this.label20.TabIndex = 42;
            this.label20.Text = "FeedbackResistor";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(395, 494);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(98, 17);
            this.label21.TabIndex = 43;
            this.label21.Text = "OffsetResistor";
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(395, 442);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(91, 17);
            this.label22.TabIndex = 44;
            this.label22.Text = "InputResistor";
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(736, 313);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(122, 17);
            this.label23.TabIndex = 46;
            this.label23.Text = "FeedbackResistor";
            // 
            // FeedbackInputCombo
            // 
            this.FeedbackInputCombo.FormattingEnabled = true;
            this.FeedbackInputCombo.Location = new System.Drawing.Point(737, 339);
            this.FeedbackInputCombo.Name = "FeedbackInputCombo";
            this.FeedbackInputCombo.Size = new System.Drawing.Size(121, 24);
            this.FeedbackInputCombo.TabIndex = 45;
            // 
            // OpampCalculator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1021, 657);
            this.Controls.Add(this.label23);
            this.Controls.Add(this.FeedbackInputCombo);
            this.Controls.Add(this.label22);
            this.Controls.Add(this.label21);
            this.Controls.Add(this.label20);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label17);
            this.Controls.Add(this.label18);
            this.Controls.Add(this.label19);
            this.Controls.Add(this.SelectedMinBox);
            this.Controls.Add(this.SelectedMaxBox);
            this.Controls.Add(this.OffsetCombo);
            this.Controls.Add(this.FeedbackCombo);
            this.Controls.Add(this.InputCombo);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.FeedbackResistorLabel);
            this.Controls.Add(this.FeedbackResistor);
            this.Controls.Add(this.Offset);
            this.Controls.Add(this.Scale);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.IN);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.InputResistor);
            this.Controls.Add(this.OffsetResistor);
            this.Controls.Add(this.OffsetVoltage);
            this.Controls.Add(this.OutVoltActualMin);
            this.Controls.Add(this.OutVoltActualMax);
            this.Controls.Add(this.SafetyMargin);
            this.Controls.Add(this.OutVoltMin);
            this.Controls.Add(this.OutvoltMax);
            this.Controls.Add(this.InVoltMin);
            this.Controls.Add(this.InVoltMax);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "OpampCalculator";
            this.Text = "TiNRS Opamp Feedback Calculator";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox InVoltMax;
        private System.Windows.Forms.TextBox InVoltMin;
        private System.Windows.Forms.TextBox OutVoltMin;
        private System.Windows.Forms.TextBox OutvoltMax;
        private System.Windows.Forms.TextBox SafetyMargin;
        private System.Windows.Forms.TextBox OutVoltActualMax;
        private System.Windows.Forms.TextBox OutVoltActualMin;
        private System.Windows.Forms.TextBox OffsetVoltage;
        private System.Windows.Forms.TextBox OffsetResistor;
        private System.Windows.Forms.TextBox InputResistor;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.Label IN;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.TextBox Scale;
        private System.Windows.Forms.TextBox Offset;
        private System.Windows.Forms.TextBox FeedbackResistor;
        private System.Windows.Forms.Label FeedbackResistorLabel;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.ComboBox InputCombo;
        private System.Windows.Forms.ComboBox FeedbackCombo;
        private System.Windows.Forms.ComboBox OffsetCombo;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.TextBox SelectedMinBox;
        private System.Windows.Forms.TextBox SelectedMaxBox;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.ComboBox FeedbackInputCombo;
    }
}

