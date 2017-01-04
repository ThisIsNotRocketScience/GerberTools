using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GerberLibrary;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
//using System.Drawing.Drawing2D;
using GerberLibrary.Core.Primitives;
using GerberLibrary.Core;
using System.Drawing.Drawing2D;


using TriangleNet.Geometry;
using TriangleNet.IO;
using TriangleNet.Meshing;
using System.IO;

namespace GerberViewer
{
    public class GerberVBO : GraphicsInterface
    {

        public struct VBOVertex
        {
            public float X, Y, Z;
            public float DX, DY, DZ;
            public float R, G, B, A;

            public VBOVertex(float x, float y, float z, float dx, float dy, float dz, float r, float g, float b, float a)
            {
                X = x;
                Y = y;
                Z = z;
                DX = dx;
                DY = dy;
                DZ = dz;
                R = r;
                G = g;
                B = b;
                A = a;
            }


            public const int Stride = 40;
        }


        private int ID_VBO = -1;


        private List<VBOVertex> Vertices = new List<VBOVertex>(100000);
        public void Reset()
        {
            Vertices.Clear();
            if (ID_VBO > -1)
            {
                GL.DeleteBuffer(ID_VBO);
                ID_VBO = -1;
            }
        }
        int TheVertexCount = 0;
        public void BuildVBO()
        {
            if (ID_VBO > -1)
            {
                GL.DeleteBuffer(ID_VBO);
                ID_VBO = -1;
            }
            GL.GenBuffers(1, out ID_VBO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, ID_VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Vertices.Count() * VBOVertex.Stride), Vertices.ToArray(), BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            TheVertexCount = Vertices.Count();
            Console.WriteLine("{0} vertices in the VBO!", Vertices.Count());
            Vertices.Clear();

        }


        public void RenderVBO(ShaderProgram Shader)
        {
            if (ID_VBO < 0) return;
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, ID_VBO);
            GL.EnableVertexAttribArray(Shader.Attributes["vPosition"].address);
            GL.EnableVertexAttribArray(Shader.Attributes["vOff"].address);
            GL.EnableVertexAttribArray(Shader.Attributes["vColor"].address);


            GL.VertexAttribPointer(Shader.Attributes["vPosition"].address, 3, VertexAttribPointerType.Float, false, VBOVertex.Stride, 0);
            GL.VertexAttribPointer(Shader.Attributes["vOff"].address, 3, VertexAttribPointerType.Float, false, VBOVertex.Stride, 4*3);
            GL.VertexAttribPointer(Shader.Attributes["vColor"].address, 4 , VertexAttribPointerType.Float, false, VBOVertex.Stride, 4*6);



            GL.DrawArrays(PrimitiveType.Triangles, 0, TheVertexCount);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);


        }
        RectangleF ClipRect;

        public void SetupClipRect(float x, float y, float w, float h)
        {
            ClipRect = new RectangleF(x, y, w, h);
        }

        public RectangleF ClipBounds
        {
            get { return ClipRect; }
        }

        public static Matrix4 Mat4FromMat(System.Drawing.Drawing2D.Matrix inp)
        {
            Matrix4 M = Matrix4.Identity;
            M.M11 = inp.Elements[0];
            M.M12 = inp.Elements[1];
            M.M21 = inp.Elements[2];
            M.M22 = inp.Elements[3];
            M[3, 0] = inp.OffsetX;
            M[3, 1] = inp.OffsetY;

            return M;

        }

        public CompositingMode CompositingMode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public System.Drawing.Drawing2D.InterpolationMode InterpolationMode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool IsFast { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        private System.Drawing.Drawing2D.Matrix trans = new System.Drawing.Drawing2D.Matrix();

        public System.Drawing.Drawing2D.Matrix Transform
        {
            get
            {
                return trans;
            }
            set
            {
                trans = value;
                Matrix4 M = Mat4FromMat(trans);
                GL.LoadIdentity();
                GL.MultMatrix(ref M);

            }
        }

        public bool Dotted
        {
            get; set;
        }

        public void Clear(Color color)
        {

        }

        public void DrawImage(Bitmap MMGrid, float p1, float p2, float p3, float p4)
        {

        }

        public void DrawLine(Pen P, PointF p1, PointF p2)
        {
            DrawLine(P, p1.X, p1.Y, p2.X, p2.Y);
        }

        public void DrawLine(Pen P, float x1, float y1, float x2, float y2)
        {
            float dx = x2 - x1;
            float dy = y2 - y1;
            float L = (float)Math.Sqrt(dx * dx + dy * dy);
            if (L > 0)
            {
                float t = dx;
                dx = -dy;
                dy = t;

                dx *= (float)( P.Width / (L*2.0f));


                dx += 1;dx -= 1;
                dy *=(float)( P.Width / (L*2.0f));




                AddTriangleThick(x1, y1, dx, dy,
                            x2, y2, dx, dy,
                            x1, y1, -dx, -dy,
                            P.Color);


                AddTriangleThick(x1, y1, -dx, -dy,
                            x2, y2, dx, dy,
                            x2, y2, -dx, -dy,
                            P.Color);

                bool caps = false;
                if (caps)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        float Ph = (float)(i * Math.PI) / 3.0f;
                        float S = (float)Math.Sin(Ph);
                        float C = (float)Math.Cos(Ph);
                        float Ph2 = (float)((i + 1) * Math.PI) / 3.0f;
                        float S2 = (float)Math.Sin(Ph2);
                        float C2 = (float)Math.Cos(Ph2);

                        AddTriangleThick(x1, y1, 0, 0,
                                   x1, y1, S * P.Width, C * P.Width,
                                   x1, y1, S2 * P.Width, C2 * P.Width,
                                   P.Color);

                        AddTriangleThick(x2, y2, 0, 0,
                                   x2, y2, S * P.Width, C * P.Width,
                                   x2, y2, S2 * P.Width, C2 * P.Width,
                                   P.Color);
                    }
                }
            }
        }
        public void AddTriangle(float x1, float y1, float x2, float y2, float x3, float y3, Color C)
        {
            Vertices.Add(new VBOVertex(x1, y1, 0, 0, 0, 0, C.R / 255.0f, C.G / 255.0f, C.B / 255.0f, C.A / 255.0f));
            Vertices.Add(new VBOVertex(x2, y2, 0, 0, 0, 0, C.R / 255.0f, C.G / 255.0f, C.B / 255.0f,  C.A / 255.0f));
            Vertices.Add(new VBOVertex(x3, y3, 0, 0, 0, 0, C.R / 255.0f, C.G / 255.0f, C.B / 255.0f,  C.A / 255.0f));

        }

        public void AddTriangleThick(float x1, float y1, float dx1, float dy1,
                                    float x2, float y2, float dx2, float dy2,
                                    float x3, float y3, float dx3, float dy3, Color C)
        {
            Vertices.Add(new VBOVertex(x1, y1, 0, dx1, dy1, 0, C.R / 255.0f, C.G / 255.0f, C.B / 255.0f, C.A / 255.0f));
            Vertices.Add(new VBOVertex(x2, y2, 0, dx2, dy2, 0, C.R / 255.0f, C.G / 255.0f, C.B / 255.0f, C.A / 255.0f));
            Vertices.Add(new VBOVertex(x3, y3, 0, dx3, dy3, 0, C.R / 255.0f, C.G / 255.0f, C.B / 255.0f,  C.A / 255.0f));

        }

        public void DrawLines(Pen P, PointF[] Points)
        {
            for (int i = 0; i < Points.Length - 1; i++)
            {
                DrawLine(P, Points[i], Points[i + 1]);
            }
        }

        public void DrawPath(Color black, GraphicsPath path, float v)
        {
            GraphicsPathIterator pathIterator = new GraphicsPathIterator(path);


            //Rewind
            // pathIterator.Rewind();

            // Read all subpaths and their properties
            for (int i = 0; i < pathIterator.SubpathCount; i++)
            {
                int strIdx, endIdx;
                bool bClosedCurve;


                pathIterator.NextSubpath(out strIdx, out endIdx, out bClosedCurve);

                for (int j = strIdx; j < endIdx - 1; j++)
                {
                    DrawLine(new Pen(black, v), path.PathPoints[j], path.PathPoints[j + 1]);
                }

                if (bClosedCurve)
                {
                    DrawLine(new Pen(black, v), path.PathPoints[endIdx - 1], path.PathPoints[strIdx]);


                }
            }
        }

        public void DrawRectangle(Color color, float x, float y, float w, float h, float strokewidth = 1)
        {

        }

        public void DrawString(string text, Font font, SolidBrush solidBrush, float x, float y, StringFormat sF)
        {

        }

        internal int VertexCount()
        {
            return TheVertexCount;            
        }

        public void DrawString(PointD pos, string text, double scale, bool centered, float r = 0.2F, float g = 0.2F, float b = 0.2F, float a = 1)
        {

        }

        public void FillPath(Color c, PointF [] path)
        {
            var polygon = new Polygon();
          
            List<TriangleNet.Geometry.Vertex> V = new List<TriangleNet.Geometry.Vertex>();
            for (int j = 0; j < path.Length; j++)
            {
                V.Add(new TriangleNet.Geometry.Vertex(path[j].X, path[j].Y));
            }
            polygon.AddContour(V);

            if (V.Count >= 3)
            {
                try
                {
                    var options = new ConstraintOptions() { };
                    var quality = new QualityOptions() { };

                    var mesh = polygon.Triangulate(options, quality);

                    foreach (var t in mesh.Triangles)
                    {
                        var A = t.GetVertex(0);
                        var B = t.GetVertex(1);
                        var C = t.GetVertex(2);
                        AddTriangle((float)A.X, (float)A.Y, (float)B.X, (float)B.Y, (float)C.X, (float)C.Y, c);
                    }
                }
                catch(Exception E)
                {

                }
            }
        }


        public void DrawPath(Color c, PointF[] path, float v, bool bClosedCurve = true)
        {
            for (int j = 0; j < path.Length - 1; j++)
            {
                DrawLine(new Pen(c, v), path[j], path[j + 1]);
            }

            if (bClosedCurve)
            {
                DrawLine(new Pen(c, v), path[path.Length-1], path[0]);


            }
        }

        public void FillPath(Color c, GraphicsPath path)
        {
            GraphicsPathIterator pathIterator = new GraphicsPathIterator(path);


            var polygon = new Polygon();
            for (int i = 0; i < pathIterator.SubpathCount; i++)
            {
                int strIdx, endIdx;
                bool bClosedCurve;


                pathIterator.NextSubpath(out strIdx, out endIdx, out bClosedCurve);


                List<TriangleNet.Geometry.Vertex> V = new List<TriangleNet.Geometry.Vertex>();
                for (int j = strIdx; j <= endIdx; j++)
                {
                    V.Add(new TriangleNet.Geometry.Vertex(path.PathPoints[j].X, path.PathPoints[j].Y));
                }
                polygon.AddContour(V);

            }




            var options = new ConstraintOptions() { ConformingDelaunay = true };
            var quality = new QualityOptions() { MinimumAngle = 25 };

            var mesh = polygon.Triangulate(options, quality);

            foreach (var t in mesh.Triangles)
            {
                var A = t.GetVertex(0);
                var B = t.GetVertex(1);
                var C = t.GetVertex(2);
                AddTriangle((float)A.X, (float)A.Y, (float)B.X, (float)B.Y, (float)C.X, (float)C.Y, c);
            }

        }

        public void FillPolygon(SolidBrush solidBrush, PointF[] pointF)
        {

        }

        public void FillRectangle(Color color, float x, float y, float w, float h)
        {

        }

        public void FillShape(SolidBrush BR, PolyLine Shape)
        {

        }

        public PointD MeasureString(string p)
        {
            return new PointD(0, 0);
        }

        public void RotateTransform(float p)
        {

        }

        public void ScaleTransform(float sx, float sy)
        {

        }

        public void TranslateTransform(float p1, float p2)
        {

        }
    }


    public class ShaderProgram
    {
        private int ProgramID;
        private int VShaderID;
        private int FShaderID;
        private int AttributeCount;
        private int UniformCount;
        public Dictionary<String, AttributeInfo> Attributes = new Dictionary<string, AttributeInfo>();
        public Dictionary<String, UniformInfo> Uniforms = new Dictionary<string, UniformInfo>();
        public Dictionary<String, uint> Buffers = new Dictionary<string, uint>();


        public ShaderProgram()
        {
            ProgramID = GL.CreateProgram();
        }

        public ShaderProgram(String vshader, String fshader, bool fromFile = false)
        {
            ProgramID = GL.CreateProgram();

            if (fromFile)
            {
                LoadShaderFromFile(vshader, ShaderType.VertexShader);
                LoadShaderFromFile(fshader, ShaderType.FragmentShader);
            }
            else
            {
                LoadShaderFromString(vshader, ShaderType.VertexShader);
                LoadShaderFromString(fshader, ShaderType.FragmentShader);
            }

            Link();
            //       GenBuffers();
        }

        public class UniformInfo
        {
            public String name = "";
            public int address = -1;
            public int size = 0;
            public ActiveUniformType type;
        }

        public class AttributeInfo
        {
            public String name = "";
            public int address = -1;
            public int size = 0;
            public ActiveAttribType type;
        }

        private void loadShader(String code, ShaderType type, out int address)
        {
            address = GL.CreateShader(type);
            GL.ShaderSource(address, code);
            GL.CompileShader(address);
            GL.AttachShader(ProgramID, address);
            string L = GL.GetShaderInfoLog(address);
            L = L.Trim();
            if (L.Length > 0) Console.WriteLine(L);
        }

        public void LoadShaderFromString(String code, ShaderType type)
        {
            if (type == ShaderType.VertexShader)
            {
                loadShader(code, type, out VShaderID);
            }
            else if (type == ShaderType.FragmentShader)
            {
                loadShader(code, type, out FShaderID);
            }
        }

        public void LoadShaderFromFile(String filename, ShaderType type)
        {
            using (StreamReader sr = new StreamReader(filename))
            {
                if (type == ShaderType.VertexShader)
                {
                    loadShader(sr.ReadToEnd(), type, out VShaderID);
                }
                else if (type == ShaderType.FragmentShader)
                {
                    loadShader(sr.ReadToEnd(), type, out FShaderID);
                }
            }
        }

        public void Link()
        {
            GL.LinkProgram(ProgramID);

            Console.WriteLine(GL.GetProgramInfoLog(ProgramID));

            GL.GetProgram(ProgramID, GetProgramParameterName.ActiveAttributes, out AttributeCount);
            GL.GetProgram(ProgramID, GetProgramParameterName.ActiveUniforms, out UniformCount);

            for (int i = 0; i < AttributeCount; i++)
            {
                AttributeInfo info = new AttributeInfo();
                int length = 0;

                StringBuilder name = new StringBuilder();

                GL.GetActiveAttrib(ProgramID, i, 256, out length, out info.size, out info.type, name);

                info.name = name.ToString();
                info.address = GL.GetAttribLocation(ProgramID, info.name);
                Attributes.Add(name.ToString(), info);
            }

            for (int i = 0; i < UniformCount; i++)
            {
                UniformInfo info = new UniformInfo();
                int length = 0;

                StringBuilder name = new StringBuilder();

                GL.GetActiveUniform(ProgramID, i, 256, out length, out info.size, out info.type, name);

                info.name = name.ToString();
                Uniforms.Add(name.ToString(), info);
                info.address = GL.GetUniformLocation(ProgramID, info.name);
            }
        }

        internal void Bind()
        {
            GL.UseProgram(ProgramID);
            
        }

        internal void UnBind()
        {
            GL.UseProgram(0);
        }
    }
    public class GLGraphicsInterface : GraphicsInterface
    {
        RectangleF ClipRect;

        public GLGraphicsInterface(float x, float y, float w, float h)
        {
            ClipRect = new RectangleF(x, y, w, h);
        }

        public void Clear(Color color)
        {
            float r, g, b, a;
            r = color.R / 255.0f;
            g = color.G / 255.0f;
            b = color.B / 255.0f;
            a = color.A / 255.0f;
            GL.ClearColor(r, g, b, a);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        private System.Drawing.Drawing2D.Matrix trans = new System.Drawing.Drawing2D.Matrix();
        public static Matrix4 Mat4FromMat(System.Drawing.Drawing2D.Matrix inp)
        {
            Matrix4 M =
         Matrix4.Identity;
            M.M11 = inp.Elements[0];
            M.M12 = inp.Elements[1];
            M.M21 = inp.Elements[2];
            M.M22 = inp.Elements[3];
            M[3, 0] = inp.OffsetX;
            M[3, 1] = inp.OffsetY;

            return M;

        }
        public System.Drawing.Drawing2D.Matrix Transform
        {
            get
            {
                return trans;
            }
            set
            {
                trans = value;
                Matrix4 M = Mat4FromMat(trans);
                GL.LoadIdentity();
                GL.MultMatrix(ref M);

            }
        }

        public RectangleF ClipBounds
        {
            get { return ClipRect; }
        }

        public void DrawImage(Bitmap MMGrid, float p1, float p2, float p3, float p4)
        {
            return;
        }

        public System.Drawing.Drawing2D.InterpolationMode InterpolationMode
        {
            get
            {
                return System.Drawing.Drawing2D.InterpolationMode.High;
            }
            set
            {

            }
        }


        public void DrawLines(Pen P, PointF[] Points)
        {
            GL.LineWidth(P.Width);
            GL.Color4(P.Color.R, P.Color.G, P.Color.B, P.Color.A);
            GL.Begin(BeginMode.LineStrip);
            for (int i = 0; i < Points.Count(); i++)
            {
                GL.Vertex2(Points[i].X, Points[i].Y);
            }
            GL.End();
        }

        public void DrawLine(Pen P, float x1, float y1, float x2, float y2)
        {
            GL.LineWidth(P.Width);
            GL.Color4(P.Color.R, P.Color.G, P.Color.B, P.Color.A);
            GL.Begin(BeginMode.Lines);
            GL.Vertex2(x1, y1);
            GL.Vertex2(x2, y2);
            GL.End();
        }


        void DoTransform(System.Drawing.Drawing2D.Matrix M)
        {

            trans.Multiply(M);

            Transform = trans;
        }
        void DoTransform(Matrix4 M)
        {
        }

        public void RotateTransform(float p)
        {
            System.Drawing.Drawing2D.Matrix M2 = new System.Drawing.Drawing2D.Matrix();
            M2.Rotate(p);
            DoTransform(M2);
        }


        public void ScaleTransform(float sx, float sy)
        {
            System.Drawing.Drawing2D.Matrix M2 = new System.Drawing.Drawing2D.Matrix();
            M2.Scale(sx, sy);
            DoTransform(M2);
        }
        public void TranslateTransform(float p1, float p2)
        {

            System.Drawing.Drawing2D.Matrix M2 = new System.Drawing.Drawing2D.Matrix();
            M2.Translate(p1, p2);

            DoTransform(M2);
        }

        public static System.Drawing.Drawing2D.Matrix MatFromMat4(Matrix4 M42)
        {
            System.Drawing.Drawing2D.Matrix R = new System.Drawing.Drawing2D.Matrix(M42.M11, M42.M12, M42.M21, M42.M22, M42[3, 0], M42[3, 1]);
            return R;
        }


        public bool IsFast
        {
            get
            {
                return true;
            }
            set
            {
                // return false;
            }
        }

        public CompositingMode CompositingMode
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public bool Dotted { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void DrawString(PointD pos, string text, double scale, bool center, float r = 0.2f, float g = 0.2f, float b = 0.2f, float a = 1.0f)
        {

        }

        private static void CheckFont()
        {
        }

        public PointD MeasureString(string p)
        {
            return new PointD(1, 1);
        }

        public void FillShape(SolidBrush P, PolyLine Shape)
        {
            // GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            if (P.Color.A < 255)
            {
                GL.Enable(EnableCap.Blend);
            }
            var polygon = new Polygon();
            List<Vertex> V = new List<Vertex>();
            for (int i = 0; i < Shape.Count(); i++)
            {
                V.Add(new Vertex(Shape.Vertices[i].X, Shape.Vertices[i].Y));
            }
            polygon.AddContour(V);


            var options = new ConstraintOptions() { ConformingDelaunay = true };
            var quality = new QualityOptions() { MinimumAngle = 25 };

            var mesh = polygon.Triangulate(options, quality);


            GL.Begin(BeginMode.Triangles);
            GL.Color4(P.Color.R, P.Color.G, P.Color.B, P.Color.A);

            foreach (var t in mesh.Triangles)
            {

                var A = t.GetVertex(0);
                var B = t.GetVertex(1);
                var C = t.GetVertex(2);
                GL.Vertex2(A.X, A.Y);
                GL.Vertex2(B.X, B.Y);
                GL.Vertex2(C.X, C.Y);
            }

            GL.End();
        }

        public void DrawLine(Pen P, PointF p1, PointF p2)
        {
            DrawLine(P, p1.X, p1.Y, p2.X, p2.Y);
        }

        public void DrawRectangle(Color color, float x, float y, float w, float h, float strokewidth = 1)
        {
            DrawLine(new Pen(color, strokewidth), x, y, x + w, y);
            DrawLine(new Pen(color, strokewidth), x + w, y, x + w, y + h);
            DrawLine(new Pen(color, strokewidth), x + w, y + h, x, y + h);
            DrawLine(new Pen(color, strokewidth), x, y + h, x, y);
        }

        public void FillRectangle(Color color, float x, float y, float w, float h)
        {


            GL.Color4(color);
            GL.Begin(BeginMode.Quads);
            GL.Vertex2(x, y);
            GL.Vertex2(x + w, y);
            GL.Vertex2(x + w, y + h);
            GL.Vertex2(x, y + h);
            GL.End();


        }

        public void FillPath(Color c, GraphicsPath gP)
        {

            DrawPath(c, gP, 1.0f);
        }

        public void DrawString(string text, Font font, SolidBrush solidBrush, float x, float y, StringFormat sF)
        {
            // throw new NotImplementedException();
        }

        public void DrawPath(Color color, GraphicsPath path, float v)
        {

            GraphicsPathIterator pathIterator = new GraphicsPathIterator(path);


            //Rewind
            // pathIterator.Rewind();
            GL.LineWidth(v);
            // Read all subpaths and their properties
            for (int i = 0; i < pathIterator.SubpathCount; i++)
            {
                int strIdx, endIdx;
                bool bClosedCurve;


                pathIterator.NextSubpath(out strIdx, out endIdx, out bClosedCurve);
                GL.Begin(PrimitiveType.LineStrip);
                GL.Color4(color.R, color.G, color.B, color.A);
                for (int j = strIdx; j < endIdx; j++)
                {
                    GL.Vertex2(path.PathPoints[j].X, path.PathPoints[j].Y);
                }

                if (bClosedCurve)
                {
                    GL.Vertex2(path.PathPoints[strIdx].X, path.PathPoints[strIdx].Y);

                }
                GL.End();
            }
        }

        public void FillPolygon(SolidBrush solidBrush, PointF[] pointF)
        {
            // throw new NotImplementedException();
        }

        internal Matrix4 GetGlMatrix()
        {
            return Mat4FromMat(trans);
        }
    }
}
