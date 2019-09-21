using GerberLibrary;
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

        public GerberViewerMainForm(string[] args)
        {
            Gerber.ArcQualityScaleFactor = 20;


            InitializeComponent();

            this.dockPanel = new WeifenLuo.WinFormsUI.Docking.DockPanel();

            var theme = new VS2015BlueTheme();
            this.dockPanel.Theme = theme;

            this.dockPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Controls.Add(this.dockPanel);

            dockPanel.UpdateDockWindowZOrder(DockStyle.Left, true);
            ShowDockContent();

            List<String> files = new List<string>();
            foreach (string S in args)
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

        LayerList TheList;
        LayerDisplay TheTopDisplay;
        LayerDisplay TheBottomDisplay;

        List<LayerDisplay> SingleLayers = new List<LayerDisplay>();

        public void ShowDockContent()
        {

      

            TheTopDisplay = new LayerDisplay(Document, BoardSide.Top, this);
            TheTopDisplay.Show(this.dockPanel, DockState.Document);
            TheTopDisplay.Text = "Top";
            TheBottomDisplay = new LayerDisplay(Document, BoardSide.Bottom, this);
            TheBottomDisplay.Show(this.dockPanel, DockState.Document);
            TheBottomDisplay.Text = "Bottom";

            TheList = new LayerList(this, Document);
            TheList.Show(this.dockPanel, DockState.DockLeft);



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
               a.Panel = new LayerDisplay(Document, a, this);
               a.Panel.Show(this.dockPanel, DockState.Document);
                a.Panel.Text = a.File.ToString() ;
                SingleLayers.Add(a.Panel);
            }
            

        }

        private void UpdateAll(bool reloadlist = true)
        {
            //Console.WriteLine("updating all");
            TheTopDisplay.UpdateDocument(reloadlist);
            TheBottomDisplay.UpdateDocument(reloadlist);
            if (reloadlist) TheList.UpdateLoadedStuff();
            foreach(var a in SingleLayers)
            {
                a.UpdateDocument(reloadlist);
            }
        }
        public void ClearDisplays()
        {
            foreach (var a in SingleLayers)
            {
                a.DockHandler.DockPanel = null;
                a.Close();
                
            }
            TheTopDisplay.ClearCache(true);
            TheBottomDisplay.ClearCache(true);
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
                if ((e.KeyState & 8) == 8)
                {
                    e.Effect = DragDropEffects.Copy;
                }
                else
                {
                    e.Effect = DragDropEffects.Link;
                }
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
                if (Document.Gerbers.Count >0)
                {
                    //if (MessageBox.Show("Clear first?", "Clear?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    if ((e.KeyState & 8) != 8)
                    {
                        
                            ClearAll();   
                    }
                }
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

        internal void SetMouseCoord(float x, float y)
        {
            Document.CrossHairActive = true;
            Document.MouseX = x;
            Document.MouseY = y;
            UpdateAll(false);

        }

        internal void MouseOut()
        {
            Document.CrossHairActive = false;
            UpdateAll(false);
            
        }

        internal void ActivateTab(int rowIndex)
        {
            Document.Gerbers[rowIndex].Panel.Activate();
        }
    }
}
