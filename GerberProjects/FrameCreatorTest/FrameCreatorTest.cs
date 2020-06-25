using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using GerberLibrary;
using GerberLibrary.Core.Primitives;

namespace FrameCreatorTest
{
    class FrameCreatorTest: ProgressLog
    {
        static void Main(string[] args)
        {
            string basepath = Directory.GetCurrentDirectory();
            Directory.CreateDirectory(Path.Combine(basepath, "outline"));
            Directory.CreateDirectory(Path.Combine(basepath, "frame"));

            GerberFrameWriter.FrameSettings FS = new GerberFrameWriter.FrameSettings();
            PolyLine PL = new PolyLine();
            FS.FrameTitle = "Test Frame";
            FS.RenderSample = false;
            FS.margin = 3;

            PL.MakeRoundedRect(new PointD(10, 10), new PointD(200, 200), 7);
            FS.PositionAround(PL);
            //FS.offset = new PointD(200, 200);
            FS.RenderSample = true;

            GerberArtWriter GAW = new GerberArtWriter();
            GAW.AddPolyLine(PL);
            GAW.Write("outline/outtestinside.gko");


            GerberFrameWriter.WriteSideEdgeFrame(PL, FS, "frame/outtest");
            GerberFrameWriter.MergeFrameIntoGerberSet(Path.Combine(basepath, "frame"), Path.Combine(basepath, "outline"), Path.Combine(basepath, "mergedoutput"),FS, new FrameCreatorTest(),"testframe");


            GerberFrameWriter.MergeFrameIntoGerberSet(Path.Combine(basepath, "SliceFrameOutline6"), Path.Combine(basepath, "Slice6"), Path.Combine(basepath, "slice6inframe"), FS, new FrameCreatorTest(),"slice6framed" );




            //            PNL.SaveOutlineTo("panelized.gko", "panelcombinedgko.gko");
        }

        public override void AddString(string text, float progress = -1)
        {
            string output = "";
            foreach(var a in ActivityStack)
            {
                output += a + " -> ";
            }
            Console.WriteLine(output + text);
        }
    }
}
