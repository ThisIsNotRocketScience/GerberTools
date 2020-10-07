using GerberLibrary;
using GerberLibrary.Core;
using GerberLibrary.Core.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using WeifenLuo.WinFormsUI.ThemeVS2015;


namespace PnP_Processor
{
    public partial class PnPMain : Form
    {
        private DockPanel dockPanel;

        BoardDisplay BDPre;
         BoardDisplay BDPost;
        public PnPMain(string[] args)
        {
            InitializeComponent();

            this.dockPanel = new WeifenLuo.WinFormsUI.Docking.DockPanel();

            var theme = new VS2015BlueTheme();
            this.dockPanel.Theme = theme;

            this.dockPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Controls.Add(this.dockPanel);
            this.dockPanel.SetBounds(0, 0, Width, Height);
            dockPanel.UpdateDockWindowZOrder(DockStyle.Left, true);

            Actions A1 = new Actions(this);
            A1.Show(this.dockPanel, DockState.DockTop);

            BDPre = new BoardDisplay(this, false);
            BDPre.Show(this.dockPanel, DockState.DockLeft);

            BDPost = new BoardDisplay(this, true);
            BDPost.Show(this.dockPanel, DockState.Document);

            if (args.Count() >0)
            {
                ProcessFolder(args[0], new StandardConsoleLog());
            }
        }

        internal void Flip()
        {
            
        }

        internal void Rotate()
        {
            
        }

        internal void Open()
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                ProcessFolder(folderBrowserDialog1.SelectedPath, new StandardConsoleLog());
            }
        }

        private void openSourceFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Open();
        }

        string bomfile = "";
        string pnpfile = "";
        string filebase = "";
        string jigfile = "";
        string boardfile = "";
        public BOM B = new BOM();
        public BOM B_fixed = new BOM();
        public GerberImageCreator Set;
        public PointD FixOffset = new PointD();
        public bool FlipBoard = false;
        public int RotationAngle = 0;

        public void ProcessFolder(string basefolder, ProgressLog log)
        {
            String DirBaseName = basefolder;// Path.GetFileNameWithoutExtension(a);
            log.PushActivity("Processing " + DirBaseName);
           // string fixedoutputfolder = basefolder + "\\" + Path.GetFileName(a) + "_offsetandrefdes";
           // if (Directory.Exists(fixedoutputfolder) == false) Directory.CreateDirectory(fixedoutputfolder);

            var F = Directory.GetFiles(basefolder, "*.*");
            foreach (var f in F)
            {

                if (Path.GetExtension(f).ToLower().Contains("zip"))
                {
                    if (f.ToLower().Contains("jig"))
                    {
                        jigfile = f;
                    }
                    else
                    {
                        boardfile = f;
                    }
                }
                if (f.ToLower().Contains("_bom.csv"))
                {
                    bomfile = f;
                    filebase = f.Substring(0, f.Length - "_BOM.csv".Length);
                    log.AddString(String.Format("found basename {0}", Path.GetFileName(filebase)));
                }
                else
                if (f.ToLower().Contains("_ellage_pnp.csv"))
                {
                    pnpfile = f;
                }
            }

            B = new BOM();

            if (bomfile.Length > 0 && pnpfile.Length > 0)
            {
                log.PushActivity("Loading BOM");
                log.AddString(String.Format("Loading BOM! {0},{1}", Path.GetFileName(bomfile), Path.GetFileName(pnpfile)));
                B.LoadJLC(bomfile, pnpfile);
                B_fixed.LoadJLC(bomfile, pnpfile);

                Set = LoadGerberZip(boardfile, log);
    //            string OutputGerberName = fixedoutputfolder + "\\" + Path.GetFileName(boardfile);
                //B.WriteRefDesGerber(OutputGerberName+"ORIGPLACEMENT");
                FixOffset  = new PointD(-Set.BoundingBox.TopLeft.X, -Set.BoundingBox.BottomRight.Y);
//                B.WriteRefDesGerber(OutputGerberName);
  //              B.WriteJLCCSV(fixedoutputfolder, Path.GetFileName(filebase), true);
                log.PopActivity();
            }

            log.PopActivity();


            BDPre.Invalidate();
            BDPost.Invalidate();
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

    }
}
