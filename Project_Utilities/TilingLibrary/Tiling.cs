using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlmNet;
using System.Drawing;

namespace Artwork
{
    public class Settings
    {
        public enum ArtMode
        {
            Tiling,
            QuadTree,
            Delaunay
        }
        public enum TriangleScaleMode
        {
            Original,
            Balanced
        }
        public Artwork.Tiling.TilingType TileType = Artwork.Tiling.TilingType.Danzer7FoldOriginal;
        public int BaseTile = 0;
        public ArtMode Mode = ArtMode.Tiling;
        public int MaxSubDiv = 5;
        public float DegreesOff = 0;
        public Random Rand = new Random();
        public int Threshold = 30;
        public bool InvertSource = false;
        public bool InvertOutput = false;

        public bool ReloadMask = false;
        public Color BackGroundColor = Color.Black;
        public Color BackgroundHighlight;
        public int scalesmaller;
        public bool alwayssubdivide;
        public int scalesmallerlevel;
        public bool Symmetry;
        public bool SuperSymmetry;
        public int xscalesmallerlevel;
        public int xscalecenter;
        public TriangleScaleMode scalingMode = TriangleScaleMode.Balanced;
    }

    public class Tiling
    {
        public enum TilingType
        {
            Danzer7Fold,
            Danzer7FoldOriginal,
            Maloney,
            Conway,
            Walton,
            Penrose
        }

        public class Polygon
        {
            public List<vec2> Vertices = new List<vec2>();
            public int Type = 0;
            public int depth = 0;
            internal bool divided = false;

            internal bool ContainsPoint(PointF b, float rad = .10f)
            {
                var BC = Triangle.barycentric(Vertices[0], Vertices[1], Vertices[2], new vec2(b.X, b.Y));
                if (BC.x > 1 + rad || BC.x < 0 - rad) return false;
                if (BC.y > 1 + rad || BC.y < 0 - rad) return false;
                if (BC.z > 1 + rad || BC.z < 0 - rad) return false;

                return true;
            }

            internal bool ContainsPointFaster(PointF b)
            {
                var BC = Triangle.barycentric(Vertices[0], Vertices[1], Vertices[2], new vec2(b.X, b.Y));
                if (BC.x > 1 || BC.x < 0) return false;
                if (BC.y > 1 || BC.y < 0) return false;
                if (BC.z > 1 || BC.z < 0) return false;

                return true;
            }

            public vec2 Mid()
            {
                return (Vertices[0] + Vertices[1] + Vertices[2]) / 3;
            }
            public void Trans(vec2 amt)
            {
                for (int i = 0; i < Vertices.Count; i++)

                {
                    Vertices[i] += amt;
                }
            }
            public int results = 0;
            public bool callback(QuadTreeItem Item)
            {
                if (ContainsPointFaster(new PointF(Item.x, Item.y)))
                {
                    results++;
                    return false;
                }
                return true;

            }
            public bool ContainsPointsInTree(QuadTreeNode tree)
            {

                float minx = Vertices[0].x;
                float miny = Vertices[0].y;
                float maxx = Vertices[0].x;
                float maxy = Vertices[0].y;

                for (int i = 1; i < 3; i++)
                {
                    if (Vertices[i].x > maxx) maxx = Vertices[i].x; else if (Vertices[i].x < minx) minx = Vertices[i].x;
                    if (Vertices[i].y > maxy) maxy = Vertices[i].y; else if (Vertices[i].y < miny) miny = Vertices[i].y;
                }
                Rectangle R = new Rectangle() { X = (int)Math.Floor(minx), Y = (int)Math.Floor(miny), Width = (int)Math.Ceiling((maxx - minx) + 1), Height = (int)Math.Ceiling((maxy - miny) + 1) };
                results = 0;
                tree.CallBackInside(R, callback);
                return (results > 0);

            }
            public void Rotate(float degreesOff, float xoff = 0, float yoff = 0)
            {
                double h = (degreesOff * Math.PI * 2.0) / 360.0;

                double c = Math.Cos(h);
                double s = Math.Sin(h);
                for (int i = 0; i < Vertices.Count; i++)
                {
                    float x = (float)(c * (Vertices[i].x - xoff) + s * (Vertices[i].y - yoff)) + xoff;
                    float y = (float)(-s * (Vertices[i].x - xoff) + c * (Vertices[i].y - yoff)) + yoff;
                    Vertices[i] = new vec2(x, y);
                }
            }
            public void AlterToFit(int width, int height)
            {
                var M = Mid();

                for (int i = 0; i < 3; i++)
                {
                    Vertices[i] -= M;
                }
                bool done = false;
                int iterations = 0;
                while (!done && iterations < 10000)
                {
                    int corners = 0;
                    if (ContainsPoint(new PointF(-width / 2, -height / 2), 0)) corners++;
                    if (ContainsPoint(new PointF(width / 2, -height / 2), 0)) corners++;
                    if (ContainsPoint(new PointF(width / 2, height / 2), 0)) corners++;
                    if (ContainsPoint(new PointF(-width / 2, height / 2), 0)) corners++;

                    if (corners == 4)
                    {
                        done = true;
                        for (int i = 0; i < 3; i++)
                        {
                            Vertices[i] += new vec2(width / 2, height / 2);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            Vertices[i] *= 1.1f;
                        }

                        iterations++;
                    }
                }
            }

            public void ShiftToEdge(float xoff = 0, float yoff = 0)
            {
                var centeredge = (Vertices[0] + Vertices[1]) * 0.5f;
                for (int i = 0; i < Vertices.Count; i++)
                {
                    Vertices[i] = new vec2(Vertices[i].x - centeredge.x + xoff, Vertices[i].y - centeredge.y + yoff);
                }
            }

            public void Flip(float xoff = 0, float yoff = 0)
            {
                for (int i = 0; i < Vertices.Count; i++)
                {
                    Vertices[i] = new vec2(-(Vertices[i].x - xoff) + xoff, -(Vertices[i].y - yoff) + yoff);
                }
            }

            public void FlipY(float xoff = 0, float yoff = 0)
            {
                for (int i = 0; i < Vertices.Count; i++)
                {
                    Vertices[i] = new vec2(Vertices[i].x, -(Vertices[i].y - yoff) + yoff);
                }
            }
            internal void MirrorAround(float xoff, float yoff)
            {
                // FlipY(xoff, yoff);
                //     Rotate(180, xoff, yoff);
                Reflect(Vertices[0], Vertices[1]);
            }

            private void Reflect(vec2 AA, vec2 BB)
            {
                vec2 N = (BB - AA);


                N = glm.normalize(N);

                for (int i = 0; i < Vertices.Count; i++)
                {
                    //Vect2 = Vect1 - 2 * WallN * (WallN DOT Vect1)
                    Vertices[i] = Vertices[i] - 2 * N * (glm.dot(Vertices[i], N));

                }

            }

            public void ScaleDown(Settings.TriangleScaleMode scalingMode, float factor)
            {
              
                switch (scalingMode)
                {
                    case Settings.TriangleScaleMode.Original:
                        {
                            var M = Mid();
                            var b0 = Vertices[0] - M;
                            var b1 = Vertices[1] - M;
                            var b2 = Vertices[2] - M;


                            Vertices[0] = M + b0 * factor;
                            Vertices[1] = M + b1 * factor;
                            Vertices[2] = M + b2 * factor;
                          
                        }
                        break;

                    case Settings.TriangleScaleMode.Balanced:
                        {
                            var v0  = new vec2(Vertices[0]);
                            var b0 = Mid() - v0;

                            var v1 = new vec2(Vertices[1]);
                            var v2 = new vec2(Vertices[2]);
                            var L = (float)Math.Sqrt(b0.x * b0.x + b0.y * b0.y);
                            //factor /= L;
                            var d0 = Vertices[0] - Vertices[2];
                            var d1 = Vertices[1] - Vertices[0];
                            var d2 = Vertices[2] - Vertices[1];

                            d0 = Rot90(d0);
                            d1 = Rot90(d1);
                            d2 = Rot90(d2);

                            //if (factor > L) factor = L;
                            factor = -factor;

                            vec3 A = new vec3(d0, 0);
                            vec3 B = new vec3(d1, 0);
                            float crossAB = GlmNet.glm.cross(A, B).z;
                            if (crossAB  > 0) factor = -factor;
                            var d0n = GlmNet.glm.normalize(d0) *factor;
                            var d1n = GlmNet.glm.normalize(d1) *factor;
                            var d2n = GlmNet.glm.normalize(d2) *factor;

                            //var b0n = GlmNet.glm.normalize(b0);
                            //var b1n = GlmNet.glm.normalize(b1);
                            //var b2n = GlmNet.glm.normalize(b2);



                            Vertices[0] = SegmentSegmentIntersect(v0 + d1n, v1 + d1n, v2 + d0n, v0 + d0n);
                            Vertices[1] = SegmentSegmentIntersect(v1 + d2n, v2 + d2n, v0 + d1n, v1 + d1n);
                            Vertices[2] = SegmentSegmentIntersect(v2 + d0n, v0 + d0n, v1 + d2n, v2 + d2n);


                            d0 = Vertices[0] - Vertices[2];
                            d1 = Vertices[1] - Vertices[0];

                            d0 = Rot90(d0);
                            d1 = Rot90(d1);


                            vec3 A2 = new vec3(d0, 0);
                            vec3 B2 = new vec3(d1, 0);
                            float crossAB2 = GlmNet.glm.cross(A2, B2).z;
                            if (Math.Sign(crossAB2) != Math.Sign(crossAB))
                            {
                                Vertices[0] = Mid();
                                Vertices[1] = Mid();
                                Vertices[2] = Mid();
                            }





                        }
                        break;
                }
            }

            private vec2 Rot90(vec2 d1)
            {
                vec2 r = new vec2(d1.y, -d1.x);

                return r;
            }
        }


        public static vec2 SegmentSegmentIntersect(vec2 P0, vec2 P1, vec2 P2, vec2 P3)
        {
            vec2 S1 = P1 - P0;
            vec2 S2 = P3 - P2;


            double s, t;
            s = (-S1.y * (P0.x - P2.x) + S1.x * (P0.y - P2.y)) / (-S2.x * S1.y + S1.x * S2.y);
            t = (S2.x * (P0.y - P2.y) - S2.y * (P0.x - P2.x)) / (-S2.x * S1.y + S1.x * S2.y);

          //  if (s >= 0 && s <= 1 && t >= 0 && t <= 1)
           // {
                double X = P0.x + (t * S1.x); ;
                double Y = P0.y + (t * S1.y);
                return new vec2((float)X, (float)Y);
            //}

           
            return new vec2(0,0); // No collision

        }


        public class SubPolygon
        {
            public List<int> VertexIndices = new List<int>();
            public int Type = 0;
        }

        public class PolygonMapping
        {
            public vec2 A = new vec2();
            public vec2 B = new vec2();
            public vec2 C = new vec2();
            public double AB;
            public double BC;
            public double AC;
            public double Alpha;
            public double Beta;
            public double Gamma;

            public int TypeID = 0;

            public void BuildVertex(float x = 0, float y = 0)
            {
                A.x = x;
                A.y = y;
                SolveAngles();
                B.x = A.x + (float)AB;
                B.y = A.y;
                C.x = A.x + (float)(Math.Cos(Alpha) * AC);
                C.y = A.y - (float)(Math.Sin(Alpha) * AC);
            }


            public void SolveAngles()
            {
                double a = BC;
                double b = AC;
                double c = AB;

                double cosC = ((a * a) + (b * b) - (c * c)) / (2.0 * a * b);
                double cosA = ((b * b) + (c * c) - (a * a)) / (2.0 * b * c);
                double cosB = ((c * c) + (a * a) - (b * b)) / (2.0 * c * a);

                Alpha = Math.Acos(cosA);
                Beta = Math.Acos(cosB);
                Gamma = Math.Acos(cosC);

            }

            public void BuildVerticesFromEdgeLengths(double a, double b, double c)
            {
                AB = a;
                BC = b;
                AC = c;
                SolveAngles();
                BuildVertex();
            }

            public List<vec3> BaryCoords = new List<vec3>();
            public List<SubPolygon> SubPolygons = new List<SubPolygon>();



            internal int AddCorners()
            {
                AddPoint(A);
                AddPoint(B);
                return AddPoint(C);
            }
            public vec2 GetCartesian(int idx)
            {
                return Triangle.cartesian(A, B, C, BaryCoords[idx]); ;
            }
            public int AddPoint(vec2 p)
            {
                BaryCoords.Add(Triangle.barycentric(A, B, C, p));
                return BaryCoords.Count - 1;
            }

            public int AddBetweenCorners(int idx1, int idx2, double totaledgelength, double distancefrom1)
            {
                var V1 = Triangle.cartesian(A, B, C, BaryCoords[idx1]);
                var V2 = Triangle.cartesian(A, B, C, BaryCoords[idx2]);

                var D = (V2 - V1) / (float)totaledgelength;

                var V3 = V1 + D * (float)distancefrom1;

                return AddPoint(V3);


            }

            public int AddTriangle(int idx1, int idx2, int idx3, int type)
            {
                SubPolygon SP = new SubPolygon();
                SP.VertexIndices.Add(idx1);
                SP.VertexIndices.Add(idx2);
                SP.VertexIndices.Add(idx3);
                SP.Type = type;
                SubPolygons.Add(SP);
                return SubPolygons.Count - 1;
            }

            public string Str(int idx)
            {
                var V = GetCartesian(idx);
                return "(" + V.x.ToString("N2").Replace(",", ".") + ", " + V.y.ToString("N2").Replace(",", ".") + ")";
            }
        }

        public class TilingDefinition
        {
            public Dictionary<int, PolygonMapping> DivisionSet = new Dictionary<int, PolygonMapping>();
            private double InflationFactor;
            public List<Polygon> Subdivide(Polygon P, int level)
            {
                List<Polygon> CurrentSet = new List<Polygon>();
                CurrentSet.Add(P);
                for (int i = 0; i < level; i++)
                {
                    List<Polygon> NextSet = new List<Polygon>();

                    foreach (var a in CurrentSet)
                    {
                        NextSet.AddRange(TilingDefinition.Subdivide(DivisionSet[a.Type], a.Vertices[0], a.Vertices[1], a.Vertices[2]));
                    }
                    CurrentSet = NextSet;
                }
                return CurrentSet;
            }
            public static List<Polygon> Subdivide(PolygonMapping map, vec2 a, vec2 b, vec2 c, int depth = 0) // subdivide based on first 3 barycentric coords
            {
                List<Polygon> T = new List<Polygon>();
                List<vec2> Vertices = new List<vec2>();

                foreach (var bc in map.BaryCoords)
                {
                    vec2 R = bc.x * a + bc.y * b + bc.z * c;
                    Vertices.Add(R);
                }

                foreach (var sp in map.SubPolygons)
                {
                    int type = sp.Type;
                    List<vec2> PolyVertices = new List<vec2>();
                    foreach (var v in sp.VertexIndices)
                    {
                        PolyVertices.Add(Vertices[v]);
                    }

                    T.Add(new Polygon() { Type = type, Vertices = PolyVertices, depth = depth + 1 });
                }
                return T;
            }

            public void CreateDanzers7FoldOriginal()
            {
                InflationFactor = 1.0 + Math.Sin((2.0 * Math.PI) / 7.0) / Math.Sin(Math.PI / 7.0);
                PolygonMapping t0 = new PolygonMapping();
                PolygonMapping t1 = new PolygonMapping();
                PolygonMapping t2 = new PolygonMapping();


                double a = Math.Sin(Math.PI / 7.0);
                double b = Math.Sin((2.0 * Math.PI) / 7.0);
                double g = Math.Sin((3.0 * Math.PI) / 7.0);

                int vA = 0;
                int vB = 1;
                int vC = 2;
                int vt0H = 0;
                int vt0G = 0;
                {
                    t0.BuildVerticesFromEdgeLengths(g, a, b);



                    t0.AddCorners();

                    int vD = t0.AddBetweenCorners(0, 1, g + b + g, g);
                    int vE = t0.AddBetweenCorners(0, 1, g + b + g, g + b);
                    int vF = t0.AddBetweenCorners(1, 2, a + b, a);

                    int vG = t0.AddBetweenCorners(2, 0, g + b + a, a);
                    int vH = t0.AddBetweenCorners(2, 0, g + b + a, a + b);

                    vt0G = vG;
                    vt0H = vH;
                    t0.AddTriangle(vA, vH, vD, 1);
                    t0.AddTriangle(vE, vH, vD, 0);
                    t0.AddTriangle(vH, vE, vG, 2);

                    t0.AddTriangle(vE, vC, vG, 0);
                    t0.AddTriangle(vC, vE, vF, 2);
                    t0.AddTriangle(vE, vB, vF, 0);
                }
                {
                    t1.BuildVerticesFromEdgeLengths(g, a, g);
                    t1.AddCorners();

                    int vD = t1.AddBetweenCorners(0, 1, g + b + g, g);
                    int vE = t1.AddBetweenCorners(0, 1, g + b + g, g + b);

                    int vF = t1.AddBetweenCorners(1, 2, a + b, a);
                    int vG = t1.AddBetweenCorners(2, 0, g + b + g, g);
                    int vH = t1.AddBetweenCorners(2, 0, g + b + g, g + b);

                    int vI = t1.AddPoint(((t1.GetCartesian(vD) + t1.GetCartesian(vH)) * 0.5f - t1.GetCartesian(vA)) * 2 + t1.GetCartesian(vA));


                    t1.AddTriangle(vA, vD, vH, 1);
                    t1.AddTriangle(vI, vD, vH, 1);
                    t1.AddTriangle(vH, vI, vG, 0);
                    t1.AddTriangle(vD, vI, vE, 0);
                    t1.AddTriangle(vC, vI, vG, 1);
                    t1.AddTriangle(vB, vI, vE, 1);
                    t1.AddTriangle(vI, vB, vF, 0);
                    t1.AddTriangle(vC, vI, vF, 2);
                }
                {
                    t2.BuildVerticesFromEdgeLengths(g, b, b);
                    t2.AddCorners();
                    int vD = t2.AddBetweenCorners(0, 1, g + b + g, g);
                    int vE = t2.AddBetweenCorners(0, 1, g + b + g, g + b);

                    int vF = t2.AddBetweenCorners(1, 2, g + b + a, g);
                    int vG = t2.AddBetweenCorners(1, 2, g + b + a, g + b);

                    int vH = t2.AddBetweenCorners(2, 0, g + b + a, g);
                    int vI = t2.AddBetweenCorners(2, 0, g + b + a, g + b);


                    int vJ = t2.AddPoint(t2.GetCartesian(vA) + (t0.GetCartesian(vB) - t0.GetCartesian(vt0G)) * new vec2(1, -1));
                    int vK = t2.AddPoint(t2.GetCartesian(vA) + (t0.GetCartesian(vB) - t0.GetCartesian(vt0H)) * new vec2(1, -1));

                    t2.AddTriangle(vD, vA, vI, 0);
                    t2.AddTriangle(vH, vD, vI, 2);
                    t2.AddTriangle(vD, vH, vJ, 0);
                    t2.AddTriangle(vK, vD, vJ, 2);
                    t2.AddTriangle(vD, vK, vE, 0);
                    t2.AddTriangle(vB, vK, vE, 1);
                    t2.AddTriangle(vB, vF, vK, 1);
                    t2.AddTriangle(vJ, vF, vK, 0);

                    t2.AddTriangle(vF, vJ, vG, 2);
                    t2.AddTriangle(vJ, vC, vG, 0);
                    t2.AddTriangle(vC, vH, vJ, 1);


                    //int vJ = t2.AddBetweenCorners(0, 1, g + b + g, g);
                    //int vK =  t2.AddBetweenCorners(0, 1, g + b + g, g);
                }


                DivisionSet[0] = t0;
                DivisionSet[1] = t1;
                DivisionSet[2] = t2;


            }

            public Polygon CreateBaseTriangle(int type, float renderscale, float x = 0.0f, float y = 0.0f)
            {
                type = type % DivisionSet.Count;
                var T = DivisionSet[type];
                vec2 MID = (T.A + T.B + T.C) / 3.0f;

                Polygon p = new Polygon();
                // todo: do more than a triangle here..
                p.Vertices.Add((T.A - MID + new vec2(x, y)) * renderscale);
                p.Vertices.Add((T.B - MID + new vec2(x, y)) * renderscale);
                p.Vertices.Add((T.C - MID + new vec2(x, y)) * renderscale);
                p.Type = type;

                return p;
            }

            public void CreateDanzers7Fold()
            {
                InflationFactor = 1.0 + Math.Sin((2.0 * Math.PI) / 7.0) / Math.Sin(Math.PI / 7.0);
                PolygonMapping t0 = new PolygonMapping();
                PolygonMapping t1 = new PolygonMapping();
                PolygonMapping t2 = new PolygonMapping();


                double a = Math.Sin(Math.PI / 7.0);
                double c = Math.Sin((2.0 * Math.PI) / 7.0);
                double b = Math.Sin((3.0 * Math.PI) / 7.0);



                double d = Math.Sin((3.0 * Math.PI) / 7.0) + Math.Sin((2.0 * Math.PI) / 7.0);


                a = 0.44504186791263;
                b = 1.000000000000002;
                d = 1.801937735804839;
                c = 1.246979603717467;

                //Console.WriteLine("{0} : {1}", b/c,  (d+b)/(c+b+c));
                //Console.WriteLine("{0} : {1}", d,  b * InflationFactor - a);
                //Console.WriteLine("{0} : {1}", d, (d + b)/ InflationFactor );

                //double d = b + c;
                //Console.WriteLine("a: {0} b: {1} c: {2} d: {3}, inflate:{4}", a, b, c, d, InflationFactor);


                int vA = 0;
                int vB = 1;
                int vC = 2;

                {
                    t0.BuildVerticesFromEdgeLengths(a, b, b);
                    t0.AddCorners();

                    int vD = t0.AddBetweenCorners(1, 2, d + b, d);
                    int vE = t0.AddBetweenCorners(2, 0, d + b, b);

                    int vF = t0.AddPoint(((t0.GetCartesian(vD) + t0.GetCartesian(vE)) * 0.5f - t0.GetCartesian(vC)) * 2 + t0.GetCartesian(vC));

                    t0.AddTriangle(vA, vE, vF, 2);
                    t0.AddTriangle(vA, vB, vF, 1);
                    t0.AddTriangle(vB, vD, vF, 2);
                    t0.AddTriangle(vD, vE, vF, 0);
                    t0.AddTriangle(vE, vD, vC, 0);
                }

                {
                    t2.BuildVerticesFromEdgeLengths(d, b, b);
                    t2.AddCorners();

                    int vD = t2.AddBetweenCorners(0, 1, b + b + d + c, b);
                    int vE = t2.AddBetweenCorners(0, 1, b + b + d + c, b + d);
                    int vF = t2.AddBetweenCorners(0, 1, b + b + d + c, b + d + c);

                    int vG = t2.AddBetweenCorners(1, 2, d + b, d);
                    int vH = t2.AddBetweenCorners(2, 0, d + b, d);


                    int vI = t2.AddPoint(((t2.GetCartesian(vD) + t2.GetCartesian(vH)) * 0.5f - t2.GetCartesian(vA)) * 2 + t2.GetCartesian(vA));
                    t2.AddTriangle(vD, vH, vA, 0);
                    t2.AddTriangle(vH, vD, vI, 0);
                    t2.AddTriangle(vE, vD, vI, 2);
                    t2.AddTriangle(vC, vH, vI, 2);
                    t2.AddTriangle(vE, vC, vI, 1);
                    t2.AddTriangle(vC, vE, vG, 1);
                    t2.AddTriangle(vE, vF, vG, 1);
                    t2.AddTriangle(vB, vG, vF, 2);
                }

                {
                    t1.BuildVerticesFromEdgeLengths(c, b, b);
                    t1.AddCorners();

                    int vD = t1.AddBetweenCorners(0, 1, c + b + c, c);
                    int vE = t1.AddBetweenCorners(0, 1, c + b + c, c + b);

                    int vF = t1.AddBetweenCorners(1, 2, b + d, b);

                    int vG = t1.AddBetweenCorners(2, 0, d + b, d);


                    var H = ((t1.GetCartesian(vD) - t1.GetCartesian(vC)) / (float)(b + c)) * (float)b + t1.GetCartesian(vC);
                    int vH = t1.AddPoint(H);

                    var I = ((t1.GetCartesian(vE) - t1.GetCartesian(vC)) / (float)(b + c)) * (float)d + t1.GetCartesian(vC);
                    int vI = t1.AddPoint(I);

                    var J = ((t1.GetCartesian(vE) - t1.GetCartesian(vC)) / (float)(b + c)) * (float)b + t1.GetCartesian(vC);
                    int vJ = t1.AddPoint(J);

                    t1.AddTriangle(vA, vD, vG, 1);
                    t1.AddTriangle(vD, vH, vG, 1);
                    t1.AddTriangle(vC, vG, vH, 2);

                    t1.AddTriangle(vE, vI, vD, 0);
                    t1.AddTriangle(vH, vD, vI, 1);
                    t1.AddTriangle(vC, vI, vH, 2);
                    t1.AddTriangle(vB, vE, vF, 1);
                    t1.AddTriangle(vE, vJ, vF, 1);
                    t1.AddTriangle(vC, vF, vJ, 2);

                }


                DivisionSet[0] = t0;
                DivisionSet[1] = t1;
                DivisionSet[2] = t2;

            }

            public void CreateDaleWalton1()
            {
                PolygonMapping t0 = new PolygonMapping() { TypeID = 0 };
                PolygonMapping t1 = new PolygonMapping() { TypeID = 1 };
                PolygonMapping t2 = new PolygonMapping() { TypeID = 2 };

                List<PolygonMapping> T = new List<PolygonMapping>();
                int vA = 0;
                int vB = 1;
                int vC = 2;
                T.Add(t0);
                T.Add(t1);
                T.Add(t2);
                int off = 0;
                foreach (var t in T)
                {
                    t.BuildVerticesFromEdgeLengths(5, 4, 3);

                    t.AddCorners();
                    int vD = t.AddPoint(new vec2(t.C.x, 0));

                    double FC = Math.Sin(t.Beta) * t.GetCartesian(vC).y;

                    int vF = t.AddBetweenCorners(2, 1, t.BC, -FC);
                    int vE = t.AddPoint(new vec2(t.GetCartesian(vF).x, 0));
                    int vG = t.AddPoint(new vec2(t.GetCartesian(vC).x, t.GetCartesian(vF).y));

                    t.AddTriangle(vA, vC, vD, (off) % 3);
                    t.AddTriangle(vC, vF, vG, (off) % 3);
                    t.AddTriangle(vE, vG, vD, (off + 1) % 3);
                    t.AddTriangle(vG, vE, vF, (off + 1) % 3);
                    t.AddTriangle(vF, vB, vE, (off + 2) % 3);


                    off++;
                    //    t.ad
                }
                DivisionSet[0] = t0;
                DivisionSet[1] = t1;
                DivisionSet[2] = t2;

            }

            public void CreateConway()
            {
                PolygonMapping t = new PolygonMapping() { TypeID = 0 };
                PolygonMapping t2 = new PolygonMapping() { TypeID = 0 };

                int vA = 0;
                int vB = 1;
                int vC = 2;
                t.BuildVerticesFromEdgeLengths(2, Math.Sqrt(5), 1);
                t2.BuildVerticesFromEdgeLengths(2, Math.Sqrt(5), 1);

                t.AddCorners();
                t2.AddCorners();
                {
                    int vD = t.AddPoint(new vec2(1, 0));
                    int vE = t.AddBetweenCorners(1, 2, 5, 2);
                    int vF = t.AddBetweenCorners(1, 2, 5, 4);

                    int vG = t.AddPoint((t.GetCartesian(vF) + t.GetCartesian(vA)) / 2.0f);


                    t.AddTriangle(vF, vA, vC, 1);
                    t.AddTriangle(vG, vD, vA, 0);
                    t.AddTriangle(vG, vD, vF, 1);
                    t.AddTriangle(vE, vF, vD, 0);
                    t.AddTriangle(vE, vB, vD, 1);
                }

                {
                    int vD = t2.AddPoint(new vec2(1, 0));
                    int vE = t2.AddBetweenCorners(1, 2, 5, 2);
                    int vF = t2.AddBetweenCorners(1, 2, 5, 4);

                    int vG = t2.AddPoint((t2.GetCartesian(vF) + t2.GetCartesian(vA)) / 2.0f);


                    t2.AddTriangle(vF, vA, vC, 0);
                    t2.AddTriangle(vG, vD, vA, 1);
                    t2.AddTriangle(vG, vD, vF, 0);
                    t2.AddTriangle(vE, vF, vD, 1);
                    t2.AddTriangle(vE, vB, vD, 0);
                }


                DivisionSet[0] = t;
                DivisionSet[1] = t2;
            }


            vec2 mirror(vec2 p, vec2 P0, vec2 P1)
            {

                double dx, dy, a, b;
                double x2, y2;
                vec2 pres; //reflected point to be returned 
                dx = (double)(P1.x - P0.x);
                dy = (double)(P1.y - P0.y);

                a = (dx * dx - dy * dy) / (dx * dx + dy * dy);
                b = 2 * dx * dy / (dx * dx + dy * dy);

                x2 = a * (p.x - P0.x) + b * (p.y - P0.y) + P0.x;
                y2 = b * (p.x - P0.x) - a * (p.y - P0.y) + P0.y;

                pres = new vec2((float)x2, (float)y2);
                return pres;
            }

            public void CreateMaloney()
            {


                InflationFactor = 1.0 + Math.Sin((2.0 * Math.PI) / 7.0) / Math.Sin(Math.PI / 7.0);
                PolygonMapping t0 = new PolygonMapping();
                PolygonMapping t1 = new PolygonMapping();
                PolygonMapping t2 = new PolygonMapping();


                double a = Math.Sin(Math.PI / 7.0);
                double b = Math.Sin((2.0 * Math.PI) / 7.0);
                double c = Math.Sin((3.0 * Math.PI) / 7.0);
                //     Console.WriteLine("{0} {1} {2}", a, b, c);


                int vA = 0;
                int vB = 1;
                int vC = 2;

                {
                    t0.BuildVerticesFromEdgeLengths(a, b, c);
                    t0.AddCorners();

                    int vD = t0.AddBetweenCorners(0, 1, c + b, c);
                    int vE = t0.AddBetweenCorners(1, 2, c + a + b + c, c);
                    int vF = t0.AddBetweenCorners(1, 2, c + a + b + c, c + a);
                    int vG = t0.AddBetweenCorners(1, 2, c + a + b + c, c + a + b);
                    int vH = t0.AddBetweenCorners(2, 0, b + c + a + b + c, b);
                    int vI = t0.AddBetweenCorners(2, 0, b + c + a + b + c, b + c);
                    int vJ = t0.AddBetweenCorners(2, 0, b + c + a + b + c, b + c + a);
                    int vK = t0.AddBetweenCorners(2, 0, b + c + a + b + c, b + c + a + b);

                    int vL = t0.AddPoint(t0.GetCartesian(vA) + glm.normalize(((t0.GetCartesian(vK) + t0.GetCartesian(vD)) * 0.5f)) * (float)(c / (c + b + a + c + b)));


                    int vM = t0.AddPoint(mirror(t0.GetCartesian(vK), t0.GetCartesian(vJ), t0.GetCartesian(vL)));


                    t0.AddTriangle(vD, vL, vA, 1);
                    t0.AddTriangle(vK, vL, vA, 1);
                    t0.AddTriangle(vL, vD, vB, 0);
                    t0.AddTriangle(vL, vM, vB, 0);
                    t0.AddTriangle(vB, vE, vM, 2);


                    t0.AddTriangle(vL, vK, vJ, 0);
                    t0.AddTriangle(vL, vM, vJ, 0);
                    t0.AddTriangle(vJ, vE, vM, 2);

                    t0.AddTriangle(vJ, vI, vE, 1);
                    t0.AddTriangle(vE, vF, vI, 0);
                    t0.AddTriangle(vI, vG, vF, 2);
                    t0.AddTriangle(vH, vG, vI, 1);
                    t0.AddTriangle(vG, vH, vC, 0);

                    //       t0.AddTriangle(vD, vE, vF, 0);
                    //     t0.AddTriangle(vE, vD, vC, 0);
                }

                {
                    t1.BuildVerticesFromEdgeLengths(a, c, c);
                    t1.AddCorners();

                    int vD = t1.AddBetweenCorners(0, 1, c + b, b);

                    int vE = t1.AddBetweenCorners(1, 2, b + c + a + b + c, b);
                    int vF = t1.AddBetweenCorners(1, 2, b + c + a + b + c, b + c);
                    int vG = t1.AddBetweenCorners(1, 2, b + c + a + b + c, b + c + a);
                    int vH = t1.AddBetweenCorners(1, 2, b + c + a + b + c, b + c + a + b);

                    int vI = t1.AddBetweenCorners(2, 0, b + c + a + b + c, b);
                    int vJ = t1.AddBetweenCorners(2, 0, b + c + a + b + c, b + c);
                    int vK = t1.AddBetweenCorners(2, 0, b + c + a + b + c, b + c + a);
                    int vL = t1.AddBetweenCorners(2, 0, b + c + a + b + c, b + c + a + b);

                    int vM = t1.AddPoint(t1.GetCartesian(vA) + ((t1.GetCartesian(vF) - t1.GetCartesian(vA)) / (float)(b + a + c)) * (float)c);
                    int vN = t1.AddPoint(t1.GetCartesian(vA) + ((t1.GetCartesian(vF) - t1.GetCartesian(vA)) / (float)(b + a + c)) * (float)(c + a));
                    int vO = t1.AddPoint(t1.GetCartesian(vA) + glm.normalize(((t1.GetCartesian(vE)))) * (float)((c) / (c + b + a + c + b)));

                    //int vN = t1.AddPoint(mirror(t0.GetCartesian(vK), t0.GetCartesian(vJ), t0.GetCartesian(vL)));

                    t1.AddTriangle(vO, vD, vA, 0);
                    t1.AddTriangle(vO, vM, vA, 1);
                    t1.AddTriangle(vL, vM, vA, 1);
                    t1.AddTriangle(vD, vO, vB, 1);
                    t1.AddTriangle(vB, vO, vE, 2);


                    t1.AddTriangle(vM, vO, vE, 0);
                    t1.AddTriangle(vM, vN, vE, 0);


                    t1.AddTriangle(vE, vF, vN, 2);
                    t1.AddTriangle(vM, vL, vK, 0);
                    t1.AddTriangle(vM, vN, vK, 0);

                    t1.AddTriangle(vK, vF, vN, 2);
                    t1.AddTriangle(vK, vJ, vF, 1);
                    t1.AddTriangle(vF, vG, vJ, 0);

                    t1.AddTriangle(vJ, vH, vG, 2);
                    t1.AddTriangle(vI, vH, vJ, 1);


                    t1.AddTriangle(vH, vI, vC, 0);


                }

                {
                    t2.BuildVerticesFromEdgeLengths(c, b, b);
                    t2.AddCorners();

                    int vD = t2.AddBetweenCorners(0, 1, c + b + a + c + b, c);
                    int vE = t2.AddBetweenCorners(0, 1, c + b + a + c + b, c + b);
                    int vF = t2.AddBetweenCorners(0, 1, c + b + a + c + b, c + b + a);
                    int vG = t2.AddBetweenCorners(0, 1, c + b + a + c + b, c + b + a + c);


                    int vH = t2.AddBetweenCorners(1, 2, c + a + b + c, c);
                    int vI = t2.AddBetweenCorners(1, 2, c + a + b + c, c + a);
                    int vJ = t2.AddBetweenCorners(1, 2, c + a + b + c, c + a + b);

                    int vK = t2.AddBetweenCorners(2, 0, c + a + b + c, c);
                    int vL = t2.AddBetweenCorners(2, 0, c + a + b + c, c + a);
                    int vM = t2.AddBetweenCorners(2, 0, c + a + b + c, c + a + b);
                    int vP = t2.AddPoint(mirror(t2.GetCartesian(vC), t2.GetCartesian(vK), t2.GetCartesian(vJ)));

                    int vN = t2.AddBetweenCorners(vA, vP, c + a + b, c);

                    int vO = t2.AddBetweenCorners(vA, vP, c + a + b, c + a);


                    int vQ = t2.AddBetweenCorners(vP, vB, c + a + b, a);
                    int vR = t2.AddBetweenCorners(vP, vB, c + a + b, a + b);

                    int vS = t2.AddBetweenCorners(vK, vJ, b + a, b);

                    t2.AddTriangle(vD, vN, vA, 1);
                    t2.AddTriangle(vM, vN, vA, 1);

                    t2.AddTriangle(vN, vD, vE, 0);
                    t2.AddTriangle(vN, vO, vE, 0);
                    t2.AddTriangle(vE, vP, vO, 2);

                    t2.AddTriangle(vE, vF, vP, 1);

                    t2.AddTriangle(vP, vQ, vF, 0);

                    t2.AddTriangle(vG, vR, vF, 1);

                    t2.AddTriangle(vR, vG, vB, 0);

                    t2.AddTriangle(vR, vH, vB, 1);
                    t2.AddTriangle(vH, vR, vQ, 0);
                    t2.AddTriangle(vH, vI, vQ, 0);

                    t2.AddTriangle(vF, vR, vQ, 2);
                    t2.AddTriangle(vQ, vJ, vI, 2);


                    t2.AddTriangle(vQ, vP, vJ, 1);
                    t2.AddTriangle(vJ, vS, vP, 0);

                    t2.AddTriangle(vJ, vS, vC, 0);
                    t2.AddTriangle(vC, vK, vS, 2);
                    t2.AddTriangle(vP, vK, vS, 2);

                    t2.AddTriangle(vL, vK, vP, 1);
                    t2.AddTriangle(vL, vP, vO, 2);

                    t2.AddTriangle(vN, vO, vL, 0);
                    t2.AddTriangle(vN, vM, vL, 0);

                }


                DivisionSet[0] = t0;
                DivisionSet[1] = t1;
                DivisionSet[2] = t2;



            }

            public void CreatePenrose()
            {

                InflationFactor = 1.0 + Math.Sin((2.0 * Math.PI) / 7.0) / Math.Sin(Math.PI / 7.0);
                PolygonMapping t0 = new PolygonMapping();
                PolygonMapping t1 = new PolygonMapping();
                double a = 1 / ((1.0 + Math.Sqrt(5)) / 2.0);
                double b = 1;
                double c = b / a;


                int vA = 0;
                int vB = 1;
                int vC = 2;

                {
                    t0.BuildVerticesFromEdgeLengths(a, b, b);
                    t0.AddCorners();
                    int vD = t0.AddBetweenCorners(vC, vA, a + b, b);
                    t0.AddTriangle(vD, vA, vB, 0);
                    t0.AddTriangle(vB, vC, vD, 1);
                }
                {
                    t1.BuildVerticesFromEdgeLengths(c, b, b);
                    t1.AddCorners();

                    int vD = t1.AddBetweenCorners(vA, vB, c + b, c);
                    int vE = t1.AddBetweenCorners(vC, vA, a + b, a);
                    t1.AddTriangle(vD, vA, vE, 1);
                    t1.AddTriangle(vE, vC, vD, 0);
                    t1.AddTriangle(vB, vC, vD, 1);

                }
                DivisionSet[0] = t0;
                DivisionSet[1] = t1;

            }
            public vec2 NormalizeSize()
            {
                double len = 0;
                foreach (var a in DivisionSet.Values)
                {
                    len = Math.Max(len, a.AB);
                    len = Math.Max(len, a.AC);
                    len = Math.Max(len, a.BC);
                }
                double div = 1.0 / len;
                vec2 mid = new vec2(0, 0);
                foreach (var a in DivisionSet.Values)
                {
                    a.A *= (float)div;
                    a.B *= (float)div;
                    a.C *= (float)div;
                    a.AB *= div;
                    a.AC *= div;
                    a.BC *= div;
                    mid += (a.A + a.B + a.C) / 3.0f;
                }


                mid /= (float)DivisionSet.Count;

                return mid;

            }

            public vec2 Create(TilingType tilingType)
            {
                switch (tilingType)
                {
                    case TilingType.Danzer7Fold: CreateDanzers7Fold(); break;
                    case TilingType.Walton: CreateDaleWalton1(); break;
                    case TilingType.Danzer7FoldOriginal: CreateDanzers7FoldOriginal(); break;
                    case TilingType.Conway: CreateConway(); break;
                    case TilingType.Maloney: CreateMaloney(); break;
                    case TilingType.Penrose: CreatePenrose(); break;

                }
                return NormalizeSize();

            }

            public List<Polygon> SubdivideAdaptive(Polygon P, int level, QuadTreeNode Tree, bool alwayssubdivide = false)
            {


                List<Polygon> CurrentSet = new List<Polygon>(1);
                CurrentSet.Add(P);

                for (int i = 0; i < level; i++)
                {
                    List<Polygon> NextSet = new List<Polygon>(CurrentSet.Count + 1000);

                    foreach (var a in CurrentSet)
                    {
                        int divide = 0;
                        if (alwayssubdivide) divide++;
                        if (a.divided == false)
                        {
                            if (a.ContainsPointsInTree(Tree)) divide++;
                            if (divide > 0)
                            {

                                a.divided = true;
                                NextSet.AddRange(TilingDefinition.Subdivide(DivisionSet[a.Type], a.Vertices[0], a.Vertices[1], a.Vertices[2], a.depth));

                            }
                            else
                            {
                                a.divided = true;
                                NextSet.Add(a);

                            }
                        }
                        else
                        {
                            a.divided = true;
                            NextSet.Add(a);

                        }
                    }
                    CurrentSet = NextSet;
                }
                List<Polygon> ResultSet = new List<Polygon>(CurrentSet.Count);

                foreach (var a in CurrentSet)
                {
                    if (CheckPoints(a, Tree))
                    {
                        ResultSet.Add(a);
                    }
                }
                return ResultSet;
            }

            public bool CheckPoints(Polygon a, QuadTreeNode Tree)
            {
                if (a.ContainsPointsInTree(Tree)) return false;
                return true;
            }
        }

        public class Triangle
        {
            public static vec3 barycentric(vec2 a, vec2 b, vec2 c, vec2 vec)
            {
                vec3 lambda = new vec3();
                float den = 1 / ((b.y - c.y) * (a.x - c.x) + (c.x - b.x) * (a.y - c.y));

                lambda.x = ((b.y - c.y) * (vec.x - c.x) + (c.x - b.x) * (vec.y - c.y)) * den;
                lambda.y = ((c.y - a.y) * (vec.x - c.x) + (a.x - c.x) * (vec.y - c.y)) * den;
                lambda.z = 1 - lambda.x - lambda.y;

                return lambda;
            }
            public static vec2 cartesian(vec2 a, vec2 b, vec2 c, vec3 p)
            {
                return a * p.x + b * p.y + c * p.z;
            }

            public vec2 A;
            public vec2 B;
            public vec2 C;

            public double AB;
            public double AC;
            public double BC;

            public double Alpha;
            public double Beta;
            public double Gamma;

            public double rotation;
            public vec2 center;

            public int type = 0;


            internal void SolveAngles()
            {
                double a = BC;
                double b = AC;
                double c = AB;

                double cosC = ((a * a) + (b * b) - (c * c)) / (2.0 * a * b);
                double cosA = ((b * b) + (c * c) - (a * a)) / (2.0 * b * c);
                double cosB = ((c * c) + (a * a) - (b * b)) / (2.0 * c * a);

                Alpha = Math.Acos(cosA);
                Beta = Math.Acos(cosB);
                Gamma = Math.Acos(cosC);

            }
            public List<Triangle> SubTiles = new List<Triangle>();
            public float depth;
            public void Render(Graphics G, int Depth = 5)
            {
                PointF[] Ps = new PointF[3];
                Ps[0].X = (float)A.x;
                Ps[0].Y = (float)A.y;

                Ps[1].X = (float)B.x;
                Ps[1].Y = (float)B.y;
                Ps[2].X = (float)C.x;
                Ps[2].Y = (float)C.y;


                bool dAB = false;
                bool dBC = false;
                bool dCA = false;

                Color Cl = Color.Red;
                Color Cl2 = Color.FromArgb(100, 0, 0, 0);
                switch (type)
                {
                    //                case 0: dAB = false; dBC = false; dCA = false; Cl = Color.FromArgb(0xff, 0xcc, 0xca, 0xe4); Cl = Color.Blue; break;
                    //                  case 1: dAB = true; dBC = false; dCA = false; Cl = Color.FromArgb(0xff, 0xfc, 0x66, 0x13); Cl = Color.White;  break;
                    //                    case 2: dAB = true; dBC = true; dCA = true; Cl = Color.FromArgb(0xfc, 0xca, 0x64); Cl = Color.FromArgb(200,255,255); break;
                    //Cl2 = Color.Yellow; 

                    case 0: dAB = false; dBC = false; dCA = false; Cl = Color.FromArgb(0xff, 0xcc, 0xca, 0xe4); break;
                    case 1: dAB = true; dBC = false; dCA = false; Cl = Color.FromArgb(0xff, 0xfc, 0x66, 0x13); break;
                    case 2: dAB = true; dBC = true; dCA = true; Cl = Color.FromArgb(0xfc, 0xca, 0x64); break;

                    case 5: Cl = Color.Red; break;
                }

                Pen P = new Pen(Cl2, 2);
                P.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                P.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;


                if (SubTiles.Count == 0)
                {
                    G.FillPolygon(new SolidBrush(Cl), Ps);
                    G.DrawPolygon(P, Ps);
                }
                else
                    foreach (var a in SubTiles)
                    {
                        a.Render(G, Depth - 1);
                    }


                vec2 aDiff = (B - A);
                vec2 bDiff = (C - B);
                vec2 cDiff = (A - C);


                vec2 Cen = (A + B + C) / 3.0f;
                G.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                G.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

                if (false)
                {
                    drawdot(G, A, B, dAB, Cen);
                    drawdot(G, B, C, dBC, Cen);
                    drawdot(G, C, A, dCA, Cen);
                }

            }

            private void drawdot(Graphics G, vec2 A, vec2 B, bool dAB, vec2 Cen)
            {
                vec2 aA = A;
                vec2 bB = B;
                if (dAB == true)
                {
                    aA = B;
                    bB = A;
                }


                vec2 D = (bB - aA) / 20.0f;
                vec2 Rr = ((aA + D) * 10 + Cen) / 11.0f;
                G.DrawLine(new Pen(Color.Black, 3), Rr.x, Rr.y, Rr.x, Rr.y + 10);

            }

            public void SubDivide(int N)
            {
                double a = Math.Sin(Math.PI / 7.0);
                double b = Math.Sin((2.0 * Math.PI) / 7.0);
                double g = Math.Sin((3.0 * Math.PI) / 7.0);


                switch (type)
                {
                    case 0:
                        {
                            //  if (N == 0) return;
                            vec2 dAB = (B - A) / (float)(g * 2 + b);
                            vec2 dAC = (C - A) / (float)(g + b + a);
                            vec2 dBC = (C - B) / (float)(a + b);

                            vec2 D = A + dAB * (float)g;
                            vec2 E = A + dAB * (float)(g + b);
                            vec2 F = B + dBC * (float)a;
                            vec2 G = A + dAC * (float)(g + b);
                            vec2 H = A + dAC * (float)g;


                            SubTiles.Add(new Triangle() { type = 1, A = A, B = H, C = D });
                            SubTiles.Add(new Triangle() { type = 0, A = E, B = H, C = D });
                            SubTiles.Add(new Triangle() { type = 2, A = E, B = G, C = H });
                            SubTiles.Add(new Triangle() { type = 0, A = E, B = C, C = G });

                            SubTiles.Add(new Triangle() { type = 2, A = E, B = F, C = C });

                            SubTiles.Add(new Triangle() { type = 0, A = E, B = B, C = F });
                        }
                        break;
                    case 1:
                        {
                            //  if (N == 0) return;

                            vec2 dAB = (B - A) / (float)(g * 2 + b);
                            vec2 dAC = (C - A) / (float)(g * 2 + b);
                            vec2 dBC = (C - B) / (float)(a + b);


                            vec2 D = A + dAB * (float)g;
                            vec2 E = A + dAB * (float)(g + b);

                            vec2 F = B + dBC * (float)a;
                            vec2 G = A + dAC * (float)(g + b);
                            vec2 H = A + dAC * (float)g;

                            vec2 I = (((D + H) / 2.0f) - A) * 2 + A;

                            //vec2 I = A + dAC * (float)g;


                            SubTiles.Add(new Triangle() { type = 1, A = A, B = H, C = D });
                            SubTiles.Add(new Triangle() { type = 1, A = I, B = H, C = D });
                            SubTiles.Add(new Triangle() { type = 0, A = D, B = I, C = E });
                            SubTiles.Add(new Triangle() { type = 1, A = B, B = I, C = E });
                            SubTiles.Add(new Triangle() { type = 0, A = I, B = B, C = F });


                            SubTiles.Add(new Triangle() { type = 2, A = I, B = F, C = C });
                            SubTiles.Add(new Triangle() { type = 1, A = C, B = I, C = G });
                            SubTiles.Add(new Triangle() { type = 0, A = H, B = I, C = G });

                        }

                        break;
                    case 2:
                        {
                            vec2 dAB = (B - A) / (float)(g + a + b);
                            vec2 dAC = (C - A) / (float)(b + g + g);
                            vec2 dBC = (C - B) / (float)(a + b + g);

                            vec3 CB = new vec3(C - B, 0);
                            vec3 AB = new vec3(A - B, 0);
                            var cross = glm.cross(CB, AB);

                            double H1 = Math.Atan2(CB.y, CB.x);


                            Triangle T = new Triangle();

                            T.AB = a;
                            T.BC = g;
                            T.AC = g;
                            T.SolveAngles();

                            double Dif = 0;

                            if (cross.z > 0) Dif = H1 + T.Gamma; else Dif = H1 - T.Gamma;

                            vec2 D = A + dAB * (float)g;
                            vec2 E = A + dAB * (float)(g + b);

                            vec2 F = B + dBC * (float)g;
                            vec2 G = B + dBC * (float)(g + b);

                            vec2 H = A + dAC * (float)(g + b);

                            vec2 I = A + dAC * (float)g;
                            vec2 norm = glm.normalize((((I + D) / 2.0f) - A));
                            vec2 Dist = (I - A);

                            float scale = (float)Math.Sqrt(Dist.x * Dist.x + Dist.y * Dist.y);
                            vec2 J = A + norm * (float)scale;


                            vec2 K1;
                            K1.x = B.x;
                            K1.y = B.y;
                            K1.x += (float)Math.Cos(Dif) * scale;
                            K1.y += (float)Math.Sin(Dif) * scale;

                            vec2 K = K1;

                            SubTiles.Add(new Triangle() { type = 1, A = A, B = D, C = J });
                            SubTiles.Add(new Triangle() { type = 0, A = K, B = D, C = J });
                            SubTiles.Add(new Triangle() { type = 2, A = K, B = E, C = D });
                            SubTiles.Add(new Triangle() { type = 1, A = B, B = F, C = K });

                            SubTiles.Add(new Triangle() { type = 0, A = K, B = B, C = E });

                            SubTiles.Add(new Triangle() { type = 0, A = H, B = F, C = K });
                            SubTiles.Add(new Triangle() { type = 2, A = H, B = G, C = F });

                            SubTiles.Add(new Triangle() { type = 0, A = H, B = C, C = G });

                            SubTiles.Add(new Triangle() { type = 2, A = H, B = K, C = J });
                            SubTiles.Add(new Triangle() { type = 0, A = H, B = J, C = I });
                            SubTiles.Add(new Triangle() { type = 1, A = A, B = J, C = I });

                        }
                        break;
                }
                foreach (var s in SubTiles)
                {
                    s.depth = this.depth + 1;
                }
                if (N > 0)
                {
                    foreach (var s in SubTiles)
                    {
                        s.SubDivide(N - 1);
                    }
                }
            }
            public void Print()
            {

                Console.WriteLine("a {0} : b {1} : c {2}", Alpha, Beta, Gamma);
                Console.WriteLine("AB {0} : BC {1} : AC {2}", AB, BC, AC);
            }

            public void BuildVertex(float x = 0, float y = 0)
            {
                A.x = x;
                A.y = y;
                SolveAngles();
                B.x = A.x + (float)AB;
                B.y = A.y;
                C.x = A.x + (float)(Math.Cos(Alpha) * AC);
                C.y = A.y - (float)(Math.Sin(Alpha) * AC);
            }
        }


        public List<Triangle> TriangleTypes = new List<Triangle>();

        public Tiling()
        {
            double a = Math.Sin(Math.PI / 7.0);
            double b = Math.Sin((2.0 * Math.PI) / 7.0);
            double g = Math.Sin((3.0 * Math.PI) / 7.0);

            Triangle T0 = new Triangle() { type = 0, AB = g, AC = b, BC = a };
            Triangle T1 = new Triangle() { type = 1, AB = g, AC = g, BC = a };
            Triangle T2 = new Triangle() { type = 2, AB = b, AC = g, BC = b };

            T0.SolveAngles();
            T1.SolveAngles();
            T2.SolveAngles();

            TriangleTypes.Add(T0);
            TriangleTypes.Add(T1);
            TriangleTypes.Add(T2);
        }

        public static int DefaultTilingDepth(TilingType Type)
        {
            switch (Type)
            {
                case TilingType.Conway: return 5;
                case TilingType.Maloney: return 3;
                case TilingType.Penrose: return 8;
                case TilingType.Walton: return 5;
                case TilingType.Danzer7Fold: return 5;
                case TilingType.Danzer7FoldOriginal: return 5;
            }
            return 1;

        }
    }
}
