using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GerberLibrary;
using GerberLibrary.Core;
using System.IO;

namespace CaseBuilder
{
    public partial class CaseBuilder : Form
    {
       
        public CaseBuilder()
        {
            InitializeComponent();
        }
        SickOfBeige Box = new SickOfBeige();
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

        private void LoadGerberFolder(List<string> list)
        {
            Box.AddBoardsToSet(list, true, new StandardConsoleLog());
            double offset = (double)offsetbox.Value;
            double holediam = (double)holediamBox.Value;
            

            System.Diagnostics.Process.Start(Box.MinimalDXFSave(Path.Combine(Path.GetDirectoryName(list[0]),Path.GetFileNameWithoutExtension(list[0])), offset, holediam));
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

       
    }
}

