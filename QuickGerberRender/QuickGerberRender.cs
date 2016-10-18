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
            CopperColor.Items.Add("Silver");
            CopperColor.Items.Add("Gold");
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
            GerberLibrary.GerberImageCreator GIC = new GerberLibrary.GerberImageCreator();
            GIC.AddBoardsToSet(s);

            GerberLibrary.Gerber.BoardRenderColor = GerberLibrary.Gerber.ParseColor(SolderMaskColor.Text);
            GerberLibrary.Gerber.BoardRenderSilkColor = GerberLibrary.Gerber.ParseColor(SilkScreenColor.Text);
            GerberLibrary.Gerber.BoardRenderPadColor = GerberLibrary.Gerber.ParseColor(CopperColor.Text);

            GIC.WriteImageFiles(Path.GetDirectoryName(s[0]) + ".png");
            GIC.DrawAllFiles(Path.GetDirectoryName(s[0]) + "_Layer", 200);
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(GerberLibrary.Gerber.ParseColor(SolderMaskColor.Text));
            var padcolor = GerberLibrary.Gerber.ParseColor(CopperColor.Text);
            int H = pictureBox1.Height;
            int W = pictureBox1.Width;
            StringFormat SF = new StringFormat();
            SF.Alignment = StringAlignment.Center;
            SF.LineAlignment = StringAlignment.Center;
            e.Graphics.DrawString("Drop your gerber folder here!", new Font("Arial", 20), new SolidBrush(GerberLibrary.Gerber.ParseColor(SilkScreenColor.Text)), W / 2.0f, H / 2.0f, SF);

            for (int i =0;i< 10;i++)
            {
                int X = W / 2 + (i-5) * 30;
                int Y1 = H / 2 + 20;
                int Y2 = H / 2 - 20;
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
    }
}
