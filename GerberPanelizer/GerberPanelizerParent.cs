using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GerberCombinerBuilder
{
    public partial class GerberPanelizerParent : Form
    {
        private int childFormNumber = 0;

        public Treeview TV;
        public InstanceDialog ID;
        public GerberPanelize ActivePanelizeInstance = null;


        public class ControlWriter : TextWriter
        {
            public string T = "";
            public int idx;
            bool sink = true;
            public ControlWriter()
            {

            }

            public override void Write(char value)
            {
                return;
                if (sink) return;
                idx++;
                T += value;
                
            }

            public override void Write(string value)
            {
                if (sink) return;
                idx++;
                T += value;
            }

            public override Encoding Encoding
            {
                get { return Encoding.ASCII; }
            }
        }
        ControlWriter CW = new ControlWriter();

        public GerberPanelizerParent()
        {
            Console.SetOut(CW);

            InitializeComponent();
            TV = new Treeview();
            //TV.MdiParent = this;
            TV.Show();
            TV.TopLevel = false;
            TV.Dock = DockStyle.Fill;
            panel3.Controls.Add(TV);
            ID = new InstanceDialog();
            ID.Dock = DockStyle.Fill;
            ID.Show();
            ID.TopLevel = false;
            panel4.Controls.Add(ID);
            //ID.Dock = DockStyle.Left;
            //TV.Dock = DockStyle.Right;
            TV.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            ID.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            
            RemovePanelizer();

            
            //   Explanation_And_Warning EAW = new Explanation_And_Warning();
            //   EAW.Show();
            //   EAW.TopMost = true;

            //RegistryKey key = Registry.LocalMachine.OpenSubKey("CurrentUser", true);

            //key.CreateSubKey("NotRocketSciencePanelizer");
            //key = key.OpenSubKey("NotRocketSciencePanelizer", true);


            //key.CreateSubKey("0.1");
            //key = key.OpenSubKey("0.1", true);
            //int tr = (int)key.GetValue("TimesRun", (int)0);
            //key.SetValue("TimesRun", (int)(timesrun+1), RegistryValueKind.DWord);

            //if (timesrun == 0)
            //{
            //    string curDir = Directory.GetCurrentDirectory();
            //    string Url = String.Format("file:///{0}/Help/welcome.html", curDir);
            //    Process.Start(Url);
            //}
          
            
            
            // Add Edit Menu programmatically
            ToolStripMenuItem editMenu = new ToolStripMenuItem("&Edit");
            
            ToolStripMenuItem undoItem = new ToolStripMenuItem("&Undo", null, UndoToolStripMenuItem_Click);
            undoItem.ShortcutKeys = Keys.Control | Keys.Z;
            
            ToolStripMenuItem redoItem = new ToolStripMenuItem("&Redo", null, RedoToolStripMenuItem_Click);
            redoItem.ShortcutKeys = Keys.Control | Keys.Y;

            ToolStripMenuItem cutItem = new ToolStripMenuItem("Cu&t", null, CutToolStripMenuItem_Click);
            cutItem.ShortcutKeys = Keys.Control | Keys.X;

            ToolStripMenuItem copyItem = new ToolStripMenuItem("&Copy", null, CopyToolStripMenuItem_Click);
            copyItem.ShortcutKeys = Keys.Control | Keys.C;

            ToolStripMenuItem pasteItem = new ToolStripMenuItem("&Paste", null, PasteToolStripMenuItem_Click);
            pasteItem.ShortcutKeys = Keys.Control | Keys.V;

            ToolStripMenuItem deleteItem = new ToolStripMenuItem("&Delete", null, DeleteToolStripMenuItem_Click);
            deleteItem.ShortcutKeys = Keys.Delete;

            editMenu.DropDownItems.Add(undoItem);
            editMenu.DropDownItems.Add(redoItem);
            editMenu.DropDownItems.Add(new ToolStripSeparator());
            editMenu.DropDownItems.Add(cutItem);
            editMenu.DropDownItems.Add(copyItem);
            editMenu.DropDownItems.Add(pasteItem);
            editMenu.DropDownItems.Add(deleteItem);

            menuStrip.Items.Insert(1, editMenu);
        }
        public static int timesrun = 0;

        private void ShowNewForm(object sender, EventArgs e)
        {
            GerberPanelize childForm = new GerberPanelize(this, TV, ID);
            childForm.MdiParent = this;
            childForm.Show();
            childForm.ZoomToFit();
            ActivePanelizeInstance = childForm;
        }

        private void OpenFile(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            openFileDialog.Filter = "Gerber Set Files (*.gerberset)|*.gerberset|All Files (*.*)|*.*";
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                string FileName = openFileDialog.FileName;
                GerberPanelize childForm = new GerberPanelize(this, TV, ID);
                childForm.MdiParent = this;
                childForm.Show();
                childForm.LoadFile(FileName);
                childForm.ZoomToFit();
                ActivePanelizeInstance = childForm;                
            }
        }

        private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            saveFileDialog.Filter = "Gerber Set Files (*.gerberset)|*.gerberset|All Files (*.*)|*.*";
            if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                string FileName = saveFileDialog.FileName;
                if (ActivePanelizeInstance != null)
                {
                    ActivePanelizeInstance.SaveFile(FileName);
                }
            }
        }

        private void ExitToolsStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void CutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ActivePanelizeInstance != null) ActivePanelizeInstance.CutSelection();
        }

        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ActivePanelizeInstance != null) ActivePanelizeInstance.CopySelection();
        }

        private void PasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ActivePanelizeInstance != null) ActivePanelizeInstance.PasteSelection();
        }
        
        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
             if (ActivePanelizeInstance != null) ActivePanelizeInstance.DeleteSelection();
        }

        private void UndoToolStripMenuItem_Click(object sender, EventArgs e)
        {
             if (ActivePanelizeInstance != null) ActivePanelizeInstance.PerformUndo();
        }

        private void RedoToolStripMenuItem_Click(object sender, EventArgs e)
        {
             if (ActivePanelizeInstance != null) ActivePanelizeInstance.PerformRedo();
        }

        

        private void CascadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.Cascade);
        }

        private void TileVerticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileVertical);
        }

        private void TileHorizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileHorizontal);
        }

        private void ArrangeIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.ArrangeIcons);
        }

        private void CloseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form childForm in MdiChildren)
            {
                childForm.Close();
            }
        }

        public void ActivatePanelizer(GerberPanelize act)
        {
            ActivePanelizeInstance = act;
            saveToolStripMenuItem.Enabled = true;
            saveAsToolStripMenuItem.Enabled = true;
            exportMergedGerbersToolStripMenuItem.Enabled = true;
        }


        internal void RemovePanelizer()
        {
            saveToolStripMenuItem.Enabled = false;
            saveAsToolStripMenuItem.Enabled = false;
            exportMergedGerbersToolStripMenuItem.Enabled = false;
       
            ActivePanelizeInstance = null;
            TV.BuildTree(null, null);
            ID.UpdateBoxes(null);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 AB = new AboutBox1();
            AB.ShowDialog();
        }

        private void exportMergedGerbersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ActivePanelizeInstance != null)
            {
                ActivePanelizeInstance.exportAllGerbersToolStripMenuItem_Click(null, null);
            }
        }

        private void GerberPanelizerParent_DragEnter(object sender, DragEventArgs e)
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

        private void GerberPanelizerParent_DragDrop(object sender, DragEventArgs e)
        {
             if (e.Data.GetDataPresent(DataFormats.FileDrop))
             {
                 GerberPanelize childForm = new GerberPanelize(this, TV, ID);
                 childForm.MdiParent = this;
                 childForm.Show();
                 childForm.ZoomToFit();
                 childForm.glControl1_DragDrop(sender, e);
                 ActivePanelizeInstance = childForm;
                 childForm.ThePanel.MaxRectPack();
                    childForm.ThePanel.BuildAutoTabs(new GerberLibrary.StandardConsoleLog());
                 childForm.ZoomToFit();
                 
                 childForm.Redraw(true);
             }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ActivePanelizeInstance != null)
            {
                if (ActivePanelizeInstance.LoadedFile == "")
                {
                    SaveAsToolStripMenuItem_Click(sender, e);
                }
                else
                {
                    ActivePanelizeInstance.SaveFile(ActivePanelizeInstance.LoadedFile);
                }
            }
        }

        private void helpWorkflowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string curDir = Directory.GetCurrentDirectory();
            string Url = String.Format("file:///{0}/Help/welcome.html", curDir);
            Process.Start(Url);
        }
    }
}
