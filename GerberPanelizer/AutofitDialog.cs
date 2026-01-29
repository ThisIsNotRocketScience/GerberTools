using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GerberCombinerBuilder
{
    public partial class AutofitDialog : Form
    {
        private GerberPanelize Target;
        private System.Windows.Forms.NumericUpDown MarginBox;
        private System.Windows.Forms.NumericUpDown MoatBox;
        private System.Windows.Forms.Label MarginLabel;
        private System.Windows.Forms.Label MoatLabel;
        private System.Windows.Forms.Button FitButton;

        public AutofitDialog(GerberPanelize target)
        {
            Target = target;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.MarginBox = new System.Windows.Forms.NumericUpDown();
            this.MoatBox = new System.Windows.Forms.NumericUpDown();
            this.MarginLabel = new System.Windows.Forms.Label();
            this.MoatLabel = new System.Windows.Forms.Label();
            this.FitButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.MarginBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MoatBox)).BeginInit();
            this.SuspendLayout();
            // 
            // MarginBox
            // 
            this.MarginBox.DecimalPlaces = 2;
            this.MarginBox.Location = new System.Drawing.Point(120, 20);
            this.MarginBox.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.MarginBox.Name = "MarginBox";
            this.MarginBox.Size = new System.Drawing.Size(120, 20);
            this.MarginBox.TabIndex = 0;
            this.MarginBox.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // MoatBox
            // 
            this.MoatBox.DecimalPlaces = 2;
            this.MoatBox.Location = new System.Drawing.Point(120, 50);
            this.MoatBox.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.MoatBox.Name = "MoatBox";
            this.MoatBox.Size = new System.Drawing.Size(120, 20);
            this.MoatBox.TabIndex = 1;
            this.MoatBox.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // MarginLabel
            // 
            this.MarginLabel.AutoSize = true;
            this.MarginLabel.Location = new System.Drawing.Point(20, 22);
            this.MarginLabel.Name = "MarginLabel";
            this.MarginLabel.Size = new System.Drawing.Size(66, 13);
            this.MarginLabel.TabIndex = 2;
            this.MarginLabel.Text = "Margin (mm)";
            // 
            // MoatLabel
            // 
            this.MoatLabel.AutoSize = true;
            this.MoatLabel.Location = new System.Drawing.Point(20, 52);
            this.MoatLabel.Name = "MoatLabel";
            this.MoatLabel.Size = new System.Drawing.Size(58, 13);
            this.MoatLabel.TabIndex = 3;
            this.MoatLabel.Text = "Moat (mm)";
            // 
            // FitButton
            // 
            this.FitButton.Location = new System.Drawing.Point(23, 90);
            this.FitButton.Name = "FitButton";
            this.FitButton.Size = new System.Drawing.Size(217, 30);
            this.FitButton.TabIndex = 4;
            this.FitButton.Text = "Fit Canvas";
            this.FitButton.UseVisualStyleBackColor = true;
            this.FitButton.Click += new System.EventHandler(this.FitButton_Click);
            // 
            // AutofitDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(270, 140);
            this.Controls.Add(this.FitButton);
            this.Controls.Add(this.MoatLabel);
            this.Controls.Add(this.MarginLabel);
            this.Controls.Add(this.MoatBox);
            this.Controls.Add(this.MarginBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "AutofitDialog";
            this.Text = "Canvas Autofit Helper";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.MarginBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MoatBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void FitButton_Click(object sender, EventArgs e)
        {
            Target.PerformAutofit((double)MarginBox.Value, (double)MoatBox.Value);
        }
    }
}
