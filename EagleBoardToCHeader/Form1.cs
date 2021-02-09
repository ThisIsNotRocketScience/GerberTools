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


namespace EagleBoardToCHeader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
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
        class Place
        {
            public Place(double _x, double _y, double _rot, string _name)
            {
                x = _x;
                y = _y;
                rot = _rot;
                name = _name;
            }
            public double x, y;
            public double  rot;
            public string name;
        }
        public string fd(double inp)
        {
            return String.Format("{0:N2}", inp).Replace(",",".");
        }
        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {

                string[] D = e.Data.GetData(DataFormats.FileDrop) as string[];
                foreach (var s in D)
                {
                    if (File.Exists(s))
                    {
                        GerberLibrary.Eagle.EagleLoader EL = new GerberLibrary.Eagle.EagleLoader(s);

                        string name = Path.GetFileNameWithoutExtension(s);
                        string folder = Path.GetDirectoryName(s);

                        List<string> alllines = new List<string>();

                        Dictionary<string, List<Place>> placements = new Dictionary<string, List<Place>>();
                        foreach(var a in EL.DevicePlacements)
                        {
                            string N = "part_"+a.library + "_" + a.package + "_" + a.value;
                            N = N.Replace("-", "_");
                            N = N.Replace(".", "_");
                            N = N.Replace(" ", "_");
                            if (placements.ContainsKey(N) == false)
                            {
                                placements[N] = new List<Place>();
                            }
                            
                            placements[N].Add(new Place(a.x, a.y, a.rot.Degrees, a.name));
                            
                        }
                        

                        alllines.Add("#pragma once");
                        alllines.Add("");
                        alllines.Add("typedef struct Placement {");
                        alllines.Add("\tdouble x,y,rot;");
                        alllines.Add("\tconst char* name;");
                        alllines.Add("} Placement;");
                        alllines.Add("");


                        foreach (var a in placements)
                        {
                            string thename = a.Key;
                            string count = a.Value.Count().ToString();
                            alllines.Add(string.Format("#define __{0}_count {1}", thename, count));
                            alllines.Add(string.Format("const Placement {0}[{1}] = ",thename, count)+"{");

                            foreach (var b in a.Value)
                            {
                                alllines.Add("\t{" + string.Format("\t{0}, {1}, {2}, \"{3}\"", fd(b.x), fd(b.y), fd(b.rot), b.name) + "},");

                            }
                            alllines.Add("};");
                            alllines.Add("");
                        }


                        alllines.Add("");

                        File.WriteAllLines(Path.Combine(folder, name + "_components.h"), alllines);

                    }
                }
            }
        }
    }
}
