using System;
using System.Drawing;
using System.Drawing.Imaging;


namespace QuickFont
{

    public class JMath
    {
        
        public const float PI = (float)Math.PI;

        public static float Cos(float ang)
        {
            return (float)Math.Cos(ang);
        }
        public static float Sin(float ang)
        {
            return (float)Math.Sin(ang);
        }


        //fast version of gluproject which only works only for scale / translate transformations in gluOrthog - use FastProject2 for even better speed
        public static void FastProject(float px, float py, double[] modelView, double[] projection, int[] view, out float scrx, out float scry){
            float w = (float) (2 / projection[0]);
            float h = (float) (-2 / projection[5]);

            scrx = (float)((px * modelView[0] + modelView[12]) * view[2] / w + view[0]);
            scry = (float)((1 - (py * modelView[5] + modelView[13]) / h) * view[3] + view[1]);
        }

        //even faster simplified version of gluproject
        public static void FastProject2(float px, float py, out float scrx, out float scry)
        {
            scrx = px * fastProjectConsts[0] + fastProjectConsts[1];
            scry = py * fastProjectConsts[2] + fastProjectConsts[3];
        }



        private static float[] fastProjectConsts = new float[4];

        public static void FastProjectSetConsts(double[] modelView, double[] projection, int[] view){
            float w = (float) (2 / projection[0]);
            float h = (float) (-2 / projection[5]);


            fastProjectConsts[0] = (float)(modelView[0] * view[2] / w);
            fastProjectConsts[1] = (float)(modelView[12] * view[2] / w + view[0]);

            fastProjectConsts[2] = (float)(-modelView[5] * view[3] / h);
            fastProjectConsts[3] = (float)(  (1 - modelView[13] /h) * view[3] + view[1]);
         
        }



        public static float distSquared(float x1, float y1, float x2, float y2)
        {
            return (x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1);
        }


        /// <summary>
        /// Returns the power of 2 that is closest to x, but not smaller than x.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static int pot(int x){
            int shifts = 0;
            uint val = (uint)x;

            if (x < 0)
            {
                return 0;
            }
            
            while (val > 0)
            {
                val = val >> 1;
                shifts++;
            }

            val = (uint)1 << (shifts - 1);
            if (val < x)
            {
                val = val << 1;
            }
            return (int)val;
        }




        public static double StandardDeviation(double[] data)
        {

            if (data.Length == 0)
                return 0;

            double avg = Average(data);
            double totalVariance = 0;

            foreach (var v in data)
                totalVariance += (v - avg) * (v - avg);

            return Math.Sqrt(totalVariance / data.Length);


        }



        private static double Average(double[] data)
        {

            if (data.Length == 0)
                return 0d;

            double total = 0;

            foreach (var v in data)
                total += v;

            return total / data.Length;


        }


            



    }


  
}
