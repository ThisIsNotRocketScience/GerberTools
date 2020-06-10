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
using GerberLibrary.Core;
using WeifenLuo.WinFormsUI.Docking;

namespace SolderTool
{
    public partial class SolderToolMain : Form
    {


        private PartList Parts;
        public SolderToolMain(string[] args)
        {
            InitializeComponent();
            KeyPreview = true;

            var theme = new VS2015BlueTheme();
            this.dockPanel1.Theme = theme;
            this.dockPanel1.PreviewKeyDown += SolderToolMain_PreviewKeyDown;

            Parts = new PartList(this);
            Parts.Show(this.dockPanel1, DockState.DockLeft);
            // Parts.DockTo(this.dockPanel, DockStyle.Left);
            foreach (var a in args)
            {
                LoadDocument(a);
            }

        }

        private void loadFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {

            }
        }

        List<Viewer> Documents = new List<Viewer>();
        int CurrentDocument = 0;
        void LoadDocument(string basefile)
        {
            if (File.Exists(basefile) == false) return;

            Viewer V = new Viewer(this);
            V.Load(basefile);
            V.Show(this.dockPanel1, DockState.Document);
            Documents.Add(V);
            CurrentDocument = Documents.Count - 1;
            Parts.InvalidatePicture();
        }

        public string GetCurrentPartName()
        {
            return Parts.GetCurrentPartName();
        }

        internal void SetCurrent(Viewer viewer)
        {
            for (int i = 0; i < Documents.Count; i++)
            {
                if (viewer == Documents[i])

                {
                    CurrentDocument = i;
                    Parts.UpdateCurrentPart();
                    Parts.InvalidatePicture();
                    return;
                }
            }
        }

        public void CloseDocument(Viewer V)
        {
            Documents.Remove(V);
            if (Documents.Count > 0)
            {
                CurrentDocument = CurrentDocument % Documents.Count();
            }
            Parts.InvalidatePicture();
        }

        public int GetCurrentPart()
        {
            return Parts.CurrentPart;
        }

        internal BOM GetBom()
        {
            if (Documents.Count == 0) return null;
            return Documents[CurrentDocument].TheBOM;
        }
        private void _calendar_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down:
                case Keys.Right:
                    //action
                    break;
                case Keys.Up:
                case Keys.Left:
                    //action
                    break;
            }
        }

        internal void RepaintCurrent()
        {
            Documents[CurrentDocument].InvalidatePicture();
        }

        private void SolderToolMain_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = false;
        }

        private void SolderToolMain_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            switch(e.KeyCode)
            {
                case Keys.Up: Parts.Up();break;
                case Keys.Down: Parts.Down(); break;
                case Keys.Enter: Parts.Enter(); break;
            }
        }

        internal void UnSolderPart(string v)
        {
            Documents[CurrentDocument].UnSolder(v);
        }

        internal void SolderPart(string v)
        {
            Documents[CurrentDocument].Solder(v);
        }
    }
}
