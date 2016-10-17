using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GerberLibrary
{

    public class V3
    {
        public double X;
        public double Y;
        public double Z;

        public V3(double x, double y, double z)
        {
       
            X = x;
            Y = y;
            Z = z;
        }

        public V3(V3 v1)
        {
            X = v1.X;
            Y = v1.Y;
            Z = v1.Z;
        }
        public static V3 operator -(V3 v1, V3 v2)
        {
            return
            (
               new V3
               (
                   v1.X - v2.X,
                   v1.Y - v2.Y,
                   v1.Z - v2.Z
               )
            );
        }

        public static V3 operator +(V3 v1, V3 v2)
        {
            return
            (
               new V3
               (
                   v1.X + v2.X,
                   v1.Y + v2.Y,
                   v1.Z + v2.Z
               )
            );
        }

        public static V3 operator *(V3 v1, double v2)
        {
            return
            (
               new V3
               (
                   v1.X * v2,
                   v1.Y * v2,
                   v1.Z * v2
               )
            );
        }
        public static V3 operator /(V3 v1, double v2)
        {
            return
            (
               new V3
               (
                   v1.X / v2,
                   v1.Y / v2,
                   v1.Z / v2
               )
            );
        }
        public static V3 operator -(V3 v1)
        {
            return
            (
               new V3
               (
                  -v1.X,
                  -v1.Y,
                  -v1.Z
               )
            );
        }

        public static V3 Cross(V3 v1, V3 v2)
        {
            return
            (
               new V3
               (
                  v1.Y * v2.Z - v1.Z * v2.Y,
                  v1.Z * v2.X - v1.X * v2.Z,
                  v1.X * v2.Y - v1.Y * v2.X
               )
            );
        }
    }
}
