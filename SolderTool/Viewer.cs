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
using GerberLibrary;
using GerberLibrary.Core;
using Ionic.Zip;

namespace SolderTool
{
    public partial class Viewer : WeifenLuo.WinFormsUI.Docking.DockContent
    {
        public BOM TheBOM;
        private SolderToolMain SolderTool;


        public Viewer(SolderToolMain solderToolMain)
        {
            this.SolderTool = solderToolMain;
            InitializeComponent();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            var G = e.Graphics;
            G.Clear(Color.Black);
            G.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            G.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            G.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            G.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            if (TheBOM == null) return;
            Font F = new Font("Panton", 16);
            Font F2 = new Font("Panton ExtraBold", 26);
            int count = TheBOM.GetPartCount(new List<string>() { });
            int solderedcount = TheBOM.GetSolderedPartCount(new List<string>() { });

            string disp = String.Format("{0}/{1}", solderedcount, count);
            G.DrawString(disp, F, Brushes.White, 2, 2);
            G.DrawString(SolderTool.GetCurrentPartName(), F2, Brushes.White, 2, pictureBox1.Height - F2.Height);


            G.TranslateTransform(10, 10);

            float S = (float)Math.Min(pictureBox1.Width / (TheBox.Width() - 20), pictureBox1.Height / (TheBox.Height() - 20));

            if (TopView)
            {
                G.ScaleTransform(S * 0.8f, -S * 0.8f);
                G.TranslateTransform((float)-TheBox.TopLeft.X, (float)-TheBox.TopLeft.Y - (float)TheBox.Height());
            }
            else
            {
                G.ScaleTransform(-S * 0.8f, -S * 0.8f);
                G.TranslateTransform((float)(-TheBox.TopLeft.X - TheBox.Width()), (float)-TheBox.TopLeft.Y - (float)TheBox.Height());

            }

            RenderLayerSets(G, S, BoardSide.Both, BoardLayer.Outline, Color.Gray);
            if (TopView)
            {
                RenderLayerSets(G, S, BoardSide.Top, BoardLayer.SolderMask, Color.FromArgb(20, 255, 255, 50), false);
                RenderLayerSets(G, S, BoardSide.Top, BoardLayer.Silk, Color.FromArgb(100, 255, 255, 255));
            }
            else
            {
                RenderLayerSets(G, S, BoardSide.Bottom, BoardLayer.SolderMask, Color.FromArgb(20, 255, 255, 50), false);
                RenderLayerSets(G, S, BoardSide.Bottom, BoardLayer.Silk, Color.FromArgb(100, 255, 255, 255));

            }
            int i = 0;
            foreach (var a in TheBOM.DeviceTree)
            {
                foreach (var v in a.Value.Values)
                {
                    bool Current = SolderTool.GetCurrentPart() == i;
                    foreach (var r in v.RefDes)
                    {
                        DrawMarker(G, r, v.Soldered, S, Current);
                    }
                    i++;
                }
            }

        }

        private void RenderLayerSets(Graphics G, float S, BoardSide side, BoardLayer layer, Color C, bool lines = true)
        {
            foreach (var l in LayerSets)
            {
                if (l.Side == side && l.Layer == layer)
                {
                    foreach (var g in l.Gerbs)
                    {
                        RenderOutline(G, S, g, C, lines);
                    }
                }
            }
        }

        bool TopView = false;
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
                        G.DrawLines(new Pen(C, 1 / S), Pts.ToArray());
                    }
                    else
                    {
                        G.FillPolygon(new SolidBrush(C), Pts.ToArray());
                    }
                }
            }
        }

        private void DrawMarker(Graphics g, BOMEntry.RefDesc r, bool soldered, float S, bool current)
        {
            float R = 2;
            float cx = (float)r.x - R / S;
            float cy = (float)r.y - R / S;
            Color CurrentColor = soldered ? Color.Green : Color.Yellow;
            if (current)
            {
                float R2 = 5;
                float cx2 = (float)r.x - R2 / S;
                float cy2 = (float)r.y - R2 / S;
                g.FillRectangle(new SolidBrush(CurrentColor), cx2, cy2, R2 / S * 2, R2 / S * 2);
            }
            g.FillRectangle(soldered ? Brushes.Green : Brushes.Red, cx, cy, R / S * 2, R / S * 2);


        }

        private void pictureBox1_Resize(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }

        string SolderFile = "";
        internal void Load(string basefile)
        {

            Text = Path.GetFileNameWithoutExtension(basefile);

            string fn = Path.GetFileName(basefile);
            string basefolder = Path.GetDirectoryName(basefile);
            string name = fn.Split('_')[0];

            string BOMFile = "";
            string PnPFile = "";
            string gerberFile = "";
            var L = Directory.GetFiles(basefolder, name + "_*.*");
            SolderFile = "";
            foreach (var file in L)
            {
                if (file.EndsWith("_BOM.csv")) BOMFile = file;
                if (file.EndsWith("_gerbers.zip")) gerberFile = file;
                if (file.EndsWith("_PNP.csv")) PnPFile = file;
                if (file.EndsWith("_soldered.txt")) SolderFile = file;
            }

            LayerSets.Add(new LayerSet() { Side = BoardSide.Both, Layer = BoardLayer.Outline });
            LayerSets.Add(new LayerSet() { Side = BoardSide.Top, Layer = BoardLayer.SolderMask });
            LayerSets.Add(new LayerSet() { Side = BoardSide.Top, Layer = BoardLayer.Silk });
            LayerSets.Add(new LayerSet() { Side = BoardSide.Bottom, Layer = BoardLayer.SolderMask });
            LayerSets.Add(new LayerSet() { Side = BoardSide.Bottom, Layer = BoardLayer.Silk });
            if (BOMFile.Length > 0 && PnPFile.Length > 0 && gerberFile.Length > 0)
            {
                BOM B = new BOM();
                B.LoadJLC(BOMFile, PnPFile);
                TheBOM = B;

                if (SolderFile.Length > 0)
                {
                    solderedlist = File.ReadAllLines(SolderFile).ToList();

                    foreach (var a in B.DeviceTree)
                    {
                        foreach (var v in a.Value.Values)
                        {
                            if (solderedlist.Contains(v.Combined()))
                            {
                                v.Soldered = true;
                            }
                        }
                    }
                }
                else
                {
                    SolderFile = Path.Combine(basefolder, name + "_soldered.txt");
                    SaveSolderedList();
                }


                GerberLibrary.GerberImageCreator GIC = new GerberLibrary.GerberImageCreator();

                List<string> res = new List<string>();
                Dictionary<string, MemoryStream> Files = new Dictionary<string, MemoryStream>();
                using (Ionic.Zip.ZipFile zip1 = Ionic.Zip.ZipFile.Read(gerberFile))
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


                string[] FileNames = Files.Keys.ToArray();
                List<string> outlinefiles = new List<string>();
                List<string> topsilkfiles = new List<string>();
                List<string> bottomsilkfiles = new List<string>();

                foreach (var F in FileNames)
                {
                    BoardSide BS = BoardSide.Unknown;
                    BoardLayer BL = BoardLayer.Unknown;
                    Files[F].Seek(0, SeekOrigin.Begin);
                    if (Gerber.FindFileTypeFromStream(new StreamReader(Files[F]), F) == BoardFileType.Gerber)
                    {
                        Gerber.DetermineBoardSideAndLayer(F, out BS, out BL);
                        foreach (var l in LayerSets)
                        {
                            if (l.Side == BS && l.Layer == BL)
                            {
                                l.Files.Add(F);
                                Files[F].Seek(0, SeekOrigin.Begin);
                                var pls = PolyLineSet.LoadGerberFileFromStream(new StreamReader(Files[F]), F, true, false, new GerberParserState() { PreCombinePolygons = false });
                                l.Gerbs.Add(pls);
                            }
                        }
                    }
                }
                TheBox.Reset();

                foreach (var a in LayerSets[0].Gerbs)
                {
                    TheBox.AddBox(a.BoundingBox);
                }

            }

        }

        class LayerSet
        {
            public List<ParsedGerber> Gerbs = new List<ParsedGerber>();
            public List<string> Files = new List<string>();
            public BoardSide Side;
            public BoardLayer Layer;
        }

        List<LayerSet> LayerSets = new List<LayerSet>();


        Bounds TheBox = new Bounds();

        public void UnSolder(string name)
        {
            if (solderedlist.Contains(name) == true)
            {
                solderedlist.Remove(name);
                SaveSolderedList();
            }


        }

        public void Solder(string name)
        {
            if (solderedlist.Contains(name) == false)
            {
                solderedlist.Add(name);
                SaveSolderedList();
            }
        }

        private void SaveSolderedList()
        {
            if (SolderFile.Length > 0)
            {
                File.WriteAllLines(SolderFile, solderedlist);
            }
        }

        List<string> solderedlist = new List<string>();

        private void Viewer_Activated(object sender, EventArgs e)
        {
            
            SolderTool.SetCurrent(this);
            InvalidatePicture();
        }

        private void Viewer_FormClosing(object sender, FormClosingEventArgs e)
        {
            SolderTool.CloseDocument(this);
        }

        internal void InvalidatePicture()
        {
            pictureBox1.Invalidate();
        }

        private void Viewer_KeyPress(object sender, KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case 't':
                case 'T':
                    TopView = !TopView;
                    InvalidatePicture();
                    break;
            }
        }
    }
}
