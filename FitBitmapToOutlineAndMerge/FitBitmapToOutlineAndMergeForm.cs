using GerberLibrary;
using GerberLibrary.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Globalization;

namespace FitBitmapToOutlineAndMerge
{
    public partial class FitBitmapToOutlineAndMergeForm : Form
    {
        public FitBitmapToOutlineAndMergeForm()
        {
            InitializeComponent();

            //ScanFolder(@"C:\Projects\Circuits\eagle\Eurorack-Modular\Eurorack-Modules\Development\Eurorack Set 9\Gerbers\DelayExperiments");
            try
            {
                ScanFolder(@"C:\Projects\Circuits\eagle\Eurorack-Modular\Eurorack-Modules\Development\Eurorack Set 9\Gerbers\RectangularThing-SOICs-TWEAKS-3");
            }
            catch(Exception)
            {

            }
            //ScanFolder(@"C:\Projects\Circuits\eagle\Eurorack-Modular\Eurorack-Modules\EuroForError\Gerbers\BigBus_Error");

        }

        private void BitmapButton_Click(object sender, EventArgs e)
        {
            if (BitmapFileDialog.ShowDialog() == DialogResult.OK)
            {
                BitmapFileTopBox.Text = BitmapFileDialog.FileName;
            }
        }

        private void OutlineButton_Click(object sender, EventArgs e)
        {
            if (GerberFileDialog.ShowDialog() == DialogResult.OK)
            {
                OutlineFileBox.Text = GerberFileDialog.FileName;
            }
            // OutlineFile
        }

        private void SilkFileButton_Click(object sender, EventArgs e)
        {
            // SilkFile
            if (GerberFileDialog.ShowDialog() == DialogResult.OK)
            {
                SilkFileTopBox.Text = GerberFileDialog.FileName;
            }
        }

        private void ProcessButton_Click(object sender, EventArgs e)
        {
            // Process

            TheThread();

        }
        Thread BGThread;
        void DoThread()
        {
            double DPI = 0;
            if (!double.TryParse(DPIbox.Text, out DPI))
            {
                DPI = 200;
            }


            string OutlineFile = OutlineFileBox.Text;

            ParsedGerber PLS = null;
            PLS = PolyLineSet.LoadGerberFile(new StandardConsoleLog(), OutlineFile);

            string SilkFile = SilkFileTopBox.Text;
            string BitmapFile = BitmapFileTopBox.Text;
            bool Flipped = FlipBox.Checked;
            bool FlippedBottom = FlipInputBottom.Checked;
            bool Invert = InvertBox.Checked;
            bool InvertBottom = InvertBitmapBottom.Checked;
            string CopperFile = copperfilebox.Text;
            string soldermaskfile = soldermaskfilebox.Text;

            statustext = "processing top!"; 
            CreateStuff(DPI, PLS, OutlineFile, SilkFile, BitmapFile, Flipped, Invert, CopperFile, soldermaskfile);
            statustext = "processing bottom!";
            CreateStuff(DPI, PLS, OutlineFile, SilkFileBottomBox.Text, BitmapFileBottomBox.Text, FlippedBottom, InvertBottom, CopperFile, soldermaskfile);

            BGThread = null;
            statustext = "done!";

        }

        void TheThread()
        {
            this.Enabled = false;
            BGThread = new Thread(new ThreadStart(DoThread));
            CultureInfo ci = new CultureInfo("nl-NL");

            BGThread.CurrentCulture = ci;
            BGThread.CurrentUICulture = ci;

            BGThread.Start();

        }

        private void CreateStuff(double DPI,  ParsedGerber PLS, string OutlineFile, string SilkFile, string BitmapFile, bool Flipped, bool Invert,string CopperFile, string SoldermaskFile )
        {
            if (BitmapFile.Length == 0) return;
           
            if (System.IO.File.Exists(BitmapFile) == false)
            {
                MessageBox.Show("Bitmap not found or not specified... aborting", "Aborting..", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            if (System.IO.File.Exists(OutlineFile) == false)
            {
                MessageBox.Show("Outline not found or not specified... aborting", "Aborting..", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            Bitmap B = (Bitmap)Image.FromFile(BitmapFile);
            if (Flipped) B.RotateFlip(RotateFlipType.RotateNoneFlipX);
            B.RotateFlip(RotateFlipType.RotateNoneFlipY);

            Bitmap B2 = null;
            if (File.Exists(CopperFile))
            {
                B2 = (Bitmap)Image.FromFile(CopperFile);
                if (Flipped) B2.RotateFlip(RotateFlipType.RotateNoneFlipX);
                B2.RotateFlip(RotateFlipType.RotateNoneFlipY);
            }
            Bitmap B3 = null;
            if (File.Exists(SoldermaskFile))
            {
                B3 = (Bitmap)Image.FromFile(SoldermaskFile);
                if (Flipped) B3.RotateFlip(RotateFlipType.RotateNoneFlipX);
                B3.RotateFlip(RotateFlipType.RotateNoneFlipY);
            }
            bool UseSilkFile = false;
            if (System.IO.File.Exists(SilkFile)) UseSilkFile = true;


            string output = OutputFolderBox.Text;

            string OutSilk = BitmapFile + ".SILK";
            string OutCopper = BitmapFile + ".GTL";
            string OutSoldermask= BitmapFile + ".GTS";
            if (B!=null) GerberLibrary.ArtWork.Functions.WriteBitmapToGerber(OutSilk, PLS, DPI, B, Invert ? -128 : 128);
            if (B2 !=null)GerberLibrary.ArtWork.Functions.WriteBitmapToGerber(OutCopper, PLS, DPI, B2, Invert ? -128 : 128);
            if (B3 != null) GerberLibrary.ArtWork.Functions.WriteBitmapToGerber(OutSoldermask, PLS, DPI, B3, Invert ? -128 : 128);
            if (UseSilkFile)
            {
                // merge things!
                GerberLibrary.GerberMerger.Merge(OutSilk, SilkFile, Path.Combine(output, Path.GetFileName(SilkFile )), new StandardConsoleLog());
            }
            else
            {
                File.Copy(OutSilk, Path.Combine(output, Path.GetFileName(SilkFile)), true);
            }
            

        }

        public void AddString(string text, float progress = -1F)
        {
            Console.WriteLine(text);
        }

        private void OutlineFileBox_TextChanged(object sender, EventArgs e)
        {

            ParsedGerber PLS = null;
            string OutlineFile = OutlineFileBox.Text;
            if (System.IO.File.Exists(OutlineFile) == false)
            {
                return;
            }
            PLS = PolyLineSet.LoadGerberFile(new StandardConsoleLog(), OutlineFile);

            sizebox.Text = String.Format("{0}x{1}mm", PLS.BoundingBox.Width(), PLS.BoundingBox.Height());

        }

        private void button8_Click(object sender, EventArgs e)
        {
            // output folder
        }

        private void button6_Click(object sender, EventArgs e)
        {
            //BitmapBottom
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // SilkBottom
        }

        private void button7_Click(object sender, EventArgs e)
        {
            // autofill

            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                string path = folderBrowserDialog1.SelectedPath;
                ScanFolder(path);
            }
        }

        private void ScanFolder(string path)
        {
            var OF = path + "_MERGED";
            if (Directory.Exists(OF) == false) Directory.CreateDirectory(OF);
            OutputFolderBox.Text = OF;

            var V = System.IO.Directory.GetFiles(path);
            foreach (var a in V)
            {
                bool Copy = false;
                var bf = GerberLibrary.Gerber.FindFileType(a);
                if (bf == BoardFileType.Drill)
                {
                    Copy = true;
                }
                if (bf == BoardFileType.Gerber)
                {
                    Copy = true;
                    BoardSide S;
                    BoardLayer L;
                    GerberLibrary.Gerber.DetermineBoardSideAndLayer(a, out S, out L);
                    if (L == BoardLayer.Silk)
                    {
                        if (S == BoardSide.Top)
                        {
                            SilkFileTopBox.Text = a;
                            Copy = false;
                        }
                        if (S == BoardSide.Bottom)
                        {
                            SilkFileBottomBox.Text = a;
                            Copy = false;
                        }
                    }
                    if (L == BoardLayer.Outline)
                    {
                        OutlineFileBox.Text = a;
                    }
                }
                else
                {
                    if (Path.GetFileName(a).ToLower().Contains("top.png"))
                    {
                        BitmapFileTopBox.Text = a;
                    }

                    if (Path.GetFileName(a).ToLower().Contains("silkscreen.png"))
                    {
                        BitmapFileTopBox.Text = a;
                    }
                    if (Path.GetFileName(a).ToLower().Contains("copper.png"))
                    {
                        copperfilebox.Text = a;
                    }
                    if (Path.GetFileName(a).ToLower().Contains("soldermask.png"))
                    {
                        soldermaskfilebox.Text = a;
                    }

                    if (Path.GetFileName(a).ToLower().Contains("bottom.png"))
                    {
                        BitmapFileBottomBox.Text = a;
                    }
                }
                if (Copy)
                {
                    
                    File.Copy(a, Path.Combine(OF, Path.GetFileName(a)), true);
                }
            }

           
        }

        private void FitBitmapToOutlineAndMergeForm_DragDrop(object sender, DragEventArgs e)
        {
            
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {

                string[] D = e.Data.GetData(DataFormats.FileDrop) as string[];
                List<String> files = new List<string>();
                foreach (string S in D)
                {
                    if (Directory.Exists(S))
                    {
                        ScanFolder(S);
                        
                    }

                    else
                    {
                        if (File.Exists(S)) files.Add(S);
                    }
                }
                if (files.Count > 0)
                {
                    //LoadGerberFolder(files);
                }
            }
        }

        private void FitBitmapToOutlineAndMergeForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }

        }

        private void FlipInputBottom_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void InvertBitmapBottom_CheckedChanged(object sender, EventArgs e)
        {

        }

        string statustext = "";
        private void timer1_Tick(object sender, EventArgs e)
        {

            statusbox.Text = statustext;
            if (BGThread == null) Enabled = true;
        }
    }
}
