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
    class GerberToImage: ProgressLog
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Count() < 1)
            {
                Console.WriteLine("need files to render...");
                return;
            }
            Gerber.SaveIntermediateImages = false;
            Gerber.ShowProgress = false;

            if (args.Count() == 1 && File.Exists(args[0]) && Path.GetExtension(args[0]).ToLower()!= ".zip")
            {
              //  Gerber.WriteSanitized = true;
                Gerber.ExtremelyVerbose = false;
                //Gerber.Verbose = true;
                Gerber.WaitForKey = true;
                Gerber.ShowProgress = true;

               CreateImageForSingleFile(args[0], Color.Black, Color.White);
                if (Gerber.WaitForKey)
                {
                    Console.WriteLine("Press any key to continue");
                    Console.ReadKey();
                }
                return;
            }
            
            GerberImageCreator GIC = new GerberImageCreator();
            string TargetFileBaseName = "";
            if (args.Count() >= 1) TargetFileBaseName = args[0];
            
            List<String> FileList = new List<string>();

            foreach(var a in args)
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
            GIC.AddBoardsToSet(FileList, true, L);
            GIC.WriteImageFiles(TargetFileBaseName, 400, false, true, true, L);
            Console.WriteLine("Done writing {0}", TargetFileBaseName);
       //    Console.ReadKey();
        }

        private static void CreateImageForSingleFile(string arg, Color Foreground, Color Background)
        {
            
            if (arg.ToLower().EndsWith(".png") == true) return;
            GerberImageCreator.AA = false;
            //Gerber.Verbose = true;
            if (Gerber.ThrowExceptions)
            {
                Gerber.SaveGerberFileToImageUnsafe(arg, arg + "_render.png", 1000, Foreground, Background);
            }
            else
            {
                Gerber.SaveGerberFileToImage(arg, arg + "_render.png", 1000, Foreground, Background);
            }

            if (Gerber.SaveDebugImageOutput)
            {
                Gerber.SaveDebugImage(arg, arg + "_debugviz.png", 1000, Foreground, Background);
            }
        }

        public void AddString(string text, float progress = -1)
        {
            Console.WriteLine(TheName + " - " + text);
        }
        public string TheName;
        GerberToImage(string name)
        {
            TheName = name;
        }
    }
}
