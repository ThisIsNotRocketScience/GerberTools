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

            if (args.Count() == 1 && File.Exists(args[0]))
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
            if (args.Count() <= 1 || Directory.Exists(args[1]))
            {
                foreach(var a in Directory.GetFiles(args[1])){
                    try{
                     //   Console.WriteLine("Building layer image for: {0}", Path.GetFileName(a));
                      //  CreateImageForSingleFile(a);
                    }
                    catch(Exception E){
                        Console.WriteLine("Error while writing image for {0}: {1}", Path.GetFileName(a), E.Message);
                    };
                }

                if (args.Count() == 0)
                {
                    System.Windows.Forms.SaveFileDialog OFD = new System.Windows.Forms.SaveFileDialog();
                    OFD.DefaultExt = "";
                    if (OFD.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
                    TargetFileBaseName = OFD.FileName;
                }

                string foldername = "";

                if (args.Count() < 2)
                {
                    FolderBrowserDialog FBD = new FolderBrowserDialog();
                    if (FBD.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
                    foldername = FBD.SelectedPath;
                }
                else
                {
                    foldername = args[1];
                }

                FileList.AddRange(Directory.GetFiles(foldername, "*.*"));

            }
            else
            {
                FileList.AddRange(args.Skip(1));
            }

            for (int i = 0; i < FileList.Count; i++)
            {
                if (FileList[i][FileList[i].Count() - 1] == '\\')
                {
                    FileList[i] = Path.GetDirectoryName(FileList[i]);
                }
            }
            GIC.AddBoardsToSet(FileList);
            GIC.WriteImageFiles(TargetFileBaseName, 200, Gerber.DirectlyShowGeneratedBoardImages, new GerberToImage());
            Console.WriteLine("Done writing {0}", TargetFileBaseName);
           Console.ReadKey();
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
            Console.WriteLine(text);
        }
    }
}
