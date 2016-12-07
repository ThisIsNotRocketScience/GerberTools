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

namespace GerberViewer
{
    public partial class LayerList : WeifenLuo.WinFormsUI.Docking.DockContent
    {
        LoadedStuff Document;
        GerberViewerMainForm ParentGerberViewerForm;
        public LayerList(GerberViewerMainForm parent,  LoadedStuff doc)
        {

            ParentGerberViewerForm = parent;
            Document = doc;
            InitializeComponent();
            UpdateLoadedStuff();
            listView1.CheckBoxes = true;
            listView1.OwnerDraw = true;
            listView1.DrawItem += ListView1_DrawItem;

            CloseButton = false;
            CloseButtonVisible = false;


        }

        class CustomListViewItem : ListViewItem {
            public LoadedStuff.DisplayGerber Gerb;

        }
        private void ListView1_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            var G = e.Graphics;
            
            CustomListViewItem clvi = e.Item as CustomListViewItem;
            if (clvi == null) return;
            if (clvi.Gerb.visible)
            {
                G.FillRectangle(new SolidBrush(clvi.Gerb.Color), new Rectangle(e.Bounds.Left + 1, e.Bounds.Top + 1, 10, 10));
            }
            G.DrawRectangle(new Pen(Color.FromArgb(20,20,20),1), new Rectangle(e.Bounds.Left+1, e.Bounds.Top+1, 10, 10));
            G.DrawString(Path.GetFileName(clvi.Gerb.File.Name), new Font("Arial" ,10), Brushes.Black, e.Bounds.Left+13,e.Bounds.Top);
            
        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        public void UpdateLoadedStuff()
        {
            listView1.Items.Clear();
            foreach(var a in Document.Gerbers)
            {
                listView1.Items.Add(new CustomListViewItem() { Gerb = a });
            };
        }

        private void LayerList_DragEnter(object sender, DragEventArgs e)
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

        private void LayerList_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {

                string[] D = e.Data.GetData(DataFormats.FileDrop) as string[];
                List<String> files = new List<string>();
                foreach (string S in D)
                {
                    if (Directory.Exists(S))
                    {
                        ParentGerberViewerForm.LoadGerberFolder(Directory.GetFiles(S).ToList());
                    }
                    else
                    {
                        if (File.Exists(S)) files.Add(S);
                    }
                }
                if (files.Count > 0)
                {
                    ParentGerberViewerForm.LoadGerberFolder(files);
                }
            }
        }

        private void ClearAllButtonClick(object sender, EventArgs e)
        {
            ParentGerberViewerForm.ClearAll();
        }
    }
}
