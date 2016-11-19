using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GerberLibrary
{
    public static class MathHelpers
    {
        //Compute the dot product AB . AC
        public static double DotProduct(PointF pointA, PointF pointB, PointF pointC)
        {
            double[] AB = new double[2];
            double[] BC = new double[2];
            AB[0] = pointB.X - pointA.X;
            AB[1] = pointB.Y - pointA.Y;
            BC[0] = pointC.X - pointB.X;
            BC[1] = pointC.Y - pointB.Y;
            double dot = AB[0] * BC[0] + AB[1] * BC[1];

            return dot;
        }

        //Compute the cross product AB x AC
        public static double CrossProduct(PointF pointA, PointF pointB, PointF pointC)
        {
            double[] AB = new double[2];
            double[] AC = new double[2];
            AB[0] = pointB.X - pointA.X;
            AB[1] = pointB.Y - pointA.Y;
            AC[0] = pointC.X - pointA.X;
            AC[1] = pointC.Y - pointA.Y;
            double cross = AB[0] * AC[1] - AB[1] * AC[0];

            return cross;
        }

        //Compute the distance from A to B
        public static double Distance(PointF pointA, PointF pointB)
        {
            double d1 = pointA.X - pointB.X;
            double d2 = pointA.Y - pointB.Y;

            return Math.Sqrt(d1 * d1 + d2 * d2);
        }

        //Compute the distance from AB to C
        //if isSegment is true, AB is a segment, not a line.
        public static double LineToPointDistance2D(PointF pointA, PointF pointB, PointF pointC, bool isSegment)
        {
            double dist = CrossProduct(pointA, pointB, pointC) / Distance(pointA, pointB);
            if (isSegment)
            {
                double dot1 = DotProduct(pointA, pointB, pointC);
                if (dot1 > 0) return Distance(pointB, pointC);

                double dot2 = DotProduct(pointB, pointA, pointC);
                if (dot2 > 0) return Distance(pointA, pointC);
            }
            return Math.Abs(dist);
        }

        public static PointF Scale(PointF X, double V)
        {
            return new PointF((float)(X.X * V), (float)(X.Y * V));

        }

        public static double Angle(PointF A, PointF B)
        {
            var dx = B.X - A.X;
            var dy = B.Y - A.Y;
            return Math.Atan2(dy, dx);
        }

        public static PointF ScaleRelativeTo(PointF center, PointF X, double v)
        {
            var dx = X.X - center.X;
            var dy = X.Y - center.Y;

            return new PointF((float)(center.X + dx * v), (float)(center.Y + dy * v));
        }

       
        public static Color Interpolate(Color color1, Color color2, float v)
        {
            float iv = 1.0f - v;
            var R = (byte)(color1.R * iv) + (byte)(color2.R * v);
            var G = (byte)(color1.G * iv) + (byte)(color2.G * v);
            var B = (byte)(color1.B * iv) + (byte)(color2.B * v);
            var A = (byte)(color1.A * iv) + (byte)(color2.A * v);
            return Color.FromArgb(A, R, G, B);
        }

        public static double Length(PointF b)
        {
            return Math.Sqrt(b.X * b.X + b.Y * b.Y);
        }

        public static List<PointF> MakeOffsetPoly(List<PointF> inpoly, double distance, bool backwards)
        {
            if (backwards)
            {
                inpoly.Reverse();
            }
            List<PointF> Res = new List<PointF>();
            for (int i = 0; i < inpoly.Count; i++)
            {
                int idx1 = i % inpoly.Count;
                int idx2 = (i + 1) % inpoly.Count;
                int idx3 = (i + 2) % inpoly.Count;
                var A = inpoly[idx1];
                var B = inpoly[idx2];
                var C = inpoly[idx3];
                var N1 = Normal(A, B);
                var N2 = Normal(B, C);

                Res.Add(AddScaled(A, N1, distance));
                Res.Add(AddScaled(B, N1, distance));
                //                Res.Add(AddScaled(B, N2, clearance));

            }
            Res.Add(Res[0]);
            return Res;

        }

        public static PointF Normal(PointF p1, PointF p2)
        {
            var L = Distance(p1, p2);
            var D = Difference(p2, p1);
            return new PointF((float)(D.Y / L), (float)(-D.X / L));
        }

        public static PointF Difference(PointF p1, PointF p2)
        {
            return new PointF(p1.X - p2.X, p1.Y - p2.Y);
        }

        public static PointF Add(PointF p1, PointF p2)
        {
            return new PointF(p1.X + p2.X, p1.Y + p2.Y);
        }      

        public static PointF Sub(PointF force, PointF d)
        {
            return new PointF(force.X - d.X, force.Y - d.Y);
        }

        public static PointF AddScaled(PointF p1, PointF p2, double V)
        {
            return new PointF((float)(p1.X + p2.X * V), (float)(p1.Y + p2.Y * V));
        }

        public static Tuple<PointF, PointF> RungeKutta4(PointF x, PointF v, Func<PointF, PointF, double, PointF> a, double dt)
        {

            var x1 = x;
            var v1 = v;
            var a1 = a(x1, v1, 0);

            var x2 = AddScaled(x, v1, 0.5 * dt);
            var v2 = AddScaled(v, a1, dt * 0.5);
            var a2 = a(x2, v2, dt / 2);

            var x3 = AddScaled(x, v2, 0.5 * dt);
            var v3 = AddScaled(v, a2, 0.5 * dt);
            var a3 = a(x3, v3, dt / 2);

            var x4 = AddScaled(x, v3, dt);
            var v4 = AddScaled(v, a3, dt);
            var a4 = a(x4, v4, dt);

            var xf = AddScaled(x, (Add(AddScaled(AddScaled(v1, v2, 2), v3, 2), v4)), (dt / 6));
            var vf = AddScaled(v, (Add(AddScaled(AddScaled(a1, a2, 2), a3, 2), a4)), (dt / 6));

            return new Tuple<PointF, PointF>(xf, vf);
        }
    }
}
