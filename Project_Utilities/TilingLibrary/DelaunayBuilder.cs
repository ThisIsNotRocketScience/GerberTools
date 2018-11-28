using GerberLibrary;
using GerberLibrary.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Artwork;
using TriangleNet;

namespace Artwork
{
    public class DelaunayBuilder
    {
        List<TriangleNet.Geometry.Vertex> IG = new List<TriangleNet.Geometry.Vertex>();
        public void Build(QuadTreeNode Mask, double compensateangle  = 0.0)
        {
          //  M = new Mesh();
            
            Lines.Clear();
            Mask.NodeWalker(GetAllCorners, true, true);
            var GM = new TriangleNet.Meshing.GenericMesher();
            if (IG.Count < 3)
            {
                M = null;
                return;
                
            }
            else
            {
                M = GM.Triangulate(IG);
            }
            //M.Triangulate(IG);
            //M.Smooth();
            //M.Refine();
            //M.Refine();

            double S = Math.Sin(compensateangle);
            double C = Math.Cos(compensateangle);

            foreach (var a in M.Triangles)
            {
                var V1 = M.Vertices.ElementAt(a.GetVertexID(0));
                var V2 = M.Vertices.ElementAt(a.GetVertexID(1));
                var V3 = M.Vertices.ElementAt(a.GetVertexID(2));

                QuadTreeNode N = Mask.GetNode((V1.X + V2.X + V3.X) / 3.0, (V1.Y + V2.Y + V3.Y) / 3.0);

                if (N != null && N.Items.Count == 0)
                {

                    var D1 = MathHelpers.Difference(new PointF((float)V1.X, (float)V1.Y), new PointF((float)V2.X, (float)V2.Y));
                    var D2 = MathHelpers.Difference(new PointF((float)V2.X, (float)V2.Y), new PointF((float)V3.X, (float)V3.Y));
                    var D3 = MathHelpers.Difference(new PointF((float)V3.X, (float)V3.Y), new PointF((float)V1.X, (float)V1.Y));
                    var D1L = MathHelpers.Length(D1);
                    var D2L = MathHelpers.Length(D2);
                    var D3L = MathHelpers.Length(D3);
                    if (D1L < D2L && D1L<D3L)
                    {

                        //   Lines.Add(new Tuple<PointF, PointF>(new PointF((float)V2.X, (float)V2.Y), new PointF((float)V3.X, (float)V3.Y)));
                        //   Lines.Add(new Tuple<PointF, PointF>(new PointF((float)V3.X, (float)V3.Y), new PointF((float)V1.X, (float)V1.Y)));

                        Lines.Add(new Tuple<PointF, PointF>(new PointF((float)V1.X, (float)V1.Y), new PointF((float)V2.X, (float)V2.Y)));
                    }

                    if (D2L < D1L && D2L < D3L)
                    {
                        Lines.Add(new Tuple<PointF, PointF>(new PointF((float)V2.X, (float)V2.Y), new PointF((float)V3.X, (float)V3.Y)));
                        //                        Lines.Add(new Tuple<PointF, PointF>(new PointF((float)V1.X, (float)V1.Y), new PointF((float)V2.X, (float)V2.Y)));
                        //                      Lines.Add(new Tuple<PointF, PointF>(new PointF((float)V3.X, (float)V3.Y), new PointF((float)V1.X, (float)V1.Y)));
                    }

                    if (D3L < D2L && D3L < D1L)
                    {
                        Lines.Add(new Tuple<PointF, PointF>(new PointF((float)V3.X, (float)V3.Y), new PointF((float)V1.X, (float)V1.Y)));
                        //                    Lines.Add(new Tuple<PointF, PointF>(new PointF((float)V1.X, (float)V1.Y), new PointF((float)V2.X, (float)V2.Y)));
                        //                  Lines.Add(new Tuple<PointF, PointF>(new PointF((float)V2.X, (float)V2.Y), new PointF((float)V3.X, (float)V3.Y)));
                    }


                }


            }
            Matrix mM = new Matrix();
            mM.Rotate((float)compensateangle);
            List<Tuple<PointF, PointF>> l2 = new List<Tuple<PointF, PointF>>();
            foreach(var a in Lines)
            {
                PointF[] A = new PointF[2] { a.Item1, a.Item2 };
                mM.TransformPoints(A);

                l2.Add(new Tuple<PointF, PointF>(A[0], A[1]));
            }
            Lines = l2;
        }

        TriangleNet.Meshing.IMesh M;
        
        private void GetAllCorners(QuadTreeNode obj)
        {
            IG.Add(new TriangleNet.Geometry.Vertex((obj.xstart + obj.xend) / 2.0, (obj.ystart + obj.yend) / 2.0, 1));
        }

        public List<Tuple<PointF, PointF>> Lines = new List<Tuple<PointF, PointF>>();

        public void Render(GraphicsInterface G, Color FG, Color BG)
        {
            G.Clear(BG);
            Pen P = new Pen(FG, 1);
            foreach (var a in Lines)
            {
                G.DrawLine(P, a.Item1, a.Item2);
            }
        }
    }

}
