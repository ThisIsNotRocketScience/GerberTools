using GerberLibrary.Core.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TriangleNet;
using TriangleNet.Geometry;

namespace AutoPanelBuilder
{
    public class AutoPanelBuilder
    {
        public class PanelSettings
        {
            public double Width = 300;
            public double Height = 200;
            public double MarginBetweenBoards = 3;
            public bool ConstructNegativePolygon = true;
            public double FillOffset = 3;
            public double Smoothing = 3;
            public double ExtraTabDrillDistance = 0;
        }

        enum Arguments
        {
            SettingsFile,
            InputList,
            None
        }

        static void Main(string[] args)
        {
            GerberLibrary.GerberPanel GP = new GerberLibrary.GerberPanel();
            PanelSettings S = new PanelSettings();
           
            string SettingsFile = "";
            string InputFile = "";

            if (args.Count() < 2)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("AutoPanelBuilder.exe  [--settings {file}]");
                Console.WriteLine("\t[--files {filewithfolders}]");
                Console.WriteLine("\t[--dumpsample]");
                Console.WriteLine("\toutput_directory");

                return;
            }

            Arguments NextArg = Arguments.None;
            for (int i = 0; i < args.Count() - 1; i++)
            {
                switch (NextArg)
                {
                    case Arguments.SettingsFile: SettingsFile = args[i]; NextArg = Arguments.None; break;
                    case Arguments.InputList: InputFile = args[i]; NextArg = Arguments.None; break;

                    case Arguments.None:
                        switch (args[i].ToLower())
                        {
                            case "--settings": NextArg = Arguments.SettingsFile; break;
                            case "--files": NextArg = Arguments.InputList; break;
                            case "--dumpsample":
                                {
                                    XmlSerializer SerializerObj = new XmlSerializer(typeof(PanelSettings));
                                    TextWriter WriteFileStream = new StreamWriter("SettingDump.xml");
                                    SerializerObj.Serialize(WriteFileStream, S);
                                    WriteFileStream.Close();
                                }
                                break;
                        }
                        break;
                }
            }

            if (SettingsFile.Length > 0)
            {
                XmlSerializer SettingsSerialize = new XmlSerializer(typeof(PanelSettings));
                FileStream ReadFileStream = null;
                try
                {
                    ReadFileStream = new FileStream(SettingsFile, FileMode.Open, FileAccess.Read, FileShare.Read);

                    // Load the object saved above by using the Deserialize function
                    PanelSettings newset = (PanelSettings)SettingsSerialize.Deserialize(ReadFileStream);
                    if (newset != null)
                    {                        
                        S = newset;
                    }
                }
                catch (Exception)
                {
                }
            }

            if (InputFile.Length > 0)
            {
                foreach (var a in File.ReadAllLines(InputFile))
                {
                   var A = GP.AddGerberFolder(a);
                    GP.AddInstance(a, new GerberLibrary.Core.Primitives.PointD(0, 0));
                }
            }

            GP.TheSet.Width = S.Width;
            GP.TheSet.Height = S.Height;
            GP.TheSet.MarginBetweenBoards = S.MarginBetweenBoards;
            GP.TheSet.ConstructNegativePolygon = S.ConstructNegativePolygon;
            GP.TheSet.FillOffset = S.FillOffset;
            GP.TheSet.Smoothing= S.Smoothing;
            GP.TheSet.ExtraTabDrillDistance = S.ExtraTabDrillDistance;

            if (GP.MaxRectPack(GerberLibrary.MaxRectPacker.FreeRectChoiceHeuristic.RectBestAreaFit, 0, true))
            {
                List<Vertex> Vertes = new List<Vertex>();

                Dictionary<Vertex, GerberLibrary.GerberInstance> InstanceMap = new Dictionary<Vertex, GerberLibrary.GerberInstance>();

                foreach (var a in GP.TheSet.Instances)
                {
                    if (a.GerberPath.Contains("???") == false)
                    {
                        var outline = GP.GerberOutlines[a.GerberPath];
                        var P = outline.GetActualCenter();
                        P = P.Rotate(a.Angle);
                        P.X += a.Center.X;
                        P.Y += a.Center.Y;
                        var V = new Vertex(P.X, P.Y);
                        InstanceMap[V] = a;
                        Vertes.Add(V);
                    }
                }
                GP.UpdateShape();

                var M = new TriangleNet.Meshing.GenericMesher();
                var R = M.Triangulate(Vertes);


                GerberLibrary.GerberArtWriter GAW2 = new GerberLibrary.GerberArtWriter();

                GerberLibrary.GerberArtWriter GAW = new GerberLibrary.GerberArtWriter();
                foreach (var a in R.Edges)
                {
                    var A = R.Vertices.ElementAt(a.P0);
                    var B = R.Vertices.ElementAt(a.P1);
                    PolyLine P = new PolyLine();
                    P.Add(A.X, A.Y);
                    P.Add(B.X, B.Y);
                    GerberLibrary.GerberInstance iA = null;
                    GerberLibrary.GerberInstance iB = null;
                    for (int i =0;i<InstanceMap.Count;i++)
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

                        

                        var outlineA = GP.GerberOutlines[iA.GerberPath];
                        var outlineB = GP.GerberOutlines[iB.GerberPath];
                        PointD furthestA = new PointD();
                        PointD furthestB = new PointD();
                        double furthestdistA = 0.0;

                        var acA = outlineA.GetActualCenter();
                        var acB = outlineB.GetActualCenter();
                        foreach (var s in outlineA.TheGerber.OutlineShapes)
                        {
                            List<PointD> intersect =  s.GetIntersections(diffA, acA);
                            if (intersect!=null && intersect.Count>0)
                            {
                                for (int i = 0; i < intersect.Count; i++)
                                {
                                    double newD = PointD.Distance(acA, intersect[i]);

                                    PolyLine PL = new PolyLine();
                                    var CP = intersect[i].Rotate(iA.Angle);
                                    CP.X += iA.Center.X;
                                    CP.Y += iA.Center.Y;
                                    PL.MakeCircle(1);
                                    PL.Translate(CP.X , CP.Y );

                                    GAW.AddPolygon(PL);

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
                                    PL.Translate(CP.X , CP.Y);
                                    GAW.AddPolygon(PL);

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
                                var T = GP.AddTab(CP);
                                T.Radius = (float)Math.Max(Distance/1.5, 3.2f);

                                PolyLine PL = new PolyLine();
                                PL.MakeCircle(T.Radius);
                                PL.Translate(CP.X, CP.Y);
                                GAW2.AddPolygon(PL);
                            }

                        }
                        else
                        {
                            var T = GP.AddTab(new PointD((A.X + B.X) / 2, (A.Y + B.Y) / 2));
                            T.Radius = 3.0f;
                        }
                    }
                    GAW.AddPolyLine(P, 0.1);
                }


                GP.UpdateShape();
                GP.RemoveAllTabs(true);
                GP.UpdateShape();
                string basepath = args[args.Count() - 1];
                Directory.CreateDirectory(Path.Combine(basepath, "output"));
                GP.SaveFile(Path.Combine(basepath, "panel.gerberset"));
                GP.SaveOutlineTo(basepath, "paneloutline");

                GAW.Write(Path.Combine(basepath, "delaunay.gbr"));
                GAW2.Write(Path.Combine(basepath, "points.gbr"));
            //    Console.ReadKey();
            }
        }
    }
}
