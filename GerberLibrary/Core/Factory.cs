using EagleLibrary;
using GerberLibrary.Core;
using GerberLibrary.Core.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GerberLibrary
{
    public class Factory
    {
        public static void ZipGerbers(string a, bool frame, int framemm, string framename)
        {
            string BaseFolder = Path.GetDirectoryName(a);
            string Name = Path.GetFileNameWithoutExtension(a);

            string GerbersFolder = Path.Combine(BaseFolder, "Gerbers");

            string BoardGerbersFolder = Path.Combine(GerbersFolder, Name);

            string BoardGerbersMergedFolder = Path.Combine(GerbersFolder, Name + "_MERGED");

            string FactoryFolder = Path.Combine(BaseFolder, "Factory");
            if (Directory.Exists(FactoryFolder) == false) Directory.CreateDirectory(FactoryFolder);

            string BoardFactoryFolder = Path.Combine(FactoryFolder, Name);


            if (Directory.Exists(BoardFactoryFolder) == false) Directory.CreateDirectory(BoardFactoryFolder);
            Gerber.ZipGerberFolderToFactoryFolder(MakeJLCName(Name), BoardGerbersFolder, BoardFactoryFolder);
            Gerber.ZipGerberFolderToFactoryFolder(MakeJLCName(Name) + "_MERGED", BoardGerbersMergedFolder, BoardFactoryFolder);

            if (frame)
            {
                string FrameFactoryFolder = Path.Combine(FactoryFolder, Name + "_Frame");
                if (Directory.Exists(FrameFactoryFolder) == false) Directory.CreateDirectory(FrameFactoryFolder);

                string MergedFrameFactoryFolder = Path.Combine(FactoryFolder, Name + "_Framed");
                if (Directory.Exists(MergedFrameFactoryFolder) == false) Directory.CreateDirectory(MergedFrameFactoryFolder);

                string MergedFrameFactoryFolderMerged = Path.Combine(FactoryFolder, Name + "_MERGED_Framed");
                if (Directory.Exists(MergedFrameFactoryFolderMerged) == false) Directory.CreateDirectory(MergedFrameFactoryFolderMerged);

                GerberFrameWriter.FrameSettings FS = new GerberFrameWriter.FrameSettings();
                FS.FrameTitle = framename;
                FS.RenderSample = false;
                FS.margin = 3;
                FS.roundedOuterCorners = 0;
                FS.topEdge = FS.leftEdge = framemm;
                FS.RenderDirectionArrow = true;
                FS.DirectionArrowSide = GerberLibrary.Core.BoardSide.Both;
                FS.DefaultFiducials = true;
                FS.FiducialSide = BoardSide.Both;
                FS.HorizontalTabs = true;
                FS.VerticalTabs = true;
                FS.InsideEdgeMode = GerberFrameWriter.FrameSettings.InsideMode.FormFitting;

                PolyLine PL = Gerber.FindAndLoadOutlineFile(BoardGerbersFolder);
                if (PL != null)
                {
                    FS.PositionAround(PL);
                    GerberFrameWriter.WriteSideEdgeFrame(PL, FS, FrameFactoryFolder + "\\" + Name);
                    GerberFrameWriter.MergeFrameIntoGerberSet(FrameFactoryFolder, BoardGerbersFolder, MergedFrameFactoryFolder, FS, new StandardConsoleLog(), Name);
                    GerberFrameWriter.MergeFrameIntoGerberSet(FrameFactoryFolder, BoardGerbersMergedFolder, MergedFrameFactoryFolderMerged, FS, new StandardConsoleLog(), Name + "Merged");

                    Gerber.ZipGerberFolderToFactoryFolder(MakeJLCName(Name) + "_FRAMED", MergedFrameFactoryFolder, BoardFactoryFolder);
                    Gerber.ZipGerberFolderToFactoryFolder(MakeJLCName(Name) + "_MERGED_FRAMED", MergedFrameFactoryFolderMerged, BoardFactoryFolder);
                }
            }
        }

        public static void MakeBomAndPlacement(string a)
        {

            string BaseFolder = Path.GetDirectoryName(a);
            string Name = Path.GetFileNameWithoutExtension(a);

            string FactoryFolder = Path.Combine(BaseFolder, "Factory");
            if (Directory.Exists(FactoryFolder) == false) Directory.CreateDirectory(FactoryFolder);

            string BoardFactoryFolder = Path.Combine(FactoryFolder, Name);
            if (Directory.Exists(BoardFactoryFolder) == false) Directory.CreateDirectory(BoardFactoryFolder);

            Console.WriteLine("Exporting {0} in {1} to {2}", Name, BaseFolder, BoardFactoryFolder);

            GerberLibrary.Core.BOM TheBOM = new GerberLibrary.Core.BOM();


            string schname = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(a), Name + ".sch");

            if (File.Exists(schname))
            {
                try
                {
                    EagleLoader Brd = new EagleLoader(a);

                    var Sch = new EagleLibrary.EagleLoader(schname);



                    foreach (var p in Brd.DevicePlacements)
                    {
                        var SchDev = Sch.FindPlacement(p.device);
                        if (SchDev != null)
                        {
                            p.deviceset = SchDev.deviceset;
                            TheBOM.AddBOMItemExt(p.package, p.deviceset, p.value, p.device, null, Name, p.x, p.y, p.rot.Degrees, p.rot.Mirrored ? BoardSide.Bottom : BoardSide.Top);
                        }
                        else
                        {
                            Console.WriteLine(" {0} not found in schematic?? ", p.device);
                        }
                    }
                }
                catch (Exception E)
                {
                    Console.WriteLine("apparently not an eagle file..:{0}", E);
                }
            }

            string ascname = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(a), Name + ".asc");
            if (File.Exists(ascname))
            {
                try
                {
                    TheBOM.LoadDiptraceASC(ascname, new StandardConsoleLog());
                }
                catch (Exception E)
                {
                    Console.WriteLine("something went wrong loading diptrace asc..:{0}", E);
                }
            }
            TheBOM.WriteJLCCSV(BoardFactoryFolder, MakeJLCName(Name), false);
            Console.WriteLine("Done exporting.");
        }

        private static string MakeJLCName(string name)
        {
            return name.Replace(" ", "_").Replace("&", "VS");
        }
    }
}
