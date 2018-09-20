using GerberLibrary;
using GerberLibrary.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Artwork
{
    public interface QuadTreeItem
    {
        int x { get; set; }
        int y { get; set; }
    }

    public class QuadTreeNode
    {
        int contained = 0;
        public bool CallBackInside(Rectangle S, Func<QuadTreeItem, bool> callback)
        {
            if (S.X >= xend || S.Y >= yend || S.X + S.Width < xstart || S.Y + S.Height < ystart)
            {
                return true;
            }
            else
            {
                for (int i = 0; i < Items.Count; i++)
                {
                    var a = Items[i];
                    if (S.Contains(a.x, a.y))
                    {
                        if (callback(a) == false) return false;
                    }
                }
                for (int i = 0; i < Children.Count; i++)
                {
                    if (Children[i].CallBackInside(S, callback) == false) return false;
                }
            }
            return true;
        }

        public List<QuadTreeNode> Children = new List<QuadTreeNode>(4);
        public List<QuadTreeItem> Items = new List<QuadTreeItem>(3);

        public void Insert(int x, int y, QuadTreeItem Item, int maxdepth)
        {
            if (x <= xstart || y <= ystart || x > xend || y > yend)
            {
                return;
            }
            contained++;
            if (maxdepth > 0)
            {
                if (Children.Count == 0) Split();
                for (int i = 0; i < Children.Count; i++)
                {
                    Children[i].Insert(x, y, Item, maxdepth - 1);
                }
            }
            else
            {
                Items.Add(Item);
            }
        }

        void Split()
        {
            int halfy = (yend + ystart) / 2;
            int halfx = (xend + xstart) / 2;

            QuadTreeNode QTN1 = new QuadTreeNode() { xstart = xstart, ystart = ystart, xend = halfx, yend = halfy };
            QuadTreeNode QTN2 = new QuadTreeNode() { xstart = halfx, ystart = ystart, xend = xend, yend = halfy };
            QuadTreeNode QTN3 = new QuadTreeNode() { xstart = halfx, ystart = halfy, xend = xend, yend = yend };
            QuadTreeNode QTN4 = new QuadTreeNode() { xstart = xstart, ystart = halfy, xend = halfx, yend = yend };
            Children.Add(QTN1);
            Children.Add(QTN2);
            Children.Add(QTN3);
            Children.Add(QTN4);
        }

        public int xstart = 0;
        public int ystart = 0;
        public int xend = 0;
        public int yend = 0;

        public void Draw(GraphicsInterface graphics, bool drawcontained = true)
        {
            if (contained > 0 && drawcontained == false)
            {

            }
            else
            {
                if (contained == 0)
                {
                    graphics.DrawRectangle(Color.Yellow, xstart, ystart, xend - xstart - 1, yend - ystart - 1);
                }
            }
            foreach (var C in Children)
            {
                C.Draw(graphics);
            }
        }

        public void DrawArt(Graphics graphics, Color Col)
        {
            if (contained == 0)
            {
                graphics.FillRectangle(new SolidBrush(Col), xstart, ystart, xend - xstart - 1, yend - ystart - 1);
            }
            foreach (var C in Children)
            {
                C.DrawArt(graphics, Col);
            }
        }

        public void NodeWalker(Action<QuadTreeNode> NodeCallback, bool childrenonly, bool nodeswithitems)
        {
            if (childrenonly == false || Children.Count == 0)
            {
                if (nodeswithitems)
                {
                    if (this.Items.Count == 0) NodeCallback(this);
                }
                else
                {
                    NodeCallback(this);
                }
            }

            if (Children.Count > 0)
            {
                foreach (var a in Children)
                {
                    a.NodeWalker(NodeCallback, childrenonly, nodeswithitems);
                }
            }
        }

        public QuadTreeNode GetNode(double x, double y)
        {
            if (x < xstart || y < ystart || x >= xend || y >= yend)
            {
                return null;
            }
            else
            {
                if (Children.Count > 0)
                {
                    foreach (var a in Children)
                    {
                        var c = a.GetNode(x, y);
                        if (c != null) return c;

                    }
                }
                else
                {
                    return this;
                }

            }
            return null;
        }
    }
}
