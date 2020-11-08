using GerberLibrary;
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
                   var A = GP.AddGerberFolder(new StandardConsoleLog(), a);
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

                GerberLibrary.GerberArtWriter GAW2 = new GerberLibrary.GerberArtWriter();
                GerberLibrary.GerberArtWriter GAW = new GerberLibrary.GerberArtWriter();

                GP.BuildAutoTabs(new StandardConsoleLog(), GAW, GAW2);
                

                string basepath = args[args.Count() - 1];
                Directory.CreateDirectory(Path.Combine(basepath, "output"));
                GP.SaveFile(Path.Combine(basepath, "panel.gerberset"));
                GP.SaveOutlineTo(basepath, "paneloutline", new StandardConsoleLog());

                GAW.Write(Path.Combine(basepath, "delaunay.gbr"));
                GAW2.Write(Path.Combine(basepath, "points.gbr"));
            //    Console.ReadKey();
            }
        }
    }
}
