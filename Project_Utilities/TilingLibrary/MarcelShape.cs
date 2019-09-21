using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ClipperLib;


namespace ArtWork
{
    using Path = List<ClipperLib.IntPoint>;
    using Paths = List<List<ClipperLib.IntPoint>>;

    public class MarcelShape
    {
        public List<ClipperLib.IntPoint> Vertices = new List<ClipperLib.IntPoint>();

        public void  ShrinkFromShape(Path shape, float amt=7*3)
        {
            Path pp = new Path();
            pp.AddRange(shape);
            Paths pps = new Paths();
            pps.Add(pp);
            var Res = Clipper.OffsetPolygons(pps, -amt);

            if (Res.Count == 1)
            {
                Vertices.Clear();
                Vertices.AddRange(Res[0]);
            }
        }

        public Paths BuildOutlines(float growamt = 5*3)
        {
            
            ClipperLib.Clipper cp = new ClipperLib.Clipper();
            Path pp = new Path();
            pp.AddRange(Vertices);
            Paths pps = new Paths();
            pps.Add(pp);
            var Res = Clipper.OffsetPolygons(pps, growamt, JoinType.jtRound);


            return Res;
        }

        public  Paths BuildHoles(float radius = 3*3.0f/2.0f)
        {
            Paths pps = new Paths();
            foreach(var v in Vertices)
            {
                Path c = new Path();
                for(int i =0;i<20;i++)
                {
                    double P = (i * Math.PI * 2) / 20;
                    double x = Math.Cos(P) * radius + v.X;
                    double y = Math.Sin(P) * radius + v.Y;

                    c.Add(new IntPoint((long)x, (long)y));
                }

                pps.Add(c);
            }

            return pps;
        }
    }
}
