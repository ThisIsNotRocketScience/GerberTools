using GerberLibrary;
using GerberLibrary.Core.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace GerberLibrary
{
    public class RectangleD
    {
        //public PointD TopLeft = new PointD(0, 0);
        //public PointD BottomRight = new PointD(0, 0);
        public double Width;
        public double Height;
        public double X;
        public double Y;
        
        public RectangleD()
        {

        }
        public RectangleD(Bounds bounds)
        {
            X = bounds.TopLeft.X;
            Y = bounds.TopLeft.Y;
            Width = bounds.Width();
            Height = bounds.Height();
        }

        internal bool ContainsPoint(double x, double y)
        {
            if (x >= X && y >= Y && x < (X + Width) && y < (Y + Height)) return true;
            return false;
        }
    }

    public class RectanglePackerObject : RectangleD
    {
        public object ReferencedObject;
        public RectanglePackerObject lft;
        public RectanglePackerObject rgt;
        public bool used = false;

    }

    public class RectanglePacker
    {

        RectanglePackerObject root = new RectanglePackerObject();
        public List<RectanglePackerObject> AllNodes = new List<RectanglePackerObject>();

        public double GetEmptySpace()
        {
            double R = 0;
            foreach(var a in AllNodes)
            {
                if (a.used == true)
                {
                    R += a.Width * a.Height;
                }
            }
            return (root.Width*root.Height)- R;
        }

        private double usedHeight = 0;
        private double usedWidth = 0;


        public RectanglePacker(double width, double height)
        {
            reset(width, height);
            AllNodes.Add(root);
        }

        void reset(double width, double height)
        {
            root.X = 0;
            root.Y = 0;
            root.Width = width;
            root.Height = height;
            root.lft = null;
            root.rgt = null;

            usedWidth = 0;
            usedHeight = 0;
        }

        PointD getDimensions()
        {
            return new PointD(usedWidth, usedHeight);
        }

        RectanglePackerObject cloneNode(RectanglePackerObject n)
        {
            var P =new RectanglePackerObject() { ReferencedObject = n.ReferencedObject, X = n.X, Y = n.Y, Width = n.Width, Height = n.Height };
            AllNodes.Add(P);
            return P;
        }

        PointD recursiveFindCoords(RectanglePackerObject node, double w, double h)
        {
            if (node.lft != null)
            {
                var coords = recursiveFindCoords(node.lft, w, h);
                if (coords == null && (node.rgt != null))
                {
                    coords = recursiveFindCoords(node.rgt, w, h);
                }
                return coords;
            }
            else
            {
                if (node.used || w > node.Width || h > node.Height)
                    return null;

                if (w == node.Width && h == node.Height)
                {
                    node.used = true;
                    return new PointD(node.X, node.Y);

                }

                node.lft = cloneNode(node);
                node.rgt = cloneNode(node);

                if (node.Width - w > node.Height - h)
                {
                    node.lft.Width = w;
                    node.rgt.X = node.X + w;
                    node.rgt.Width = node.Width - w;
                }
                else
                {
                    node.lft.Height = h;
                    node.rgt.Y = node.Y + h;
                    node.rgt.Height = node.Height - h;
                }

                return recursiveFindCoords(node.lft, w, h);
            }
        }

        public PointD findCoords(double w, double h)
        {
            var coords = recursiveFindCoords(root, w, h);
            // var_dump(root);

            if (coords != null)
            {
                if (usedWidth < coords.X + w) usedWidth = coords.X + w;
                if (usedHeight < coords.Y + h) usedHeight = coords.Y + h;
            }
            return coords;
        }
    }
}
