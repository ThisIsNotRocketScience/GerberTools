using GerberLibrary;
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
        
        private FlowLayoutPanel buttonPanel = new FlowLayoutPanel();
        private DataGridView dataGridView1 = new DataGridView();
        private Button addNewRowButton = new Button();
        private Button deleteRowButton = new Button();
        private Button clearButton = new Button();
        private Button save2ImgButton = new Button();
        private ProgressLog _log;

        public LayerList(GerberViewerMainForm parent,  LoadedStuff doc,ProgressLog log)
        {

            ParentGerberViewerForm = parent;
            Document = doc;
            CloseButton = false;
            CloseButtonVisible = false;

            InitializeComponent();
            UpdateLoadedStuff();
         
            CloseButton = false;
            CloseButtonVisible = false;
            _log = log;

            Init();
        }

        private void SetupLayout()
        {

            addNewRowButton.Text = "Add";
            addNewRowButton.Location = new Point(10, 10);
            addNewRowButton.Click += new EventHandler(AddGerberFile);

            deleteRowButton.Text = "Remove";
            deleteRowButton.Location = new Point(100, 10);
            deleteRowButton.Click += new EventHandler(RemoveGerberFile);

            clearButton.Text = "Remove All";
            clearButton.Location = new Point(100, 10);
            clearButton.Click += new EventHandler(ClearAllButtonClick);

            save2ImgButton.Text = "Save Selected File To Image";
            save2ImgButton.Location = new Point(200, 10);
            save2ImgButton.AutoSize = true;
            save2ImgButton.Click += new EventHandler(SaveSelectedFile2Img);

            buttonPanel.Controls.Add(addNewRowButton);
            buttonPanel.Controls.Add(deleteRowButton);
            buttonPanel.Controls.Add(clearButton);
            buttonPanel.Controls.Add(save2ImgButton);
            buttonPanel.Height = 50;
            buttonPanel.Dock = DockStyle.Bottom;

            this.Controls.Add(this.buttonPanel);
        }

        private void SaveSelectedFile2Img(object sender, EventArgs e)
        {
            if (_curRowIndex >= 0)
            {
                string filepath = Document.Gerbers[_curRowIndex].File.Name;
                string savepath = CreateImageForSingleFile(filepath, Color.Black, Color.White, _log);
                MessageBox.Show("Image saved: " + savepath);
            }
            MessageBox.Show("Please choose a file first !");
        }

        private string CreateImageForSingleFile(string arg, Color Foreground, Color Background,ProgressLog log)
        {
            string savePath = string.Empty;
            int dpi = 720;
            if (arg.ToLower().EndsWith(".png") == true) return null;
            GerberImageCreator.AA = false;
            //Gerber.Verbose = true;
            if (Gerber.ThrowExceptions)
            {
                Gerber.SaveGerberFileToImageUnsafe(log, arg, arg + "_render.png", dpi, Foreground, Background);
                savePath = arg + "_render.png";
            }
            else
            {
                Gerber.SaveGerberFileToImage(log, arg, arg + "_render.png", dpi, Foreground, Background);
                savePath = arg + "_render.png";
            }

            if (Gerber.SaveDebugImageOutput)
            {
                Gerber.SaveDebugImage(arg, arg + "_debugviz.png", dpi, Foreground, Background, log);
                savePath = arg + "_debugviz.png";
            }
            return savePath;
        }
        private void RemoveGerberFile(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            MessageBox.Show("Sorry, this feature has not been implemented yet.");
        }

        private void AddGerberFile(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            MessageBox.Show("Sorry, this feature has not been implemented yet."+Environment.NewLine
                + "You can directly drag files to the list area on the left to add files.");
        }

        public void Init()
        {
            SetupLayout();
            SetupDataGridView();
        }
        private void SetupDataGridView()
        {
            this.Controls.Add(dataGridView1);

            dataGridView1.ColumnCount = 4;

            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.Navy;
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font =
                new Font(dataGridView1.Font, FontStyle.Bold);

            dataGridView1.Name = "songsDataGridView";
            dataGridView1.Location = new Point(8, 8);
            dataGridView1.Size = new Size(500, 250);
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
            dataGridView1.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            dataGridView1.GridColor = Color.DarkGray;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.CellClick += DataGridView1_CellClick;
            dataGridView1.Columns[0].Name = "Colour";
            dataGridView1.Columns[0].HeaderText = "";
            dataGridView1.Columns[0].Width = 20;

            dataGridView1.Columns[1].Name = "Name";

            dataGridView1.Columns[2].Name = "Layer";
            dataGridView1.Columns[2].Width = 80;

            dataGridView1.Columns[3].Name = "Side";
            dataGridView1.Columns[3].Width = 80;

            DataGridViewCheckBoxColumn VisibilityColumn = new DataGridViewCheckBoxColumn();

            VisibilityColumn.HeaderText = "";
            VisibilityColumn.FalseValue = "0";
            VisibilityColumn.Width = 20;           
            VisibilityColumn.TrueValue = "1";
            dataGridView1.Columns.Insert(0, VisibilityColumn);


            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.Dock = DockStyle.Fill;
            dataGridView1.CellPainting += DataGridView1_CellPainting;

         //   dataGridView1.CellFormatting += new DataGridViewCellFormattingEventHandler(songsDataGridView_CellFormatting);
        }

        private int _curRowIndex = -1;
        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex>= 0)
            {
                ParentGerberViewerForm.ActivateTab(e.RowIndex);
                _curRowIndex = e.RowIndex;
            }
        }

        private void DataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex == dataGridView1.Columns["Colour"].Index && e.RowIndex >= 0)
            {
                if (e.Value != null)
                {
                    var C = Color.FromArgb(int.Parse((string)e.Value));
                    Rectangle newRect = new Rectangle(e.CellBounds.X + 1,
               e.CellBounds.Y + 1, e.CellBounds.Width - 4,
               e.CellBounds.Height - 4);

                    using (
                        Brush gridBrush = new SolidBrush(this.dataGridView1.GridColor),
                        backColorBrush = new SolidBrush(C))
                    {
                        using (Pen gridLinePen = new Pen(gridBrush))
                        {
                            // Erase the cell.
                            e.Graphics.FillRectangle(backColorBrush, e.CellBounds);
                        }
                    }
                    e.Handled = true;
                }

            }
        }

        class CustomListViewItem : ListViewItem {
            public LoadedStuff.DisplayGerber Gerb;

        }
        private void ListView1_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            var G = e.Graphics;
            
            CustomListViewItem clvi = e.Item as CustomListViewItem;
            if (clvi == null) return;

            if (clvi.Selected)
            {
                G.FillRectangle(Brushes.Blue, e.Bounds);

            }

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
            dataGridView1.Rows.Clear();
            foreach(var a in Document.Gerbers)
            {
                List<string> V = new List<string>();
                V.Add(a.visible ? "1" : "0");
                V.Add(a.Color.ToArgb().ToString());
                V.Add(Path.GetFileName(a.File.Name));
                V.Add(a.File.Layer.ToString());
                V.Add(a.File.Side.ToString());

                dataGridView1.Rows.Add(V.ToArray());
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

        private void dataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (this.dataGridView1.Columns["Colour"].Index == e.ColumnIndex && e.RowIndex >= 0)
            {
                Rectangle newRect = new Rectangle(e.CellBounds.X + 1,
                    e.CellBounds.Y + 1, e.CellBounds.Width - 4,
                    e.CellBounds.Height - 4);

                using (
                    Brush gridBrush = new SolidBrush(this.dataGridView1.GridColor),
                    backColorBrush = new SolidBrush(e.CellStyle.BackColor))
                {
                    using (Pen gridLinePen = new Pen(gridBrush))
                    {
                        // Erase the cell.
                        e.Graphics.FillRectangle(backColorBrush, e.CellBounds);

                        // Draw the grid lines (only the right and bottom lines;
                        // DataGridView takes care of the others).
                        e.Graphics.DrawLine(gridLinePen, e.CellBounds.Left,
                            e.CellBounds.Bottom - 1, e.CellBounds.Right - 1,
                            e.CellBounds.Bottom - 1);
                        e.Graphics.DrawLine(gridLinePen, e.CellBounds.Right - 1,
                            e.CellBounds.Top, e.CellBounds.Right - 1,
                            e.CellBounds.Bottom);

                        // Draw the inset highlight box.
                        e.Graphics.DrawRectangle(Pens.Blue, newRect);

                        // Draw the text content of the cell, ignoring alignment.
                        if (e.Value != null)
                        {
                            e.Graphics.DrawString((String)e.Value, e.CellStyle.Font,
                                Brushes.Crimson, e.CellBounds.X + 2,
                                e.CellBounds.Y + 2, StringFormat.GenericDefault);
                        }
                        e.Handled = true;
                    }
                }
            }
        }
    }
}
