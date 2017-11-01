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

namespace FitBitmapToOutlineAndMerge
{
    public partial class FitBitmapToOutlineAndMergeForm : Form, ProgressLog
    {
        public FitBitmapToOutlineAndMergeForm()
        {
            InitializeComponent();
            ScanFolder(@"C:\Projects\Circuits\eagle\Eurorack-Modular\Eurorack-Modules\EuroForError\Gerbers\Organ_fix1");
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

            double DPI = 0;
            if (!double.TryParse(DPIbox.Text, out DPI))
            {
                DPI = 200;
            }


            string OutlineFile = OutlineFileBox.Text;

            ParsedGerber PLS = null;
            PLS = PolyLineSet.LoadGerberFile(OutlineFile);
            
            string SilkFile = SilkFileTopBox.Text;
            string BitmapFile = BitmapFileTopBox.Text;
            bool Flipped = FlipBox.Checked;
            bool Invert = InvertBox.Checked;
            CreateStuff(DPI,  PLS, OutlineFile, SilkFile, BitmapFile, Flipped, Invert);
            CreateStuff(DPI, PLS, OutlineFile, SilkFileBottomBox.Text, BitmapFileBottomBox.Text, FlipInputBottom.Checked, InvertBitmapBottom.Checked);

        }

        private void CreateStuff(double DPI,  ParsedGerber PLS, string OutlineFile, string SilkFile, string BitmapFile, bool Flipped, bool Invert)
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

            bool UseSilkFile = false;

            if (System.IO.File.Exists(SilkFile)) UseSilkFile = true;


            string output = OutputFolderBox.Text;

            string OutSilk = BitmapFile + ".SILK";
            GerberLibrary.ArtWork.Functions.WriteBitmapToGerber(OutSilk, PLS, DPI, B, Invert ? -128 : 128);
            if (UseSilkFile)
            {
                // merge things!
                GerberLibrary.GerberMerger.Merge(OutSilk, SilkFile, Path.Combine(output, Path.GetFileName(SilkFile )), this);
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
            PLS = PolyLineSet.LoadGerberFile(OutlineFile);

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
    }
}
