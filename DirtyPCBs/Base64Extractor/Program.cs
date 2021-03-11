using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base64Extractor
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (var a in args)
            {
                if (Directory.Exists(a))
                {
                    HandleFolder(a);
                }
                else
                {
                    HandleFile(a);
                }
            }

        }

        private static void HandleFolder(string a)
        {
           foreach(var f in Directory.GetFiles(a))
            {
                if (Directory.Exists(f))
                {
                    HandleFolder(f);
                }
                else
                {
                    HandleFile(f);
                }
            }
        }

        private static void HandleFile(string a)
        {
            var b = ReadAndExtract(a);
            if (b != null)
            {
                File.WriteAllBytes(  a+ "___.zip", b);
            }
        }

        private static byte[] ReadAndExtract(string a)
        {
            if (File.Exists(a) )
            {
                var lines = File.ReadAllLines(a);
                for (int i = 0;i<lines.Count();i++)
                {
                    if (lines[i].Contains("Content-Transfer-Encoding: base64"))
                    {
                        i+=2;
                        string bulk = "";
                        while(lines[i].Trim().Length > 0)
                        {
                            bulk += lines[i];
                            i++;
                        }
                        return Convert.FromBase64CharArray(bulk.ToCharArray(), 0, bulk.Length);
                    }
                }
             

            }
            return null;

        }
    }
}
