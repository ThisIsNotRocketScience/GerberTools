using ClipperLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Serialization;
using Polygons = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;
using GerberLibrary.Core.Primitives;
using GerberLibrary.Core;
using Ionic.Zip;
using TriangleNet;
using TriangleNet.Geometry;


namespace GerberLibrary
{
    public interface ProgressLog
    {
        void AddString(string text, float progress = -1);
    }


    public class GerberPanel
    {
        public GerberLayoutSet TheSet;

        public GerberPanel(double width = 100, double height = 100)
        {
            TheSet = new GerberLayoutSet();
            TheSet.Width = width;
            TheSet.Height = height;
        }

        public Bitmap BoardBitmap = new Bitmap(1000, 1000);
        public List<PolyLine> CombinedOutline = new List<PolyLine>();
        GraphicsPath GP = new GraphicsPath();
        Bitmap MMGrid = null;

        public List<string> AddGerberFolder(string path, bool add = true)
        {
            if (File.Exists(path) && (Path.GetExtension(path).ToLower() == ".zip" || Path.GetExtension(path).ToLower() == "zip"))
            {
                return AddGerberZip(path, add);

            }

            List<string> res = new List<string>();
            if (add) TheSet.LoadedOutlines.Add(path);
            string foldername = Path.GetDirectoryName(path + Path.DirectorySeparatorChar);
            Console.WriteLine("adding folder {0}", foldername);
            bool had = false;

            string[] FileNames = Directory.GetFiles(foldername);
            List<string> outlinefiles = new List<string>();
            List<string> millfiles = new List<string>();
            List<string> copperfiles = new List<string>();

            foreach (var F in FileNames)
            {
                BoardSide BS = BoardSide.Unknown;
                BoardLayer BL = BoardLayer.Unknown;
                if (Gerber.FindFileType(F) == BoardFileType.Gerber)
                {
                    Gerber.DetermineBoardSideAndLayer(F, out BS, out BL);
                    if (BS == BoardSide.Both && BL == BoardLayer.Outline)
                    {
                        outlinefiles.Add(F);
                    }
                    else
                    {
                        if (BS == BoardSide.Both && BL == BoardLayer.Mill)
                        {
                            millfiles.Add(F);
                        }
                        else
                        {
                            if (BL == BoardLayer.Copper)
                            {
                                copperfiles.Add(F);
                            }
                        }
                    }
                }
            }
            foreach (var a in outlinefiles)
            {
                GerberOutlines[path] = new GerberOutline(a);
                if (GerberOutlines[path].TheGerber.DisplayShapes.Count > 0) had = true;
            }

            if (had == false)
            {
                foreach (var a in millfiles)
                {
                    GerberOutlines[path] = new GerberOutline(a);
                    if (GerberOutlines[path].TheGerber.DisplayShapes.Count > 0) had = true;
                }

            }
            if (had == false)
            {
                // TODO: extract an outline from other layers? THIS IS DANGEROUS!
                Console.WriteLine("{0} has no outline available?", path);
            }
            else
            {
                res.Add(path);
            }
            return res;
        }

        private List<string> AddGerberZip(string path, bool add)
        {
            List<string> res = new List<string>();
            Dictionary<string, MemoryStream> Files = new Dictionary<string, MemoryStream>();
            using (Ionic.Zip.ZipFile zip1 = Ionic.Zip.ZipFile.Read(path))
            {
                foreach (ZipEntry e in zip1)
                {
                    MemoryStream MS = new MemoryStream();
                    if (e.IsDirectory == false)
                    {
                        e.Extract(MS);
                        MS.Seek(0, SeekOrigin.Begin);
                        Files[e.FileName] = MS;
                    }
                }
            }

            if (add) TheSet.LoadedOutlines.Add(path);
            string foldername = Path.GetDirectoryName(path + Path.DirectorySeparatorChar);
            Console.WriteLine("adding zip file {0}", foldername);
            bool had = false;

            string[] FileNames = Files.Keys.ToArray();
            List<string> outlinefiles = new List<string>();
            List<string> millfiles = new List<string>();
            List<string> copperfiles = new List<string>();

            foreach (var F in FileNames)
            {
                BoardSide BS = BoardSide.Unknown;
                BoardLayer BL = BoardLayer.Unknown;
                Files[F].Seek(0, SeekOrigin.Begin);
                if (Gerber.FindFileTypeFromStream(new StreamReader(Files[F]), F) == BoardFileType.Gerber)
                {
                    Gerber.DetermineBoardSideAndLayer(F, out BS, out BL);
                    if (BS == BoardSide.Both && BL == BoardLayer.Outline)
                    {
                        outlinefiles.Add(F);
                    }
                    else
                    {
                        if (BS == BoardSide.Both && BL == BoardLayer.Mill)
                        {
                            millfiles.Add(F);
                        }
                        else
                        {
                            if (BL == BoardLayer.Copper)
                            {
                                copperfiles.Add(F);
                            }
                        }
                    }
                }
            }
            foreach (var a in outlinefiles)
            {
                Files[a].Seek(0, SeekOrigin.Begin);
                GerberOutlines[path] = new GerberOutline(new StreamReader(Files[a]), a);
                if (GerberOutlines[path].TheGerber.DisplayShapes.Count > 0) had = true;
            }

            if (had == false)
            {
                foreach (var a in millfiles)
                {
                    Files[a].Seek(0, SeekOrigin.Begin);
                    GerberOutlines[path] = new GerberOutline(new StreamReader(Files[a]), a);
                    if (GerberOutlines[path].TheGerber.DisplayShapes.Count > 0) had = true;
                }

            }
            if (had == false)
            {
                // TODO: extract an outline from other layers? THIS IS DANGEROUS!
                Console.WriteLine("{0} has no outline available?", path);
            }
            else
            {
                res.Add(path);
            }
            return res;

        }

        public void BuildAutoTabs(GerberArtWriter GAW = null, GerberArtWriter GAW2 = null)
        {
            if (TheSet.Instances.Count < 3) return;
            List<Vertex> Vertes = new List<Vertex>();

            Dictionary<Vertex, GerberLibrary.GerberInstance> InstanceMap = new Dictionary<Vertex, GerberLibrary.GerberInstance>();

            foreach (var a in TheSet.Instances)
            {
                if (a.GerberPath.Contains("???") == false)
                {
                    var outline = GerberOutlines[a.GerberPath];
                    var P = outline.GetActualCenter();
                    P = P.Rotate(a.Angle);
                    P.X += a.Center.X;
                    P.Y += a.Center.Y;
                    var V = new Vertex(P.X, P.Y);
                    InstanceMap[V] = a;
                    Vertes.Add(V);
                }
            }
            UpdateShape();

            var M = new TriangleNet.Meshing.GenericMesher();
            var R = M.Triangulate(Vertes);


            foreach (var a in R.Edges)
            {
                var A = R.Vertices.ElementAt(a.P0);
                var B = R.Vertices.ElementAt(a.P1);
                PolyLine P = new PolyLine();
                P.Add(A.X, A.Y);
                P.Add(B.X, B.Y);
                GerberLibrary.GerberInstance iA = null;
                GerberLibrary.GerberInstance iB = null;
                for (int i = 0; i < InstanceMap.Count; i++)
                {
                    var V = InstanceMap.Keys.ElementAt(i);
                    if (V.ID == A.ID) iA = InstanceMap.Values.ElementAt(i);
                    if (V.ID == B.ID) iB = InstanceMap.Values.ElementAt(i);
                }

                if (iA != null && iB != null)
                {
                    PointD vA = new PointD(A.X, A.Y);
                    PointD vB = new PointD(B.X, B.Y);

                    PointD diffA = vB;
                    diffA.X -= iA.Center.X;
                    diffA.Y -= iA.Center.Y;
                    diffA = diffA.Rotate(-iA.Angle);



                    PointD diffB = vA;
                    diffB.X -= iB.Center.X;
                    diffB.Y -= iB.Center.Y;
                    diffB = diffB.Rotate(-iB.Angle);



                    var outlineA = GerberOutlines[iA.GerberPath];
                    var outlineB = GerberOutlines[iB.GerberPath];
                    PointD furthestA = new PointD();
                    PointD furthestB = new PointD();
                    double furthestdistA = 0.0;

                    var acA = outlineA.GetActualCenter();
                    var acB = outlineB.GetActualCenter();
                    foreach (var s in outlineA.TheGerber.OutlineShapes)
                    {
                        List<PointD> intersect = s.GetIntersections(diffA, acA);
                        if (intersect != null && intersect.Count > 0)
                        {
                            for (int i = 0; i < intersect.Count; i++)
                            {
                                double newD = PointD.Distance(acA, intersect[i]);

                                PolyLine PL = new PolyLine();
                                var CP = intersect[i].Rotate(iA.Angle);
                                CP.X += iA.Center.X;
                                CP.Y += iA.Center.Y;
                                PL.MakeCircle(1);
                                PL.Translate(CP.X, CP.Y);

                                if (GAW != null) GAW.AddPolygon(PL);

                                if (newD > furthestdistA)
                                {
                                    furthestdistA = newD;
                                    furthestA = intersect[i];
                                }
                            }
                        }
                    }
                    double furthestdistB = 0.0;

                    foreach (var s in outlineB.TheGerber.OutlineShapes)
                    {
                        List<PointD> intersect = s.GetIntersections(diffB, acB);
                        if (intersect != null && intersect.Count > 0)
                        {
                            for (int i = 0; i < intersect.Count; i++)
                            {
                                double newD = PointD.Distance(acB, intersect[i]);
                                PolyLine PL = new PolyLine();
                                var CP = intersect[i].Rotate(iB.Angle);
                                CP.X += iB.Center.X;
                                CP.Y += iB.Center.Y;
                                PL.MakeCircle(1);
                                PL.Translate(CP.X, CP.Y);
                                if (GAW != null) GAW.AddPolygon(PL);

                                if (newD > furthestdistB)
                                {
                                    furthestdistB = newD;
                                    furthestB = intersect[i];
                                }
                            }
                        }
                    }

                    if (furthestdistB != 0 && furthestdistA != 0)
                    {
                        furthestA = furthestA.Rotate(iA.Angle);
                        furthestA.X += iA.Center.X;
                        furthestA.Y += iA.Center.Y;

                        furthestB = furthestB.Rotate(iB.Angle);
                        furthestB.X += iB.Center.X;
                        furthestB.Y += iB.Center.Y;

                        var Distance = PointD.Distance(furthestA, furthestB);
                        if (Distance < 7)
                        {
                            var CP = new PointD((furthestA.X + furthestB.X) / 2, (furthestA.Y + furthestB.Y) / 2);
                            var T = AddTab(CP);
                            T.Radius = (float)Math.Max(Distance / 1.5, 3.2f);

                            PolyLine PL = new PolyLine();
                            PL.MakeCircle(T.Radius);
                            PL.Translate(CP.X, CP.Y);
                            if (GAW2 != null) GAW2.AddPolygon(PL);
                        }

                    }
                    else
                    {
                        var T = AddTab(new PointD((A.X + B.X) / 2, (A.Y + B.Y) / 2));
                        T.Radius = 3.0f;
                    }
                }
                if (GAW != null) GAW.AddPolyLine(P, 0.1);
            }


            UpdateShape();
            RemoveAllTabs(true);
            UpdateShape();
        }

        public Dictionary<string, GerberOutline> GerberOutlines = new Dictionary<string, GerberOutline>();

        /// <summary>
        /// Create a board-sized square and subtract all the boardshapes from this shape. 
        /// </summary>
        /// <param name="offsetinMM"></param>
        /// <param name="retractMM"></param>
        /// <returns></returns>
        public List<PolyLine> GenerateNegativePolygon(double offsetinMM = 3, double retractMM = 1)
        {
            Polygons CombinedInstanceOutline = new Polygons();

            foreach (var b in TheSet.Instances)
            {
                Polygons clips = new Polygons();
                var a = GerberOutlines[b.GerberPath];
                foreach (var c in b.TransformedOutlines)
                {
                    //       PolyLine PL = new PolyLine();
                    //     PL.FillTransformed(c, new PointD(b.Center), b.Angle);
                    clips.Add(c.toPolygon());
                }

                Clipper cp = new Clipper();
                cp.AddPolygons(CombinedInstanceOutline, PolyType.ptSubject);
                cp.AddPolygons(clips, PolyType.ptClip);

                cp.Execute(ClipType.ctUnion, CombinedInstanceOutline, PolyFillType.pftNonZero, PolyFillType.pftNonZero);
            }

            Polygons BoardMinusCombinedInstanceOutline = new Polygons();
            PolyLine Board = new PolyLine();
            Board.Vertices.Add(new PointD(0, 0));
            Board.Vertices.Add(new PointD(TheSet.Width, 0));
            Board.Vertices.Add(new PointD(TheSet.Width, TheSet.Height));
            Board.Vertices.Add(new PointD(0, TheSet.Height));
            Board.Vertices.Add(new PointD(0, 0));
            BoardMinusCombinedInstanceOutline.Add(Board.toPolygon());
            foreach (var b in CombinedInstanceOutline)
            {
                Polygons clips = new Polygons();

                clips.Add(b);
                Polygons clips2 = Clipper.OffsetPolygons(clips, offsetinMM * 100000.0f, JoinType.jtMiter);

                Clipper cp = new Clipper();
                cp.AddPolygons(BoardMinusCombinedInstanceOutline, PolyType.ptSubject);
                cp.AddPolygons(clips2, PolyType.ptClip);

                cp.Execute(ClipType.ctDifference, BoardMinusCombinedInstanceOutline, PolyFillType.pftNonZero, PolyFillType.pftNonZero);
            }

            //CombinedOutline.Clear();

            Polygons shrunk = Clipper.OffsetPolygons(BoardMinusCombinedInstanceOutline, -retractMM * 100000.0f, JoinType.jtRound);
            Polygons expanded = Clipper.OffsetPolygons(shrunk, retractMM * 100000.0f, JoinType.jtRound);

            //CombinedOutline.Clear();
            //foreach (var b in TheSet.Tabs)
            //{
            //    Polygons clips = new Polygons();
            //    PolyLine Circle = new PolyLine();
            //    Circle.MakeCircle(b.Radius);
            //    PolyLine PL = new PolyLine();
            //    PL.FillTransformed(Circle, new PointD( b.Center), b.Angle);
            //    clips.Add(PL.toPolygon());

            //    Clipper cp = new Clipper();
            //    cp.AddPolygons(expanded, PolyType.ptSubject);
            //    cp.AddPolygons(clips, PolyType.ptClip);

            //    cp.Execute(ClipType.ctUnion, expanded, PolyFillType.pftNonZero, PolyFillType.pftNonZero);
            //}
            //foreach (var b in CombinedInstanceOutline)
            //{
            //    Polygons clips = new Polygons();



            //    clips.Add(b);
            //    Clipper cp = new Clipper();
            //    cp.AddPolygons(expanded, PolyType.ptSubject);
            //    cp.AddPolygons(clips, PolyType.ptClip);

            //    cp.Execute(ClipType.ctUnion, expanded, PolyFillType.pftNonZero, PolyFillType.pftNonZero);
            //}

            List<PolyLine> Res = new List<PolyLine>();

            foreach (var s in expanded)
            {
                PolyLine PL = new PolyLine();
                PL.fromPolygon(s);
                Res.Add(PL);
            }
            return Res;

        }

        public void UpdateShape()
        {
            CombinedOutline.Clear();

            foreach (var a in TheSet.Instances)
            {
                if (GerberOutlines.ContainsKey(a.GerberPath))
                {
                    bool doit = false;
                    if (a.LastCenter == null) doit = true;
                    if (doit || (PointD.Distance(new PointD(a.Center), a.LastCenter) != 0 || a.Angle != a.LastAngle))
                    {
                        a.RebuildTransformed(GerberOutlines[a.GerberPath], TheSet.ExtraTabDrillDistance);
                    }

                }
            }

            if (TheSet.ConstructNegativePolygon)
            {
                RemoveInstance("???_negative");

                var Neg = GenerateNegativePolygon(TheSet.FillOffset, TheSet.Smoothing);
                var G = new GerberOutline("");
                G.TheGerber.Name = "???_negative";
                G.TheGerber.OutlineShapes = Neg;
                G.TheGerber.DisplayShapes = Neg;
                G.TheGerber.FixPolygonWindings();
                GerberOutlines["???_negative"] = G;
                AddInstance("???_negative", new PointD(0, 0), true);
            }

            foreach (var aa in TheSet.Instances)
            {
                aa.Tabs.Clear();
            }
            FindOutlineIntersections();

        }

        private void RemoveInstance(string path)
        {
            foreach (var a in TheSet.Instances)
            {
                if (a.GerberPath == path)
                {
                    RemoveInstance(a);
                    return;
                }
            }
        }

        public void RenderInstanceHoles(GraphicsInterface G, float PW, Color C, AngledThing b, bool errors = false, bool active = false, bool hover = false)
        {
            if (b == null) return;
            var T = G.Transform.Clone();

            G.TranslateTransform((float)b.Center.X, (float)b.Center.Y);
            G.RotateTransform(b.Angle);
            if (b.GetType() == typeof(GerberInstance))
            {
                GerberInstance GI = b as GerberInstance;
                if (GerberOutlines.ContainsKey(GI.GerberPath))
                {

                    var a = GerberOutlines[GI.GerberPath];

                    foreach (var Shape in a.TheGerber.OutlineShapes)
                    {

                        if (Shape.Hole == true)
                        {
                            FillShape(G, new SolidBrush(Color.FromArgb(100, 0, 0, 0)), Shape);

                        }
                    }
                }
            }
            G.Transform = T;
        }

        /// <summary>
        /// Render a gerber instance to a graphics interface
        /// </summary>
        /// <param name="G"></param>
        /// <param name="PW"></param>
        /// <param name="C"></param>
        /// <param name="b"></param>
        /// <param name="errors"></param>
        /// <param name="active"></param>
        public void RenderInstance(GraphicsInterface G, float PW, Color C, AngledThing b, bool errors = false, bool active = false, bool hover = false)
        {
            if (b == null) return;
            var T = G.Transform.Clone();

            G.TranslateTransform((float)b.Center.X, (float)b.Center.Y);
            G.RotateTransform(b.Angle);
            Pen P = new Pen(C, PW) { LineJoin = System.Drawing.Drawing2D.LineJoin.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round, StartCap = System.Drawing.Drawing2D.LineCap.Round };
            Pen ActiveP = new Pen(Color.FromArgb(200, 150, 20), PW * 2) { LineJoin = System.Drawing.Drawing2D.LineJoin.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round, StartCap = System.Drawing.Drawing2D.LineCap.Round };
            Pen ActivePD = new Pen(Color.Green, PW * 1) { LineJoin = System.Drawing.Drawing2D.LineJoin.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round, StartCap = System.Drawing.Drawing2D.LineCap.Round };

            Pen ErrorP = new Pen(Color.Red, PW * 2.5f) { LineJoin = System.Drawing.Drawing2D.LineJoin.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round, StartCap = System.Drawing.Drawing2D.LineCap.Round };
            if (b.GetType() == typeof(BreakTab))
            {
                BreakTab BT = b as BreakTab;
                if (BT.Errors.Count > 0) errors = true;
                DrawMarker(errors, G, new PointD(0, 0), 1, PW, errors ? ErrorP : P);
                PolyLine Circle = new PolyLine();
                Circle.MakeCircle((b as BreakTab).Radius);
                if (errors)
                {
                    DrawShape(G, ErrorP, Circle);
                    for (int i = 0; i < BT.Errors.Count; i++)
                    {
                        G.DrawString(new PointD(BT.Radius + 1, PW * 10 * i), BT.Errors[i], PW, false);
                    }
                }
                else
                {
                    DrawShape(G, P, Circle);
                }
                if (active)
                {
                    DrawShape(G, errors ? ErrorP : ActivePD, Circle);
                    DrawMarker(errors, G, new PointD(0, 0), 1, PW, errors ? ErrorP : ActivePD);
                    DrawShape(G, ActiveP, Circle);
                    DrawMarker(errors, G, new PointD(0, 0), 1, PW, ActiveP);

                }
                if (hover)
                {
                    DrawMarker(errors, G, new PointD(0, 0), 1, PW, new Pen(Color.Blue, PW * 2));

                }
            }
            if (b.GetType() == typeof(GerberInstance))
            {
                GerberInstance GI = b as GerberInstance;
                if (GerberOutlines.ContainsKey(GI.GerberPath))
                {

                    var a = GerberOutlines[GI.GerberPath];
                    //  a.BuildShapeCache();
                    float R = 0;
                    float Gf = 0;
                    float B = 0;
                    float A = 0.8f;
                    switch (GI.Tabs.Count)
                    {
                        case 0:
                            R = .70f;
                            break;
                        case 1:
                            R = .70f; Gf = 0.35f;
                            break;
                        case 2:
                            R = .70f; Gf = 0.70f;
                            break;

                        default:
                            Gf = 1.0f;
                            break;
                    }
                    //G.FillTriangles(a.TheGerber.ShapeCacheTriangles, Color.FromArgb((byte)(A * 255.0), (byte)(R * 255.0), (byte)(Gf * 255.0), (byte)(B * 255.0)));

                    foreach (var Shape in a.TheGerber.OutlineShapes)
                    {

                        if (Shape.Hole == false)
                        {
                            //FillShape(G, new SolidBrush(Color.FromArgb(100, 255, 255, 255)), Shape);
                        }
                        else
                        {
                            //             FillShape(G, new SolidBrush(Color.FromArgb(100, 0, 0, 0)), Shape);   
                        }
                        if (active)
                        {
                            DrawShape(G, ActivePD, Shape);
                            //  DrawShapeNormals(G, ActivePD, Shape);
                        }
                        else
                        {
                            DrawShape(G, active ? ActiveP : P, Shape);
                        }
                    }
                    foreach (var Shape in a.TheGerber.DisplayShapes)
                    {
                        DrawShape(G, active ? ActiveP : P, Shape);
                    }
                    var width = (int)(Math.Ceiling(a.TheGerber.BoundingBox.BottomRight.X - a.TheGerber.BoundingBox.TopLeft.X));
                    var height = (int)(Math.Ceiling(a.TheGerber.BoundingBox.BottomRight.Y - a.TheGerber.BoundingBox.TopLeft.Y));

                    double ox = (float)(a.TheGerber.TranslationSinceLoad.X + a.TheGerber.BoundingBox.TopLeft.X) + width / 2;
                    double oy = (float)(a.TheGerber.TranslationSinceLoad.Y + a.TheGerber.BoundingBox.TopLeft.Y + height / 2);

                    PointD Ext = G.MeasureString(Path.GetFileName(GI.GerberPath));
                    double Z = 1;
                    if (Ext.X > width) Z = width / Ext.X;
                    if (Ext.Y * Z > height) Z = height / Ext.Y;



                    G.DrawString(new PointD(ox, oy), Path.GetFileName(GI.GerberPath), Z * 30, true, R, Gf, B, A);


                }

                G.Transform = T;
                if (active)
                {
                    DrawMarker(false, G, new PointD(b.Center), 1, PW, ActivePD);
                    DrawMarker(false, G, new PointD(b.Center), 1, PW, ActiveP);

                }





            }
            G.Transform = T;
        }

        private void DrawShapeNormals(GraphicsInterface G, Pen P, PolyLine Shape)
        {
            PointF[] Points = new PointF[2];

            for (int j = 0; j < Shape.Count() - 1; j++)
            {

                var P1 = Shape.Vertices[j];
                var P2 = Shape.Vertices[j + 1];
                Points[0].X = (float)((P1.X + P2.X) / 2);
                Points[0].Y = (float)((P1.Y + P2.Y) / 2);

                var DP = (P2 - P1);
                if (DP.Length() > 0)
                {
                    DP.Normalize();
                    DP = DP.Rotate(90);
                    Points[1].X = (float)((Points[0].X + DP.X * 2));
                    Points[1].Y = (float)((Points[0].Y + DP.Y * 2));
                    G.DrawLines(P, Points);
                }

            }
        }

        /// <summary>
        /// Render a full panel to a graphics interface
        /// </summary>
        /// <param name="PW"></param>
        /// <param name="G"></param>
        /// <param name="targetwidth"></param>
        /// <param name="targetheight"></param>
        /// <param name="SelectedInstance"></param>
        public void DrawBoardBitmap(float PW = 1, GraphicsInterface G = null, int targetwidth = 0, int targetheight = 0, AngledThing SelectedInstance = null, AngledThing Hoverinstance = null, double snapdistance = 1)
        {
            if (G == null)
            {
                return;
            }
            G.Clear(System.Drawing.ColorTranslator.FromHtml("#888885"));

            RectangleF RR = G.ClipBounds;
            if (G.IsFast)
            {
                G.FillRectangle(System.Drawing.ColorTranslator.FromHtml("#f5f4e8"), 0, 0, (int)TheSet.Width, (int)TheSet.Height);
                Helpers.DrawMMGrid(G, PW, (float)TheSet.Width, (float)TheSet.Height, (float)snapdistance, (float)snapdistance * 10.0f);

            }
            else
                if ((MMGrid == null || MMGrid.Width != targetwidth || MMGrid.Height != targetheight))
            {
                MMGrid = new Bitmap(targetwidth, targetheight);
                Graphics G2 = Graphics.FromImage(MMGrid);
                G2.SmoothingMode = SmoothingMode.HighQuality;
                G2.Transform = G.Transform;
                Helpers.DrawMMGrid(new GraphicsGraphicsInterface(G2), PW, (float)TheSet.Width, (float)TheSet.Height, (float)snapdistance, (float)snapdistance * 10.0f);
                Console.WriteLine("building new millimeter grid!");
            }
            G.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
            G.DrawImage(MMGrid, RR.Left, RR.Bottom, RR.Width, -RR.Height);

            foreach (var b in TheSet.Tabs)
            {
                if (b.Errors.Count > 0)
                {

                    RenderInstance(G, PW, Color.Gray, b, true);
                }
                else
                {
                    RenderInstance(G, PW, Color.Gray, b, false);
                }
            }


            foreach (var b in TheSet.Instances)
            {
                RenderInstance(G, PW, Color.DarkGray, b);
            }

            foreach (var b in TheSet.Instances)
            {
                RenderInstanceHoles(G, PW, Color.DarkGray, b);
            }
            Pen OO = new Pen(Color.Orange, PW) { LineJoin = System.Drawing.Drawing2D.LineJoin.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round, StartCap = System.Drawing.Drawing2D.LineCap.Round };

            Pen OO3 = new Pen(Color.FromArgb(140, 40, 40, 30), PW) { LineJoin = System.Drawing.Drawing2D.LineJoin.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round, StartCap = System.Drawing.Drawing2D.LineCap.Round };
            SolidBrush BR = new SolidBrush(Color.FromArgb(180, 0, 100, 0));
            //BR.Color.A = 180;
            GP.Reset();

            foreach (var FinalPoly in FinalPolygonsWithTabs)
            {
                PolyLine PL2 = new PolyLine();
                PL2.Vertices = FinalPoly.Vertices;
                DrawShape(G, OO3, PL2);
                //  DrawMarker(G, PL2.Vertices.First(), 1, PW);
                //  DrawMarker(G, PL2.Vertices.Last(), 1, PW);
            }


            //
            PolyLine PL = new PolyLine();
            PL.MakeSquare(0.15);

            foreach (var s in DrillHoles)
            {
                var T = G.Transform.Clone();
                G.TranslateTransform((float)s.X, (float)s.Y);
                FillShape(G, new SolidBrush(Color.FromArgb(140, 0, 0, 0)), PL);
                DrawShape(G, new Pen(Color.Black), PL);
                G.Transform = T;
            }

            if (Hoverinstance != null)
            {
                RenderInstance(G, PW, Color.Blue, Hoverinstance, false, false);
            }

            if (SelectedInstance != null)
            {
                RenderInstance(G, PW * 2, Color.Black, null, false, true);
                RenderInstance(G, PW, Color.Black, SelectedInstance, false, true);
            }

        }

        /// <summary>
        /// Render a solid polygon
        /// </summary>
        /// <param name="G"></param>
        /// <param name="BR"></param>
        /// <param name="Shape"></param>
        private void FillShape(GraphicsInterface G, SolidBrush BR, PolyLine Shape)
        {
            // todo: cache the triangulated polygon!
            //G.FillShape(BR, Shape);
        }

        /// <summary>
        /// Render a marker-cross
        /// </summary>
        /// <param name="cross">Cross or plus shape?</param>
        /// <param name="G">Target graphics interface</param>
        /// <param name="pointD">Cross position</param>
        /// <param name="crossize">Cross size</param>
        /// <param name="PW">Global Pen width</param>
        /// <param name="P">Pen to use (white/PW is default)</param>
        private static void DrawMarker(bool cross, GraphicsInterface G, PointD pointD, float crossize, float PW, Pen P = null)
        {
            if (P == null)
                P = new Pen(Color.White, PW);
            if (cross)
            {
                G.DrawLine(P, (float)pointD.X - crossize, (float)pointD.Y - crossize,
                    (float)pointD.X + crossize, (float)pointD.Y + crossize);
                G.DrawLine(P, (float)pointD.X + crossize, (float)pointD.Y - crossize,
                    (float)pointD.X - crossize, (float)pointD.Y + crossize);
            }

            else // plus
            {
                G.DrawLine(P, (float)pointD.X, (float)pointD.Y - crossize,
                    (float)pointD.X, (float)pointD.Y + crossize);
                G.DrawLine(P, (float)pointD.X + crossize, (float)pointD.Y,
                    (float)pointD.X - crossize, (float)pointD.Y);
            }
        }

        /// <summary>
        /// Draw an open polygon
        /// </summary>
        /// <param name="G">Target graphicsinterface</param>
        /// <param name="P">Pen to use</param>
        /// <param name="Shape"></param>
        private static void DrawShape(GraphicsInterface G, Pen P, PolyLine Shape)
        {

            PointF[] Points = new PointF[Shape.Count()];

            for (int j = 0; j < Shape.Count(); j++)
            {

                var P1 = Shape.Vertices[j];
                Points[j].X = (float)((P1.X));
                Points[j].Y = (float)((P1.Y));


            }
            if (Points.Count() > 1)
                G.DrawLines(P, Points);
        }

        List<PointD> DrillHoles = new List<PointD>();

        public class TabIntersection
        {
            public bool Start;
            public double Angle;
            public PointD Location = new PointD();
            public PointD Direction = new PointD();
            public override string ToString()
            {
                return String.Format("{0},{1} , {2}", Start ? "start" : "end", Angle, Location);
            }
        }

        List<PolyLine> GeneratedArcs = new List<PolyLine>();

        bool GenerateTabArcs(List<TabIntersection> intersects, BreakTab t, double drillradius = 1)
        {
            if (t.Errors.Count > 0) return false;

            if (intersects.Count < 4)
            {
                t.Valid = false;
                t.Errors.Add("not enough intersections!");
                return false;
            }


            PointD center = new PointD(t.Center);
            bool result = true;
            intersects = (from i in intersects orderby i.Angle select i).ToList();
            //intersects.Sort(x => x.Angle);
            int had = 0;
            int current = 0;
            while (intersects[current].Start == true) current = (current + 1) % intersects.Count;

            int startcurrent = current;

            while (had < intersects.Count)
            {

                var PStart = intersects[current].Location;
                PointD StartDir = intersects[current].Direction;
                current = (current + 1) % intersects.Count;
                had++;

                PointD EndDir = intersects[current].Direction;
                var PEnd = intersects[current].Location;
                current = (current + 1) % intersects.Count;
                had++;

                PointD StartNorm = StartDir.Rotate(90);
                PointD EndNorm = EndDir.Rotate(90);

                if ((center - PStart).Dot(StartNorm) < 0)
                {
                    //     StartNorm = StartNorm * -1;
                }
                if ((center - PEnd).Dot(EndNorm) < 0)
                {
                    //  EndNorm = EndNorm * -1;
                }
                if (Helpers.Distance(PStart, PEnd) < drillradius * 2)
                {
                    t.Errors.Add("distance closer than drillradius");
                    result = false;
                }

                var C1 = PStart + StartNorm * drillradius;
                var C2 = PEnd + EndNorm * drillradius;

                var DF = C2 - C1;
                DF.Normalize();
                var DR = DF.Rotate(90);

                var A1 = C1 + DR * drillradius;
                var A2 = C2 + DR * drillradius;

                double AS = Helpers.AngleBetween(C1, PStart);
                double AE = Helpers.AngleBetween(C1, A1);

                double BS = Helpers.AngleBetween(C2, PEnd);
                double BE = Helpers.AngleBetween(C2, A2);

                if (AE > AS) AE -= Math.PI * 2;
                if (BE < BS) BE += Math.PI * 2;

                if (Math.Abs(AE - AS) >= Math.PI * 1.9)
                {
                    t.Errors.Add("angle too big");
                    result = false;
                }
                if (Math.Abs(BE - BS) >= Math.PI * 1.9)
                {
                    t.Errors.Add("angle too big");
                    result = false;
                }
            }

            if (result == false) return false;
            current = startcurrent;
            had = 0;
            while (had < intersects.Count)
            {

                var PStart = intersects[current].Location;
                PointD StartDir = intersects[current].Direction;
                current = (current + 1) % intersects.Count;
                had++;

                PointD EndDir = intersects[current].Direction;
                var PEnd = intersects[current].Location;
                current = (current + 1) % intersects.Count;
                had++;

                PointD StartNorm = StartDir.Rotate(90);
                PointD EndNorm = EndDir.Rotate(90);

                if ((center - PStart).Dot(StartNorm) < 0)
                {
                    //     StartNorm = StartNorm * -1;
                }

                if ((center - PEnd).Dot(EndNorm) < 0)
                {
                    //  EndNorm = EndNorm * -1;
                }

                if (Helpers.Distance(PStart, PEnd) < drillradius * 2)
                {
                    t.Errors.Add("distance closer than drillradius");
                    result = false;
                }

                var C1 = PStart + StartNorm * drillradius;
                var C2 = PEnd + EndNorm * drillradius;

                var DF = C2 - C1;
                DF.Normalize();
                var DR = DF.Rotate(90);

                var A1 = C1 + DR * drillradius;
                var A2 = C2 + DR * drillradius;

                double AS = Helpers.AngleBetween(C1, PStart);
                double AE = Helpers.AngleBetween(C1, A1);

                double BS = Helpers.AngleBetween(C2, PEnd);
                double BE = Helpers.AngleBetween(C2, A2);

                if (AE > AS) AE -= Math.PI * 2;
                if (BE < BS) BE += Math.PI * 2;

                if (Math.Abs(AE - AS) >= Math.PI * 2)
                {
                    t.Errors.Add("angle too big");
                }

                if (Math.Abs(BE - BS) >= Math.PI * 2)
                {
                    t.Errors.Add("angle too big");
                }
                //while (AS < 0 || AE < 0) { AS += Math.PI * 2; AE += Math.PI * 2; };
                //while (BS < 0 || BE < 0) { BS += Math.PI * 2; BE += Math.PI * 2; };
                //if (Math.Abs(BE - BS) > Math.PI)
                //{
                //    double Dif = BE - BS;

                //    BE = Math.PI * 2 - Dif;

                //}
                //if (Math.Abs(AE - AS) > Math.PI)
                //{
                //    double Dif = AE - AS;
                //    AE = Math.PI * 2 - Dif;
                //    //AE -= Math.PI * 2;
                //}
                //  while (BE < BS) BE += Math.PI * 2;


                List<PointD> Arc1 = new List<PointD>();
                List<PointD> Arc2 = new List<PointD>();
                Arc1.Add(PStart);
                Arc2.Add(PEnd);
                for (double T = 0; T <= 1; T += 1.0 / 20)
                {
                    double P = AS + (AE - AS) * T;
                    PointD newP = C1 + new PointD(-Math.Cos(P) * drillradius, -Math.Sin(P) * drillradius);
                    Arc1.Add(newP);

                    double P2 = BS + (BE - BS) * T;
                    PointD newP2 = C2 + new PointD(-Math.Cos(P2) * drillradius, -Math.Sin(P2) * drillradius);
                    Arc2.Add(newP2);
                }
                //      Arc1.Add(A1.ToF());
                //      Arc2.Add(A2.ToF());

                PolyLine Middle = new PolyLine();
                Middle.Vertices.Add(A1);
                Middle.Vertices.Add(A2);

                PolyLine Combined = new PolyLine();
                //               GeneratedArcs.Add(PL);
                //                GeneratedArcs.Add(PL2);
                //             GeneratedArcs.Add(Middle);
                Arc2.Reverse();
                Combined.Vertices.AddRange(Arc1);
                Combined.Vertices.AddRange(Middle.Vertices);
                Combined.Vertices.AddRange(Arc2);

                GeneratedArcs.Add(Combined);

                //PolyLine N1 = new PolyLine();
                //N1.Vertices.Add(PStart);
                //N1.Vertices.Add(PStart + StartNorm *4);
                //GeneratedArcs.Add(N1);
                //PolyLine N2 = new PolyLine();
                //N2.Vertices.Add(PEnd);
                //N2.Vertices.Add(PEnd + EndNorm * 4);
                //GeneratedArcs.Add(N2);

                //  var P2 = PStart - StartDir * drillradius  + StartNorm * drillradius;
                //  var P3 = PEnd - EndDir * drillradius  + EndNorm * drillradius;

                //   GP.AddBezier(PStart.ToF(), P2.ToF(), P3.ToF(), PEnd.ToF());





            }

            return result;
        }

        public void RemoveAllTabs(bool errortabsonly = false)
        {
            if (errortabsonly)
            {
                var T = (from i in TheSet.Tabs where i.Errors.Count > 0 select i).ToList();
                if (T.Count() > 0)
                {
                    foreach (var bt in T)
                    {
                        TheSet.Tabs.Remove(bt);
                    }
                }
            }
            else
                TheSet.Tabs.Clear();
        }

        public void GenerateTabLocations()
        {
            RemoveAllTabs();
            foreach (var a in TheSet.Instances)
            {
                if (GerberOutlines.ContainsKey(a.GerberPath) && a.GerberPath.Contains("???") == false)
                {
                    var g = GerberOutlines[a.GerberPath];
                    var TabsLocs = PolyLineSet.FindOptimalBreaktTabLocations(g.TheGerber);

                    foreach (var b in TabsLocs)
                    {
                        PointD loc = b.Item1 - (b.Item2 * TheSet.MarginBetweenBoards * 0.5);
                        loc = loc.Rotate(a.Angle);
                        loc += new PointD(a.Center);
                        if (loc.X >= 0 && loc.X <= TheSet.Width && loc.Y >= 0 && loc.Y <= TheSet.Height)
                        {
                            var BT = AddTab(loc);
                            BT.Radius = (float)TheSet.MarginBetweenBoards + 2;
                        }
                    }
                }
            }
        }

        class CutUpLine
        {
            public List<List<PointD>> Lines = new List<List<PointD>>();

            public void AddVertex(PointD D)
            {
                if (Lines.Count == 0)
                {
                    Lines.Add(new List<PointD>());
                }
                Lines.Last().Add(D);
            }

            public void NewLine()
            {
                if (Lines.Count > 0 && Lines.Last().Count == 0) return;
                Lines.Add(new List<PointD>());

            }
        }

        List<CutUpLine> CutLines = new List<CutUpLine>();
        class LineSeg
        {
            public PointD PStart = new PointD();
            public PointD PEnd = new PointD();
            public override string ToString()
            {
                return PStart.ToString() + " - " + PEnd.ToString() + String.Format(" ({0}mm)", (PEnd - PStart).Length());
            }
        }

        List<LineSeg> FinalSegs = new List<LineSeg>();

        void CutUpOutlines()
        {
            CutLines.Clear();
            FinalSegs.Clear();

            foreach (var b in TheSet.Instances)
            {
                // Polygons clips = new Polygons();
                if (GerberOutlines.ContainsKey(b.GerberPath))
                {
                    var a = GerberOutlines[b.GerberPath];
                    foreach (var c in b.TransformedOutlines)
                    {
                        //  PolyLine PL = new PolyLine();
                        //  PL.FillTransformed(c, new PointD(b.Center), b.Angle);
                        SplitPolyLineAndAddSegs(c);

                    }
                }
            }
            foreach (var b in CombinedOutline)
            {
                //   SplitPolyLineAndAddSegs(b);

            }
            foreach (var a in FinalSegs)
            {
                PolyLine PL = new PolyLine();

                PL.Vertices.Add(a.PStart);
                PL.Vertices.Add(a.PEnd);
                GeneratedArcs.Add(PL);
            }

        }

        private void SplitPolyLineAndAddSegs(PolyLine PL)
        {
            int adjust = 0;
            if (PL.Vertices.First() == PL.Vertices.Last()) adjust = 1;
            List<LineSeg> Segs = new List<LineSeg>(Math.Max(1, PL.Vertices.Count - adjust + 100));
            for (int i = 0; i < PL.Vertices.Count - adjust; i++)
            {
                Segs.Add(new LineSeg() { PStart = PL.Vertices[i].Copy(), PEnd = PL.Vertices[(i + 1) % PL.Vertices.Count].Copy() });
            }

            foreach (var t in TheSet.Tabs.Where(x => x.Errors.Count == 0))
            {
                PointD center = new PointD(t.Center);
                List<LineSeg> ToDelete = new List<LineSeg>();
                List<LineSeg> ToAdd = new List<LineSeg>();
                foreach (var s in Segs)
                {
                    bool StartInside = Helpers.Distance(s.PStart, center) < t.Radius;
                    bool EndInside = Helpers.Distance(s.PEnd, center) < t.Radius;
                    if (StartInside && EndInside)
                    {
                        ToDelete.Add(s);
                    }
                    else
                    {
                        PointD I1;
                        PointD I2;
                        int ints = Helpers.FindLineCircleIntersections(center.X, center.Y, t.Radius, s.PStart, s.PEnd, out I1, out I2);
                        switch (ints)
                        {
                            case 0: // skip;
                                break;
                            case 1: // adjust 1 point
                                if (StartInside)
                                {
                                    s.PStart = I1.Copy();
                                }
                                else
                                {
                                    s.PEnd = I1.Copy();
                                }
                                break;
                            case 2: // split and add 1
                                LineSeg NS1 = new LineSeg();
                                LineSeg NS2 = new LineSeg();
                                NS1.PEnd = s.PEnd.Copy();
                                NS1.PStart = I1.Copy();
                                NS2.PEnd = s.PStart.Copy();
                                NS2.PStart = I2.Copy();
                                ToAdd.Add(NS1);
                                ToAdd.Add(NS2);
                                ToDelete.Add(s);
                                break;

                        }
                    }
                }
                foreach (var s in ToDelete)
                {
                    Segs.Remove(s);
                }
                foreach (var s in ToAdd)
                {
                    Segs.Add(s);
                }
            }
            FinalSegs.AddRange(Segs);
        }

        private void FindOutlineIntersections()
        {
            GeneratedArcs.Clear();
            DrillHoles.Clear();
            CutLines.Clear();
            foreach (var t in TheSet.Tabs)
            {
                t.EvenOdd = 0;
                t.Errors.Clear();
                List<TabIntersection> Intersections = new List<TabIntersection>();
                float R2 = t.Radius * t.Radius;
                foreach (var b in TheSet.Instances)
                {
                    // Polygons clips = new Polygons();
                    // if (b.GerberPath.Contains("???") == false)
                    {
                        var a = GerberOutlines[b.GerberPath];
                        var C = new PointD(t.Center);
                        C.X -= b.Center.X;
                        C.Y -= b.Center.Y;
                        C = C.Rotate(-b.Angle);
                        var Box2 = a.TheGerber.BoundingBox.Grow(t.Radius * 2);

                        if (Box2.Contains(C))
                        {
                            //Console.WriteLine("{0},{1}", a.TheGerber.BoundingBox, C);

                            for (int i = 0; i < b.TransformedOutlines.Count; i++)
                            {
                                var c = b.TransformedOutlines[i];

                                //  var poly = c.toPolygon();
                                // bool winding = Clipper.Orientation(poly);


                                //  PolyLine PL = new PolyLine();
                                // PL.FillTransformed(c, new PointD(b.Center), b.Angle);

                                if (Helpers.IsInPolygon(c.Vertices, new PointD(t.Center), false))
                                {
                                    t.EvenOdd++;
                                    // t.Errors.Add("inside a polygon!");
                                    //  t.Valid = false;
                                }

                                BuildDrillsForTabAndPolyLine(t, c, b.OffsetOutlines[i]);

                                //bool inside = false;
                                //bool newinside = false;
                                // List<PolyLine> Lines = new List<PolyLine>();

                                //PolyLine Current = null;
                                if (AddIntersectionsForTabAndPolyLine(t, Intersections, c))
                                {
                                    b.Tabs.Add(t);
                                }

                            }
                        }
                    }
                }
                if (t.EvenOdd % 2 == 1)
                {
                    t.Errors.Add("inside a polygon!");
                    t.Valid = false;

                }
                foreach (var a in CombinedOutline)
                {
                    AddIntersectionsForTabAndPolyLine(t, Intersections, a);

                }
                bool Succes = GenerateTabArcs(Intersections, t); ;

            }
            CutUpOutlines();
            CombineGeneratedArcsAndCutlines();
        }

        private static bool AddIntersectionsForTabAndPolyLine(BreakTab t, List<TabIntersection> Intersections, PolyLine PL)
        {
            //   Polygons clips = new Polygons();
            //var poly = PL.toPolygon();
            //bool winding = Clipper.Orientation(poly);
            bool ret = false;
            for (int i = 0; i < PL.Vertices.Count; i++)
            {
                PointD V1 = PL.Vertices[i];
                PointD V2 = PL.Vertices[(i + 1) % PL.Vertices.Count];

                // if (winding == false)
                // {
                //    var V3 = V1;
                //   V1 = V2;
                //   V2 = V3;
                // }

                PointD I1 = new PointD();
                PointD I2 = new PointD();
                double Len = Helpers.Distance(V1, V2);
                int ints = Helpers.FindLineCircleIntersections(t.Center.X, t.Center.Y, t.Radius, V1, V2, out I1, out I2);
                if (ints > 0)
                {
                    ret = true;
                    if (ints == 1)
                    {
                        TabIntersection TI = new TabIntersection();
                        TI.Location = I1;
                        TI.Direction.X = (V1.X - V2.X) / Len;
                        TI.Direction.Y = (V1.Y - V2.Y) / Len;
                        TI.Angle = Helpers.AngleBetween(new PointD(t.Center), I1);
                        bool addedV1 = false;

                        bool V1Inside = Helpers.Distance(new PointD(t.Center.X, t.Center.Y), V1) < t.Radius;
                        bool V2Inside = Helpers.Distance(new PointD(t.Center.X, t.Center.Y), V2) < t.Radius;

                        if (Helpers.Distance(new PointD(t.Center.X, t.Center.Y), V1) < t.Radius)
                        {
                            addedV1 = true;
                            TI.Start = true;
                        }

                        if (Helpers.Distance(new PointD(t.Center.X, t.Center.Y), V2) < t.Radius)
                        {
                            TI.Start = false;
                        }

                        Intersections.Add(TI);
                    }

                    if (ints == 2)
                    {
                        TabIntersection TI1 = new TabIntersection();
                        TI1.Start = true;
                        TI1.Location = I1;
                        TI1.Angle = Helpers.AngleBetween(new PointD(t.Center), I1);
                        TI1.Direction.X = (V1.X - V2.X) / Len;
                        TI1.Direction.Y = (V1.Y - V2.Y) / Len;
                        Intersections.Add(TI1);

                        TabIntersection TI2 = new TabIntersection();
                        TI2.Start = false;
                        TI2.Location = I2;
                        TI2.Direction.X = TI1.Direction.X;
                        TI2.Direction.Y = TI1.Direction.Y;
                        TI2.Angle = Helpers.AngleBetween(new PointD(t.Center), I2);
                        Intersections.Add(TI2);

                    }

                }


            }
            return ret;
        }

        private void BuildDrillsForTabAndPolyLine(BreakTab t, PolyLine PL, List<PolyLine> Offsetted)
        {

            foreach (var sub in Offsetted)
            {
                //  PolyLine sub = new PolyLine();
                //    sub.fromPolygon(subpoly);
                double Len = 0;
                PointD last = sub.Vertices.Last();
                for (int i = 0; i < sub.Vertices.Count; i++)
                {
                    double dx = sub.Vertices[i].X - last.X;
                    double dy = sub.Vertices[i].Y - last.Y;
                    double seglen = Math.Sqrt(dx * dx + dy * dy);
                    if (Len + seglen < 1)
                    {
                        Len += seglen;
                    }
                    else
                    {
                        double ndx = dx / seglen;
                        double ndy = dy / seglen;
                        double left = 1 - Len;
                        double had = left;
                        PointD place = new PointD(last.X + ndx * had, last.Y + ndy * had);
                        if (Helpers.Distance(place, new PointD(t.Center.X, t.Center.Y)) < t.Radius)
                        {
                            DrillHoles.Add(place);
                        }
                        seglen -= left;
                        while (seglen > 0)
                        {
                            double amt = Math.Min(1, seglen);
                            seglen -= amt;
                            had += amt;
                            if (amt == 1)
                            {
                                place = new PointD(last.X + ndx * had, last.Y + ndy * had);
                                if (Helpers.Distance(place, new PointD(t.Center.X, t.Center.Y)) < t.Radius)
                                {
                                    DrillHoles.Add(place);
                                }
                                //    DrillHoles.Add(new PointD(last.X + ndx * had, last.Y + ndy * had));
                            }
                            else
                            {
                                Len = amt;
                            }
                        }
                    }
                    last = sub.Vertices[i];
                }
            }

        }

        List<PathDefWithClosed> FinalPolygonsWithTabs = new List<PathDefWithClosed>();

        private void CombineGeneratedArcsAndCutlines()
        {
            List<PathDefWithClosed> SourceLines = new List<PathDefWithClosed>();
            foreach (var a in GeneratedArcs)
            {
                SourceLines.Add(new PathDefWithClosed() { Vertices = a.Vertices, Width = 0 });
            }
            foreach (var cl in CutLines)
            {
                foreach (var l in cl.Lines)
                {
                    SourceLines.Add(new PathDefWithClosed() { Vertices = l, Width = 0 });
                }
            }
            FinalPolygonsWithTabs = Helpers.LineSegmentsToPolygons(SourceLines, true);
        }

        public List<string> SaveOutlineTo(string p, string combinedfilename)
        {
            List<string> R = new List<string>();
            string DrillFile = Path.Combine(p, "tabdrills.TXT");
            string OutlineFile = Path.Combine(p, combinedfilename + ".GKO");
            R.Add(DrillFile);
            //  R.Add(OutlineFile);
            ExcellonFile EF = new ExcellonFile();
            EF.Tools[1] = new ExcellonTool() { ID = 1, Radius = 0.25 };
            foreach (var a in DrillHoles)
            {
                EF.Tools[1].Drills.Add(a);
            }
            EF.Write(Path.Combine(p, DrillFile), 0, 0, 0, 0);

            GerberOutlineWriter GOW = new GerberOutlineWriter();
            foreach (var a in FinalPolygonsWithTabs)
            {
                PolyLine PL = new PolyLine();
                PL.Vertices = a.Vertices;
                // todo: check if closed/opened things need special treatment here. 
                // width is defaulted to 0
                GOW.AddPolyLine(PL);
            }
            GOW.Write(Path.Combine(p, OutlineFile));

            return R;
        }

        public List<String> SaveGerbersToFolder(string BaseName, string targetfolder, ProgressLog Logger, bool SaveOutline = true, bool GenerateImages = true, bool DeleteGenerated = true, string combinedfilename = "combined")
        {
            Logger.AddString("Starting export to " + targetfolder);
            List<string> GeneratedFiles = TheSet.SaveTo(targetfolder, GerberOutlines, Logger);
            List<String> FinalFiles = new List<string>();

            if (SaveOutline)
            {
                GeneratedFiles.AddRange(SaveOutlineTo(targetfolder, combinedfilename));
                FinalFiles.Add(Path.Combine(targetfolder, combinedfilename + ".gko"));
            }

            // TODO: use the new Gerber.DetermineFile to actually group based on layer/type instead of extentions only!

            Dictionary<string, List<string>> FilesPerExt = new Dictionary<string, List<string>>();
            Dictionary<string, BoardFileType> FileTypePerExt = new Dictionary<string, BoardFileType>();
            foreach (var s in GeneratedFiles)
            {
                string ext = Path.GetExtension(s).ToLower(); ;
                if (ext == "xln") ext = "txt";
                if (FilesPerExt.ContainsKey(ext) == false)
                {
                    FilesPerExt[ext] = new List<string>();
                }

                FileTypePerExt[ext] = Gerber.FindFileType(s);
                FilesPerExt[ext].Add(s);
            }
            int count = 0;
            foreach (var a in FilesPerExt)
            {
                count++;
                Logger.AddString("merging *" + a.Key.ToLower(), ((float)count / (float)FilesPerExt.Keys.Count) * 0.5f + 0.3f);
                switch (FileTypePerExt[a.Key])
                {
                    case BoardFileType.Drill:
                        {
                            string Filename = Path.Combine(targetfolder, combinedfilename + a.Key);
                            FinalFiles.Add(Filename);
                            ExcellonFile.MergeAll(a.Value, Filename, Logger);
                        }
                        break;
                    case BoardFileType.Gerber:
                        {
                            if (a.Key.ToLower() != ".gko")
                            {
                                string Filename = Path.Combine(targetfolder, combinedfilename + a.Key);
                                FinalFiles.Add(Filename);
                                GerberMerger.MergeAll(a.Value, Filename, Logger);
                            }
                        }
                        break;
                }
            }

            //Logger.AddString("Writing source material zipfile", 0.80f);
            //string SeparateZipFile = Path.Combine(targetfolder, BaseName + ".separate_boards.zip");
            //if (File.Exists(SeparateZipFile)) File.Delete(SeparateZipFile);
            //ZipArchive zip = ZipFile.Open(SeparateZipFile, ZipArchiveMode.Create);
            //foreach (string file in GeneratedFiles)
            //{
            //    zip.CreateEntryFromFile(file, Path.GetFileName(file), CompressionLevel.Optimal);
            //}
            //zip.Dispose();

            //Logger.AddString("Writing combined zipfile", 0.85f);

            //string CombinedZipFile = Path.Combine(targetfolder, BaseName + ".combined_boards.zip");
            //if (File.Exists(CombinedZipFile)) File.Delete(CombinedZipFile);
            //ZipArchive zip2 = ZipFile.Open(CombinedZipFile, ZipArchiveMode.Create);
            //foreach (string file in FinalFiles)
            //{
            //    zip2.CreateEntryFromFile(file, Path.GetFileName(file), CompressionLevel.Optimal);
            //}
            //zip2.Dispose();

            Logger.AddString("Deleting tempfiles", 0.9f);

            if (DeleteGenerated)
            {
                foreach (var a in GeneratedFiles)
                {
                    File.Delete(a);
                }
            }

            if (GenerateImages)
            {
                try
                {
                    Logger.AddString("Writing board bitmaps", 0.95f);
                    GerberImageCreator GIC = new GerberImageCreator();
                    GIC.AddBoardsToSet(FinalFiles);

                    GIC.WriteImageFiles(Path.Combine(targetfolder, BaseName), 400, Gerber.DirectlyShowGeneratedBoardImages, Logger);
                }
                catch (Exception E)
                {
                    Logger.AddString("Some errors while exporting board images.. but this should be no problem?");
                    Logger.AddString(String.Format("The exception: {0}", E.Message));
                }
            }
            Logger.AddString("Done", 1);

            return GeneratedFiles;
        }

        public void SaveFile(string FileName)
        {
            try
            {

                XmlSerializer SerializerObj = new XmlSerializer(typeof(GerberLayoutSet));

                // Create a new file stream to write the serialized object to a file
                TextWriter WriteFileStream = new StreamWriter(FileName);
                SerializerObj.Serialize(WriteFileStream, TheSet);

                // Cleanup
                WriteFileStream.Close();


            }
            catch (Exception)
            {
            }
        }

        public void RemoveInstance(AngledThing angledThing)
        {
            if (angledThing.GetType() == typeof(GerberInstance))
            {
                TheSet.Instances.Remove(angledThing as GerberInstance);


            }
            else
            {
                if (angledThing.GetType() == typeof(BreakTab))
                {
                    TheSet.Tabs.Remove(angledThing as BreakTab);
                }
            }

        }

        public GerberInstance AddInstance(string path, PointD coord, bool generateTransformed = false)
        {
            GerberInstance GI = new GerberInstance() { GerberPath = path, Center = coord.ToF() };
            TheSet.Instances.Add(GI);
            if (generateTransformed)
            {
                if (GerberOutlines.ContainsKey(path))
                {
                    var GO = GerberOutlines[path];
                    foreach (var b in GO.TheGerber.OutlineShapes)
                    {
                        PolyLine PL = new PolyLine();
                        PL.FillTransformed(b, new PointD(GI.Center), GI.Angle);
                        GI.TransformedOutlines.Add(PL);

                    }
                    GI.CreateOffsetLines(TheSet.ExtraTabDrillDistance);
                }
            }
            return GI;
        }

        public void LoadFile(string filename)
        {
            XmlSerializer SerializerObj = new XmlSerializer(typeof(GerberLayoutSet));
            FileStream ReadFileStream = null;
            try
            {
                ReadFileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);

                // Load the object saved above by using the Deserialize function
                GerberLayoutSet newset = (GerberLayoutSet)SerializerObj.Deserialize(ReadFileStream);
                if (newset != null)
                {
                    foreach (var a in newset.LoadedOutlines)
                    {
                        AddGerberFolder(a, false);
                    }
                    TheSet = newset;
                }
            }
            catch (Exception)
            {
            }
            // Cleanup
            if (ReadFileStream != null) ReadFileStream.Close();
        }

        public AngledThing FindOutlineUnderPoint(PointD pt)
        {
            foreach (var b in TheSet.Tabs)
            {
                if (Helpers.Distance(new PointD(b.Center), pt) <= b.Radius)
                {
                    return b;
                }
            }

            foreach (var b in TheSet.Instances)
            {
                // Polygons clips = new Polygons();
                if (GerberOutlines.ContainsKey(b.GerberPath))
                {
                    var a = GerberOutlines[b.GerberPath];
                    int cc = 0;
                    foreach (var c in b.TransformedOutlines)
                    {


                        if (Helpers.IsInPolygon(c.Vertices, pt))
                        {
                            cc++;
                        }
                    }
                    if (cc % 2 == 1) return b;
                }
            }
            return null;
        }

        public void RectanglePack()
        {
            RectanglePacker RP = new RectanglePacker(TheSet.Width + TheSet.MarginBetweenBoards, TheSet.Height + TheSet.MarginBetweenBoards);

            List<Tuple<GerberInstance, double>> Instances = new List<Tuple<GerberInstance, double>>();
            foreach (var a in TheSet.Instances)
            {
                if (GerberOutlines.ContainsKey(a.GerberPath))
                {
                    var OL = GerberOutlines[a.GerberPath];
                    var Width = OL.TheGerber.BoundingBox.BottomRight.X - OL.TheGerber.BoundingBox.TopLeft.X;
                    var Height = OL.TheGerber.BoundingBox.BottomRight.Y - OL.TheGerber.BoundingBox.TopLeft.Y;

                    Instances.Add(new Tuple<GerberInstance, double>(a, Math.Max(Width, Height)));
                }
            }
            foreach (var a in Instances.OrderByDescending(x => x.Item2))
            {
                if (GerberOutlines.ContainsKey(a.Item1.GerberPath))
                {
                    var OL = GerberOutlines[a.Item1.GerberPath];
                    RectangleD RD = new RectangleD();
                    RD.Width = OL.TheGerber.BoundingBox.BottomRight.X - OL.TheGerber.BoundingBox.TopLeft.X;
                    RD.Height = OL.TheGerber.BoundingBox.BottomRight.Y - OL.TheGerber.BoundingBox.TopLeft.Y;

                    var Coord = RP.findCoords(RD.Width + TheSet.MarginBetweenBoards, RD.Height + TheSet.MarginBetweenBoards);

                    //     Console.WriteLine("empty space: {0}", RP.GetEmptySpace());
                    if (Coord != null)
                    {
                        a.Item1.Angle = 0;

                        a.Item1.Center = (Coord - OL.TheGerber.BoundingBox.TopLeft).ToF();
                        // a.Item1.Center.X += 1;
                        // a.Item1.Center.Y += 1;
                    }

                    else
                    {
                        Coord = RP.findCoords(RD.Height + TheSet.MarginBetweenBoards, RD.Width + TheSet.MarginBetweenBoards);
                        //     Console.WriteLine("empty space: {0}", RP.GetEmptySpace());
                        if (Coord != null)
                        {
                            a.Item1.Angle = 90;
                            a.Item1.Center = (Coord - OL.TheGerber.BoundingBox.TopLeft).ToF();
                            a.Item1.Center.Y += (float)RD.Width;

                            // a.Item1.Center.X += 1;
                            // a.Item1.Center.Y += 1;
                        }

                        else
                        {

                            a.Item1.Center.X = -20;
                            a.Item1.Center.Y = -20;
                        }
                    }
                }
            }
        }

        Random R = new Random();

        public bool MaxRectPack(MaxRectPacker.FreeRectChoiceHeuristic strategy = MaxRectPacker.FreeRectChoiceHeuristic.RectBestAreaFit, double randomness = 0, bool allowrotation = true)
        {
            bool Succes = true;
            //allowrotation = true;
            MaxRectPacker MRP = new MaxRectPacker((int)(TheSet.Width + TheSet.MarginBetweenBoards), (int)(TheSet.Height + TheSet.MarginBetweenBoards), allowrotation); // mm is base unit for packing!

            List<Tuple<GerberInstance, double>> Instances = new List<Tuple<GerberInstance, double>>();
            foreach (var a in TheSet.Instances)
            {
                if (GerberOutlines.ContainsKey(a.GerberPath))
                {
                    var OL = GerberOutlines[a.GerberPath];
                    var Width = OL.TheGerber.BoundingBox.BottomRight.X - OL.TheGerber.BoundingBox.TopLeft.X;
                    var Height = OL.TheGerber.BoundingBox.BottomRight.Y - OL.TheGerber.BoundingBox.TopLeft.Y;

                    Instances.Add(new Tuple<GerberInstance, double>(a, Math.Max(Width, Height) * (1 - randomness) + randomness * R.NextDouble()));
                }
            }
            List<MaxRectPacker.Rect> InputRects = new List<MaxRectPacker.Rect>();
            List<MaxRectPacker.Rect> OutputRects = new List<MaxRectPacker.Rect>();

            foreach (var a in Instances.OrderByDescending(x => x.Item2))
            {
                if (GerberOutlines.ContainsKey(a.Item1.GerberPath))
                {
                    var OL = GerberOutlines[a.Item1.GerberPath];
                    MaxRectPacker.Rect RD = new MaxRectPacker.Rect() { refobject = a };
                    RD.width = (int)(Math.Ceiling(OL.TheGerber.BoundingBox.BottomRight.X - OL.TheGerber.BoundingBox.TopLeft.X));
                    RD.height = (int)(Math.Ceiling(OL.TheGerber.BoundingBox.BottomRight.Y - OL.TheGerber.BoundingBox.TopLeft.Y));

                    InputRects.Add(RD);

                    var R = MRP.Insert((int)(RD.width + TheSet.MarginBetweenBoards), (int)(RD.height + TheSet.MarginBetweenBoards), strategy);
                    if (R.height == 0)
                    {
                        //   Console.WriteLine("{0} not packed - too big!", a.Item1.GerberPath);
                        a.Item1.Center.X = -20;
                        a.Item1.Center.Y = -20;
                        return false;
                    }
                    else
                    {

                        if (R.height == (int)(RD.height + TheSet.MarginBetweenBoards))
                        {
                            a.Item1.Center = (new PointD(R.x, R.y) - OL.TheGerber.BoundingBox.TopLeft).ToF();

                            a.Item1.Angle = 0;
                            // regular
                        }
                        else
                        {

                            a.Item1.Center = (new PointD(R.x, R.y)).ToF();// - OL.TheGerber.TopLeft).ToF();

                            a.Item1.Center.X += (float)OL.TheGerber.BoundingBox.TopLeft.Y;
                            a.Item1.Center.Y -= (float)OL.TheGerber.BoundingBox.TopLeft.X;

                            a.Item1.Angle = 90;
                            //                            a.Item1.Center.Y += RD.width;
                            a.Item1.Center.X += RD.height;
                        }
                        //           Console.WriteLine("{0},{1}  {2},{3}", R.x, R.y, R.width, R.height);

                    }
                }
            }


            //MaxRectPacker MRP2 = new MaxRectPacker((int)(TheSet.Width + 2), (int)(TheSet.Height + 2), false); // mm is base unit for packing!
            //MRP2.Insert(InputRects, OutputRects, MaxRectPacker.FreeRectChoiceHeuristic.RectBestAreaFit);

            //foreach(var a in OutputRects)
            //{
            //    Console.WriteLine("{0}", a.refobject as GerberInstance);
            //}
            return Succes;
        }

        public BreakTab AddTab(PointD center)
        {

            BreakTab BT = new BreakTab() { Radius = 3, Center = center.ToF() };
            TheSet.Tabs.Add(BT);

            return BT;

        }

        public void MergeOverlappingTabs()
        {
            RemoveAllTabs(true);
            List<bool> Tabs = new List<bool>();
            List<BreakTab> Removethese = new List<BreakTab>();
            for (int i = 0; i < TheSet.Tabs.Count; i++)
            {
                Tabs.Add(false);
            }
            for (int i = 0; i < TheSet.Tabs.Count; i++)
            {
                PointD A = new PointD(TheSet.Tabs[i].Center);

                for (int j = i + 1; j < TheSet.Tabs.Count; j++)
                {
                    if (Tabs[j] == false)
                    {
                        PointD B = new PointD(TheSet.Tabs[j].Center);
                        if (PointD.Distance(A, B) < (TheSet.Tabs[j].Radius + TheSet.Tabs[i].Radius) * 0.75)
                        {
                            Tabs[j] = true;
                            TheSet.Tabs[i].Center = ((A + B) * 0.5).ToF();
                            Removethese.Add(TheSet.Tabs[j]);
                        }
                    }
                }
            }

            foreach (var L in Removethese)
            {
                TheSet.Tabs.Remove(L);
            }

        }
    }

    public class AngledThing
    {
        public PointF Center = new PointF(); // float for serializer... need to investigate
        public float Angle;
    }

    public class GerberInstance : AngledThing
    {
        public string GerberPath;
        public bool Generated = false;

        [System.Xml.Serialization.XmlIgnore]
        public List<PolyLine> TransformedOutlines = new List<PolyLine>();

        [System.Xml.Serialization.XmlIgnore]
        public List<List<PolyLine>> OffsetOutlines = new List<List<PolyLine>>();

        [System.Xml.Serialization.XmlIgnore]
        internal float LastAngle;
        [System.Xml.Serialization.XmlIgnore]
        internal PointD LastCenter;

        [System.Xml.Serialization.XmlIgnore]
        public List<BreakTab> Tabs = new List<BreakTab>();

        [System.Xml.Serialization.XmlIgnore]
        public PolyLineSet.Bounds BoundingBox = new PolyLineSet.Bounds();
        internal void CreateOffsetLines(double extradrilldistance)
        {
            OffsetOutlines = new List<List<PolyLine>>(TransformedOutlines.Count);
            for (int i = 0; i < TransformedOutlines.Count; i++)
            {
                var L = new List<PolyLine>();
                Polygons clips = new Polygons();
                var poly = TransformedOutlines[i].toPolygon();
                bool winding = Clipper.Orientation(poly);

                clips.Add(poly);
                double offset = 0.25 * 100000.0f + extradrilldistance;
                if (winding == false) offset *= -1;
                Polygons clips2 = Clipper.OffsetPolygons(clips, offset, JoinType.jtRound);
                foreach (var a in clips2)
                {
                    PolyLine P = new PolyLine();
                    P.fromPolygon(a);
                    L.Add(P);
                }

                OffsetOutlines.Add(L);
            }

        }

        public void RebuildTransformed(GerberOutline gerberOutline, double extra)
        {
            BoundingBox.Reset();
            LastAngle = Angle;
            LastCenter = new PointD(Center.X, Center.Y);
            TransformedOutlines = new List<PolyLine>();
            var GO = gerberOutline;
            foreach (var b in GO.TheGerber.OutlineShapes)
            {
                PolyLine PL = new PolyLine();
                PL.FillTransformed(b, new PointD(Center), Angle);
                TransformedOutlines.Add(PL);
                BoundingBox.AddPolyLine(PL);
            }
            CreateOffsetLines(extra);



        }
    }

    public class BreakTab : AngledThing
    {
        public float Radius;
        public bool Valid;

        [System.Xml.Serialization.XmlIgnore]
        public List<string> Errors = new List<string>();

        [System.Xml.Serialization.XmlIgnore]
        public int EvenOdd;

    }

    public class GerberLayoutSet
    {
        public List<string> LoadedOutlines = new List<string>();
        public List<GerberInstance> Instances = new List<GerberInstance>();
        public List<BreakTab> Tabs = new List<BreakTab>();

        public double Width = 100;
        public double Height = 100;
        public double MarginBetweenBoards = 2;
        public bool ConstructNegativePolygon = false;
        public double FillOffset = 3;
        public double Smoothing = 1;
        public double ExtraTabDrillDistance = 0;
        public bool ClipToOutlines = true;
        public string LastExportFolder = "";

        public bool DoNotGenerateMouseBites { get; set; }

        public List<string> SaveTo(string p, Dictionary<string, GerberOutline> GerberOutlines, ProgressLog Logger)
        {
            LastExportFolder = p;

            List<string> GeneratedFiles = new List<string>();
            List<String> UnzippedList = new List<string>();

            int instanceID = 1;
            int current = 0;
            foreach (var a in Instances)
            {
                current++;
                Logger.AddString("writing " + a.GerberPath, ((float)current / (float)Instances.Count) * 0.3f);
                if (a.GerberPath.Contains("???") == false)
                {
                    var outline = GerberOutlines[a.GerberPath];
                    List<String> FileList = new List<string>();

                    if (Directory.Exists(a.GerberPath))
                    {
                        FileList = Directory.GetFiles(a.GerberPath).ToList();
                    }
                    else
                    {
                        if (File.Exists(a.GerberPath) && (Path.GetExtension(a.GerberPath).ToLower() == ".zip" || Path.GetExtension(a.GerberPath).ToLower() == "zip"))
                        {
                            string BaseUnzip = Path.Combine(p, "unzipped");
                            if (Directory.Exists(BaseUnzip) == false)
                            {
                                Directory.CreateDirectory(BaseUnzip);
                            }
                            using (Ionic.Zip.ZipFile zip1 = Ionic.Zip.ZipFile.Read(a.GerberPath))
                            {
                                foreach (ZipEntry e in zip1)
                                {
                                    if (e.IsDirectory == false)
                                    {
                                        string Unzipped = Path.Combine(BaseUnzip, (instanceID++).ToString() + "_" + Path.GetFileName(e.FileName));
                                        if (File.Exists(Unzipped)) File.Delete(Unzipped);
                                        FileStream FS = new FileStream(Unzipped, FileMode.CreateNew);
                                        FileList.Add(Unzipped);
                                        UnzippedList.Add(Unzipped);
                                        e.Extract(FS);
                                        FS.Close();




                                    }
                                }
                            }
                        }
                    }

                    instanceID = AddFilesForInstance(p,a.Center.X, a.Center.Y, a.Angle, FileList, instanceID, GeneratedFiles, outline, Logger);


                    instanceID++;
                }
            }
            foreach (var a in UnzippedList)
            {
                try
                {
                    File.Delete(a);
                }
                catch (Exception)
                {
                    Logger.AddString(String.Format("warning: {0} not deleted: it was locked?", a));
                }
            }
            return GeneratedFiles;
        }

        private int AddFilesForInstance(string p, double x, double y, double angle, List<string> FileList, int isntid, List<string> GeneratedFiles, GerberOutline outline, ProgressLog Logger)
        {

            GerberImageCreator GIC = new GerberImageCreator();
            GIC.AddBoardsToSet(FileList);



            foreach (var f in FileList)
            {
                var FileType = Gerber.FindFileType(f);

                switch (FileType)
                {
                    case BoardFileType.Drill:

                        try
                        {
                            double scaler = GIC.GetDrillScaler(f);
                            ExcellonFile EF = new ExcellonFile();
                            EF.Load(f, scaler);
                            string Filename = Path.Combine(p, (isntid++).ToString() + "_" + Path.GetFileName(f));
                            EF.Write(Filename, x,y, outline.TheGerber.TranslationSinceLoad.X, outline.TheGerber.TranslationSinceLoad.Y, angle);
                            GeneratedFiles.Add(Filename);
                        }
                        catch (Exception E)
                        {
                            while (E != null)
                            {
                                Logger.AddString("Exception: " + E.Message);
                                E = E.InnerException;
                            }
                        }
                        break;
                    case BoardFileType.Gerber:

                        try
                        {
                            string ext = Path.GetExtension(f).ToLower();
                            string Filename = Path.Combine(p, (isntid++).ToString() + "_" + Path.GetFileName(f));
                            string sourcefile = f;
                            string tempfile = "";
                            if (ClipToOutlines)
                            {
                                BoardSide Side = BoardSide.Unknown;
                                BoardLayer Layer = BoardLayer.Unknown;
                                Gerber.DetermineBoardSideAndLayer(f, out Side, out Layer);
                                if (Layer == BoardLayer.Silk)
                                {
                                    tempfile = Path.Combine(p, (isntid++).ToString() + "_" + Path.GetFileName(f));
                                    GerberImageCreator GIC2 = new GerberImageCreator();
                                    GIC2.AddBoardsToSet(FileList);
                                    GIC2.ClipBoard(f, tempfile,Logger);
                                    sourcefile = tempfile;
                                }
                            }
                            GerberTransposer.Transform(sourcefile, Filename, x, y, outline.TheGerber.TranslationSinceLoad.X, outline.TheGerber.TranslationSinceLoad.Y, angle);
                            GeneratedFiles.Add(Filename);
                            if (tempfile.Length > 0) File.Delete(tempfile);
                        }
                        catch (Exception E)
                        {
                            while (E != null)
                            {
                                Logger.AddString("Exception: " + E.Message);
                                E = E.InnerException;
                            }
                        }
                        break;

                }

            }
            return isntid;
        }

        internal void ClearTransformedOutlines()
        {
            foreach (var a in Instances)
            {

                a.TransformedOutlines = new List<PolyLine>();

            }

        }
    }

    public class GerberOutline
    {

        public ParsedGerber TheGerber;
        public GerberOutline(string filename)
        {
            if (filename.Length > 0)
            {
                TheGerber = PolyLineSet.LoadGerberFile(filename, true, false, new GerberParserState() { PreCombinePolygons = false });
                TheGerber.FixPolygonWindings();
                foreach (var a in TheGerber.OutlineShapes)
                {
                    a.CheckIfHole();
                }
            }
            else
            {
                TheGerber = new ParsedGerber();
            }
        }

        public GerberOutline(StreamReader sr, string originalfilename)
        {

            TheGerber = PolyLineSet.LoadGerberFileFromStream(sr, originalfilename, true, false, new GerberParserState() { PreCombinePolygons = false });
            TheGerber.FixPolygonWindings();
            foreach (var a in TheGerber.OutlineShapes)
            {
                a.CheckIfHole();
            }

        }

        public PointD GetActualCenter()
        {
            return TheGerber.BoundingBox.Middle();

        }

        internal void BuildShapeCache()
        {
            TheGerber.BuildShapeCache();
        }
    }
}