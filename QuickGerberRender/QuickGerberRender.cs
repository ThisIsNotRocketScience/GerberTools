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

namespace WindowsFormsApplication1
{
    public partial class QuickGerberRender : Form
    {
        public QuickGerberRender()
        {
            InitializeComponent();

            SilkScreenColor.Items.Add("White");
            SilkScreenColor.Items.Add("Black");

            SolderMaskColor.Items.Add("Red");
            SolderMaskColor.Items.Add("Green");
            SolderMaskColor.Items.Add("Blue");
            SolderMaskColor.Items.Add("Yellow");
            SolderMaskColor.Items.Add("Black");
            SolderMaskColor.Items.Add("White");
            SolderMaskColor.Items.Add("Purple");

            TracesBox.Items.Add("Auto");
            TracesBox.Items.Add("Red");
            TracesBox.Items.Add("Green");
            TracesBox.Items.Add("Blue");
            TracesBox.Items.Add("Yellow");
            TracesBox.Items.Add("Black");
            TracesBox.Items.Add("White");
            TracesBox.Items.Add("Purple");

            
            CopperColor.Items.Add("Silver");
            CopperColor.Items.Add("Gold");

            DPIBox.Items.Add("10");
            DPIBox.Items.Add("25");
            DPIBox.Items.Add("100");
            DPIBox.Items.Add("200");
            DPIBox.Items.Add("300");
            DPIBox.Items.Add("400");
            DPIBox.Items.Add("800");
            ReDoColor();
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {

                string[] D = e.Data.GetData(DataFormats.FileDrop) as string[];
                List<String> files = new List<string>();
                foreach (string S in D)
                {
                    if (Directory.Exists(S))
                    {
                        LoadGerberFolder(Directory.GetFiles(S).ToList());
                    }

                    else
                    {
                        if (File.Exists(S)) files.Add(S);
                    }
                }
                if (files.Count > 0)
                {
                    LoadGerberFolder(files);
                }
            }
        }

        private void LoadGerberFolder(List<string> s)
        {
            Progress P = new Progress(s, SolderMaskColor.Text, SilkScreenColor.Text, CopperColor.Text, TracesBox.Text, DPIBox.Text, XRayOut.Checked, PCBOut.Checked);
            P.Show();
            P.StartThread();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(GerberLibrary.Gerber.ParseColor(SolderMaskColor.Text));
            var padcolor = GerberLibrary.Gerber.ParseColor(CopperColor.Text);
            var tracecolor = GerberLibrary.Gerber.ParseColor(TracesBox.Text);
            int H = pictureBox1.Height;
            int W = pictureBox1.Width;
            StringFormat SF = new StringFormat();
            SF.Alignment = StringAlignment.Center;
            SF.LineAlignment = StringAlignment.Center;
            e.Graphics.DrawString("Drop your gerber folder here!", new Font("Arial", 20), new SolidBrush(GerberLibrary.Gerber.ParseColor(SilkScreenColor.Text)), W / 2.0f, H / 2.0f, SF);
            int Y1 = H / 2 + 20;
            int Y2 = H / 2 - 20;
            e.Graphics.DrawLine(new Pen(tracecolor, 4), W / 4, Y1, (W * 3) / 4, Y1);
            e.Graphics.DrawLine(new Pen(tracecolor, 4), W / 4,Y2, (W * 3) / 4, Y2);

            for (int i =0;i< 10;i++)
            {
                int X = W / 2 + (i-5) * 30;
               
                Rectangle R = new Rectangle(X - 5, Y1 - 5, 10, 10);
                Rectangle R2 = new Rectangle(X - 2, Y1 - 2, 4, 4);
                e.Graphics.FillEllipse(new SolidBrush(padcolor), R);
                e.Graphics.FillEllipse(Brushes.Black, R2);

                Rectangle R3 = new Rectangle(X - 5, Y2 - 5, 10, 10);
                Rectangle R4 = new Rectangle(X - 2, Y2 - 2, 4, 4);
                e.Graphics.FillEllipse(new SolidBrush(padcolor), R3);
                e.Graphics.FillEllipse(Brushes.Black, R4);
            }
        }

        private void SolderMaskColor_TextChanged(object sender, EventArgs e)
        {
            ReDoColor();
        }

        private void SolderMaskColor_SelectedIndexChanged(object sender, EventArgs e)
        {
            ReDoColor();
        }

        public void ReDoColor()
        {
            pictureBox1.Invalidate();
        }

        private void SilkScreenColor_SelectedIndexChanged(object sender, EventArgs e)
        {
            ReDoColor();
        }

        private void CopperColor_SelectedIndexChanged(object sender, EventArgs e)
        {
            ReDoColor();
        }

        private void SilkScreenColor_TextUpdate(object sender, EventArgs e)
        {
            ReDoColor();
        }

        private void CopperColor_TextUpdate(object sender, EventArgs e)
        {
            ReDoColor();
        }

        private void SolderMaskColor_TextUpdate(object sender, EventArgs e)
        {
            ReDoColor();
        }

        private void pictureBox1_Resize(object sender, EventArgs e)
        {
            ReDoColor();
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void TracesBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ReDoColor();
        }

        private void TracesBox_TextUpdate(object sender, EventArgs e)
        {
            ReDoColor();
        }
    }
}
