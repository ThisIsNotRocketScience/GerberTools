using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using GerberLibrary;
using GerberLibrary.Core;

namespace SolderTool
{


    public partial class PartList : WeifenLuo.WinFormsUI.Docking.DockContent
    {
        private SolderToolMain Main;

        public PartList(SolderToolMain Parent)
        {
            Main = Parent;
            InitializeComponent();
            this.KeyPreview = true;


        }

        public void UpdateCurrentPart()
        {
            BOM B = Main.GetBom();

            if (B == null)
            {
                return;
            }

            int pc = B.GetPartCount(new List<string>() { });
            CurrentPart = (CurrentPart + pc) % pc;
            if (CurrentPart < 0) CurrentPart = 0;
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            var G = e.Graphics;
            G.Clear(Color.Black);

            G.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            G.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            G.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            G.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            BOM B = Main.GetBom();

            Font F = new Font("Panton", 10);
            Font SF = new Font("Panton", 6);
            if (B == null)
            {
                G.DrawString("Not loaded..", F, Brushes.White, 2, 2);
                return;
            }

            int i = 0;
            int pc = B.GetPartCount(new List<string>() { });
            CurrentPart = (CurrentPart + pc) % pc;
            int y = 2;
            if (CurrentPart * 14 + y > pictureBox1.Height - 50)

            {
                while (CurrentPart * 14 + y > pictureBox1.Height - 50)
                {
                    y -= 14;
                }
            }
            
            foreach (var a in B.DeviceTree)
            {
                foreach (var v in a.Value.Values)
                {
                    string count = v.RefDes.Count().ToString();
                    Brush Br = Brushes.Red;

                    Font TheF = F;
                    bool dorender = true;
                    if (v.Soldered)
                    {
                        Br = Brushes.Green;

                        TheF = SF;
                    }
                    else
                    {
                    }
                    if (dorender)
                    {
                        if (i == CurrentPart)
                        {
                            G.FillRectangle(new SolidBrush(Color.FromArgb(10, 255, 255, 0)), 0, y, pictureBox1.Width, 14);
                        }
                        G.DrawString(count, TheF, Br, 2, y);
                        G.DrawString(v.DisplayLine(), TheF, Br, 22, y);
                        y += TheF.Height;
                    }
                    i++;
                }
            }

        }

        internal string GetCurrentPartName()
        {
            int i = 0;
            var B = Main.GetBom();
            int pc = B.GetPartCount(new List<string>() { });
            CurrentPart = (CurrentPart + pc) % pc;
            foreach (var a in B.DeviceTree)
            {
                foreach (var v in a.Value.Values)
                {
                    if (i == CurrentPart)
                    {
                        return v.DisplayLine();
                    }
                    i++;
                }
            }
            return "out of range?";
        }

        public int CurrentPart = 0;

        private void pictureBox1_Resize(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }

        internal void InvalidatePicture()
        {
            pictureBox1.Invalidate();
        }

        public void Up()
        {
            CurrentPart--;
            InvalidatePicture();
            Main.RepaintCurrent();
        }

        public void Down()
        {
            CurrentPart++;
            InvalidatePicture();
            Main.RepaintCurrent();
        }

        public void Enter()
        {
            ToggleCurrentPart();
            InvalidatePicture();
            Main.RepaintCurrent();
        }
        private void PartList_KeyDown(object sender, KeyEventArgs e)
        {

            switch (e.KeyCode)
            {
                case Keys.Q: Up(); break;
                case Keys.E: Down(); break;
                case Keys.R: Enter(); break;
            }
        }

        private void ToggleCurrentPart()
        {
            BOM B = Main.GetBom();
            if (B == null)
            {
                return;
            }
            int i = 0;

            int pc = B.GetPartCount(new List<string>() { });
            CurrentPart = (CurrentPart + pc) % pc;

            foreach (var a in B.DeviceTree)
            {
                foreach (var v in a.Value.Values)
                {
                    if (i == CurrentPart)
                    {
                        v.Soldered = !v.Soldered;
                        if (v.Soldered)
                        {
                            Main.SolderPart(v.Combined());
                        }
                        else
                        {
                            Main.UnSolderPart(v.Combined());
                        }
                        return;
                    }
                    i++;

                }
            }
        }

        private void PartList_KeyPress(object sender, KeyPressEventArgs e)
        {

        }
    }
}
