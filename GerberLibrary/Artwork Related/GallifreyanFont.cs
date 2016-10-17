using GerberLibrary.Core.Primitives;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GerberLibrary
{
    public class GallifreyanFont
    {

        public interface GraphicsInterface
        {
            void stroke(Color strokecol);
            void strokeWeight(double p);
            void strokeCap(CAPSHAPE cAPSHAPE);
            void arc(double nX, double nY, double p1, double p2, double start, double end);
            void line(double x1, double y1, double x2, double y2);
            void noFill();

            void ellipse(double x, double y, double w, double h);

            void fill(Color fg);

            void noStroke();

        }

        public class GerberWriter:GraphicsInterface
        {
            bool DoFill = false;
            bool DoStroke = true;
            GerberLibrary.GerberArtWriter GAW = new GerberLibrary.GerberArtWriter();

            CAPSHAPE CurrentShape = CAPSHAPE.ROUND;
            double strokewidth = 0.1;

            public GerberWriter()
            {

            }

            public void stroke(Color strokecol)
            {
                DoStroke = true;
            }

            public void strokeWeight(double p)
            {
                strokewidth = p*0.25 * 4.0;
            }

            public void strokeCap(CAPSHAPE cAPSHAPE)
            {
                CurrentShape = cAPSHAPE;
            }

            public void arc(double nX, double nY, double p1, double p2, double start, double end)
            {
                PolyLine PL = new PolyLine();
                PL.AddArc(nX, nY, p1, p2, start, end);
                GAW.AddPolyLine(PL, strokewidth);
            }

            public void line(double x1, double y1, double x2, double y2)
            {
                PolyLine PL = new PolyLine();
                PL.Add(x1, y1);
                PL.Add(x2, y2);
                GAW.AddPolyLine(PL, strokewidth);

            }

            public void noFill()
            {
                DoFill = false;
            }

            public void ellipse(double x, double y, double w, double h)
            {
                PolyLine PL = new PolyLine();
                PL.AddEllipse(x,y,w,h); 


                if (DoStroke) GAW.AddPolyLine(PL, strokewidth);
                if (DoFill) GAW.AddPolygon(PL);
            }

            public void fill(Color fg)
            {
                DoFill = true;                
            }

            public void noStroke()
            {
                DoStroke = false;
            }

            public void Write(string filename)
            {
                GAW.Write(filename);
            }
        }

        public class GraphicsWriter: GraphicsInterface
        {
            bool DoFill = false;
            bool DoStroke = true;
            CAPSHAPE CurrentShape = CAPSHAPE.ROUND;
            double strokewidth = 1;

            public void noFill()
            {
                DoFill = false;
            }

            public void ellipse(double x, double y, double w, double h)
            {
                x -= w / 2;
                y -= h / 2;
                if (DoFill)
                {
                    this.G.FillEllipse(new SolidBrush(sfg), (float)x, (float)y, (float)w, (float)h);
                    if (DoStroke) this.G.DrawEllipse(GetPen(), (float)x, (float)y, (float)w, (float)h);
                }
                else
                {
                    if (DoStroke) this.G.DrawEllipse(GetPen(), (float)x, (float)y, (float)w, (float)h);
                }
            }

            public void fill(Color fg)
            {
                ffg = fg;
                DoFill = true;
            }

            public void noStroke()
            {
                DoStroke = false;
            }


            System.Drawing.Color sfg = Color.Black;
            System.Drawing.Color ffg = Color.Blue;
            System.Drawing.Color sbg = Color.White;        


            public Graphics G;

            public GraphicsWriter(Graphics G)
            {
                this.G = G;
                this.G.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            
            }
            public void strokeCap(CAPSHAPE cAPSHAPE)
            {
                CurrentShape = cAPSHAPE;
            }

            public void arc(double nX, double nY, double p1, double p2, double start, double end)
            {
                Pen p = GetPen();
                double sweep = end - start;
                this.G.DrawArc(p, new RectangleF(new PointF((float)(nX - p1 / 2), (float)(nY - p2 / 2)), new SizeF((float)p1, (float)p2)), (float)conv(start), conv(sweep));
            }

            public void line(double x1, double y1, double x2, double y2)
            {
                Pen p = GetPen();
                this.G.DrawLine(p, (float)x1, (float)y1, (float)x2, (float)y2);
            }

            public float conv(double inp)
            {
                return (float)((inp / (Math.PI * 2)) * 360.0);
            }

            public Pen GetPen()
            {
                var p = new Pen(new SolidBrush(sfg), (float)strokewidth);

                System.Drawing.Drawing2D.LineCap Cap = CurrentShape == CAPSHAPE.ROUND ? System.Drawing.Drawing2D.LineCap.Round : System.Drawing.Drawing2D.LineCap.Square;
                System.Drawing.Drawing2D.DashCap DCap = CurrentShape == CAPSHAPE.ROUND ? System.Drawing.Drawing2D.DashCap.Round : System.Drawing.Drawing2D.DashCap.Flat;
                p.SetLineCap(Cap, Cap, DCap);
                return p;
            }
            public void stroke(Color strokecol)
            {
                sfg = strokecol;
                DoStroke = true;
            }

            public void strokeWeight(double p)
            {
                strokewidth = p;
            }
        }

        GraphicsInterface Target;
        System.Drawing.Color fg = Color.DarkBlue;
        System.Drawing.Color bg = Color.White;        


        public void DrawToInterface(GraphicsInterface Gi, string S, double width, double height)
        {
            Target = Gi;
            sentenceRadius = Math.Min(width, height) / 3.0;
            english = S.Trim();
            if (english.Length == 0) return;

            //            try
            //          {

            transliterate(width, height);
            //        }
            //      catch (Exception) { };
        }

        public void DrawToContext(Graphics G, string S, double width, double height)
        {
            DrawToInterface(new GraphicsWriter(G), S, width, height);            

        }
        
        public enum CAPSHAPE
        {
            SQUARE,
            ROUND
        }
        Random R = new Random();
        double random(double min, double max)
        {
            double rr = R.NextDouble();
            return rr * (max - min) + min;
        }

        int random(int max)
        {
            return R.Next(max);
        }


        double lerp(double a, double b, double c)
        {
            return a + (b - a) * c;
        }
        double dist(double x1, double y1, double x2, double y2)
        {
            double dx = x2 - x1;
            double dy = y2 - y1;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        double map(double inp, double inplow, double inphigh, double outlow, double outhigh)
        {
            return ((inp - inplow) / (inphigh - inplow)) * (outhigh - outlow) + outlow;
        }

        double constrain(double inp, double low, double high)
        {
            if (inp < low) return low;
            if (inp > high) return high;
            return inp;

        }

        double sentenceRadius = 256;
        string english = "";

        void transliterate(double width, double height)
        {
            english = english.ToLower();
            english = english.Replace(" -", "-");
            english = english.Replace("- ", "-");
            english = english.Replace("-", "- ");
            english = english.Replace("ch", "#");
            english = english.Replace("sh", "$");
            english = english.Replace("th", "%");
            english = english.Replace("ng", "&");
            english = english.Replace("qu", "q");

            int spaces = 0;
            int sentences = 1;
            for (int i = 0; i < english.Count(); i++)
            {
                switch (english[i])
                {
                    case 'c':
                        Console.WriteLine("ERROR: Replace the C with K or S!");
                        return;
                    case ' ':
                        spaces++;
                        break;
                    case '.':
                    case '!':
                    case '?':
                        if (i < english.Count() - 1)
                        {
                            if (english[i + 1] == ' ')
                            {
                                sentences++;
                            }
                        }
                        break;
                }
            }
            if (spaces == 0)
            {
                writeSentence(0, width, height);
            }
            else if (sentences == 1)
            {
                writeSentence(1, width, height);
            }
            else
            {
                Console.WriteLine("ERROR: Multiple sentences are not yet supported.");
                return;
            }
        }

        void writeSentence(int type, double width, double height)
        {
            List<double> wordRadius = new List<double>();
            double1 = 0;
            double2 = 0;
            double charCount = 0;
            english = english.Trim();
            var Sentence = english.Split(' ');
            List<List<string>> sentence = new List<List<string>>();
            List<char> punctuation = new List<char>(Sentence.Length);
            for (int i = 0; i < Sentence.Length; i++)
            {
                sentence.Add(new List<string>());
                punctuation.Add('0');
            }
            bool[,] apostrophes = new bool[Sentence.Length, 100];
            for (int j = 0; j < Sentence.Length; j++)
            {
                List<String> word = new List<string>();
                Sentence[j] = Sentence[j].Replace(" ", "");
                bool vowel = true;
                for (int i = 0; i < Sentence[j].Length; i++)
                {
                    if (i != 0)
                    {
                        if (Sentence[j][i] == Sentence[j][i - 1])
                        {
                            word[word.Count - 1] = word[word.Count - 1] + '@';
                            continue;
                        }
                    }
                    if (Sentence[j][i] == 'a' || Sentence[j][i] == 'e' || Sentence[j][i] == 'i' || Sentence[j][i] == 'o' || Sentence[j][i] == 'u')
                    {
                        if (vowel)
                        {
                            word.Add(Sentence[j][i].ToString());
                        }
                        else
                        {
                            word[word.Count - 1] = word[word.Count - 1] + Sentence[j][i].ToString();
                        }
                        vowel = true;
                    }
                    else if (Sentence[j][i] == '.' || Sentence[j][i] == '?' || Sentence[j][i] == '!' || Sentence[j][i] == '"' || Sentence[j][i] == "'"[0] || Sentence[j][i] == '-' || Sentence[j][i] == ',' || Sentence[j][i] == ';' || Sentence[j][i] == ':')
                    {
                        if (Sentence[j][i] == "'"[0])
                        {
                            apostrophes[j, i] = true;
                        }
                        else
                        {
                            punctuation[j] = Sentence[j][i];
                        }
                    }
                    else
                    {
                        word.Add(Sentence[j][i].ToString());
                        if (Sentence[j][i] == 't' || Sentence[j][i] == '$' || Sentence[j][i] == 'r' || Sentence[j][i] == 's' || Sentence[j][i] == 'v' || Sentence[j][i] == 'w')
                        {
                            vowel = true;
                        }
                        else
                        {
                            vowel = false;
                        }
                    }
                }
                sentence[j] = word;
                charCount += word.Count;
            }
            Target.stroke(fg);
            if (type > 0)
            {
                Target.strokeWeight(3);
                Target.ellipse(width / 2, height / 2, sentenceRadius * 2, sentenceRadius * 2);
            }
            Target.strokeWeight(4);
            Target.ellipse(width / 2, height / 2, sentenceRadius * 2 + 40, sentenceRadius * 2 + 40);
            double pos = Math.PI / 2;
            double maxRadius = 0;
            for (int i = 0; i < sentence.Count; i++)
            {
                wordRadius.Add(constrain(sentenceRadius * sentence[i].Count / charCount * 1.2, 0, sentenceRadius / 2));
                if (wordRadius[i] > maxRadius)
                {
                    maxRadius = wordRadius[i];
                }
            }
            double scaleFactor = sentenceRadius / (maxRadius + (sentenceRadius / 2));
            double distance = scaleFactor * sentenceRadius / 2;
            for (int i = 0; i < wordRadius.Count; i++)
            {
                wordRadius[i] *= scaleFactor;
            }
            List<double> x = new List<double>();
            List<double> y = new List<double>();
            //  stroke(fg);
            for (int i = 0; i < sentence.Count; i++)
            {
                x.Add(width / 2 + distance * Math.Cos(pos));
                y.Add(height / 2 + distance * Math.Sin(pos));
                int nextIndex = 0;
                if (i != sentence.Count - 1)
                {
                    nextIndex = i + 1;
                }
                pos -= (double)(sentence[i].Count + sentence[nextIndex].Count) / (2 * charCount) * Math.PI * 2;
                double pX = width / 2 + Math.Cos(pos + (double)(sentence[i].Count + sentence[nextIndex].Count) / (2 * charCount) * Math.PI) * sentenceRadius;
                double pY = height / 2 + Math.Sin(pos + (double)(sentence[i].Count + sentence[nextIndex].Count) / (2 * charCount) * Math.PI) * sentenceRadius;
                switch (punctuation[i])
                {
                    case '.':
                        Target.ellipse(pX, pY, 20, 20);
                        break;
                    case '?':
                        makeDots(width / 2, height / 2, sentenceRadius * 1.4, 2, -1.2, 0.1);
                        break;
                    case '!':
                        makeDots(width / 2, height / 2, sentenceRadius * 1.4, 3, -1.2, 0.1);
                        break;
                    case '"':
                        Target.line(width / 2 + Math.Cos(pos + (double)(sentence[i].Count + sentence[nextIndex].Count) / (2 * charCount) * Math.PI) * sentenceRadius, height / 2 + Math.Sin(pos + (double)(sentence[i].Count + sentence[nextIndex].Count) / (2 * charCount) * Math.PI) * sentenceRadius, width / 2 + Math.Cos(pos + (double)(sentence[i].Count + sentence[nextIndex].Count) / (2 * charCount) * Math.PI) * (sentenceRadius + 20), height / 2 + Math.Sin(pos + (double)(sentence[i].Count + sentence[nextIndex].Count) / (2 * charCount) * Math.PI) * (sentenceRadius + 20));
                        break;
                    case '-':
                        Target.line(width / 2 + Math.Cos(pos + (double)(sentence[i].Count + sentence[nextIndex].Count) / (2 * charCount) * Math.PI) * sentenceRadius, height / 2 + Math.Sin(pos + (double)(sentence[i].Count + sentence[nextIndex].Count) / (2 * charCount) * Math.PI) * sentenceRadius, width / 2 + Math.Cos(pos + (double)(sentence[i].Count + sentence[nextIndex].Count) / (2 * charCount) * Math.PI) * (sentenceRadius + 20), height / 2 + Math.Sin(pos + (double)(sentence[i].Count + sentence[nextIndex].Count) / (2 * charCount) * Math.PI) * (sentenceRadius + 20));
                        Target.line(width / 2 + Math.Cos(pos + (sentence[i].Count + sentence[nextIndex].Count + 0.3) / (2 * charCount) * Math.PI) * sentenceRadius, height / 2 + Math.Sin(pos + (sentence[i].Count + sentence[nextIndex].Count + 0.2) / (2 * charCount) * Math.PI) * sentenceRadius, width / 2 + Math.Cos(pos + (sentence[i].Count + sentence[nextIndex].Count + 0.2) / (2 * charCount) * Math.PI) * (sentenceRadius + 20), height / 2 + Math.Sin(pos + (sentence[i].Count + sentence[nextIndex].Count + 0.3) / (2 * charCount) * Math.PI) * (sentenceRadius + 20));
                        Target.line(width / 2 + Math.Cos(pos + (sentence[i].Count + sentence[nextIndex].Count - 0.3) / (2 * charCount) * Math.PI) * sentenceRadius, height / 2 + Math.Sin(pos + (sentence[i].Count + sentence[nextIndex].Count - 0.2) / (2 * charCount) * Math.PI) * sentenceRadius, width / 2 + Math.Cos(pos + (sentence[i].Count + sentence[nextIndex].Count - 0.2) / (2 * charCount) * Math.PI) * (sentenceRadius + 20), height / 2 + Math.Sin(pos + (sentence[i].Count + sentence[nextIndex].Count - 0.3) / (2 * charCount) * Math.PI) * (sentenceRadius + 20));
                        break;
                    case ',':
                        Target.fill(fg);
                        Target.ellipse(pX, pY, 20, 20);
                        Target.noFill();
                        break;
                    case ';':
                        Target.fill(fg);
                        Target.ellipse(width / 2 + Math.Cos(pos + (double)(sentence[i].Count + sentence[nextIndex].Count) / (2 * charCount) * Math.PI) * sentenceRadius - 10, height / 2 + Math.Sin(pos + (double)(sentence[i].Count + sentence[nextIndex].Count) / (2 * charCount) * Math.PI) * sentenceRadius - 10, 10, 10);
                        Target.noFill();
                        break;
                    case ':':
                        Target.ellipse(pX, pY, 25, 25);
                        Target.strokeWeight(2);
                        Target.ellipse(pX, pY, 15, 15);
                        Target.strokeWeight(4);
                        break;
                    default:
                        break;
                }
            }
            int otherIndex = 0;
            bool[,] nested = new bool[sentence.Count, 100];
            for (int i = 0; i < sentence.Count; i++)
            {
                double angle1 = 0;//angle facing onwards
                //double angle2 = 0;//backwards
                if (i == sentence.Count - 1)
                {
                    otherIndex = 0;
                }
                else
                {
                    otherIndex = i + 1;
                }
                angle1 = Math.Atan((y[i] - y[otherIndex]) / (x[i] - x[otherIndex]));
                if (double.IsNaN(angle1)) angle1 = 0;
                if (dist(x[i] + (Math.Cos(angle1) * 20), y[i] + (Math.Sin(angle1) * 20), x[otherIndex], y[otherIndex]) > dist(x[i], y[i], x[otherIndex], y[otherIndex]))
                {
                    angle1 -= Math.PI;
                }
                if (angle1 < 0)
                {
                    angle1 += Math.PI * 2.0;
                }
                if (angle1 < 0)
                {
                    angle1 += Math.PI * 2.0;
                }
                angle1 -= Math.PI / 2;
                if (angle1 < 0)
                {
                    angle1 += Math.PI * 2.0;
                }
                angle1 = Math.PI * 2.0 - angle1;
                int index = (int)Math.Round(map(angle1, 0, Math.PI * 2.0, 0, (double)(sentence[i].Count)));
                if (index == sentence[i].Count)
                {
                    index = 0;
                }
                char tempChar = sentence[i][index][0];
                if ((tempChar == 't' || tempChar == '$' || tempChar == 'r' || tempChar == 's' || tempChar == 'v' || tempChar == 'w') && type > 0)
                {
                    nested[i, index] = true;
                    wordRadius[i] = constrain(wordRadius[i] * 1.2, 0, maxRadius * scaleFactor);
                    while (dist(x[i], y[i], x[otherIndex], y[otherIndex]) > wordRadius[i] + wordRadius[otherIndex])
                    {
                        x[i] = lerp(x[i], x[otherIndex], 0.05);
                        y[i] = lerp(y[i], y[otherIndex], 0.05);
                    }
                }
            }

            List<double> lineX = new List<double>();
            List<double> lineY = new List<double>();
            List<double> arcBegin = new List<double>();
            List<double> arcEnd = new List<double>();
            List<double> lineRad = new List<double>();

            Target.strokeWeight(2);
            if (type == 0)
            {
                wordRadius[0] = sentenceRadius * 0.9;
                x[0] = width / 2;
                y[0] = height / 2;
            }
            for (int i = 0; i < sentence.Count; i++)
            {
                pos = Math.PI / 2;
                double letterRadius = wordRadius[i] / (sentence[i].Count + 1) * 1.5;
                for (int j = 0; j < sentence[i].Count; j++)
                {
                    if (apostrophes[i, j])
                    {
                        double a = pos + Math.PI / sentence[i].Count - 0.1;
                        double d = 0;
                        double tempX2 = x[i];
                        double tempY2 = y[i];
                        while (Math.Pow(tempX2 - width / 2, 2) + Math.Pow(tempY2 - height / 2, 2) < Math.Pow(sentenceRadius + 20, 2))
                        {
                            tempX2 = x[i] + Math.Cos(a) * d;
                            tempY2 = y[i] + Math.Sin(a) * d;
                            d += 1;
                        }
                        Target.line(x[i] + Math.Cos(a) * wordRadius[i], y[i] + Math.Sin(a) * wordRadius[i], tempX2, tempY2);
                        a = pos + Math.PI / sentence[i].Count + 0.1;
                        d = 0;
                        tempX2 = x[i];
                        tempY2 = y[i];
                        while (Math.Pow(tempX2 - width / 2, 2) + Math.Pow(tempY2 - height / 2, 2) < Math.Pow(sentenceRadius + 20, 2))
                        {
                            tempX2 = x[i] + Math.Cos(a) * d;
                            tempY2 = y[i] + Math.Sin(a) * d;
                            d += 1;
                        }
                        Target.line(x[i] + Math.Cos(a) * wordRadius[i], y[i] + Math.Sin(a) * wordRadius[i], tempX2, tempY2);
                    }
                    bool vowel = true;
                    double tempX = 0;
                    double tempY = 0;

                    //Math.Single vowels

                    if (sentence[i][j][0] == 'a')
                    {
                        tempX = x[i] + Math.Cos(pos) * (wordRadius[i] + letterRadius / 2);
                        tempY = y[i] + Math.Sin(pos) * (wordRadius[i] + letterRadius / 2);
                        Target.ellipse(tempX, tempY, letterRadius, letterRadius);
                    }
                    else if (sentence[i][j][0] == 'e')
                    {
                        tempX = x[i] + Math.Cos(pos) * (wordRadius[i]);
                        tempY = y[i] + Math.Sin(pos) * (wordRadius[i]);
                        Target.ellipse(tempX, tempY, letterRadius, letterRadius);
                    }
                    else if (sentence[i][j][0] == 'i')
                    {
                        tempX = x[i] + Math.Cos(pos) * (wordRadius[i]);
                        tempY = y[i] + Math.Sin(pos) * (wordRadius[i]);
                        Target.ellipse(tempX, tempY, letterRadius, letterRadius);
                        lineX.Add(tempX);
                        lineY.Add(tempY);
                        arcBegin.Add(pos + Math.PI / 2);
                        arcEnd.Add(pos + 3 * Math.PI / 2);
                        lineRad.Add(letterRadius);
                    }
                    else if (sentence[i][j][0] == 'o')
                    {
                        tempX = x[i] + Math.Cos(pos) * (wordRadius[i] - letterRadius / 1.6);
                        tempY = y[i] + Math.Sin(pos) * (wordRadius[i] - letterRadius / 1.6);
                        Target.ellipse(tempX, tempY, letterRadius, letterRadius);
                    }
                    else if (sentence[i][j][0] == 'u')
                    {
                        tempX = x[i] + Math.Cos(pos) * (wordRadius[i]);
                        tempY = y[i] + Math.Sin(pos) * (wordRadius[i]);
                        Target.ellipse(tempX, tempY, letterRadius, letterRadius);
                        lineX.Add(tempX);
                        lineY.Add(tempY);
                        arcBegin.Add(pos - Math.PI / 2);
                        arcEnd.Add(pos + Math.PI / 2);
                        lineRad.Add(letterRadius);
                    }
                    else
                    {
                        vowel = false;
                    }

                    if (vowel)
                    {
                        Target.arc(x[i], y[i], 
                                wordRadius[i] * 2, 
                                wordRadius[i] * 2, pos - Math.PI / sentence[i].Count, 
                                pos + Math.PI / sentence[i].Count);
                        if (sentence[i][j].Count() == 1)
                        {
                        }
                        else
                        {

                            //double vowels

                            if (sentence[i][j][1] == '@')
                            {
                                Target.ellipse(tempX, tempY, letterRadius * 1.3, letterRadius * 1.3);
                            }
                        }
                    }
                    else
                    {

                        // consonants

                        if (sentence[i][j][0] == 'b' || sentence[i][j][0] == '#' || sentence[i][j][0] == 'd' || sentence[i][j][0] == 'f' || sentence[i][j][0] == 'g' || sentence[i][j][0] == 'h')
                        {
                            tempX = x[i] + Math.Cos(pos) * (wordRadius[i] - letterRadius * 0.95);
                            tempY = y[i] + Math.Sin(pos) * (wordRadius[i] - letterRadius * 0.95);
                            makeArcs(tempX, tempY, x[i], y[i], wordRadius[i], letterRadius, pos - Math.PI / sentence[i].Count, pos + Math.PI / sentence[i].Count);
                            int lines = 0;
                            if (sentence[i][j][0] == '#')
                            {
                                makeDots(tempX, tempY, letterRadius, 2, pos, 1);
                            }
                            else if (sentence[i][j][0] == 'd')
                            {
                                makeDots(tempX, tempY, letterRadius, 3, pos, 1);
                            }
                            else if (sentence[i][j][0] == 'f')
                            {
                                lines = 3;
                            }
                            else if (sentence[i][j][0] == 'g')
                            {
                                lines = 1;
                            }
                            else if (sentence[i][j][0] == 'h')
                            {
                                lines = 2;
                            }
                            for (int k = 0; k < lines; k++)
                            {
                                lineX.Add(tempX);
                                lineY.Add(tempY);
                                arcBegin.Add(pos + 0.5);
                                arcEnd.Add(pos + Math.PI * 2.0 - 0.5);
                                lineRad.Add(letterRadius * 2);
                            }
                            if (sentence[i][j].Count() > 1)
                            {
                                int vowelIndex = 1;
                                if (sentence[i][j][1] == '@')
                                {
                                    makeArcs(tempX, tempY, x[i], y[i], wordRadius[i], letterRadius * 1.3, pos + Math.PI * 2.0, pos - Math.PI * 2.0);
                                    vowelIndex = 2;
                                }
                                if (sentence[i][j].Count() == vowelIndex)
                                {
                                    pos -= Math.PI * 2.0 / sentence[i].Count;
                                    continue;
                                }
                                if (sentence[i][j][vowelIndex] == 'a')
                                {
                                    tempX = x[i] + Math.Cos(pos) * (wordRadius[i] + letterRadius / 2);
                                    tempY = y[i] + Math.Sin(pos) * (wordRadius[i] + letterRadius / 2);
                                    Target.ellipse(tempX, tempY, letterRadius, letterRadius);
                                }
                                else if (sentence[i][j][vowelIndex] == 'e')
                                {
                                    Target.ellipse(tempX, tempY, letterRadius, letterRadius);
                                }
                                else if (sentence[i][j][vowelIndex] == 'i')
                                {
                                    Target.ellipse(tempX, tempY, letterRadius, letterRadius);
                                    lineX.Add(tempX);
                                    lineY.Add(tempY);
                                    arcBegin.Add(pos + Math.PI / 2);
                                    arcEnd.Add(pos + 3 * Math.PI / 2);
                                    lineRad.Add(letterRadius);
                                }
                                else if (sentence[i][j][vowelIndex] == 'o')
                                {
                                    tempX = x[i] + Math.Cos(pos) * (wordRadius[i] - letterRadius * 2);
                                    tempY = y[i] + Math.Sin(pos) * (wordRadius[i] - letterRadius * 2);
                                    Target.ellipse(tempX, tempY, letterRadius, letterRadius);
                                }
                                else if (sentence[i][j][vowelIndex] == 'u')
                                {
                                    Target.ellipse(tempX, tempY, letterRadius, letterRadius);
                                    lineX.Add(tempX);
                                    lineY.Add(tempY);
                                    arcBegin.Add(pos - Math.PI / 2);
                                    arcEnd.Add(pos + Math.PI / 2);
                                    lineRad.Add(letterRadius);
                                }
                                if (sentence[i][j].Count() == (vowelIndex + 2))
                                {
                                    if (sentence[i][j][vowelIndex + 1] == '@')
                                    {
                                        Target.ellipse(tempX, tempY, letterRadius * 1.3, letterRadius * 1.3);
                                    }
                                }
                            }
                        }
                        if (sentence[i][j][0] == 'j' || sentence[i][j][0] == 'k' || sentence[i][j][0] == 'l' || sentence[i][j][0] == 'm' || sentence[i][j][0] == 'n' || sentence[i][j][0] == 'p')
                        {
                            tempX = x[i] + Math.Cos(pos) * (wordRadius[i] - letterRadius);
                            tempY = y[i] + Math.Sin(pos) * (wordRadius[i] - letterRadius);
                            Target.ellipse(tempX, tempY, letterRadius * 1.9, letterRadius * 1.9);
                            Target.arc(x[i], y[i], wordRadius[i] * 2, wordRadius[i] * 2, pos - Math.PI / sentence[i].Count, pos + Math.PI / sentence[i].Count);
                            int lines = 0;
                            if (sentence[i][j][0] == 'k')
                            {
                                makeDots(tempX, tempY, letterRadius, 2, pos, 1);
                            }
                            else if (sentence[i][j][0] == 'l')
                            {
                                makeDots(tempX, tempY, letterRadius, 3, pos, 1);
                            }
                            else if (sentence[i][j][0] == 'm')
                            {
                                lines = 3;
                            }
                            else if (sentence[i][j][0] == 'n')
                            {
                                lines = 1;
                            }
                            else if (sentence[i][j][0] == 'p')
                            {
                                lines = 2;
                            }
                            for (int k = 0; k < lines; k++)
                            {
                                lineX.Add(tempX);
                                lineY.Add(tempY);
                                arcBegin.Add(0);
                                arcEnd.Add(Math.PI * 2.0);
                                lineRad.Add(letterRadius * 1.9);
                            }
                            if (sentence[i][j].Count() > 1)
                            {
                                int vowelIndex = 1;
                                if (sentence[i][j][1] == '@')
                                {
                                    Target.ellipse(tempX, tempY, letterRadius * 2.3, letterRadius * 2.3);
                                    vowelIndex = 2;
                                }
                                if (sentence[i][j].Count() == vowelIndex)
                                {
                                    pos -= Math.PI * 2.0 / sentence[i].Count;
                                    continue;
                                }
                                if (sentence[i][j][vowelIndex] == 'a')
                                {
                                    tempX = x[i] + Math.Cos(pos) * (wordRadius[i] + letterRadius / 2);
                                    tempY = y[i] + Math.Sin(pos) * (wordRadius[i] + letterRadius / 2);
                                    Target.ellipse(tempX, tempY, letterRadius, letterRadius);
                                }
                                else if (sentence[i][j][vowelIndex] == 'e')
                                {
                                    Target.ellipse(tempX, tempY, letterRadius, letterRadius);
                                }
                                else if (sentence[i][j][vowelIndex] == 'i')
                                {
                                    Target.ellipse(tempX, tempY, letterRadius, letterRadius);
                                    lineX.Add(tempX);
                                    lineY.Add(tempY);
                                    arcBegin.Add(pos + Math.PI / 2);
                                    arcEnd.Add(pos + 3 * Math.PI / 2);
                                    lineRad.Add(letterRadius);
                                }
                                else if (sentence[i][j][vowelIndex] == 'o')
                                {
                                    tempX = x[i] + Math.Cos(pos) * (wordRadius[i] - letterRadius * 2);
                                    tempY = y[i] + Math.Sin(pos) * (wordRadius[i] - letterRadius * 2);
                                    Target.ellipse(tempX, tempY, letterRadius, letterRadius);
                                }
                                else if (sentence[i][j][vowelIndex] == 'u')
                                {
                                    Target.ellipse(tempX, tempY, letterRadius, letterRadius);
                                    lineX.Add(tempX);
                                    lineY.Add(tempY);
                                    arcBegin.Add(pos - Math.PI / 2);
                                    arcEnd.Add(pos + Math.PI / 2);
                                    lineRad.Add(letterRadius);
                                }
                                if (sentence[i][j].Count() == (vowelIndex + 2))
                                {
                                    if (sentence[i][j][vowelIndex + 1] == '@')
                                    {
                                        Target.ellipse(tempX, tempY, letterRadius * 1.3, letterRadius * 1.3);
                                    }
                                }
                            }
                        }
                        if (sentence[i][j][0] == 't' || sentence[i][j][0] == '$' || sentence[i][j][0] == 'r' || sentence[i][j][0] == 's' || sentence[i][j][0] == 'v' || sentence[i][j][0] == 'w')
                        {
                            tempX = x[i] + Math.Cos(pos) * (wordRadius[i]);
                            tempY = y[i] + Math.Sin(pos) * (wordRadius[i]);
                            int nextIndex;
                            if (i == sentence.Count - 1)
                            {
                                nextIndex = 0;
                            }
                            else
                            {
                                nextIndex = i + 1;
                            }
                            double angle1 = Math.Atan((y[i] - y[nextIndex]) / (x[i] - x[nextIndex]));
                            if (dist(x[i] + (Math.Cos(angle1) * 20), y[i] + (Math.Sin(angle1) * 20), x[nextIndex], y[nextIndex]) > dist(x[i], y[i], x[nextIndex], y[nextIndex]))
                            {
                                angle1 -= Math.PI;
                            }
                            if (angle1 < 0)
                            {
                                angle1 += Math.PI * 2.0;
                            }
                            if (angle1 < 0)
                            {
                                angle1 += Math.PI * 2.0;
                            }
                            if (nested[i, j])
                            {
                                makeArcs(x[nextIndex], y[nextIndex], x[i], y[i], wordRadius[i], wordRadius[nextIndex] + 20, pos - Math.PI / sentence[i].Count, pos + Math.PI / sentence[i].Count);
                            }
                            else
                            {
                                makeArcs(tempX, tempY, x[i], y[i], wordRadius[i], letterRadius * 1.5, pos - Math.PI / sentence[i].Count, pos + Math.PI / sentence[i].Count);
                            }
                            int lines = 0;
                            if (sentence[i][j][0] == '$')
                            {
                                if (nested[i, j])
                                {
                                    makeDots(x[nextIndex], y[nextIndex], (wordRadius[nextIndex] * 1.4) + 14, 2, angle1, wordRadius[nextIndex] / 500);
                                }
                                else
                                {
                                    makeDots(tempX, tempY, letterRadius * 2.6, 2, pos, 0.5);
                                }
                            }
                            else if (sentence[i][j][0] == 'r')
                            {
                                if (nested[i, j])
                                {
                                    makeDots(x[nextIndex], y[nextIndex], (wordRadius[nextIndex] * 1.4) + 14, 3, angle1, wordRadius[nextIndex] / 500);
                                }
                                else
                                {
                                    makeDots(tempX, tempY, letterRadius * 2.6, 3, pos, 0.5);
                                }
                            }
                            else if (sentence[i][j][0] == 's')
                            {
                                lines = 3;
                            }
                            else if (sentence[i][j][0] == 'v')
                            {
                                lines = 1;
                            }
                            else if (sentence[i][j][0] == 'w')
                            {
                                lines = 2;
                            }
                            if (nested[i, j])
                            {
                                for (int k = 0; k < lines; k++)
                                {
                                    lineX.Add(x[nextIndex]);
                                    lineY.Add(y[nextIndex]);
                                    arcBegin.Add(double1);
                                    arcEnd.Add(double2);
                                    lineRad.Add(wordRadius[nextIndex] * 2 + 40);
                                }
                            }
                            else
                            {
                                for (int k = 0; k < lines; k++)
                                {
                                    lineX.Add(tempX);
                                    lineY.Add(tempY);
                                    arcBegin.Add(double1);
                                    arcEnd.Add(double2);
                                    lineRad.Add(letterRadius * 3);
                                }
                            }
                            if (sentence[i][j].Count() > 1)
                            {
                                if (sentence[i][j][1] == '@')
                                {
                                    if (nested[i, j])
                                    {
                                        makeArcs(x[nextIndex], y[nextIndex], x[i], y[i], wordRadius[i], (wordRadius[nextIndex] + 20) * 1.2, pos + Math.PI * 2.0, pos - Math.PI * 2.0);
                                    }
                                    else
                                    {
                                        makeArcs(tempX, tempY, x[i], y[i], wordRadius[i], letterRadius * 1.8, pos + Math.PI * 2.0, pos - Math.PI * 2.0);
                                    }
                                }
                            }
                        }
                        if (sentence[i][j][0] == '%' || sentence[i][j][0] == 'y' || sentence[i][j][0] == 'z' || sentence[i][j][0] == '&' || sentence[i][j][0] == 'q' || sentence[i][j][0] == 'x')
                        {
                            tempX = x[i] + Math.Cos(pos) * (wordRadius[i]);
                            tempY = y[i] + Math.Sin(pos) * (wordRadius[i]);
                            Target.ellipse(tempX, tempY, letterRadius * 2, letterRadius * 2);
                            Target.arc(x[i], y[i], wordRadius[i] * 2, wordRadius[i] * 2, pos - Math.PI / sentence[i].Count, pos + Math.PI / sentence[i].Count);
                            int lines = 0;
                            if (sentence[i][j][0] == 'y')
                            {
                                makeDots(tempX, tempY, letterRadius, 2, pos, 1);
                            }
                            else if (sentence[i][j][0] == 'z')
                            {
                                makeDots(tempX, tempY, letterRadius, 3, pos, 1);
                            }
                            else if (sentence[i][j][0] == '&')
                            {
                                lines = 3;
                            }
                            else if (sentence[i][j][0] == 'q')
                            {
                                lines = 1;
                            }
                            else if (sentence[i][j][0] == 'x')
                            {
                                lines = 2;
                            }
                            for (int k = 0; k < lines; k++)
                            {
                                lineX.Add(tempX);
                                lineY.Add(tempY);
                                arcBegin.Add(0);
                                arcEnd.Add(Math.PI * 2.0);
                                lineRad.Add(letterRadius * 2);
                            }
                            if (sentence[i][j].Count() > 1)
                            {
                                int vowelIndex = 1;
                                if (sentence[i][j][1] == '@')
                                {
                                    Target.ellipse(tempX, tempY, letterRadius * 2.3, letterRadius * 2.3);
                                    vowelIndex = 2;
                                }
                                if (sentence[i][j].Count() == vowelIndex)
                                {
                                    pos -= Math.PI * 2.0 / sentence[i].Count;
                                    continue;
                                }
                                if (sentence[i][j][vowelIndex] == 'a')
                                {
                                    tempX = x[i] + Math.Cos(pos) * (wordRadius[i] + letterRadius / 2);
                                    tempY = y[i] + Math.Sin(pos) * (wordRadius[i] + letterRadius / 2);
                                    Target.ellipse(tempX, tempY, letterRadius, letterRadius);
                                }
                                else if (sentence[i][j][vowelIndex] == 'e')
                                {
                                    Target.ellipse(tempX, tempY, letterRadius, letterRadius);
                                }
                                else if (sentence[i][j][vowelIndex] == 'i')
                                {
                                    Target.ellipse(tempX, tempY, letterRadius, letterRadius);
                                    lineX.Add(tempX);
                                    lineY.Add(tempY);
                                    arcBegin.Add(pos + Math.PI / 2);
                                    arcEnd.Add(pos + 3 * Math.PI / 2);
                                    lineRad.Add(letterRadius);
                                }
                                else if (sentence[i][j][vowelIndex] == 'o')
                                {
                                    tempX = x[i] + Math.Cos(pos) * (wordRadius[i] - letterRadius);
                                    tempY = y[i] + Math.Sin(pos) * (wordRadius[i] - letterRadius);
                                    Target.ellipse(tempX, tempY, letterRadius, letterRadius);
                                }
                                else if (sentence[i][j][vowelIndex] == 'u')
                                {
                                    Target.ellipse(tempX, tempY, letterRadius, letterRadius);
                                    lineX.Add(tempX);
                                    lineY.Add(tempY);
                                    arcBegin.Add(pos - Math.PI / 2);
                                    arcEnd.Add(pos + Math.PI / 2);
                                    lineRad.Add(letterRadius);
                                }
                                if (sentence[i][j].Count() == (vowelIndex + 2))
                                {
                                    if (sentence[i][j][vowelIndex + 1] == '@')
                                    {
                                        Target.ellipse(tempX, tempY, letterRadius * 1.8, letterRadius * 1.8);
                                    }
                                }
                            }
                        }
                    }
                    pos -= Math.PI * 2.0 / sentence[i].Count;
                }
            }
            Target.strokeWeight(2);
            List<double> lineLengths = new List<double>();
            Target.stroke(fg);
            for (int i = 0; i < lineX.Count; i++)
            {
                List<int> indexes = new List<int>();
                List<double> angles = new List<double>();

                for (int j = 0; j < lineX.Count; j++)
                {
                    if ((int)Math.Round(lineY[i]) == (int)Math.Round(lineY[j]) && (int)Math.Round(lineX[i]) == (int)Math.Round(lineX[j]))
                    {
                        continue;
                    }
                    bool b = false;
                    for (int k = 0; k < lineLengths.Count; k++)
                    {
                        if (lineLengths[k] == dist(lineX[i], lineY[i], lineX[j], lineY[j]) + lineX[i] + lineY[i] + lineX[j] + lineY[j])
                        {
                            b = true;
                            break;
                        }
                    }
                    if (b)
                    {
                        continue;
                    }
                    double angle1 = Math.Atan((lineY[i] - lineY[j]) / (lineX[i] - lineX[j]));
                    if (dist(lineX[i] + (Math.Cos(angle1) * 20), lineY[i] + (Math.Sin(angle1) * 20), lineX[j], lineY[j]) > dist(lineX[i], lineY[i], lineX[j], lineY[j]))
                    {
                        angle1 -= Math.PI;
                    }
                    if (angle1 < 0)
                    {
                        angle1 += Math.PI * 2.0;
                    }
                    if (angle1 < 0)
                    {
                        angle1 += Math.PI * 2.0;
                    }
                    if (angle1 < arcEnd[i] && angle1 > arcBegin[i])
                    {
                        angle1 -= Math.PI;
                        if (angle1 < 0)
                        {
                            angle1 += Math.PI * 2.0;
                        }
                        if (angle1 < arcEnd[j] && angle1 > arcBegin[j])
                        {
                            indexes.Add(j);
                            angles.Add(angle1);
                        }
                    }
                }
                if (indexes.Count() == 0)
                {
                    double a;

                    a = random(arcBegin[i], arcEnd[i]);

                    double d = 0;
                    double tempX = lineX[i] + Math.Cos(a) * d;
                    double tempY = lineY[i] + Math.Sin(a) * d;
                    while (Math.Pow(tempX - width / 2, 2) + Math.Pow(tempY - height / 2, 2) < Math.Pow(sentenceRadius + 20, 2))
                    {
                        tempX = lineX[i] + Math.Cos(a) * d;
                        tempY = lineY[i] + Math.Sin(a) * d;
                        d += 1;
                    }
                    Target.line(lineX[i] + Math.Cos(a) * lineRad[i] / 2, lineY[i] + Math.Sin(a) * lineRad[i] / 2, tempX, tempY);
                }
                else
                {
                    int r;

                    r = (int)Math.Floor((double)random((int)indexes.Count()));

                    int j = indexes[r];
                    double a = angles[r] + Math.PI;
                    Target.line(lineX[i] + Math.Cos(a) * lineRad[i] / 2, lineY[i] + Math.Sin(a) * lineRad[i] / 2, lineX[j] + Math.Cos(a + Math.PI) * lineRad[j] / 2, lineY[j] + Math.Sin(a + Math.PI) * lineRad[j] / 2);
                    lineLengths.Add(dist(lineX[i], lineY[i], lineX[j], lineY[j]) + lineX[i] + lineY[i] + lineX[j] + lineY[j]);
                    List<double> templineX = new List<double>();
                    List<double> templineY = new List<double>();
                    List<double> temparcBegin = new List<double>();
                    List<double> temparcEnd = new List<double>();
                    List<double> templineRad = new List<double>();
                    for (int k = 0; k < lineX.Count; k++)
                    {
                        if (k != j && k != i)
                        {
                            templineX.Add(lineX[k]);
                            templineY.Add(lineY[k]);
                            temparcBegin.Add(arcBegin[k]);
                            temparcEnd.Add(arcEnd[k]);
                            templineRad.Add(lineRad[k]);
                        }
                    }
                    lineX = templineX;
                    lineY = templineY;
                    arcBegin = temparcBegin;
                    arcEnd = temparcEnd;
                    lineRad = templineRad;
                    i -= 1;
                }
            }
        }


        void makeDots(double mX, double mY, double r, int amnt, double pos, double scaleFactor)
        {
            Target.noStroke();
            Target.fill(fg);
            if (amnt == 3)
            {
                Target.ellipse(mX + Math.Cos(pos + Math.PI) * r / 1.4, mY + Math.Sin(pos + Math.PI) * r / 1.4, r / 3 * scaleFactor, r / 3 * scaleFactor);
            }
            Target.ellipse(mX + Math.Cos(pos + Math.PI + scaleFactor) * r / 1.4, mY + Math.Sin(pos + Math.PI + scaleFactor) * r / 1.4, r / 3 * scaleFactor, r / 3 * scaleFactor);
            Target.ellipse(mX + Math.Cos(pos + Math.PI - scaleFactor) * r / 1.4, mY + Math.Sin(pos + Math.PI - scaleFactor) * r / 1.4, r / 3 * scaleFactor, r / 3 * scaleFactor);
            Target.noFill();
            Target.stroke(fg);
        }

        
        double double1 = 0;
        double double2 = 0;

        void makeArcs(double mX, double mY, double nX, double nY, double r1, double r2, double begin, double end)
        {
            double theta;
            double omega = 0;
            double d = dist(mX, mY, nX, nY);
            theta = Math.Acos((Math.Pow(r1, 2) - Math.Pow(r2, 2) + Math.Pow(d, 2)) / (2 * d * r1));
            if (nX - mX < 0)
            {
                omega = Math.Atan((mY - nY) / (mX - nX));
            }
            else if (nX - mX > 0)
            {
                omega = Math.PI + Math.Atan((mY - nY) / (mX - nX));
            }
            else if (nX - mX == 0)
            {
                if (nY > mY)
                {
                    omega = 3 * Math.PI / 2;
                }
                else
                {
                    omega = Math.PI / 2;
                }
            }
            if (omega + theta - end > 0)
            {
                Target.arc(nX, nY, r1 * 2, r1 * 2, omega + theta, end + Math.PI * 2.0);
                Target.arc(nX, nY, r1 * 2, r1 * 2, begin + Math.PI * 2.0, omega - theta);
            }
            else
            {
                Target.arc(nX, nY, r1 * 2, r1 * 2, omega + theta, end);
                Target.arc(nX, nY, r1 * 2, r1 * 2, begin + Math.PI * 2.0, omega - theta + Math.PI * 2.0);
            }
            if (omega + theta < end || omega - theta > begin)
            {
                Target.strokeCap(CAPSHAPE.SQUARE);
                Target.stroke(bg);
                Target.strokeWeight(4);
                Target.arc(nX, nY, r1*2, r1*2, omega-theta,omega+theta);
                Target.strokeWeight(2);
                Target.stroke(fg);
                Target.strokeCap(CAPSHAPE.ROUND);
            }
            theta = Math.PI - Math.Acos((Math.Pow(r2, 2) - Math.Pow(r1, 2) + Math.Pow(d, 2)) / (2 * d * r2));
            if (nX - mX < 0)
            {
                omega = Math.Atan((mY - nY) / (mX - nX));
            }
            else if (nX - mX > 0)
            {
                omega = Math.PI + Math.Atan((mY - nY) / (mX - nX));
            }
            else if (nX - mX == 0)
            {
                if (nY > mY)
                {
                    omega = 3 * Math.PI / 2;
                }
                else
                {
                    omega = Math.PI / 2;
                }
            }
            Target.arc(mX, mY, r2 * 2, r2 * 2, omega + theta, omega - theta + Math.PI * 2.0);
            double1 = omega + theta;
            double2 = omega - theta + Math.PI * 2.0;
        }

       

    }
}
