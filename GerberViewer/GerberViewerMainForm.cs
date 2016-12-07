using GerberLibrary.Core;
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

namespace GerberViewer
{
    public partial class GerberViewerMainForm : Form
    {
        private DockPanel dockPanel;
        LoadedStuff Document = new LoadedStuff();
        public GerberViewerMainForm()
        {
            InitializeComponent();

            this.dockPanel = new WeifenLuo.WinFormsUI.Docking.DockPanel();

           // var theme = new VS2015BlueTheme();
            //this.dockPanel.Theme = theme;

            this.dockPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Controls.Add(this.dockPanel);
            ShowDockContent();
        }

        LayerList TheList;
        LayerDisplay TheTopDisplay;
        LayerDisplay TheBottomDisplay;

        List<LayerDisplay> SingleLayers = new List<LayerDisplay>();

        public void ShowDockContent()
        {
            TheList = new LayerList(this, Document);
            TheList.Show(this.dockPanel, DockState.DockLeft);
            TheTopDisplay= new LayerDisplay(Document, BoardSide.Top);
            TheTopDisplay.Show(this.dockPanel, DockState.Document);
            TheTopDisplay.Text = "Top";
            TheBottomDisplay = new LayerDisplay(Document, BoardSide.Bottom);
            TheBottomDisplay.Show(this.dockPanel, DockState.Document);
            TheBottomDisplay.Text = "Bottom";

        }

        public void LoadGerberFolder(List<string> list)
        {
            if (list == null) return;

            foreach (var a in list)
            {
                Document.AddFile(a);
            }
            UpdateAll();

            ClearDisplays();

            foreach (var a in Document.Gerbers)
            {
                var Display = new LayerDisplay(Document, a);
                 Display.Show(this.dockPanel, DockState.Document);
                Display.Text = a.File.ToString() ;
                SingleLayers.Add(Display);
            }


        }

        private void UpdateAll()
        {
            TheTopDisplay.UpdateDocument();
            TheBottomDisplay.UpdateDocument();
            TheList.UpdateLoadedStuff();
            foreach(var a in SingleLayers)
            {
                a.UpdateDocument();
            }
        }
        public void ClearDisplays()
        {
            foreach (var a in SingleLayers)
            {
                a.DockHandler.DockPanel = null;
                a.Close();
                
            }

            SingleLayers.Clear();
        }
        internal void ClearAll()
        {
            Document.Gerbers.Clear();
            ClearDisplays();
            UpdateAll();
        }


        private void GerberViewerMainForm_DragEnter(object sender, DragEventArgs e)
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

        private void GerberViewerMainForm_DragDrop(object sender, DragEventArgs e)
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

       

    }
}
