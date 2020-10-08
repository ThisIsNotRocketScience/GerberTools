using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GerberLibrary;
using GerberLibrary.Core;
using GerberLibrary.Core.Primitives;

namespace PnP_Processor
{
    public class PnPProcDoc : ProgressLog
    {

        StandardConsoleLog log;
        public string stock;
        public string silk;
        public string pnp;
        public string bom;
        public string outline;
        public string gerberzip;
        public bool loaded = false;
        public PointD FixOffset = new PointD();
        public bool FlipBoard = false;
        public int RotationAngle = 0;

        public PnPProcDoc()
        {
            log = new StandardConsoleLog(this);
        }

        Thread T = null;

        public void StartLoad()
        {
            T = new Thread(new ThreadStart(LoadStuff));
            T.Start();
        }
        private static GerberImageCreator LoadGerberZip(string v, ProgressLog log)
        {
            log.PushActivity("LoadingGerbers");
            GerberImageCreator GIC = new GerberImageCreator();
            List<String> Files = new List<string>();
            Files.Add(v);
            GIC.AddBoardsToSet(Files, log);

            // log.AddString(GIC.GetOutlineBoundingBox().ToString());
            log.PopActivity();
            return GIC;
        }
        public BOM B;
       public  GerberImageCreator Set;
        private void LoadStuff()
        {


            log.PushActivity("Loading document");
            B = new BOM();

            if (bom.Length > 0 && pnp.Length > 0)
            {
                String DirBaseName = Path.GetFileNameWithoutExtension(pnp);
                log.PushActivity("Processing " + DirBaseName);

                log.PushActivity("Loading BOM");
                log.AddString(String.Format("Loading BOM! {0},{1}", Path.GetFileName(bom), Path.GetFileName(pnp)));
                B.LoadJLC(bom, pnp);
                log.PopActivity();

                if (gerberzip != null && File.Exists(gerberzip))
                {
                    Set = LoadGerberZip(gerberzip, log);
                }
                else
                {
                    Set = new GerberImageCreator();
                    Set.AddBoardToSet(silk, log);
                    Set.AddBoardToSet(outline, log);
                }
                Box = Set.BoundingBox;


                //            string OutputGerberName = fixedoutputfolder + "\\" + Path.GetFileName(boardfile);
                //B.WriteRefDesGerber(OutputGerberName+"ORIGPLACEMENT");
                FixOffset = new PointD(-Set.BoundingBox.TopLeft.X, -Set.BoundingBox.BottomRight.Y);
                //                B.WriteRefDesGerber(OutputGerberName);
                //              B.WriteJLCCSV(fixedoutputfolder, Path.GetFileName(filebase), true);

                log.PopActivity();
            }
            else
            {
                log.AddString(String.Format("pnp and bom need to be valid! bom:{0} pnp:{1}", bom, pnp));
            }

            
            loaded = true;
            log.AddString("Done!");
            log.PopActivity();
        }

        public List<string> Log = new List<string>();
        public int Stamp = 0;
        static int MainStamp = 0;
        public Bounds Box = new Bounds();

        public override void AddString(string text, float progress = -1)
        {
            Stamp = ++MainStamp;
            Log.Add(text);
        }
    }
}
