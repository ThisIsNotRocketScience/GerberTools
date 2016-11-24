using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GerberLibrary.Core.Primitives
{
    public class PointD
    {
        ///Compute the dot product AB . AC
        public static double DotProduct(PointD pointA, PointD pointB, PointD pointC)
        {
            PointD AB = new PointD();
            PointD BC = new PointD();
            AB.X = pointB.X - pointA.X;
            AB.Y = pointB.Y - pointA.Y;
            BC.X = pointC.X - pointB.X;
            BC.Y = pointC.Y - pointB.Y;
            double dot = AB.X * BC.X + AB.Y * BC.Y;

            return dot;
        }

        ///Compute the distance from A to B
        public static double Distance(PointD pointA, PointD pointB)
        {
            double d1 = pointA.X - pointB.X;
            double d2 = pointA.Y - pointB.Y;

            return Math.Sqrt(d1 * d1 + d2 * d2);
        }

        public static double LineToPointDistance2D(PointD pointA, PointD pointB, PointD pointC, bool isSegment)
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


        ///Compute the cross product AB x AC
        public static double CrossProduct(PointD pointA, PointD pointB, PointD pointC)
        {
            PointD AB = new PointD();
            PointD AC = new PointD();
            AB.X = pointB.X - pointA.X;
            AB.Y = pointB.Y - pointA.Y;
            AC.X = pointC.X - pointA.X;
            AC.Y = pointC.Y - pointA.Y;
            double cross = AB.X * AC.Y - AB.Y * AC.X;

            return cross;
        }

        public static PointD GetClosestPointOnLineSegment(PointD A, PointD B, PointD P)
        {
            PointD AP = P - A;       //Vector from A to P   
            PointD AB = B - A;       //Vector from A to B  

            double magnitudeAB = AB.LengthSquared();     //Magnitude of AB vector (it's length squared)     
            double ABAPproduct = AP.Dot(AB);    //The DOT product of a_to_p and a_to_b     
            double distance = ABAPproduct / magnitudeAB; //The normalized "distance" from a to your closest point  

            if (distance < 0)     //Check if P projection is over vectorAB     
            {
                return A;

            }
            else if (distance > 1)
            {
                return B;
            }
            else
            {
                return A + AB * distance;
            }
        }

        private double LengthSquared()
        {
            return X * X + Y * Y;
        }

        public PointD(double _x = 0, double _y = 0)
        {
            X = _x;
            Y = _y;
        }
        public PointD(float _x, float _y)
        {
            X = _x;
            Y = _y;
        }
        public double Dot(PointD B)
        {
            return X * B.X + Y * B.Y;
        }
        public PointD(PointF pointF)
        {
            X = pointF.X;
            Y = pointF.Y;
        }
        public double X;
        public double Y;

        public override bool Equals(object B)
        {
            if (base.Equals(B)) return true;
            if (B.GetType() == typeof(PointD))
            {
                PointD pB = B as PointD;
                if (pB.X == X && pB.Y == Y) return true;
            }
            return false;
        }

        public static bool operator ==(PointD a, PointD b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(PointD x, PointD y)
        {
            return !(x == y);
        }

        public static PointD operator +(PointD a, PointD b)
        {
            return new PointD(a.X + b.X, a.Y + b.Y);
        }
        public static PointD operator -(PointD a, PointD b)
        {
            return new PointD(a.X - b.X, a.Y - b.Y);
        }
        public static PointD operator *(PointD a, double b)
        {
            return new PointD(a.X * b, a.Y * b);
        }
        public static PointD operator *(PointD a, float b)
        {
            return new PointD(a.X * b, a.Y * b);
        }
        public override string ToString()
        {
            return String.Format("{0:N2},{1:N2}", X, Y);
        }

        public PointF ToF()
        {
            return new PointF((float)X, (float)Y);
        }

        public void Normalize()
        {
            double L = Length();
            if (L > 0)
            {
                X /= L;
                Y /= L;
            }
        }

        public double Length()
        {
            return Math.Sqrt(X * X + Y * Y);
        }

        public PointD Rotate(double AngleInDeg)
        {
            double Angle = AngleInDeg * (Math.PI * 2.0) / 360.0;
            double CA = Math.Cos(Angle);
            double SA = Math.Sin(Angle);

            double nX = X * CA - Y * SA;
            double nY = X * SA + Y * CA;
            return new PointD(nX, nY);
        }

        public PointD Copy()
        {
            return new PointD(X, Y);
        }
    }

}
