using GerberLibrary;
using GerberLibrary.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PnP_Processor
{
    public partial class BoardDisplay :  WeifenLuo.WinFormsUI.Docking.DockContent
    {
        public PnPMain pnp;
        public bool PostDisplay = false;
        Bounds TheBox = new Bounds();
        
        public BoardDisplay(PnPMain parent, bool postdisplay)
        {
            InitializeComponent();
            pnp = parent;
            PostDisplay = postdisplay;
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            TheBox.Reset();
            TheBox.FitPoint(0, 0);


            e.Graphics.Clear(Color.Black);

            //            Bitmap B = new Bitmap(10, 10);
            Graphics G = e.Graphics;
            G.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            G.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            G.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            G.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            if (pnp.ActiveDoc == null) return;
            var D = pnp.ActiveDoc;
            TheBox.AddBox(D.Box);
            Font F = new Font("Arial", 10);
            Font F2 = new Font("Arial Bold", 16);


            

            if (D.loaded == false)
            {
                int H = Height - F.Height;
                for (int i = 0;i<D.Log.Count;i++)
                {
                    G.DrawString(D.Log[i], F, Brushes.LightGray, 10, (i-D.Log.Count) * F.Height + H);
                }
            }
            else
            {

                G.DrawLine(Pens.White, pictureBox1.Width / 2, 0, pictureBox1.Width / 2, pictureBox1.Height);
                G.DrawString("Before", F2, Brushes.White, new RectangleF(0, pictureBox1.Height - 40, pictureBox1.Width / 2, 40), new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far });
                G.DrawString("After", F2, Brushes.White, new RectangleF(pictureBox1.Width / 2, pictureBox1.Height - 40, pictureBox1.Width / 2, 40), new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far });
                var T = G.Transform.Clone();

                G.Transform.Reset();
                G.SetClip(new Rectangle(0, 0, pictureBox1.Width / 2, pictureBox1.Height));
                Render(D, G, false);
                G.Transform = T;

                G.SetClip(new Rectangle(pictureBox1.Width / 2, 0, pictureBox1.Width / 2, pictureBox1.Height));
                G.TranslateTransform(pictureBox1.Width / 2, 0);
                Render(D, G, true);

              
            }
        }

        private void Render(PnPProcDoc D,  Graphics G, bool v)
        {
            

          //  G.TranslateTransform(10, 10);

            float S = (float)Math.Min(pictureBox1.Width / (TheBox.Width() - 20), pictureBox1.Height / (TheBox.Height() - 20));


            bool TopView = false;
            if (PostDisplay) TopView = D.FlipBoard ? false : true;

            if (TopView)
            {
                G.ScaleTransform(S * 0.8f, -S * 0.8f);
                G.TranslateTransform((float)-TheBox.TopLeft.X, (float)-TheBox.TopLeft.Y - (float)TheBox.Height());
            }
            else
            {
                G.ScaleTransform(-S * 0.8f, -S * 0.8f);
                G.TranslateTransform((float)(-TheBox.TopLeft.X - TheBox.Width()), (float)-TheBox.TopLeft.Y - (float)TheBox.Height());

            }
            RenderLayerSets(G, S, BoardSide.Both, BoardLayer.Outline, Color.Gray, true);

            //      RenderLayerSets(G, S, BoardSide.Bottom, BoardLayer.Silk, Color.DarkGray, true);
            //  RenderLayerSets(G, S, BoardSide.Top, BoardLayer.Silk, Color.White, true);

            var B = D.B;
            foreach (var p in B.DeviceTree)
            {
                foreach (var pp in p.Value.Values)
                {
                    foreach (var rf in pp.RefDes)
                    {
                        DrawMarker(G, rf, true, S, false, pnp.selectedrefdes.Contains(rf.NameOnBoard));
                    }
                }
            }
        }

        internal void RefreshPic()
        {
            pictureBox1.Invalidate();
        }

        private void RenderLayerSets(Graphics G, float S, BoardSide side, BoardLayer layer, Color C, bool lines = true)
        {

            if (pnp.ActiveDoc == null) return;
            var D = pnp.ActiveDoc;
            foreach (var l in D.Set.PLSs)
            {
                if (l.Side == side && l.Layer == layer)
                {
                     RenderOutline(G, S, l, C, lines);
                }
            }
        }

        private static void RenderOutline(Graphics G, float S, ParsedGerber d, Color C, bool lines = true)
        {
            foreach (var ds in d.DisplayShapes)
            {
                List<PointF> Pts = new List<PointF>();
                foreach (var V in ds.Vertices)
                {
                    Pts.Add(new PointF((float)((V.X)), (float)((V.Y))));
                }

                if (Pts.Count > 2)
                {
                    if (lines)
                    {
                        G.DrawLines(new Pen(C, (float)ds.Width*0.2f / S), Pts.ToArray());
                    }
                    else
                    {
                        G.FillPolygon(new SolidBrush(C), Pts.ToArray());
                    }
                }
            }
        }

        private void DrawMarker(Graphics g, BOMEntry.RefDesc r, bool soldered, float S, bool current, bool activedes)
        {
            float R = 2;
            float cx = (float)r.x - R / S;
            float cy = (float)r.y - R / S;

            float sa = (float)Math.Sin((r.angle * Math.PI * 2) / 360.0);
            float ca = (float)Math.Cos((r.angle * Math.PI * 2) / 360.0);

            g.DrawArc(new Pen(Color.Yellow, 1.0f / S), new RectangleF((float)r.x - 9 / S, (float)r.y - 9 / S, 18 / S, 18 / S), 270, (float)r.angle);
            g.DrawLine(new Pen(Color.LightYellow, 1.0f / S), (float)r.x, (float)r.y, (float)r.x + sa * 10.0f/S, (float)r.y - ca * 10.0f / S );

            Color CurrentColor = soldered ? Color.Green : Color.Yellow;
            if (current)
            {
                float R2 = 5;
                float cx2 = (float)r.x - R2 / S;
                float cy2 = (float)r.y - R2 / S;
                g.FillRectangle(new SolidBrush(CurrentColor), cx2, cy2, R2 / S * 2, R2 / S * 2);
            }
            if (activedes)
            {
                float R2 = 8;
                float cx2 = (float)r.x - R2 / S;
                float cy2 = (float)r.y - R2 / S;
                g.FillRectangle(new SolidBrush(Color.HotPink), cx2, cy2, R2 / S * 2, R2 / S * 2);

            }
            g.FillRectangle(soldered ? Brushes.Green : Brushes.Red, cx, cy, R / S * 2, R / S * 2);


        }
        int lastlogstamp = -1;
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (pnp.ActiveDoc == null) return;
            if (pnp.ActiveDoc.Stamp != lastlogstamp)
            {
                lastlogstamp = pnp.ActiveDoc.Stamp;
                pictureBox1.Invalidate();
            }
        }

        private void pictureBox1_Resize(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }
    }
}
