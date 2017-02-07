using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Ionic.Zip;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Resources;
using System.Collections;

namespace ReleaseBuilder
{
    class ReleaseBuilder
    {
        static bool Ignored(string name)
        {
            if (name.ToLower().Contains("releases")) return true;
            if (name.ToLower().Contains("legacy")) return true;

            return false;
        }

        static void Main(string[] args)
        {

            if (args.Length < 2)
            {
                Console.WriteLine("Usage: ReleaseBuilder <basename> <inputfolder>");
                return;
            }

            string basename = args[0];
            string inputfolder = args[1];
                
            if (Directory.Exists(inputfolder) == false)
            {
                Console.WriteLine("Directory not found!");
                return;
            }
            string outputfolder = Path.Combine(inputfolder, "Releases");
            if (Directory.Exists(outputfolder) == false)
            {
                Console.WriteLine("Outputfolder not found, creating it");
                Directory.CreateDirectory(outputfolder);
            }

            string ReleaseName = string.Format("{0}_{1}_{2}_{3}.zip", basename, DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

            using (ZipFile zip = new ZipFile())
            {
                AddFilesToFolder(zip, inputfolder, inputfolder);
                foreach (var a in Directory.GetDirectories(inputfolder))
                {
                    if (Ignored(a) == false)
                    {
                        AddFolder(zip, a, inputfolder+"\\" );
                    }
                }
                string outp = Path.Combine(outputfolder, ReleaseName);
                zip.Save(outp);
                Console.WriteLine("Release created: {0}", outp);
            }
        }

        public static string RelativePath(string path1, string path2)
        {
            Uri fullPath = new Uri(path1, UriKind.Absolute);
            Uri relRoot = new Uri(path2, UriKind.Absolute);

            return Uri.UnescapeDataString(relRoot.MakeRelativeUri(fullPath).ToString());
        }

        private static void AddFolder(ZipFile zip, string inputfolder, string basefolder)
        {
            foreach (var a in Directory.GetDirectories(inputfolder))
            {
                if (Ignored(a) == false)
                {
                    AddFolder(zip, a, basefolder);
                } 
            }
            AddFilesToFolder(zip, inputfolder, basefolder);
            
        }

        private static void AddFilesToFolder(ZipFile zip, string inputfolder, string basefolder)
        {
            foreach (var a in Directory.GetFiles(inputfolder))
            {
                if (Directory.Exists(a))
                {
                    if (Ignored(a) == false)
                    {
                        AddFolder(zip, a, basefolder);
                    }
                }
                else
                {
                    string dirname = Path.GetDirectoryName(a);
                    string target = RelativePath(dirname, basefolder);
                    string ext = Path.GetExtension(a).ToLower();

                    if (ext == ".exe" && (IgnoreFile(a) == false))
                    {

                        FileVersionInfo myFileVersionInfo = FileVersionInfo.GetVersionInfo(a);
                        Artwork.TINRSArtWorkRenderer.SaveMultiIcon(a.Substring(0, a.Length - 4) + ".ico", myFileVersionInfo.ProductName);
                        Console.WriteLine(myFileVersionInfo.ProductName);

                    }
                    if (IgnoreExt(ext) == false && IgnoreFile(a) == false)
                    {
                        ZipEntry e = zip.AddFile(a, target);
                    }
                }
            }
        }


        private static bool IgnoreFile(string f)
        {
            if (f.ToLower().Contains(".vshost")) return true;
            return false;
        }
        private static bool IgnoreExt(string ext)
        {
            if (ext == ".pdb") return true;
            if (ext == ".zip") return true;

            return false;       
        }
    }
}
