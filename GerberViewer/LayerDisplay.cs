using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GerberLibrary.Core;
using GerberLibrary;
using System.Drawing.Drawing2D;

namespace GerberViewer
{
    public partial class LayerDisplay : WeifenLuo.WinFormsUI.Docking.DockContent
    {
        public LoadedStuff Document;
        public BoardSide DisplaySide;
        public LoadedStuff.DisplayGerber DispGerb;

        public LayerDisplay(LoadedStuff doc, BoardSide Side)
        {
            DisplaySide = Side;
            Document = doc;
            InitializeComponent();
            CloseButton = false;
            CloseButtonVisible = false;
        }

        public LayerDisplay(LoadedStuff doc, LoadedStuff.DisplayGerber Gerb)
        {
            CloseButton = false;

            CloseButtonVisible = false;

            DispGerb = Gerb;
            DisplaySide = Gerb.File.Side;
            Document = doc;
            InitializeComponent();
        }
        public void UpdateDocument()
        {
            pictureBox1.Invalidate();
        }
        private void LayerDisplay_Paint(object sender, PaintEventArgs e)
        {

        }



        private void DrawGerber(Graphics G, ParsedGerber file, float S, Color C, bool dotted= false)
        {


            Pen P = new Pen(C, 1.0f / S);
            if (dotted) P.DashPattern = new float[2] { 2, 2 };
            SolidBrush B = new SolidBrush(C);
            GraphicsPath GP = new GraphicsPath();
            var Out = file.IsOutline();

            foreach (var a in file.DisplayShapes)
            {

                if (a.Vertices.Count > 1)
                {

                    PointF[] Points = new PointF[a.Vertices.Count];
                    for (int i = 0; i < a.Vertices.Count; i++)
                    {
                        Points[i] = a.Vertices[i].ToF();
                    }
                    GP.AddPolygon(Points);
                    if (Out == false)
                    {
                        G.FillPath(B, GP);
                        GP = new GraphicsPath();
                    }
                }
            }
            if (Out)
            {
                G.DrawPath(P, GP);
            }

        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            var G = e.Graphics;
            PolyLineSet.Bounds Bounds = new PolyLineSet.Bounds();
            G.Clear(Document.Colors.BackgroundColor);
            GerberImageCreator.ApplyAASettings(G);

            if (Document.Gerbers.Count == 0) return;
            foreach (var a in Document.Gerbers.OrderBy(x => x.sortindex))
            {
                Bounds.AddBox(a.File.BoundingBox);
            }
            float S = 1;
            if (DisplaySide == BoardSide.Bottom)
            {
                S = Bounds.GenerateTransform(G, Width, Height, 4, false);
            }
            else
            {
                S = Bounds.GenerateTransform(G, Width, Height, 4, true);

            }
            if (DispGerb == null)
            {
                if (DisplaySide == BoardSide.Bottom)
                {
                    foreach (var a in Document.Gerbers.OrderByDescending(x => x.sortindex))
                    {
                        if (a.File.Layer != BoardLayer.Drill)
                        {
                            DrawGerber(G, a.File, S, a.Color);
                        }
                    }
                }
                else
                {
                    foreach (var a in Document.Gerbers.OrderBy(x => x.sortindex))
                    {
                        if (a.File.Layer != BoardLayer.Drill)
                        {

                            DrawGerber(G, a.File, S, a.Color);
                        }
                    }
                }

                foreach (var a in Document.Gerbers.OrderBy(x => x.sortindex))
                {
                    if (a.File.Layer == BoardLayer.Drill)
                    {
                        DrawGerber(G, a.File, S, a.Color);
                    }
                }
            }
            else
            {
                foreach (var a in Document.Gerbers.OrderBy(x => x.sortindex))
                {
                    if (a.File.Layer == BoardLayer.Outline || a.File.Layer == BoardLayer.Mill)
                    {
                        DrawGerber(G, a.File, S, Color.FromArgb(20, 255, 255, 255), true);
                    }
                }
                DrawGerber(G, DispGerb.File, S, Color.White);
            }
        }

        private void pictureBox1_Resize(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }
    }
}
