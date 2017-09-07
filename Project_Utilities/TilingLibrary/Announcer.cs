using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Artwork
{
    public static class Announcer
    {
        public static void DrawAnnouncement(Bitmap B, AnnouncementDetails AD)
        {
            Graphics G = Graphics.FromImage(B);
            Bitmap B2 = new Bitmap(B.Width, B.Height);
            Graphics G2 = Graphics.FromImage(B2);

            G2.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
            G2.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            G.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
            G.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            G.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            G2.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;


            G.Clear(Color.Black);

            if (AD.Background != null)
            {


                RectangleF From = new RectangleF(0, 0, AD.Background.Width, AD.Background.Height);
                RectangleF To = new RectangleF(0, 0, B.Width, B.Height);

                if (From.Width > To.Width || From.Height > To.Height)
                {
                    double fromas = From.Width / From.Height;
                    double toas = To.Width / To.Height;
                    double scale = 1;
                    if (fromas < toas)
                    {
                        scale = From.Width / To.Width;
                        From.Y = From.Height / 2 - (float)(To.Height * scale) / 2;

                        From.Height = (float)(To.Height * scale);

                    }
                    else
                    {
                        scale = From.Height / To.Height;
                        From.X = From.Width / 2 - (float)(To.Width * scale) / 2;
                           From.Width = (float)(To.Width * scale);
                        
                    }



                    Console.WriteLine("{0}", scale);
                }
                G.DrawImage(AD.Background, To, From, GraphicsUnit.Pixel);
            }

            for (int i = 0; i < 5; i++)
            {
                DrawTo(AD, G2, 10 + i *5, true);
            }

             TINRSArtWorkRenderer ArtRender = new TINRSArtWorkRenderer();
            Settings TheSettings = new Settings();
            TheSettings.InvertSource = false;
            TheSettings.DegreesOff = 14;
            //TheSettings.MaxSubDiv;

           
            ArtRender.BuildTree(B2, TheSettings);
            ArtRender.BuildStuff(B2, TheSettings);
            ArtRender.DrawTiling(TheSettings, B2, G, Color.Yellow, Color.Black, 1.2f, false);
            DrawTo(AD, G, 2, false);



        }

        private static void DrawTo(AnnouncementDetails AD, Graphics G, double fuzz, bool mask)
        {
            DrawTitle(AD, AD.Title, G, fuzz,10,10,mask, PantonBig);
            DrawTitle(AD,AD.Bodytext, G, fuzz, 20, 120, mask, Panton);
            DrawTitle(AD, "www.thisisnotrocketscience.nl", G, fuzz, 0, AD.Height-Panton.Height, mask, Panton, true);


        }
        public static Font Panton = new Font("Panton Bold", 20);
        public static Font PantonBig = new Font("Panton Bold", 30);

        private static void DrawTitle(AnnouncementDetails AD, string lbl, Graphics g, double fuzz, double x, double y, bool mask, Font F, bool rightalign = false)
        {
            Brush B = Brushes.Black;

            if (mask)
            {
                B = Brushes.White;

            }

            if (rightalign)
            {
                var S = g.MeasureString(lbl, F);
                x = AD.Width - S.Width - x;
            }
            for (double i =0;i<20;i++)
            {
                double p = Math.PI*2 *( i / 20.0);
                
                g.DrawString(lbl, F, B, (float)(x + Math.Cos(p)*fuzz) , (float)(y + Math.Sin(p) * fuzz));
            }
           g.DrawString(lbl, F, Brushes.White, (float)x,(float)y);
        }
    }

    public class AnnouncementDetails
    {
        public string Title;
        public string Bodytext;
        public Bitmap Background;
        public DateTime ShowDate;
        public float Height = 640;
        public float Width = 640;
    }
}
