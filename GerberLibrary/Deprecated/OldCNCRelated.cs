using GerberLibrary.Core;
using GerberLibrary.Core.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GerberLibrary.Deprecated
{
    namespace OldCNC
    {

        public class CommandSet
        {
            public PointD TopLeft = new PointD(0, 0);
            public PointD BottomRight = new PointD(0, 0);
            void FitPoint(PointD P)
            {
                if (P.X < TopLeft.X) TopLeft.X = P.X;
                if (P.Y < TopLeft.Y) TopLeft.Y = P.Y;
                if (P.X > BottomRight.X) BottomRight.X = P.X;
                if (P.Y > BottomRight.Y) BottomRight.Y = P.Y;
            }

            public void CalcPathBounds()
            {
                TopLeft.X = 10000;
                TopLeft.Y = 10000;
                BottomRight.X = -10000;
                BottomRight.Y = -10000;
                for (int i = 0; i < Count(); i++)
                {
                    Command C = Get(i);
                    if (C.C == Command.CommandType.Move) FitPoint(new PointD(C.x, C.y));
                }
            }

            public List<Command> CommandList = new List<Command>();

            public void Reset()
            {
                CommandList.Clear();
            }

            public Command Get(int CurrentCommand)
            {
                return CommandList[CurrentCommand];
            }

            public int Count()
            {
                return CommandList.Count;
            }

            public void Down(Tool T, bool force)
            {

                if (force == false && (CommandList.Count > 0 && CommandList[CommandList.Count - 1].C == T.UpCommand))
                {
                    CommandList.RemoveAt(CommandList.Count - 1);
                }
                else
                {
                    CommandList.Add(new Command() { C = T.DownCommand });
                }
            }

            public void Up(Tool T, bool force)
            {
                if (force == false && (CommandList.Count > 0 && CommandList[CommandList.Count - 1].C == T.DownCommand))
                {
                    CommandList.RemoveAt(CommandList.Count - 1);
                }
                else
                {
                    CommandList.Add(new Command() { C = T.UpCommand });
                }


                //Path.Add(new Command() { C = CommandType.PenUp });
            }

            PointD lastMove = new PointD(-100000, -100000);

            public void Move(double x, double y)
            {
                if ((int)(lastMove.X * 100.0f) != (int)(x * 100.0f) || (int)(lastMove.Y * 100.0f) != (int)(y * 100.0f))
                //if (lastMove.X != x|| lastMove.Y != y )
                {
                    CommandList.Add(new Command() { C = Command.CommandType.Move, x = x, y = y });
                    lastMove.X = x;
                    lastMove.Y = y;
                }
            }

            bool bezier = false;

            public void Build(ParsedGerber TheLines, List<Tool> Tools, Tool DefaultTool)
            {
                List<PolyLine> Shapes = new List<PolyLine>();
                foreach (var A in TheLines.Shapes)
                {
                    Shapes.Add(A);
                }

                Reset();
                double lastx = 0;
                double lasty = 0;
                double lastdx = 0;
                double lastdy = 0;
                // add a final point to make the bezier curve neatly downward at the last PolyLine. This gets added AFTER optimize to ensure it is actually the last shape.

                PolyLine EndPoint = new PolyLine(-3);
                EndPoint.Add(0, 0);
                EndPoint.Draw = false;
                Shapes.Add(EndPoint);
                Reset();

                // go to homing point
                Up(DefaultTool, true);
                Move(0, 0);

                // loop through all the polylines, add bezier-curves between them for continuous movements past the edges. This should also be done at line-kinks.. but not yet
                foreach (var a in Shapes)
                {

                    double newx = a.Vertices[0].X;
                    double newy = a.Vertices[0].Y;

                    if (bezier)
                    {
                        double newdx = lastdx;
                        double newdy = lastdy;
                        if (a.Count() > 1)
                        {
                            newdx = a.Vertices[1].X - a.Vertices[0].X;
                            newdy = a.Vertices[1].Y - a.Vertices[0].Y;
                        }

                        PointD D1 = Helpers.Normalize(new PointD((float)lastdx, (float)lastdy));
                        PointD D2 = Helpers.Normalize(new PointD((float)newdx, (float)newdy));

                        PointD AM = new PointD((float)lastx - D1.X * 10, (float)lasty - D1.Y * 10); ;
                        PointD A = new PointD((float)lastx, (float)lasty);
                        PointD B = new PointD((float)lastx + D1.X * 7, (float)lasty + D1.Y * 7);
                        PointD C = new PointD((float)newx - D2.X * 7, (float)newy - D2.Y * 7);
                        PointD D = new PointD((float)newx, (float)newy);
                        PointD DP = new PointD((float)newx + D2.X * 10, (float)newy + D2.Y * 10);



                        for (int i = 0; i < 10; i++)
                        {
                            PointD T = Helpers.BezierPoint(A, B, C, D, i * 0.1f);
                            Move(T.X, T.Y);
                        }
                    }

                    Move(newx, newy);
                    bool force = false;
                    if (a.Count() == 1) force = true;

                    if (a.Draw == true) Down(null, force);
                    lastx = newx;
                    lasty = newy;

                    for (int i = 1; i < a.Count(); i++)
                    {
                        newx = a.Vertices[i].X;
                        newy = a.Vertices[i].Y;
                        Move(newx, newy);
                        lastdx = newx - lastx;
                        lastdy = newy - lasty;
                        lastx = newx;
                        lasty = newy;
                    }
                    if (a.Draw == true) Up(null, force);
                }
                // move back to homing point
                Move(0, 0);
                if (Gerber.ShowProgress) Console.WriteLine("command count: {0}", CommandList.Count);
            }
        }

        public class Command
        {

            public enum CommandType
            {
                Move,
                MillUp,
                MillDown,
                DrillUp,
                DrillDown,
                Probe
            };

            public CommandType C;
            public double x;
            public double y;
            public double z;
            public byte CommandByte;
        }


        public class Tool
        {
            public string Name;
            public double XOffset;
            public double YOffset;
            public Command.CommandType UpCommand;
            public Command.CommandType DownCommand;
        };
    }

}
