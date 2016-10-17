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

namespace GerberCombinerBuilder
{
    public partial class Treeview : Form
    {
        TreeNode Gerbers;
        TreeNode BreakTabs;
        TreeNode RootNode;
        GerberPanelize TargetHost;
        public Treeview()
        {
            InitializeComponent();

            Gerbers = new TreeNode("Gerbers");
            BreakTabs = new TreeNode("Breaktabs");

            TreeNode[] array = new TreeNode[] { Gerbers, BreakTabs };

            RootNode = new TreeNode("Board", array);
            treeView1.Nodes.Add(RootNode);
            treeView1.ExpandAll();
            treeView1.Enabled = false;
        }
        class InstanceTreeNode : TreeNode
        {
            public AngledThing TargetInstance;

            public InstanceTreeNode(AngledThing GI)
                : base("instance")
            {
                TargetInstance = GI;
                Text = ToString();
            }
            public override string ToString()
            {
                if (TargetInstance.GetType() == typeof(GerberInstance))
                {
                    return String.Format("Instance: {0} {1},{2} {3}", Path.GetFileNameWithoutExtension((TargetInstance as GerberInstance).GerberPath), TargetInstance.Center.X, TargetInstance.Center.Y, TargetInstance.Angle);
                }
                else
                {
                    return "tab";
                }
            }
        }

        class GerberFileNode : TreeNode
        {
            public string pPath;
            public GerberFileNode(string path)
                : base("gerber")
            {
                pPath = path;
                Text = ToString();
            }
            public override string ToString()
            {
                return Path.GetFileNameWithoutExtension(pPath);
            }
        }

        public void BuildTree(GerberPanelize Parent,  GerberLayoutSet S)
        {
            TargetHost = Parent;
            if (TargetHost == null) { treeView1.Enabled = false; return; } else { treeView1.Enabled = true; };
            while (Gerbers.Nodes.Count > 0)
            {
                Gerbers.Nodes[0].Remove();
            }
            while (BreakTabs.Nodes.Count > 0)
            {
                BreakTabs.Nodes[0].Remove();
            }
            foreach (var a in S.LoadedOutlines)
            {
                Gerbers.Nodes.Add(new GerberFileNode(a));
            }

            foreach (var a in S.Instances)
            {
                foreach (GerberFileNode t in Gerbers.Nodes)
                {
                    if (t.pPath == a.GerberPath)
                    {
                        t.Nodes.Add(new InstanceTreeNode(a));
                    }
                }

            }


            foreach (var t in S.Tabs)
            {
                BreakTabs.Nodes.Add(new InstanceTreeNode(t));
            }

            treeView1.ExpandAll();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (treeView1.SelectedNode.GetType() == typeof(InstanceTreeNode))
            {
                TargetHost.SetSelectedInstance((treeView1.SelectedNode as InstanceTreeNode).TargetInstance);
            }
        }

        private void treeView1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                TreeNode node = treeView1.GetNodeAt(e.Location);
                if (node.GetType() == typeof(InstanceTreeNode))
                {
                    var P = PointToScreen(new Point(e.X, e.Y));
                    contextMenuStrip1.Show(P);
                    treeView1.SelectedNode = node;
                }
                if (node.GetType() == typeof(GerberFileNode))
                {
                    var P = PointToScreen(new Point(e.X, e.Y));
                    contextMenuStrip1.Show(P);
                    treeView1.SelectedNode = node;
                }
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.GetType() == typeof(InstanceTreeNode))
            {
                TargetHost.RemoveInstance((treeView1.SelectedNode as InstanceTreeNode).TargetInstance);
            }
        }

        private void addInstanceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = "";
            if (treeView1.SelectedNode.GetType() == typeof(GerberFileNode))
            {
                path = (treeView1.SelectedNode as GerberFileNode).pPath;
            }
            if (treeView1.SelectedNode.GetType() == typeof(InstanceTreeNode))
            {
                if ((treeView1.SelectedNode as InstanceTreeNode).TargetInstance.GetType() == typeof(GerberInstance))
                {
                    path = ((treeView1.SelectedNode as InstanceTreeNode).TargetInstance as GerberInstance).GerberPath;
                }
            }
            if (path.Length > 0)
            {
                TargetHost.AddInstance(path, new GerberLibrary.Core.Primitives.PointD(0,0));
            }
        }

        private void exportBoardImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = "";
            if (treeView1.SelectedNode.GetType() == typeof(GerberFileNode))
            {
                path = (treeView1.SelectedNode as GerberFileNode).pPath;
            }
            if (treeView1.SelectedNode.GetType() == typeof(InstanceTreeNode))
            {
                if ((treeView1.SelectedNode as InstanceTreeNode).TargetInstance.GetType() == typeof(GerberInstance))
                {
                    path = ((treeView1.SelectedNode as InstanceTreeNode).TargetInstance as GerberInstance).GerberPath;
                }
            }
            if (path.Length > 0)
            {

                try
                {
                    System.Windows.Forms.SaveFileDialog OFD = new System.Windows.Forms.SaveFileDialog();

                    OFD.DefaultExt = "";
                    if (OFD.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
                    Console.WriteLine("path selected: {0}", path);

                    GerberImageCreator GIC = new GerberImageCreator();
                    foreach (var a in Directory.GetFiles(path, "*.*"))
                    {
                    }


                    foreach (var a in Directory.GetFiles(path, "*.*"))
                    {
                        GIC.AddBoardToSet(a);
                    }

                    GIC.WriteImageFiles(OFD.FileName, showimage: Gerber.DirectlyShowGeneratedBoardImages);

                }
                catch(Exception)
                { }
            }
        }
    }
}
