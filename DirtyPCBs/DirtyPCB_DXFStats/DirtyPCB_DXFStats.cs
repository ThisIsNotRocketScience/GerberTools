using netDxf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using GerberLibrary;



namespace DirtyPCB_DXFStats
{
    class DirtyPCB_DXFStats
    {
        public class DXFStats
        {
            public double Width;
            public double Height;
            private GerberLibrary.Bounds Box = new GerberLibrary.Bounds();
            public double Tracelength;
#if DEBUG
            Bitmap B;
            private List<Tuple<PointF, PointF>> Segs = new List<Tuple<PointF, PointF>>();
#endif
            public void Finish()
            {
                Width = Box.Width();
                Height = Box.Height();

#if DEBUG
                B = new Bitmap((int)(Width+200)/10, (int)((Height+200)/10));
                Graphics G = Graphics.FromImage(B);
                G.Clear(Color.White);
                G.TranslateTransform(0, B.Height);
                G.ScaleTransform(0.1f, -0.1f);
                G.TranslateTransform((float)-Box.TopLeft.X+100 ,  (float)-Box.TopLeft.Y +100 );
                foreach(var S in Segs)
                {
                    G.DrawLine(Pens.Black, S.Item1, S.Item2);
                }
                B.Save("debugout.png");
#endif
            }

            internal void AddSegment(double x1, double y1, double x2, double y2)
            {
#if DEBUG
                Segs.Add(new Tuple<PointF, PointF>(new PointF((float)x1, (float)y1), new PointF((float)x2, (float)y2)));
#endif  
                double dx = x2 - x1;
                double dy = y2 - y1;
                Box.FitPoint(x1, y1);
                Box.FitPoint(x2, y2);
                Tracelength += Math.Sqrt(dx * dx + dy * dy);
            }

            internal void Fit(double x, double  y)
            {
                Box.FitPoint(x, y);                
            }
        }

        static void Main(string[] args)
        {
            try
            {
                if (args.Count() < 1)
                {
                    Console.WriteLine("Usage: DirtyPCB_DXFStats.exe <infile>");
                    return;
                }

                DXFStats TheStats = new DXFStats();

                DxfDocument dxf = DxfDocument.Load(args[0]);
                if (dxf == null)
                {
                    bool bin;
                    var ver = DxfDocument.CheckDxfFileVersion(args[0], out bin);
                    Console.WriteLine("Error: failed to load {0} - netdxf not happy! Suspected dxf version {1}", args[0], ver);
                    return;
                }
                
                foreach (var a in dxf.Lines)
                {
                    TheStats.AddSegment(a.StartPoint.X, a.StartPoint.Y, a.EndPoint.X, a.EndPoint.Y);
                }

                foreach (var a in dxf.LwPolylines)
                {
                    for (int i = 0; i < a.Vertexes.Count - 1; i++)
                    {

                        TheStats.AddSegment(a.Vertexes[i].Position.X, a.Vertexes[i].Position.Y, a.Vertexes[i + 1].Position.X, a.Vertexes[i + 1].Position.Y);
                    }
                    if (a.IsClosed)
                    {
                        TheStats.AddSegment(a.Vertexes[0].Position.X, a.Vertexes[0].Position.Y, a.Vertexes[a.Vertexes.Count - 1].Position.X, a.Vertexes[a.Vertexes.Count - 1].Position.Y);

                    }
                }

                foreach (var a in dxf.Polylines)
                {
                    for (int i = 0; i < a.Vertexes.Count - 1; i++)
                    {
                        TheStats.AddSegment(a.Vertexes[i].Position.X, a.Vertexes[i].Position.Y, a.Vertexes[i + 1].Position.X, a.Vertexes[i + 1].Position.Y);
                    }
                }

                foreach (var a in dxf.Circles)
                {
                    double length = 2 * Math.PI * a.Radius; ;
                    int count = (int)length;
                    for (int i = 0; i < count; i++)
                    {
                        double P1 = (i * Math.PI * 2.0 / (double)count);
                        double P2 = ((i + 1) * Math.PI * 2.0 / (double)count);
                        TheStats.AddSegment(Math.Sin(P1) * a.Radius + a.Center.X, Math.Cos(P1) * a.Radius + a.Center.Y, Math.Sin(P2) * a.Radius + a.Center.X, Math.Cos(P2) * a.Radius + a.Center.Y);
                    }
                }

                foreach (var a in dxf.Ellipses)
                {
                    double R1 = a.MajorAxis;
                    double R2 = a.MinorAxis;

                    // approximate length of curve..                
                    double length = 2 * Math.PI * (Math.Sqrt((R1 * R1 + R2 * R2) / 2)); ;
                    int count = (int)length;
                    for (int i = 0; i < count; i++)
                    {
                        double P1 = (i * 360.0 / (double)count);
                        double P2 = ((i + 1) * 360.0 / (double)count);
                        var C = a.PolarCoordinateRelativeToCenter(P1);
                        var C2 = a.PolarCoordinateRelativeToCenter(P2);
                        TheStats.AddSegment(C.X + a.Center.X, C.Y + a.Center.Y, C2.X + a.Center.X, C2.Y + a.Center.Y);
                    }
                }

                foreach (var a in dxf.Splines)
                {
                    var V = a.PolygonalVertexes(a.ControlPoints.Count * 20);
                    for (int i = 0; i < V.Count - 1; i++)
                    {
                        TheStats.AddSegment(V[i].X, V[i].Y, V[i + 1].X, V[i + 1].Y);
                    }
                    if (a.IsClosed)
                    {
                        TheStats.AddSegment(V[0].X, V[0].Y, V[V.Count - 1].X, V[V.Count - 1].Y);
                    }
                }

                foreach (var a in dxf.Arcs)
                {
                    var V = a.ToPolyline((int)(2 + a.Radius * Math.PI * 2));
                    for (int i = 0; i < V.Vertexes.Count - 1; i++)
                    {
                        TheStats.AddSegment(V.Vertexes[i].Position.X, V.Vertexes[i].Position.Y, V.Vertexes[i + 1].Position.X, V.Vertexes[i + 1].Position.Y);
                    }
                }

                foreach (var a in dxf.MLines)
                {
                    for (int i = 0; i < a.Vertexes.Count - 1; i++)
                    {
                        TheStats.AddSegment(a.Vertexes[i].Location.X, a.Vertexes[i].Location.Y, a.Vertexes[i + 1].Location.X, a.Vertexes[i + 1].Location.Y);
                    }
                }

                //Should points be fitted too? 
                //foreach (var a in dxf.Points)
                //{
                //   TheStats.Fit(a.Location.X, a.Location.Y);
                //}


                
                TheStats.Finish();
                var json = new JavaScriptSerializer().Serialize(TheStats);
                Console.WriteLine(json);
            }
            catch(Exception E)
            {
                Console.WriteLine("Error: {0}", E.Message);
            }            
        }
    }
}
