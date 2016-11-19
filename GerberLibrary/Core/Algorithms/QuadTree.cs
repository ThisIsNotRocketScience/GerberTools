using GerberLibrary.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GerberLibrary.Core.Algorithms
{
    public interface QuadTreeItem
    {
        double x { get;  }
        double y { get; }
    }

    public class QuadTreeNode
    {
        int contained = 0;
        public bool CallBackInside(RectangleF S, Func<QuadTreeItem, bool> callback)
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
                    if (S.Contains((float)a.x, (float)a.y))
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

        public void Insert(QuadTreeItem Item, int maxdepth)
        {
            Insert(Item.x, Item.y, Item, maxdepth);
        }


        public void Insert(double x, double y, QuadTreeItem Item, int maxdepth)
        {
            if (x < xstart || y < ystart || x >= xend || y >= yend)
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
            double halfy = (yend + ystart) / 2.0;
            double halfx = (xend + xstart) / 2.0;

            QuadTreeNode QTN1 = new QuadTreeNode() { xstart = xstart, ystart = ystart, xend = halfx, yend = halfy };
            QuadTreeNode QTN2 = new QuadTreeNode() { xstart = halfx, ystart = ystart, xend = xend, yend = halfy };
            QuadTreeNode QTN3 = new QuadTreeNode() { xstart = halfx, ystart = halfy, xend = xend, yend = yend };
            QuadTreeNode QTN4 = new QuadTreeNode() { xstart = xstart, ystart = halfy, xend = halfx, yend = yend };
            Children.Add(QTN1);
            Children.Add(QTN2);
            Children.Add(QTN3);
            Children.Add(QTN4);
        }

        public double xstart = 0;
        public double ystart = 0;
        public double xend = 0;
        public double yend = 0;

        public void Draw(GraphicsInterface graphics)
        {
            graphics.DrawRectangle(contained > 0 ? Color.Red : Color.Yellow, (float)xstart, (float)ystart, (float)xend - (float)xstart , (float)yend - (float)ystart , (float)((xend-xstart)*0.001));
            foreach (var C in Children)
            {
                C.Draw(graphics);
            }
        }

        public void DrawArt(GraphicsInterface graphics, Color Col)
        {
            if (contained == 0)
            {
                graphics.FillRectangle(Col, (float)xstart, (float)ystart, (float)xend - (float)xstart - 1, (float)yend - (float)ystart - 1);
            }
            foreach (var C in Children)
            {
                C.DrawArt(graphics, Col);
            }
        }
    }
}


