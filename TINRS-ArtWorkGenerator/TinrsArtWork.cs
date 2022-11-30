using Artwork;
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
using System.Xml.Serialization;
using Artwork;

namespace TINRS_ArtWorkGenerator
{
    public partial class TinrsArtWork : Form
    {
        public enum MaskType{
            Circle
        }
     
        public Rectangle Selection = new Rectangle() { X = 20, Y = 20, Height = 30, Width = 100 };
       
        Bitmap Mask;
        Bitmap Output;
        SettingsDialog TheSettingsDialog;

        public TinrsArtWork()
        {
            InitializeComponent();
            TheSettings.SetupFenixDefault();
            TheSettingsDialog = new SettingsDialog(TheSettings, UpdateFunc, ProcessFunc);

            string[] args = Environment.GetCommandLineArgs();
            if (args.Count() > 1)
            {
                LoadMask(args[1]);
            }
            else
            {
                CreateCustomMask(MaskType.Circle);
            }

            TheSettingsDialog.Show();


            TheSettingsDialog.BringToFront();
        }

        private void CreateCustomMask(MaskType circle)
        {

            LoadedMask = "custom ";

            var bmpTemp = new Bitmap(512, 512);

            var G = Graphics.FromImage(bmpTemp);

            G.Clear(Color.Black);
            G.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            G.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            G.FillEllipse(Brushes.White, 20, 20, 512 - 40, 512 - 40);
            
            Mask = new Bitmap(bmpTemp);
            


            Output = new Bitmap(Mask.Width, Mask.Height);
            ArtRender.BuildTree(Mask, TheSettings);
            TheSettings.ReloadMask = false;
            UpdateFunc();

            this.Height = Mask.Height + 40;
            this.Width = Mask.Width + 20;



        }

        public void ProcessFunc(string file)
        {
            LoadMask(file);
            UpdateFunc();
            SaveSVG(file + ".svg");
        }

        public void UpdateFunc()
        {
            if (TheSettings.ReloadMask)
            {
                TheSettings.ReloadMask = false;
                LoadMask(LoadedMask);
                return;
            }
            pictureBox1.Invalidate();
           // pictureBox2.Invalidate();

            if (Mask == null) return;

            ArtRender.BuildStuff(Mask, TheSettings);
            
        }


        public Settings TheSettings = new Settings();

    
        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            if (Mask == null)
            {
                e.Graphics.Clear(Color.Gray);
                e.Graphics.DrawString("Load a mask first!", new Font("Arial", 13), Brushes.Black, new PointF(10, 10));
                return;
            }
            Graphics G = Graphics.FromImage(Output);
            G.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
            G.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            ArtRender.DrawTiling(TheSettings, Mask, G,Color.White, Color.Black, 1);

            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;


            float aspect = pictureBox1.Width / (float)pictureBox1.Height;
            float maskaspect = Mask.Width / (float)Mask.Height;
            float scale = 1.0f;

            if (maskaspect > aspect)
            {
                scale = pictureBox1.Width / (float)Mask.Width;
            }
            else
            {
                scale = pictureBox1.Height / (float)Mask.Height;
            }

            e.Graphics.ScaleTransform(scale, scale);



            e.Graphics.DrawImage(Output, 0, 0);
        }

        public TINRSArtWorkRenderer ArtRender = new TINRSArtWorkRenderer();

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (!MouseDown)
            {
                pictureBox2_Paint(sender, e);
                return;
            }
            try
            {
                e.Graphics.Clear(Color.Gray);
                if (Mask == null)
                {
                    e.Graphics.DrawString("Load a mask first!", new Font("Arial", 13), Brushes.Black, new PointF(10, 10));
                    return;
                }

                float aspect = pictureBox1.Width / (float)pictureBox1.Height;

                float maskaspect = Mask.Width / (float)Mask.Height;
                float scale = 1.0f;
                if (maskaspect > aspect)
                {
                    scale = pictureBox1.Width / (float)Mask.Width;
                }
                else
                {

                    scale = pictureBox1.Height / (float)Mask.Height;
                }

                e.Graphics.ScaleTransform(scale, scale);
                e.Graphics.DrawImage(Mask, 0, 0, Mask.Width, Mask.Height);
                if (ArtRender.MaskTree != null)
                {
                    ArtRender.MaskTree.Draw(new GraphicsGraphicsInterface(e.Graphics));
                }
            }
            catch (Exception)
            {
                e.Graphics.Clear(Color.Gray);
                e.Graphics.DrawString("Whoops - error! - try resizing to repaint", new Font("Arial", 13), Brushes.Black, new PointF(10, 10));

            }
        }

        public void DrawItem(Graphics g, QuadTreeItem a)
        {
            g.DrawRectangle(Pens.LightBlue, a.x, a.y, 1, 1);
        }

        string LoadedMask = "";


        FileSystemWatcher watcher;

        public void CreateFileWatcher(string path)
        {
            if (watcher != null)
            {
                watcher.EnableRaisingEvents = false;
                watcher = null;
            }
            // Create a new FileSystemWatcher and set its properties.
            watcher = new FileSystemWatcher();
            watcher.Path = System.IO.Path.GetDirectoryName(path);
            /* Watch for changes in LastAccess and LastWrite times, and 
               the renaming of files or directories. */
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
               | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            // Only watch text files.
            watcher.Filter = System.IO.Path.GetFileName(path);

            // Add event handlers.
            watcher.Changed += new FileSystemEventHandler(OnFileChanged);
            watcher.Created += new FileSystemEventHandler(OnFileChanged);
            watcher.Deleted += new FileSystemEventHandler(OnFileChanged);

            // Begin watching.
            watcher.EnableRaisingEvents = true;
        }

        public static bool IsFileLocked(string file)
        {
            try
            {
                using (var stream = File.OpenRead(file))
                    return false;
            }
            catch (IOException)
            {
                return true;
            }
        }
        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            while (IsFileLocked(LoadedMask))
            {
                System.Threading.Thread.Sleep(100);
            }
            LoadMask(LoadedMask);
            pictureBox1.Invalidate();
           // pictureBox2.Invalidate();
        }

        void LoadMask(string filename)
        {
            try
            {

                LoadedMask = filename;

                using (var bmpTemp = new Bitmap(filename))
                {
                    Mask = new Bitmap(bmpTemp);
                }


                CreateFileWatcher(filename);
                Output = new Bitmap(Mask.Width, Mask.Height);
                ArtRender.BuildTree(Mask, TheSettings);
                TheSettings.ReloadMask = false;
                UpdateFunc();

                this.Height = Mask.Height + 40;
                this.Width = Mask.Width  + 20;

            }
            catch (Exception)
            {
                Mask = null;
                ArtRender.MaskTree = null;
                this.Height = 100;
                this.Width = 100;
            }

        }
        
        private void loadMaskToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openBitmapDialog.ShowDialog() == DialogResult.OK)
            {
                LoadMask(openBitmapDialog.FileName);
            }
        }

        private void saveArtworkbmpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (bmpsaveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if (Output != null) Output.Save(bmpsaveFileDialog1.FileName);
                }
                catch (Exception)
                {

                }
            }
        }

        private void saveArtworkgerberToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (gerbersaveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {

                    GerberArtWriter GAW = new GerberArtWriter();

                    double S = 72.0 / 200.0;
                    foreach (var a in ArtRender.SubDivPoly)
                    {
                        PolyLine PL = new PolyLine(  PolyLine.PolyIDs.ArtWork );
                        PL.Add(a.Vertices[0].x * S, (Mask.Height - a.Vertices[0].y) * S);
                        PL.Add(a.Vertices[1].x * S, (Mask.Height - a.Vertices[1].y) * S);
                        PL.Add(a.Vertices[2].x * S, (Mask.Height - a.Vertices[2].y) * S);
                        PL.Add(a.Vertices[0].x * S, (Mask.Height - a.Vertices[0].y) * S);
                        GAW.AddPolyLine(PL, 0.1);
                    }
                    GAW.Write(gerbersaveFileDialog1.FileName);
                }
                catch (Exception E)
                {

                }
            }

        }

        private void pictureBox1_Resize(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }

        private void pictureBox2_Resize(object sender, EventArgs e)
        {
            //pictureBox2.Invalidate();
        }

        private void saveArtworksvgToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (svgsaveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                SaveSVG(svgsaveFileDialog1.FileName);
            }
        }

        private void SaveSVG(string filename)
        {
            try
            {
                switch (TheSettings.Mode)
                {
                    case Settings.ArtMode.QuadTree:
                        {
                            SVGGraphicsInterface SGI = new SVGGraphicsInterface(Mask.Width, Mask.Height);
                            SGI.WriteOutline();
                            SGI.RotateTransform(TheSettings.DegreesOff);
                            ArtRender.ArtTree.Draw(SGI, false);
                            SGI.Save(filename);
                        }
                        break;

                    case Settings.ArtMode.Tiling:
                        {
                            SVGGraphicsInterface SGI = new SVGGraphicsInterface(Mask.Width, Mask.Height);
                            SGI.WriteOutline();
                            foreach (var a in ArtRender.SubDivPoly)
                            {
                                List<PointF> P = new List<PointF>();
                                foreach (var v in a.Vertices)
                                {
                                    P.Add(new PointF(v.x, v.y));
                                }



                                SGI.DrawPolyline(new Pen(Color.FromArgb(0x008b9e), 0.25f), P, true, true);
                            }
                            //SVGWriter.Write(filename, Mask.Width, Mask.Height, SubDivPoly, 1);
                            SGI.Save(filename);
                        }
                        break;
                    case Settings.ArtMode.Delaunay:
                        {
                            SVGGraphicsInterface SGI = new SVGGraphicsInterface(Mask.Width, Mask.Height);
                            SGI.Save(filename);
                            ArtRender.Delaunay.Render(SGI, Color.Black, Color.White);
                            SGI.Save(filename);
                        }
                        break;
                }
                if (TheSettings.Mode == Settings.ArtMode.QuadTree)
                {
                }
                else
                {
                }
            }
            catch (Exception)
            {
            }
        }

        private void saveSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (settingssaveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Settings));
                    using (Stream Writer = new FileStream(settingssaveFileDialog1.FileName, FileMode.Create))
                    {
                        serializer.Serialize(Writer, TheSettings);
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        private void loadSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (settingsopenFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Settings));
                    Settings s;

                    using (Stream reader = new FileStream(settingsopenFileDialog1.FileName, FileMode.Open))
                    {
                        s = (Settings)serializer.Deserialize(reader);
                    }
                    if (s != null)
                    {
                        TheSettings = s;
                        TheSettingsDialog.UpdateFromSettings();
                        UpdateFunc();
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        private void TinrsArtWork_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files) LoadMask(file);
            }
            catch (Exception)
            {

            }
        }

        private void TinrsArtWork_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void blankMaskToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadedMask = "blankmask";

            using (var bmpTemp = new Bitmap(1000,1000))
            {
                Mask = new Bitmap(bmpTemp);
            }


            Output = new Bitmap(Mask.Width, Mask.Height);
            ArtRender.BuildTree(Mask, TheSettings);
            TheSettings.ReloadMask = false;
            UpdateFunc();

            this.Height = Mask.Height + 40;
            this.Width = Mask.Width  + 20;

        }

        private void createSleeveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //422 205
            TheSettings.alwayssubdivide = true;
            TheSettings.InvertSource = true;
            TheSettings.MaxSubDiv = 4;
            CreateMaskMM(422, 205, "NewSleeve");

        }

        private void CreateMaskMM(double width, double height, string name)
        {
            int w = (int)(width * 72.0 / 25.4);
            int h = (int)(height * 72.0 / 25.4);
            CreateMask(w, h, name);     
        }

        private void CreateMask(int w, int h, string name)
        {
            LoadedMask = name;

            using (var bmpTemp = new Bitmap(w, h))
            {
                Mask = new Bitmap(bmpTemp);
            }


            Output = new Bitmap(Mask.Width, Mask.Height);
            ArtRender.BuildTree(Mask, TheSettings);
            TheSettings.ReloadMask = false;
            UpdateFunc();

            this.Height = Mask.Height + 40;
            this.Width = Mask.Width  + 20;

        }

        bool MouseDown = false;

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            pictureBox1.Invalidate();
            MouseDown = true;

        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            MouseDown = false;
            pictureBox1.Invalidate();

        }

        private void bottomEdgeMaskToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadedMask = "bottomedgemask";

            using (var bmpTemp = new Bitmap(1000, 1000))
            {
                Mask = new Bitmap(bmpTemp);
            }
            Graphics G = Graphics.FromImage(Mask);
            G.Clear(Color.Black);
            G.FillRectangle(Brushes.White, new RectangleF(0, 800, 1000, 200));


            Output = new Bitmap(Mask.Width, Mask.Height);
            ArtRender.BuildTree(Mask, TheSettings);
            TheSettings.ReloadMask = false;
            UpdateFunc();

            this.Height = Mask.Height + 40;
            this.Width = Mask.Width + 20;
        }

        private void saveArtworkMultilevelsvgToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (svgsaveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                int startlevel = TheSettings.MaxSubDiv;
                for(int i =1;i<=startlevel;i++)
                {
                    TheSettings.MaxSubDiv = i;
                    UpdateFunc();
                    string P = Path.GetDirectoryName(svgsaveFileDialog1.FileName);
                    string TargetFile = Path.GetFileNameWithoutExtension(svgsaveFileDialog1.FileName) + "_" + i.ToString() + ".svg";
                    SaveSVG(Path.Combine(P, TargetFile));
                }
                
            }
        }
    }
}
