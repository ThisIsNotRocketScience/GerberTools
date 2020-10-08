using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GerberLibrary;
using GerberLibrary.Core;
using System.Globalization;
using System.Threading;

namespace DirtyPCB_BoardRender
{
    class DirtyPCB_BoardRender : ProgressLog
    {
        enum Arguments
        {
            SolderMask,
            SilkScreen,
            Copper,
            SkipFix,
            None,
            TimeOut,
            TraceColor
        }

        public class BoardRenderSettings
        {
            public Color SolderMaskColor = Color.Green;
            public Color SilkScreenColor = Color.White;
            public Color CopperColor = Color.Gold;

            public string InputFolder { get; internal set; }
            public string OutputFolder { get; internal set; }
            public int TimeOut = -1;
            public bool TimeOutExpired = false;
            public Color TraceColor;
            public bool HasTraceColor = false;
        }
        public static BoardRenderSettings TheSettings = new BoardRenderSettings();

        static void Main(string[] args)
        {

            //          CultureInfo ci = new CultureInfo("en-US");
            //        Thread.CurrentThread.CurrentCulture = ci;
            //      Thread.CurrentThread.CurrentUICulture = ci;

            if (args.Count() < 2)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("DirtyPCB_BoardRender.exe  [--soldermask_color {blue,yellow,green,black,white,red}]");
                Console.WriteLine("\t[--silkscreen_color {white, black}]");
                Console.WriteLine("\t[--trace_color {blue,yellow,green,black,white,red}]");
                Console.WriteLine("\t[--copper_color {silver, gold}]");
                Console.WriteLine("\t[--timeout {seconds}]");
                Console.WriteLine("\t[--skipeaglefix]");
                Console.WriteLine("\tinput_path");
                Console.WriteLine("\toutput_directory");
                Console.WriteLine("");
                Console.WriteLine("\tHTML colors in the form of #rrggbb are also supported");

                return;
            }

            Arguments NextArg = Arguments.None;
            for (int i = 0; i < args.Count() - 2; i++)
            {
                switch (NextArg)
                {
                    case Arguments.SilkScreen: TheSettings.SilkScreenColor = GerberLibrary.Gerber.ParseColor(args[i]); NextArg = Arguments.None; break;
                    case Arguments.Copper: TheSettings.CopperColor = GerberLibrary.Gerber.ParseColor(args[i]); NextArg = Arguments.None; break;
                    case Arguments.SolderMask: TheSettings.SolderMaskColor = GerberLibrary.Gerber.ParseColor(args[i]); NextArg = Arguments.None; break;
                    case Arguments.TraceColor: TheSettings.TraceColor = GerberLibrary.Gerber.ParseColor(args[i]); NextArg = Arguments.None; TheSettings.HasTraceColor = true;  break;
                    case Arguments.TimeOut: TheSettings.TimeOut = int.Parse(args[i]); NextArg = Arguments.None; break;

                    case Arguments.None:
                        switch (args[i].ToLower())
                        {
                            case "--silkscreen_color": NextArg = Arguments.SilkScreen; break;
                            case "--copper_color": NextArg = Arguments.Copper; break;
                            case "--soldermask_color": NextArg = Arguments.SolderMask; break;
                            case "--trace_color": NextArg = Arguments.TraceColor; break;
                            case "--timeout": NextArg = Arguments.TimeOut; break;
                            case "--skipeaglefix": NextArg = Arguments.None; Gerber.SkipEagleDrillFix = true; break;
                        }
                        break;
                }
            }

            TheSettings.InputFolder = args[args.Count() - 2];
            TheSettings.OutputFolder = args[args.Count() - 1];

            if (Directory.Exists(TheSettings.InputFolder) == false)
            {
                Console.WriteLine("Error: {0} is not a valid path!", TheSettings.InputFolder);
                return;
            }

            if (Directory.Exists(TheSettings.OutputFolder) == false)
            {
                Console.WriteLine("Error: {0} is not a valid path!", TheSettings.InputFolder);
                return;
            }

            Console.WriteLine("Progress: Input parameters validated");

            Thread T = new Thread(new ThreadStart(RunImageGeneration));
            T.Start();
            bool done = false;
            if (TheSettings.TimeOut > 0) TheSettings.TimeOut *= 100;
            while (T.ThreadState != ThreadState.Stopped && !done)
            {
                Thread.Sleep(10);
                if (TheSettings.TimeOut > 0)
                {
                    TheSettings.TimeOut--;
                    if (TheSettings.TimeOut == 0)
                    {
                        TheSettings.TimeOutExpired = true;
                        T.Abort();
                        done = true;
                        Console.WriteLine("Error: Maximum gerber time generation limit expired! Aborting!");
                    }
                }
            }
        }

        private static void RunImageGeneration()
        {
            try
            {
                var InputFiles = Directory.GetFiles(TheSettings.InputFolder);
                GerberImageCreator GIC = new GerberLibrary.GerberImageCreator();
                GerberImageCreator.AA = true;

                BoardRenderColorSet Colors = new BoardRenderColorSet();
                Colors.BoardRenderColor = TheSettings.SolderMaskColor;
                Colors.BoardRenderTraceColor = TheSettings.SolderMaskColor;
                if (TheSettings.HasTraceColor) Colors.BoardRenderTraceColor = TheSettings.TraceColor;

                Colors.BoardRenderSilkColor = TheSettings.SilkScreenColor;
                Colors.BoardRenderPadColor = TheSettings.CopperColor;
                GIC.AddBoardsToSet(InputFiles.ToList(), new SilentLog());
                GIC.SetColors(Colors);
                if (GIC.Errors.Count > 0)
                {
                    foreach (var a in GIC.Errors)
                    {
                        Console.WriteLine("Error: {0}", a);
                    }
                    // return;
                }

                // Console.WriteLine("Progress: Estimated board bounding box:{0:N2},{1:N2} - {2:N2},{3:N2} -> {4:N2},{5:N2}", GIC.BoundingBox.TopLeft.X, GIC.BoundingBox.TopLeft.Y, GIC.BoundingBox.BottomRight.X, GIC.BoundingBox.BottomRight.Y, GIC.BoundingBox.Width(), GIC.BoundingBox.Height());
                // Gerber.SaveIntermediateImages = true;
                Console.WriteLine("Progress: Rendering Top");
                GIC.DrawToFile(TheSettings.OutputFolder + "/FullRender", BoardSide.Top, 200, false);
                Console.WriteLine("Progress: Rendering Bottom");
                GIC.DrawToFile(TheSettings.OutputFolder + "/FullRender", BoardSide.Bottom, 200, false);
                GIC.DrawAllFiles(TheSettings.OutputFolder + "/Layer", 200, new DirtyPCB_BoardRender());
                Console.WriteLine("Progress: Done!");
            }
            catch (Exception E)
            {
                if (TheSettings.TimeOutExpired == false)
                {
                    Console.WriteLine("Error: {0}", E.Message);
                }
            }
        }

        public override void AddString(string text, float progress = -1F)
        {
            Console.WriteLine("Progress: {0}", text);
        }
    }
}
