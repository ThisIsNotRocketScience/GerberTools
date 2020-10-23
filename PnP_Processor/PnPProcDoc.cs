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
        public enum FlipMode
        {
            NoFlip,
            FlipDiagonal,
            FlipHorizontal
        }

        StandardConsoleLog log;
        public string stock ="";
        public string pnp ="";
        public string bom = "";
        public string gerberzip = "";
        public bool loaded = false;
        public PointD FixOffset = new PointD();
        public FlipMode FlipBoard = FlipMode.NoFlip;
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
            GIC.AddBoardsToSet(Files, log, true, false);

            // log.AddString(GIC.GetOutlineBoundingBox().ToString());
            log.PopActivity();
            return GIC;
        }

        internal static FlipMode DecodeFlip(string a)
        {
            switch (a.ToLower())
            {
                case "d":
                case "diagonal":
                    return FlipMode.FlipDiagonal;

                case "horizontal":
                case "h":
                    return FlipMode.FlipHorizontal;

            }
            return FlipMode.NoFlip;
        }

        public BOM B = new BOM();
        public BOM BPost = new BOM();

        public void BuildPostBom()
        {
            BPost = new BOM();
            BOMNumberSet s = new BOMNumberSet();

            switch (FlipBoard)
            {
                case FlipMode.NoFlip:
                    FixOffset = new PointD(Set.BoundingBox.TopLeft.X, Set.BoundingBox.TopLeft.Y);
                    break;
                case FlipMode.FlipDiagonal:
                    FixOffset = new PointD(Set.BoundingBox.TopLeft.X, Set.BoundingBox.TopLeft.Y);
                    break;
                case FlipMode.FlipHorizontal:
                    FixOffset = new PointD(Set.BoundingBox.TopLeft.X, Set.BoundingBox.TopLeft.Y);
                    break;
            }

            BPost.MergeBOM(B, s, 0, 0, -FixOffset.X, -FixOffset.Y, 0);

            FixSet = new GerberImageCreator();
            FixSet.CopyFrom(Set);

            switch (FlipBoard)
            {
                case FlipMode.NoFlip:
                    FixSet.SetBottomLeftToZero();
                    break;
                case FlipMode.FlipDiagonal:
                    FixSet.SetBottomRightToZero();
                    FixSet.FlipXY();
                    FixSet.Translate(0, FixSet.BoundingBox.Height());
                    BPost.SwapXY();
                    BPost.FlipSides();
                    //                    BPost.Translate(0, FixSet.BoundingBox.Height());
                    break;
                case FlipMode.FlipHorizontal:
                    FixSet.FlipX();
                    FixSet.SetBottomRightToZero();
                    BPost.FlipSides();
                    BPost.FlipX();
                    break;
            }
            BPost.FixupAngles(StockDoc);
            BPost.WriteJLCPnpFile(B.OriginalBasefolder, B.OriginalPnpName + "_rotated");
        }

        public GerberImageCreator Set;
        public GerberImageCreator FixSet;
        private void LoadStuff()
        {

            if (stock.Length > 0 && File.Exists(stock))
            {
                try
                {
                    log.PushActivity("Loading stock");
                    StockDoc = StockDocument.Load(stock);
                    if (StockDoc == null) StockDoc = new StockDocument();
                }
                catch (Exception)
                {
                    StockDoc = new StockDocument();
                }
                log.PopActivity();
            }
            else
            {
                StockDoc = new StockDocument();
            }
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
                }
                Box = Set.BoundingBox;

                BuildPostBom();

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
        private StockDocument StockDoc;

        public override void AddString(string text, float progress = -1)
        {
            Stamp = ++MainStamp;
            Log.Add(text);
        }
    }
}
