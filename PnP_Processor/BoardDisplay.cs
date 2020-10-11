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
    public partial class BoardDisplay : WeifenLuo.WinFormsUI.Docking.DockContent
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


            e.Graphics.Clear(Color.Black);

            //            Bitmap B = new Bitmap(10, 10);
            Graphics G = e.Graphics;
            G.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            G.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            G.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            G.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            if (pnp.ActiveDoc == null) return;
            var D = pnp.ActiveDoc;
            Font F = new Font("Arial", 10);
            Font F2 = new Font("Arial Bold", 16);




            if (D.loaded == false)
            {
                int H = Height - F.Height;
                for (int i = 0; i < D.Log.Count; i++)
                {
                    G.DrawString(D.Log[i], F, Brushes.LightGray, 10, (i - D.Log.Count) * F.Height + H);
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

        private void Render(PnPProcDoc D, Graphics G, bool after)
        {

            G.TranslateTransform(G.ClipBounds.Width / 2, G.ClipBounds.Height / 2);

            TheBox.Reset();
            //   TheBox.FitPoint(0, 0);
            //TheBox.AddBox(D.Box);

            TheBox.FitPoint(-250, -250);
            TheBox.FitPoint(250, 250);
            if (idx > -1)
            {
                var rd = pnp.selectedrefdes[idx % pnp.selectedrefdes.Count()];
                BOMEntry.RefDesc refd = D.B.GetRefDes(rd);
                if (after)
                {
                    refd = D.BPost.GetRefDes(rd);

                }
                if (refd != null)
                {
                    TheBox.Reset();
                    TheBox.FitPoint(refd.x - 20, refd.y - 20);
                    TheBox.FitPoint(refd.x + 20, refd.y + 20);
                }

            }

            float S = (float)Math.Min(pictureBox1.Width / (TheBox.Width()), pictureBox1.Height / (TheBox.Height()));


            var C = TheBox.Center();
            G.ScaleTransform(S * 0.8f, -S * 0.8f);
            if (idx > -1)
            {
                G.TranslateTransform((float)-C.X, (float)-C.Y);
            }

            MarkPoint(G, Color.Blue, "zero", 0, 0, S);
            MarkPoint(G, Color.Green, "zero", (float)D.FixOffset.X, (float)D.FixOffset.Y, S);


            RenderParts(D, after, G, BoardSide.Bottom, S);
            RenderLayerSets(after, G, S, BoardSide.Both, BoardLayer.Outline, Color.FromArgb(210, 4, 20, 4), false);
            RenderLayerSets(after, G, S, BoardSide.Both, BoardLayer.Outline, Color.Gray, true);

            if (pnp.bottomsilkvisible)
            {
                RenderLayerSets(after, G, S, BoardSide.Bottom, BoardLayer.Silk, Color.FromArgb(60, 60, 60), false); ;
                RenderLayerSets(after, G, S, BoardSide.Bottom, BoardLayer.SolderMask, Color.FromArgb(100, 100, 10), false);
            }
            if (pnp.topsilkvisible)
            {
                RenderLayerSets(after, G, S, BoardSide.Top, BoardLayer.Silk, Color.FromArgb(60, 60, 60), false);
                RenderLayerSets(after, G, S, BoardSide.Top, BoardLayer.SolderMask, Color.FromArgb(100, 100, 10), false);
            }



            RenderParts(D, after, G, BoardSide.Top, S);

            var B = D.B;
            if (after) B = D.BPost;
            int curpart = 0;

            foreach (var p in B.DeviceTree)
            {
                foreach (var pp in p.Value.Values)
                {
                    var curcol = Helpers.RefractionNormalledMaxBrightnessAndSat(curpart / p.Value.Values.Count());
                    curpart++;
                    foreach (var rf in pp.RefDes)
                    {
                        DrawMarker(curcol, G, rf, true, S, false, pnp.selectedrefdes.Contains(rf.NameOnBoard));
                    }
                }
            }
        }

        private void RenderParts(PnPProcDoc D, bool after, Graphics G, BoardSide side, float S)
        {

            var B = D.B;
            if (after) B = D.BPost;
            int curpart = 0;

            foreach (var p in B.DeviceTree)
            {
                foreach (var pp in p.Value.Values)
                {
                    var curcol = Helpers.RefractionNormalledMaxBrightnessAndSat(curpart / p.Value.Values.Count());
                    curpart++;
                    foreach (var rf in pp.RefDes)
                    {
                        if (rf.Side == side)
                        {
                            BOM.RenderPackage(G, rf.x, rf.y, rf.angle, pp.PackageName, rf.Side);
                        }
                    }
                }
            }
        }

        private void MarkPoint(Graphics g, Color blue, string lbl, float x, float y, float S)
        {
            g.DrawRectangle(new Pen(blue, 1.0f / S), x - 2.0f / S, y - 2 / S, 4 / S, 4 / S);
        }

        internal void RefreshPic()
        {
            pictureBox1.Invalidate();
        }

        private void RenderLayerSets(bool after, Graphics G, float S, BoardSide side, BoardLayer layer, Color C, bool lines = true)
        {

            if (pnp.ActiveDoc == null) return;
            var D = pnp.ActiveDoc;
            List<ParsedGerber> s;
            if (after)
            {
                s = D.FixSet.PLSs;

            }
            else
            {
                s = D.Set.PLSs;
            }
            foreach (var l in s)
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
                        G.DrawLines(new Pen(C, (float)ds.Width * 0.2f / S), Pts.ToArray());
                    }
                    else
                    {
                        G.FillPolygon(new SolidBrush(C), Pts.ToArray());
                    }
                }
            }
        }

        private void DrawMarker(Color PartCol, Graphics g, BOMEntry.RefDesc r, bool soldered, float S, bool current, bool activedes)
        {
            float R = 2;
            float cx = (float)r.x - R / S;
            float cy = (float)r.y - R / S;

            float sa = (float)Math.Sin((-r.angle * Math.PI * 2) / 360.0);
            float ca = (float)Math.Cos((-r.angle * Math.PI * 2) / 360.0);
            g.DrawArc(new Pen(PartCol, 1.0f / S), new RectangleF((float)r.x - 0.5f, (float)r.y - 0.5f, 1, 1), 270, (float)-r.angle);
            g.DrawLine(new Pen(PartCol, 1.0f / S), (float)r.x, (float)r.y, (float)r.x + sa * 1.0f, (float)r.y - ca * 1.0f);

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

                g.DrawArc(new Pen(Color.HotPink, 1.0f / S), new RectangleF((float)r.x - 2.5f, (float)r.y - 2.5f, 5, 5), 270, (float)-r.angle);
                g.DrawLine(new Pen(Color.HotPink, 1.0f / S), (float)r.x, (float)r.y, (float)r.x + sa * 3.0f, (float)r.y - ca * 3.0f);

                g.FillRectangle(new SolidBrush(Color.HotPink), cx2, cy2, R2 / S * 2, R2 / S * 2);

            }
            g.FillRectangle(new SolidBrush(PartCol), cx, cy, R / S * 2, R / S * 2);


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
        int idx = -1;
        private void BoardDisplay_KeyPress(object sender, KeyPressEventArgs e)
        {

            switch (e.KeyChar)
            {
                case 'f':
                    {
                        if (pnp.selectedrefdes.Count > 0) idx = (idx + 1) % pnp.selectedrefdes.Count; else idx = -1;
                    }
                    break;
                case (char)27:
                    idx = -1;
                    break;

            }
            pictureBox1.Invalidate();

        }
    }
}
