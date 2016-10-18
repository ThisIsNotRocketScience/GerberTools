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

namespace WindowsFormsApplication1
{
    public partial class QuickGerberRender : Form
    {
        public QuickGerberRender()
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

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {

                string[] D = e.Data.GetData(DataFormats.FileDrop) as string[];
                List<String> files = new List<string>();
                foreach (string S in D)
                {
                    if (Directory.Exists(S))
                    {
                        LoadGerberFolder(Directory.GetFiles(S).ToList());
                    }

                    else
                    {
                        if (File.Exists(S)) files.Add(S);
                    }
                }
                if (files.Count > 0)
                {
                    LoadGerberFolder(files);
                }
            }
        }

        private void LoadGerberFolder(List<string> s)
        {
            GerberLibrary.GerberImageCreator GIC = new GerberLibrary.GerberImageCreator();
            GIC.AddBoardsToSet(s);

            GerberLibrary.Gerber.BoardRenderColor = GerberLibrary.Gerber.ParseColor(SolderMaskColor.Text);
            GerberLibrary.Gerber.BoardRenderSilkColor = GerberLibrary.Gerber.ParseColor(SilkScreenColor.Text);
            GerberLibrary.Gerber.BoardRenderPadColor = GerberLibrary.Gerber.ParseColor(CopperColor.Text);

            GIC.WriteImageFiles(Path.GetDirectoryName(s[0]) + ".png");
            GIC.DrawAllFiles(Path.GetDirectoryName(s[0]) + "_Layer", 200);
        }
    }
}
