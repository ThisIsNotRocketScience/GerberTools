using GerberLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GerberToImage
{
    class GerberToImage
    {
        enum Arguments
        {
            dpi,
            noxray,
            nonormal,
            silk,
            mask,
            None
        }

        [STAThread]
        static void Main(string[] args)
        {
            if (args.Count() < 1)
            {
                Console.WriteLine("need files to render...");
                Console.WriteLine("GerberToImage <files> [--dpi N] [--noxray] [--nopcb] [--silk black:white] [--mask yellow:green:red:black:white:blue]");
                return;
            }

            int dpi = 400;
            Arguments NextArg = Arguments.None;
            bool xray = true;
               bool normal = true;
            string pcbcolor = "green";
            string silkcolor = "";
            List<string> RestList = new List<string>();
            for (int i = 0; i < args.Count() ; i++)
            {
                switch (NextArg)
                {

                    case Arguments.dpi: dpi = Int32.Parse(args[i]); NextArg = Arguments.None; break;
                    case Arguments.silk: silkcolor = args[i];NextArg = Arguments.None;break;
                    case Arguments.mask: pcbcolor = args[i]; NextArg = Arguments.None; break;
                    case Arguments.None:
                        switch (args[i].ToLower())
                        {
                            case "--dpi": NextArg = Arguments.dpi; break;
                            case "--silk": NextArg = Arguments.silk;break;
                            case "--mask": NextArg = Arguments.mask; break;
                            case "--noxray": xray = false; NextArg = Arguments.None; break;
                            case "--nopcb": normal = false; NextArg = Arguments.None; break;

                            default:
                                RestList.Add(args[i]);break;
                        }
                        break;
                }
            }


            Gerber.SaveIntermediateImages = false;
            Gerber.ShowProgress = false;

            if (RestList.Count() == 1 && File.Exists(RestList[0]) && Path.GetExtension(RestList[0]).ToLower()!= ".zip")
            {
              //  Gerber.WriteSanitized = true;
                Gerber.ExtremelyVerbose = false;
                //Gerber.Verbose = true;
                Gerber.WaitForKey = true;
                Gerber.ShowProgress = true;

               CreateImageForSingleFile(new StandardConsoleLog(),RestList[0], Color.Black, Color.White);
                if (Gerber.WaitForKey)
                {
                    Console.WriteLine("Press any key to continue");
                    Console.ReadKey();
                }
                return;
            }
            
            GerberImageCreator GIC = new GerberImageCreator();
            string TargetFileBaseName = "";
            if (RestList.Count() >= 1) TargetFileBaseName = RestList[0];
            
            List<String> FileList = new List<string>();

            foreach(var a in RestList)
            { 
                if (Directory.Exists(a))
                {
                    FileList.AddRange(Directory.GetFiles(a, "*.*"));
                }
                else
                {
                    if (File.Exists(a))
                     {
                        FileList.Add(a);
                    }
                }

            }

            for (int i = 0; i < FileList.Count; i++)
            {
                if (FileList[i][FileList[i].Count() - 1] == '\\')
                {
                    FileList[i] = Path.GetDirectoryName(FileList[i]);
                }
            }
            var L = new GerberToImage( Path.GetFileNameWithoutExtension(TargetFileBaseName));
            GIC.AddBoardsToSet(FileList,new StandardConsoleLog(),  true);
            BoardRenderColorSet colors = new BoardRenderColorSet();


            switch(pcbcolor)
            {
                case "yellow": colors.BoardRenderColor = Gerber.ParseColor("yellow");
                               colors.BoardRenderSilkColor = Gerber.ParseColor("white");
                    break;
                case "green":
                    colors.BoardRenderColor = Gerber.ParseColor("green");
                    colors.BoardRenderSilkColor = Gerber.ParseColor("white");
                    break;
                case "black":
                    colors.BoardRenderColor = Gerber.ParseColor("black");
                    colors.BoardRenderSilkColor = Gerber.ParseColor("white");
                    break;
                case "white":
                    colors.BoardRenderColor = Gerber.ParseColor("white");
                    colors.BoardRenderSilkColor = Gerber.ParseColor("black");
                    break;
                case "blue":
                    colors.BoardRenderColor = Gerber.ParseColor("yellow");
                    colors.BoardRenderSilkColor = Gerber.ParseColor("white");
                    break;
                case "red":
                    colors.BoardRenderColor = Gerber.ParseColor("red");
                    colors.BoardRenderSilkColor = Gerber.ParseColor("white");
                    break;
            }

            colors.BoardRenderTraceColor = colors.BoardRenderColor;
            if (silkcolor.Length > 0)
            {
                switch(silkcolor)
                {
                    case "white":
                        colors.BoardRenderSilkColor = Gerber.ParseColor("white"); 
                        break;

                    case "black":
                        colors.BoardRenderSilkColor = Gerber.ParseColor("black"); 
                        break;


                }
            }
            colors.BoardRenderPadColor = Gerber.ParseColor("silver");

            GIC.SetColors(colors);
            GIC.WriteImageFiles(TargetFileBaseName, dpi, false, xray, normal, new StandardConsoleLog());
            Console.WriteLine("Done writing {0}", TargetFileBaseName);
       //    Console.ReadKey();
        }

        private static void CreateImageForSingleFile(ProgressLog log, string arg, Color Foreground, Color Background)
        {
            
            if (arg.ToLower().EndsWith(".png") == true) return;
            GerberImageCreator.AA = false;
            //Gerber.Verbose = true;
            if (Gerber.ThrowExceptions)
            {
                Gerber.SaveGerberFileToImageUnsafe(log, arg, arg + "_render.png", 1000, Foreground, Background);
            }
            else
            {
                Gerber.SaveGerberFileToImage(log, arg, arg + "_render.png", 1000, Foreground, Background);
            }

            if (Gerber.SaveDebugImageOutput)
            {
                Gerber.SaveDebugImage(arg, arg + "_debugviz.png", 1000, Foreground, Background, new StandardConsoleLog());
            }
        }

        public string TheName;
        GerberToImage(string name)
        {
            TheName = name;
        }
    }
}
