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

        public class ListItem
        {
            public string DispName;
            
            public int useagecount;
            public bool soldered;
            public List<BOMEntry.RefDesc> RefDes;
        }

        public List<ListItem> GetPartList()
        {
            List<ListItem> L = new List<ListItem>();
            BOM B = Main.GetBom();
            foreach (var a in B.DeviceTree)
            {
                foreach (var v in a.Value.Values)
                {

                    L.Add(new ListItem() { soldered = v.Soldered, DispName = v.Combined(), useagecount = v.RefDes.Count(), RefDes = v.RefDes });
                }
            }
            return L.OrderBy(x =>x.soldered).ThenByDescending(x => x.useagecount).ToList();
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
            if (B == null)
            {
                G.DrawString("Not loaded..", F, Brushes.White, 2, 2);
                return;
            }

            int i = 0;
            int pc = B.GetUniquePartCount(new List<string>() { });
            CurrentPart = (CurrentPart + pc) % pc;
            var PL = GetPartList();
            foreach (var v in PL)
            {
                string count = v.useagecount.ToString();
                Brush Br = Brushes.Red;
                if (v.soldered) Br = Brushes.Green;
                int y = 2 + i * 14;
                if (i == CurrentPart)
                {
                    G.FillRectangle(new SolidBrush(Color.FromArgb(100, 255, 255, 0)), 0, y, pictureBox1.Width, 14);
                }
                G.DrawString(count, F, Br, 2, y);
                G.DrawString(v.DispName, F, Br, 22, y);
                i++;

            }

        }

        internal string GetCurrentPartName()
        {
            int i = 0;
            var B = Main.GetBom();
            int pc = B.GetUniquePartCount(new List<string>() { });
            CurrentPart = (CurrentPart + pc) % pc;
            var PL = GetPartList();
            foreach (var v in PL)
            {

                if (i == CurrentPart)
                {
                    return v.DispName;
                }
                i++;

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

            int pc = B.GetUniquePartCount(new List<string>() { });
            CurrentPart = (CurrentPart + pc) % pc;

            foreach (var v in GetPartList())
            {
                if (i == CurrentPart)
                {
                    v.soldered = !v.soldered;

                    foreach (var a in B.DeviceTree)
                    {
                        foreach(var vv in a.Value)
                        {
                            if (vv.Value.Combined() == v.DispName)
                            {
                                vv.Value.Soldered = v.soldered;
                            }
                        }
                    }
                    if (v.soldered)
                    {
                        Main.SolderPart(v.DispName);
                    }
                    else
                    {
                        Main.UnSolderPart(v.DispName);
                    }
                    return;
                }
                i++;


            }
        }

        private void PartList_KeyPress(object sender, KeyPressEventArgs e)
        {

        }
    }
}
