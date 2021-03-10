using ClipperLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;

using ClipPath = System.Collections.Generic.List<ClipperLib.IntPoint>;
using ClipPaths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;
using GerberLibrary;

namespace CarvedOut
{
    public class GCodeCommand
    {
        public Dictionary<string, double> Numbers = new Dictionary<string, double>();
        public int Command;

        public override string ToString()
        {
            string P = "";
            foreach (var a in Numbers)
            {
                P += String.Format("{0}{1} ", a.Key, a.Value).Replace(",", ".");
            }
          
            return String.Format("G{0} {1}", Command, P.Trim()).Trim();
            
        }
    }
   public class GCodeFile
    {
        List<GCodeCommand> Commands = new List<GCodeCommand>();
        public void Write(string outfile)
        {
            List<String> outlines = new List<string>();

            foreach(var a in Commands)
            {
                outlines.Add(a.ToString());
            }

            System.IO.File.WriteAllLines(outfile, outlines);
        }

        double x =0 , y =0 , z = 0;

        public GCodeFile()
        {
            GCodeCommand mm = new GCodeCommand() { Command = 21 };
            GCodeCommand feed = new GCodeCommand() { Command = 1 };
            feed.Numbers["F"] = 50;

            Commands.Add(mm);
            Commands.Add(feed);

            Commands.Add(new GCodeCommand() { Command = 94 });
            Commands.Add(new GCodeCommand() { Command = 90 });
            Commands.Add(new GCodeCommand() { Command = 54 });
            Commands.Add(new GCodeCommand() { Command = 17 });

        }
        public enum MoveMode
        {
            Slow,
            Fast,
            ReallySlow
        }
        public void MoveTo(double tx, double ty, double tz, MoveMode mode = MoveMode.Slow)
        {
            GCodeCommand GCC = new GCodeCommand();
            GCC.Command =  1;
            if (x != tx)
            {
                GCC.Numbers["X"] = tx;
                x = tx;
            }
            if (y != ty)
            {
                GCC.Numbers["Y"] = ty;
                y = ty;
            }
            if (z != tz)
            {
                GCC.Numbers["Z"] = tz;
                z = tz;
            }
            switch (mode)
            {
                case MoveMode.ReallySlow:

                    GCC.Numbers["F"] = 30; ;
                    break;
                case MoveMode.Slow:

                    GCC.Numbers["F"] = 65; ;
                    break;
                case MoveMode.Fast:

                    GCC.Numbers["F"] = 65; ;
                    break;
            }
            Commands.Add(GCC);

        }

        internal void AddClipperPolygonsToCarve(ClipPaths r, double cuttingdepth, double rapiddepth, double maxstepdown)
        {
            MoveTo(x, y, rapiddepth, GCodeFile.MoveMode.Fast);
            List<double> cuts = new List<double>();
            double depthstepcount = (0 - cuttingdepth) / maxstepdown;
            double depthstep = (0 - cuttingdepth) / Math.Ceiling(depthstepcount);
            for (double depth = 0 - depthstep; depth>cuttingdepth;depth-=depthstep)
            {
                cuts.Add(depth);
            }
            if (cuts.Last() != cuttingdepth) cuts.Add(cuttingdepth);
            foreach (var d in cuts)
            {
                foreach (var a in r)
                {
                    MoveTo(a[0].X / 100000.0, a[0].Y / 100000.0, rapiddepth, GCodeFile.MoveMode.Fast);
                    MoveTo(a[0].X / 100000.0, a[0].Y / 100000.0, d, GCodeFile.MoveMode.ReallySlow);
                    bool first = true;
                    foreach (var v in a)
                    {
                        if (!first)
                        {
                            MoveTo(v.X / 100000.0, v.Y / 100000.0, d, MoveMode.Slow);
                        }
                        else
                        {
                            first = false;
                        }
                    }
                    MoveTo(a[0].X / 100000.0, a[0].Y / 100000.0, d);
                    MoveTo(a[0].X / 100000.0, a[0].Y / 100000.0, rapiddepth, GCodeFile.MoveMode.ReallySlow);
                }
            }

        }

        public void RectangularFillBetweenPolygons(ClipPaths paths, double cuttingdepth, double rapiddepth, double ystep, double x1, double x2, double y1, double y2, double stepdown)
        {
            ClipPaths clips = paths;
       //     ClipPaths paths = new ClipPaths();
            ClipperLib.Clipper cp = new Clipper();
            //cp.add
            cp.AddPolygons(paths, PolyType.ptClip);

            for(double y = y1;y<y2;y+= ystep  * 2)
            {
                ClipPath p = new ClipPath();
                p.Add(new IntPoint() { X = (int)(x1 * 100000), Y = (int)(y * 100000) });
                p.Add(new IntPoint() { X = (int)(x2 * 100000), Y = (int)(y * 100000) });
                p.Add(new IntPoint() { X = (int)(x2 * 100000), Y = (int)((y+ystep) * 100000) });
                p.Add(new IntPoint() { X = (int)(x1 * 100000), Y = (int)((y + ystep) * 100000) });
                cp.AddPolygon(p, PolyType.ptSubject);

            }
            ClipPaths res = new ClipPaths();
            cp.Execute(ClipType.ctDifference, res);
            AddClipperPolygonsToCarve(res, cuttingdepth, rapiddepth, stepdown);
        }
    }
    public partial class CarveDrop : Form
    {
        public static void SetInvariantCulture()
        {
            CultureInfo ci = new CultureInfo("nl-NL");
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
        }



        public CarveDrop()
        {
            InitializeComponent();
            SetInvariantCulture();
        }

        private void CarveDrop_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {

                string[] D = e.Data.GetData(DataFormats.FileDrop) as string[];

                BackColor = Color.Green;

                foreach (string S in D)
                {
                    if (File.Exists(S))
                    {
                        LoadFile(S);
                    }


                }
            }
        }
        double toolradiusatcuttingdepth = 0.5;
        double cuttingdepth = -1.0;

        double rapiddepth =  1.0;

        double toolangle = (45 * Math.PI * 2.0) / 360.0;
        private void LoadFile(string s)
        {

            string outfile = Path.Combine(Path.GetDirectoryName(s), Path.GetFileNameWithoutExtension(s) + "_carved.gcode");
            try
            {

                var P = FenixSVG.LoadSVGToPolies(s,0,0);
                double tantoolangle = Math.Tan(toolangle / 2);
                cuttingdepth = -toolradiusatcuttingdepth / tantoolangle;

                Bounds B = new Bounds();
                foreach(var a in P)
                {
                    foreach( var b in a)
                    {
                        B.AddPolygon(b.ToPath());
                    }
                    
                }
                var C = B.Center();
                
    	        ClipPaths subj = new ClipPaths();
                foreach(var a in P)
                {
                    foreach( var b in a)
                    {
                        subj.Add(b.ToPath(-C.X*100000, -C.Y*100000));   
                    }
                }

                GCodeFile F = new GCodeFile();

                var R1 = ClipperLib.Clipper.OffsetPolygons(subj, (toolradiusatcuttingdepth * 1.5)*100000.0);

                // t = o / a
                // a = o / t

//                F.RectangularFillBetweenPolygons(R1, cuttingdepth, rapiddepth, toolradiusatcuttingdepth * 0.75, -B.Width() / 2 - 2, B.Width() / 2 + 2, -B.Height() / 2 - 2, B.Height() / 2 + 2, 0.2);
  //              F.AddClipperPolygonsToCarve(R1, cuttingdepth, rapiddepth,0.5);


                var R =  ClipperLib.Clipper.OffsetPolygons(subj, toolradiusatcuttingdepth * 100000.0);                                
                F.AddClipperPolygonsToCarve(R, cuttingdepth, rapiddepth,0.5);
                
                toolradiusatcuttingdepth = 0.25;
                cuttingdepth = -toolradiusatcuttingdepth / tantoolangle;

                var R3 = ClipperLib.Clipper.OffsetPolygons(subj, toolradiusatcuttingdepth * 100000.0);
                F.AddClipperPolygonsToCarve(R3, cuttingdepth, rapiddepth,0.5);


                F.MoveTo(0, 0, rapiddepth, GCodeFile.MoveMode.Fast);
                F.MoveTo(0, 0, 0, GCodeFile.MoveMode.Slow);
                F.Write(outfile);

            }
            catch (Exception)
            {
                BackColor = Color.Red;
            }
        }

        private void CarveDrop_DragEnter(object sender, DragEventArgs e)
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
    }
}
