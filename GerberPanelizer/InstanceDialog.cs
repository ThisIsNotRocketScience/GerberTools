using GerberLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GerberCombinerBuilder
{
    public partial class InstanceDialog :Form
    {
        GerberPanelize TargetInstance;
        public InstanceDialog()
        {
            InitializeComponent();
            panel1.Enabled = false;
        }
        private bool Initializing = true;
        public void UpdateBoxes(GerberPanelize newTarget)
        {
            TargetInstance = newTarget;

            if (TargetInstance == null || TargetInstance.SelectedInstance == null) { panel1.Enabled = false; return; } else { panel1.Enabled = true; }
            newTarget.SuspendRedraw = true;
            double x = TargetInstance.SelectedInstance.Center.X;
            double y = TargetInstance.SelectedInstance.Center.Y;
            double r = TargetInstance.SelectedInstance.Angle;
            double rad = 0;

            radiusbox.Enabled = false;
            Bigger.Enabled = false;
            Smaller.Enabled = false;

            if (TargetInstance.SelectedInstance.GetType() == typeof(BreakTab))
            {
                BreakTab BT = TargetInstance.SelectedInstance as BreakTab;
                NameLabel.Text = "Break tab";
                Bigger.Enabled = true;
                Smaller.Enabled = true;
                rad = BT.Radius;
                radiusbox.Enabled = true;
            }
           
           if (TargetInstance.SelectedInstance.GetType() == typeof(GerberInstance))
            {
                  GerberInstance GI = TargetInstance.SelectedInstance as GerberInstance;
                  NameLabel.Text = Path.GetFileName(Path.GetDirectoryName( GI.GerberPath));
            }
            
            xbox.Value = (decimal)x;
            ybox.Value = (decimal)y;
            rbox.Value = (decimal)r;
            radiusbox.Value = (decimal) rad;
            Initializing = false;
            newTarget.SuspendRedraw = false;

        }

        void UpdateInstance()
        {
            if (Initializing) return;

            if (TargetInstance == null) return;
            
            if (TargetInstance.SelectedInstance == null) return;

            TargetInstance.SelectedInstance.Center.X = (float)xbox.Value;
            TargetInstance.SelectedInstance.Center.Y = (float)ybox.Value;
            TargetInstance.SelectedInstance.Angle = (float)rbox.Value;

            if (TargetInstance.SelectedInstance.GetType() == typeof(BreakTab))
            {
                BreakTab BT = TargetInstance.SelectedInstance as BreakTab;
                BT.Radius = (float)radiusbox.Value;
            }
            TargetInstance.Redraw(true);
        }

        private void Up_Click(object sender, EventArgs e)
        {
            ybox.Value += 1;
            UpdateInstance();
        }

        private void Left_Click(object sender, EventArgs e)
        {
            xbox.Value -= 1;
            UpdateInstance();
        }

        private void Right_Click(object sender, EventArgs e)
        {
            xbox.Value += 1;
            UpdateInstance();
        }

        private void Down_Click(object sender, EventArgs e)
        {
            ybox.Value -= 1;
            UpdateInstance();
        }

        private void AClock_Click(object sender, EventArgs e)
        {
            decimal newval = rbox.Value - 90;
            if (newval < -180) newval += 360;
            rbox.Value = newval;
            UpdateInstance();
        }

        private void Clock_Click(object sender, EventArgs e)
        {
            decimal newval = rbox.Value + 90;
            if (newval > 180) newval -= 360;
            rbox.Value = newval;
            UpdateInstance();
        }

        private void xbox_ValueChanged(object sender, EventArgs e)
        {
            UpdateInstance();
        }

        private void ybox_ValueChanged(object sender, EventArgs e)
        {
            UpdateInstance();
       
        }

        private void rbox_ValueChanged(object sender, EventArgs e)
        {
            UpdateInstance();
       
        }

        private void Bigger_Click(object sender, EventArgs e)
        {
            radiusbox.Value += 1;
            UpdateInstance();
      
        }

        private void Smaller_Click(object sender, EventArgs e)
        {
            radiusbox.Value -= 1;
            
            UpdateInstance();
      
        }

        private void radiusbox_ValueChanged(object sender, EventArgs e)
        {
            UpdateInstance();
     
        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void tableLayoutPanel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Up_Click_1(object sender, EventArgs e)
        {
            Up_Click(sender, e);
        }

        private void Down_Click_1(object sender, EventArgs e)
        {
            Down_Click(sender, e);
        }

        private void Clock_Click_1(object sender, EventArgs e)
        {
            Clock_Click(sender, e);
        }

        private void AClock_Click_1(object sender, EventArgs e)
        {
            AClock_Click(sender, e);
        }

        private void Left_Click_1(object sender, EventArgs e)
        {
            Left_Click(sender, e);
        }

        private void Right_Click_1(object sender, EventArgs e)
        {
            Right_Click(sender, e);
        }

        private void Bigger_Click_1(object sender, EventArgs e)
        {
            Bigger_Click(sender, e);
        }

        private void Smaller_Click_1(object sender, EventArgs e)
        {
            Smaller_Click(sender, e);
        }
    }
}
