using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GerberLibrary;
using GerberLibrary.Core;

namespace DirtyPCB_BoardRender
{
    class DirtyPCBBoardRender: ProgressLog
    {
        enum Arguments
        {
            SolderMask,
            SilkScreen,
            Copper,
            None
        }
       
        static void Main(string[] args)
        {

            if (args.Count() < 2)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("DirtyPCB_BoardRender.exe  [--soldermask_color {blue,yellow,green,black,white,red}]");
                Console.WriteLine("\t[--silkscreen_color {white, black}]");
                Console.WriteLine("\t[--copper_color {silver, gold}]");
                Console.WriteLine("\tinput_path");
                Console.WriteLine("\toutput_directory");
                
                return;
            }

            Color SolderMaskColor = Color.Green;
            Color SilkScreenColor = Color.White;
            Color CopperColor = Color.Gold;

            Arguments NextArg = Arguments.None;
            for(int i =0;i<args.Count()-2;i++)
            {
                switch(NextArg)
                {
                    case Arguments.SilkScreen: SilkScreenColor = GerberLibrary.Gerber.ParseColor(args[i]); NextArg = Arguments.None; break;
                    case Arguments.Copper: CopperColor = GerberLibrary.Gerber.ParseColor(args[i]); NextArg = Arguments.None; break;
                    case Arguments.SolderMask: SolderMaskColor = GerberLibrary.Gerber.ParseColor(args[i]); NextArg = Arguments.None; break;
                    
                    case Arguments.None:
                        switch(args[i].ToLower())
                        {
                            case "--silkscreen_color": NextArg = Arguments.SilkScreen;break;
                            case "--copper_color": NextArg = Arguments.Copper;break;
                            case "--soldermask_color": NextArg = Arguments.SolderMask;break;
                        }
                        break;
                }
            }

            string InputFolder = args[args.Count() - 2];
            string OutputFolder = args[args.Count() - 1];

            if (Directory.Exists(InputFolder) == false)
            {
                Console.WriteLine("Error: {0} is not a valid path!", InputFolder);
                return;
            }

            if (Directory.Exists(OutputFolder) == false)
            {
                Console.WriteLine("Error: {0} is not a valid path!", InputFolder);
                return;
            }

            Console.WriteLine("Progress: Input parameters validated");
            try
            {
                var InputFiles = Directory.GetFiles(InputFolder);
                GerberImageCreator GIC = new GerberLibrary.GerberImageCreator();
                GerberImageCreator.AA = true;
                Gerber.BoardRenderColor = SolderMaskColor;
                Gerber.BoardRenderSilkColor = SilkScreenColor;
                Gerber.BoardRenderPadColor = CopperColor;
                GIC.AddBoardsToSet(InputFiles.ToList());
                Console.WriteLine("Progress: Rendering Top");
                GIC.DrawToFile(OutputFolder + "/FullRender", BoardSide.Top, 200, false);
                Console.WriteLine("Progress: Rendering Bottom");
                GIC.DrawToFile(OutputFolder + "/FullRender", BoardSide.Bottom, 200, false);
                GIC.DrawAllFiles(OutputFolder + "/Layer", 200, new DirtyPCBBoardRender());
                Console.WriteLine("Progress: Done!");
            }
            catch (Exception E)
            {
                Console.WriteLine("Error: {0}", E.Message);
            }
        }

        public void AddString(string text, float progress = -1F)
        {
            Console.WriteLine("Progress: {0}", text);
        }
    }
}
