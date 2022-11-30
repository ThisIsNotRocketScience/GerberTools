using GerberLibrary.Core.Primitives;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GerberLibrary.Core
{
    public enum PartOrderingStatus
    {
        Unknown,
        Meta,
        Unavailable,
        Backorder,
        Kippevoer,
        MustBeOrdered,
        InCart,
        Ordered,
        Stocked
    }

    public class StockDocument
    {
        public DateTime LastUpdated;
        public List<StockPart> Parts = new List<StockPart>();

        public static StockDocument Load(string filename)
        {
            XmlSerializer SettingsSerialize = new XmlSerializer(typeof(StockDocument));
            FileStream ReadFileStream = null;

            try
            {
                ReadFileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);

                // Load the object saved above by using the Deserialize function
                StockDocument newset = (StockDocument)SettingsSerialize.Deserialize(ReadFileStream);
                return newset;

            }
            catch (Exception)
            {
                return null;
            }
        }

        public StockPart FindPart(string combinedname)
        {
            foreach (var a in Parts)
            {
                if (a.name == combinedname) return a;
            }
            return null;
        }
        public void Save(string filename)
        {
            XmlSerializer SerializerObj = new XmlSerializer(typeof(StockDocument));
            TextWriter WriteFileStream = new StreamWriter(filename);
            SerializerObj.Serialize(WriteFileStream, this);
            WriteFileStream.Close();

        }
    }

    public class StockPart
    {
        public int InStock = 0;
        public int NeededForProduction = 0;
        public string name;
        public string MFGPartname;
        public string MFG;
        public string value;
        public PartOrderingStatus status;
        public string Stock;
        public string Notes;
        public bool IsPolarized;

        public MountingType Mounting;
    }

    public class BoardContainer
    {
        public static int boardcount = 0;


        public string Name = "";
        public Color color = Color.LightGray;
        public Bitmap PrintableLabel = new Bitmap(256, 256);
        public int BoardID;
        public string BoardIDname = "";

        public BoardContainer(string basename)
        {
            BoardID = boardcount;
            boardcount++;


            Name = basename;
            if (BoardID > 0)
            {
                color = Helpers.RefractionNormalledMaxBrightnessAndSat((BoardID + 1) / 13.0f);
                BuildBitmap();
                char id = (char)((int)'A' + BoardID - 1);
                BoardIDname += id;
            }
            else
            {
                BoardIDname = "X";
            }
        }

        void BuildBitmap()
        {

        }

        public void DrawLabel(Graphics graphics, Font font, Rectangle rectangle, bool darkedge = false, float fontangle = 0)
        {
            if (darkedge) graphics.FillRectangle(new SolidBrush(Color.FromArgb(30, 30, 30)), new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height));
            graphics.FillRectangle(new SolidBrush(color), new Rectangle(rectangle.X + 2, rectangle.Y + 2, rectangle.Width - 4, rectangle.Height - 4));
            var S = graphics.MeasureString(BoardIDname, font);
            var store = graphics.Transform.Clone();
            graphics.TranslateTransform(rectangle.X + (rectangle.Width) / 2, rectangle.Y + (rectangle.Height) / 2);
            graphics.RotateTransform(fontangle);
            graphics.DrawString(BoardIDname, font, Brushes.Black, new PointF(-S.Width / 2, -S.Height / 2 + 3));
            graphics.Transform = store;
        }
    }

    public class PartListDisplayItem
    {
        public BOMEntry value;
        public int theidx;
        public PartListDisplayItem(int idx, BOMEntry value, StockPart P)
        {
            theidx = idx;
            this.value = value;
            Part = P;

        }
        public StockPart Part;

        public List<BoardContainer> BoardsThisPartAppearsOn = new List<BoardContainer>();
        public override string ToString()
        {
            return String.Format("{0}: {1} - {2} ({3})", theidx, value.RefDes.Count(), value.Combined(), value.PackageName);
        }

        public void FillBoardsFrom(List<BoardContainer> boards)
        {
            Dictionary<BoardContainer, bool> hadboards = new Dictionary<BoardContainer, bool>();
            var refgroups = from i in value.RefDes group i by i.SourceBoard;
            foreach (var g in refgroups)
            {
                var name = Path.GetFileNameWithoutExtension(g.Key);
                var bc = (from i in boards where (i.Name + "_BOM" == name) select i).FirstOrDefault();
                if (bc != null)
                {
                    hadboards[bc] = true;
                }

            }

            BoardsThisPartAppearsOn.Clear();
            foreach (var a in hadboards.Keys) BoardsThisPartAppearsOn.Add(a);
        }
    }

    public enum MountingType
    {
        SMD,
        Throughhole,
        Meta
    }
    public class BOMEntry
    {
        public string Name;
        public string Desc;
        public string Value;
        public string Notes;
        public string PackageName;
        public string MfgPartNumber;
        public string Mfg;
        public string CombinedName;
        public MountingType Mounting;
        public class RefDesc
        {
            public string NameOnBoard;
            public string SourceBoard;
            public string OriginalName;
            public double x;
            public double y;
            public double angle;

            public BoardSide Side = BoardSide.Top;


            public override string ToString()
            {
                return String.Format("{0} ({1},{2}) {3} - {4}", NameOnBoard, x, y, angle, Side);
            }

        }
        public List<RefDesc> RefDes = new List<RefDesc>();
        public bool Soldered = false;

        internal string AddRef(string refdes, string source, BOMNumberSet set, double x, double y, double angle, BoardSide side)
        {
            if (set == null)
            {
                RefDes.Add(new RefDesc() { NameOnBoard = refdes, angle = angle, OriginalName = refdes, SourceBoard = source, x = x, y = y, Side = side });
                return refdes;
            }
            string newref = set.GetNew(refdes);
            RefDes.Add(new RefDesc() { NameOnBoard = newref, angle = angle, OriginalName = refdes, SourceBoard = source, x = x, y = y, Side = side });
            return newref;
        }

        public void SetCombined(String c)
        {
            CombinedName = c;
        }

        public string Combined()
        {
            if (CombinedName != null)
            {
                return CombinedName;
            }
            // else
            return (Name + "_" + PackageName + "_" + Value).Replace(' ', '_').Replace(',', '.');
        }

        public string DisplayLine()
        {
            return RefDes.Count() + " " + Name + " " + Value + " (" + PackageName + ")";
        }
    }

    public enum DefaultPackages
    {
        Custom,
        p0402,
        p0603,
        p0805,
        QFN16_05,
        QFN24_05,
        QFN32_05,
        QFP16_05,
        QFP24_05,
        QFP32_05,
        QFP40_05,
        QFP48_05,
        SOIC8,
        SOIC14,
        SOIC16,
        SOIC18,
        DIP8,
        DIP14,
        DIP16,
        DIP18,
        DIP20,
        DIP24,
        M0101,
        M0102,
        M0103,
        M0104,
        M0105,
        M0106
    }

    public enum Units
    {
        Ohm,
        Farad,
        Henry,
        None,
        Volt
    }

    public class PartLibrary
    {
        public static Dictionary<string, string> CreatePassivesMapping()
        {
            Dictionary<string, string> Res = new Dictionary<string, string>();
            foreach (var R in Helpers.ResistorRanges)
            {
                foreach (var V in Helpers.E96)
                {
                    string unit = Helpers.MakeNiceUnitString(V * R, Units.None).Replace(" ", "").Replace(',', '.');
                    string F = "R*_R0603_" + unit.ToUpper();
                    string F2 = "R*_R0603_" + unit.ToLower();
                    string F3 = "RES_0603_1%_0603_" + unit.ToUpper();
                    string F4 = "RES_0603_1%_0603_" + unit.ToLower();


                    string T = "RES_0603_1%_RES_0603_" + unit;
                    Res[F] = T;
                    Res[F2] = T;
                    Res[F3] = T;
                    Res[F4] = T;
                }

                foreach (var V in Helpers.E24)
                {
                    string unit = Helpers.MakeNiceUnitString(V * R, Units.None).Replace(" ", "").Replace(',', '.');
                    string F = "R*_R0603_" + unit.ToUpper();
                    string F2 = "R*_R0603_" + unit.ToLower();
                    string F3 = "RES_0603_1%_0603_" + unit.ToUpper();
                    string F4 = "RES_0603_1%_0603_" + unit.ToLower();


                    string T = "RES_0603_1%_RES_0603_" + unit;
                    Res[F] = T;
                    Res[F2] = T;
                    Res[F3] = T;
                    Res[F4] = T;
                }

                foreach (var V in Helpers.E48)
                {
                    string unit = Helpers.MakeNiceUnitString(V * R, Units.None).Replace(" ", "").Replace(',', '.');
                    string F = "R*_R0603_" + unit.ToUpper();
                    string F2 = "R*_R0603_" + unit.ToLower();
                    string F3 = "RES_0603_1%_0603_" + unit.ToUpper();
                    string F4 = "RES_0603_1%_0603_" + unit.ToLower();


                    string T = "RES_0603_1%_RES_0603_" + unit;
                    Res[F] = T;
                    Res[F2] = T;
                    Res[F3] = T;
                    Res[F4] = T;
                }
            }
            foreach (var R in Helpers.CapacitorRanges)
            {
                foreach (var V in Helpers.E24)
                {
                    string unit = Helpers.MakeNiceUnitString(V * R, Units.None).Replace(" ", "").Replace(',', '.');
                    string F = "C*_C0603_" + unit.ToUpper();
                    string F2 = "C*_C0603_" + unit.ToLower();
                    string F3 = "CAP_C0G_0603_50V_5%_0603_" + unit.ToUpper();
                    string F4 = "CAP_C0G_0603_50V_5%_0603_" + unit.ToLower();

                    string T;
                    if (V * R < 48e-9)
                    {
                        T = "CAP_C0G_0603_50V_5%_CAP_0603_" + unit;
                    }
                    else
                    {
                        T = "CAP_X7R_0603_25V_CAP_0603_" + unit;
                    }
                    Res[F] = T;
                    Res[F2] = T;
                    Res[F3] = T;
                    Res[F4] = T;
                }
            }
            return Res;
        }


    }

    public class BOMNumberSet
    {
        public Dictionary<string, int> prefixmap = new Dictionary<string, int>();
        public string GetNew(string raw)
        {
            raw = raw.Split(':').Last();
            string prefix = "";
            int L = raw.Length - 1;
            while (char.IsNumber(raw[L]) && L > 0)
            {
                L--;
            }
            prefix = raw.Substring(0, L + 1);
            if (prefixmap.ContainsKey(prefix) == false)
            {
                prefixmap[prefix] = 0;
            }

            prefixmap[prefix] = prefixmap[prefix] + 1;
            return prefix + prefixmap[prefix].ToString();
        }
    }


    public class BOM
    {

        public List<BOM> SplitOverPolygons(List<Primitives.PolyLine> polygons)
        {
            List<BOM> Res = new List<BOM>();
            List<BOMNumberSet> Ns = new List<BOMNumberSet>();

            foreach (var p in polygons)
            {
                BOM SubBom = new BOM();
                Res.Add(SubBom);
                Ns.Add(new BOMNumberSet());
            }
            foreach (var packagepair in DeviceTree)
            {
                foreach (var devicevalue in packagepair.Value)
                {
                    foreach (var n in devicevalue.Value.RefDes)
                    {
                        int i = 0;
                        int added = 0;
                        foreach (var p in polygons)
                        {
                            if (p.PointInPoly(new Primitives.PointD(n.x, n.y)))
                            {

                                OptionalOut<BOMEntry> bom = new OptionalOut<BOMEntry>();

                                Res[i].AddBOMItemInt(devicevalue.Value.PackageName, devicevalue.Value.Name, devicevalue.Value.Value, n.NameOnBoard, Ns[i], n.SourceBoard, n.x, n.y, n.angle, n.Side, bom);
                                bom.Result.SetCombined(devicevalue.Value.Combined());

                                added++;
                            }
                            i++;
                        }
                        if (added == 0)
                        {
                            Console.WriteLine("part skipped for some reason: {0}", n.NameOnBoard);
                        }
                        if (added > 1)
                        {
                            Console.WriteLine("part doublebooked for some reason: {0}", n.NameOnBoard);

                        }
                    }
                }
            }


            return Res;
        }

        public void RemoveIgnored(List<string> toIgnore)
        {
            List<BOMEntry> ToRemove = new List<BOMEntry>();
            foreach (var a in DeviceTree)
            {
                //Console.WriteLine(a.Key);
                foreach (var b in a.Value)
                {
                    if (toIgnore.Contains(b.Value.PackageName) == true)
                    {
                        ToRemove.Add(b.Value);
                    }
                }
            }
            foreach (var a in ToRemove)
            {
                RemoveEntry(a);
            }
        }


        public Dictionary<string, Dictionary<string, BOMEntry>> DeviceTree = new Dictionary<string, Dictionary<string, BOMEntry>>();

        public string AddBOMItemExt(string package, string device, string value, string refdes, BOMNumberSet set, string SourceBoard, double x, double y, double angle, BoardSide side = BoardSide.Top)
        {
            string ID = GetID(package, device, refdes);
            return AddBOMItemInt(package, device, value, refdes, set, SourceBoard, x, y, angle, side);
        }

        public class OptionalOut<Type>
        {
            public Type Result { get; set; }
        }

        string AddBOMItemInt(string package, string device, string value, string refdes, BOMNumberSet set, string SourceBoard, double x, double y, double angle, BoardSide side = BoardSide.Top, OptionalOut<BOMEntry> bom = null)
        {

            string ID = GetID(package, device, refdes);


            if (DeviceTree.ContainsKey(ID) == false) DeviceTree[ID] = new Dictionary<string, BOMEntry>();
            if (DeviceTree[ID].ContainsKey(value) == false) DeviceTree[ID][value] = new BOMEntry() { Name = device, Value = value, PackageName = package };
            BOMEntry BE = DeviceTree[ID][value];

            if (bom != null) { bom.Result = BE; };

            return BE.AddRef(refdes, SourceBoard, set, x, y, angle, side);




        }

        private string GetID(string package, string device, string refdes)
        {
            string ID = package + device;
            if (refdes == device) ID = package;
            return ID;
        }

        public void Translate(double x, double y)
        {
            foreach (var a in DeviceTree)
            {
                foreach (var b in a.Value)
                {
                    foreach (var c in b.Value.RefDes)
                    {
                        c.x += x;
                        c.y += y;
                    }
                }
            }
        }

        public void WriteRefDesGerber(string outputbasename)
        {
            FontSet fnt = FontSet.Load("Font.xml");
            GerberArtWriter Top = new GerberArtWriter();
            GerberArtWriter Bottom = new GerberArtWriter();
            int polyid = 0;
            foreach (var a in DeviceTree)
            {
                foreach (var b in a.Value)
                {
                    foreach (var c in b.Value.RefDes)
                    {
                        GerberArtWriter dest = (c.Side == BoardSide.Top) ? Top : Bottom;
                        float CX = (float)Math.Cos((c.angle * Math.PI*2.0)/360.0);
                        float SX = (float)Math.Sin((c.angle * Math.PI * 2.0) / 360.0);

                        dest.DrawString(new Primitives.PointD(c.x + CX, c.y + SX), fnt, c.NameOnBoard, 1, 0.02, StringAlign.CenterLeft, (c.Side == BoardSide.Top) ? false : true, c.angle);
                        PolyLine CrossA = new PolyLine(polyid++);
                        PolyLine CrossB = new PolyLine(polyid++);
                        CrossA.Add(c.x - 0.5, c.y - 0.5);
                        CrossA.Add(c.x + 0.5, c.y + 0.5);
                        CrossB.Add(c.x - 0.5, c.y + 0.5);
                        CrossB.Add(c.x + 0.5, c.y - 0.5);
                        dest.AddPolyLine(CrossA, 0.1);
                        dest.AddPolyLine(CrossB, 0.1);
                       
                    }

                }
            }

            Top.Write(outputbasename + "_top_refdes.gbr");
            Bottom.Write(outputbasename + "_bottom_refdes.gbr");
        }

        public void MergeBOM(BOM B, BOMNumberSet set, double dx, double dy, double cx, double cy, double angle)
        {
            foreach (var a in B.DeviceTree)
            {
                foreach (var b in a.Value)
                {
                    foreach (var c in b.Value.RefDes)
                    {
                        double X = c.x;
                        double Y = c.y;
                        Helpers.Transform(dx, dy, cx, cy, angle, ref X, ref Y);
                        OptionalOut<BOMEntry> bom = new OptionalOut<BOMEntry>();
                        AddBOMItemInt(b.Value.PackageName, b.Value.Name, b.Value.Value, c.OriginalName, set, c.SourceBoard, X, Y, (c.angle - angle) % 360, c.Side, bom);
                        bom.Result.SetCombined(b.Value.Combined());
                        bom.Result.Mounting = b.Value.Mounting;
                    }
                }
            }
        }
        public int GetPartCount(List<String> ToIgnore)
        {
            int partcount = 0;
            foreach (var a in DeviceTree)
            {
                //Console.WriteLine(a.Key);
                foreach (var b in a.Value)
                {
                    if (ToIgnore.Contains(b.Value.PackageName) == false)
                    {
                        partcount += b.Value.RefDes.Count;
                    }
                }
            }

            return partcount;

        }

        public int GetUniquePartCount(List<String> ToIgnore)
        {
            int partcount = 0;
            foreach (var a in DeviceTree)
            {
                //Console.WriteLine(a.Key);
                foreach (var b in a.Value)
                {
                    if (ToIgnore.Contains(b.Value.PackageName) == false)
                    {
                        partcount++;
                    }
                }
            }

            return partcount;
        }

        public void LoadPrinted(string filename)
        {
            BOMNumberSet BS = new BOMNumberSet();
            try
            {
                var L = File.ReadAllLines(filename);

                //string Header = "Count,Name,Type,Package,Value,MfgPartNumber,RefDes";

                int idxcount = 0;
                int idxname = 1;
                int idxtype = 2;
                int idxpackage = 3;
                int idxvalue = 4;
                int idxmfg = 5;
                int idxrefdes = 6;
                var Hsplit = L[0].Split(',');



                for (int l = 1; l < L.Count(); l++)
                {
                    var lsplit = L[l].Split(',');
                    var refsplit = lsplit[idxrefdes].Split(' ');
                    foreach (var refdes in refsplit)
                    {
                        AddBOMItemInt(lsplit[idxpackage], lsplit[idxname], lsplit[idxvalue], refdes, BS, filename, 0, 0, 0);
                    }
                }


            }
            catch (Exception)
            {

            }


        }

        public List<string> PrintBOM(List<String> IgnoreList, bool AddDefaultIgnoreList = true)
        {
            List<string> ToIgnore;

            if (AddDefaultIgnoreList)
            {
                ToIgnore = new List<string>() { "FENIXINTERNALCONNECTIONPAD" };

            }
            else
            {
                ToIgnore = new List<string>();

            }
            ToIgnore.AddRange(IgnoreList);
            List<Tuple<string, string, string>> Lines = new List<Tuple<string, string, string>>();

            string Header = "Count,Name,Type,Package,Value,MfgPartNumber,RefDes";
            Lines.Add(new Tuple<string, string, string>("!", "!", Header));

            int partcount = 0;
            List<String> re = new List<string>();
            foreach (var a in DeviceTree)
            {
                //Console.WriteLine(a.Key);
                foreach (var b in a.Value)
                {
                    if (ToIgnore.Contains(b.Value.PackageName) == false)
                    {
                        partcount += b.Value.RefDes.Count;
                        string L = String.Format("{3},{4}, {0}, {1}, {2}, {5}, ", b.Value.Name, b.Value.PackageName, b.Value.Value, b.Value.RefDes.Count, b.Value.Combined(), b.Value.MfgPartNumber);

                        foreach (var c in b.Value.RefDes)
                        {
                            L = L + String.Format("{0} ", c.NameOnBoard);
                        }

                        Lines.Add(new Tuple<string, string, string>(b.Value.Name, b.Value.Value, L));
                    }

                }
            }

            foreach (var a in Lines.OrderBy(x => x.Item1).ThenBy(x => x.Item2))
            {

                re.Add(a.Item3);
            }
            re.Add(String.Format("{0} parts total.", partcount));
            return re;

        }

        public List<string> PrintBOMperModule(List<String> IgnoreList)
        {
            Dictionary<string, bool> Boards = new Dictionary<string, bool>();

            foreach (var a in DeviceTree)
            {
                //Console.WriteLine(a.Key);
                foreach (var b in a.Value)
                {
                    foreach (var c in b.Value.RefDes)
                    {
                        Boards[c.SourceBoard] = true;
                    }
                }
            }

            List<string> ToIgnore = new List<string>() { "FENIXINTERNALCONNECTIONPAD" };
            ToIgnore.AddRange(IgnoreList);
            List<String> re = new List<string>();

            foreach (var board in Boards.Keys)
            {
                int partcount = 0;

                re.Add(String.Format("***** {0}", board));
                List<Tuple<string, string, string>> Lines = new List<Tuple<string, string, string>>();
                foreach (var a in DeviceTree)
                {
                    //Console.WriteLine(a.Key);
                    foreach (var b in a.Value)
                    {
                        if (ToIgnore.Contains(b.Value.PackageName) == false)
                        {
                            int cnt = b.Value.RefDes.Where(x => x.SourceBoard == board).Count();
                            if (cnt > 0)
                            {
                                partcount += cnt;
                                string L = String.Format("{3},{4}, {0}, {1}, {2}, ", b.Value.Name, b.Value.PackageName, b.Value.Value, cnt, b.Value.Combined());

                                foreach (var c in b.Value.RefDes.Where(x => x.SourceBoard == board))
                                {
                                    L = L + String.Format("{0}({1}) ", c.NameOnBoard, c.OriginalName);
                                }
                                Lines.Add(new Tuple<string, string, string>(b.Value.Name, b.Value.Value, L));
                            }
                        }

                    }
                }
                foreach (var a in Lines.OrderBy(x => x.Item1).ThenBy(x => x.Item2))
                {

                    re.Add(a.Item3);
                }
                re.Add(String.Format("{0} parts total.", partcount));
                re.Add(String.Format(""));
            }


            return re;
        }


        public void FillPartno(string basefolder)
        {
            var map = File.ReadAllLines(Path.Combine(basefolder, "partnomapping.txt"));

            foreach (var l in map)
            {
                var A = l.Split(' ');
                if (A.Count() == 2)
                {
                    string from = A[0];
                    string to = A[1];

                    SetPartno(from, to);

                }
            }
        }
        public static double ParseValue(string value)
        {
            var r = new Regex("^([1-9][0-9]*(?:\\.[0-9]+)?)([pnumkMG])?$");
            Match m = r.Match(value);

            String valstring = m.Groups[1].Value.Replace('.', ',');
            double v = Convert.ToDouble(valstring);

            if (m.Groups.Count == 3)
            {
                Dictionary<string, double> Scale = new Dictionary<string, double>();
                Scale["p"] = 1e-12;
                Scale["n"] = 1e-9;
                Scale["u"] = 1e-6;
                Scale["m"] = 1e-3;
                Scale[""] = 1;
                Scale["k"] = 1e3;
                Scale["M"] = 1e6;
                Scale["G"] = 1e9;
                v *= Scale[m.Groups[2].Value];
            }

            return v;
        }

        public static string KemetC0GPartno(string value)
        {
            var r = new Regex("CAP_C0G_0603_50V_5%_CAP_0603_([^_]*)$");
            Match m = r.Match(value);
            if (m.Groups.Count != 2) return "";

            double val = ParseValue(m.Groups[1].Value) * 1000000000000;

            int n = 0;
            string valstring = "";
            if (val >= 100)
            {
                while (val >= 100)
                {
                    n++;
                    val /= 10;
                }
                valstring = Convert.ToString(val);
                valstring += n;
            }
            else if (val < 10)
            {
                valstring = Math.Round(val * 10) + "9";
            }
            else if (val < 100)
            {
                valstring = Math.Round(val) + "8";
            }

            string s = "C0603C" + valstring + "J?G";
            return s;
        }

        public static string VishayThickFilmResistorPartno(string value)
        {
            var r = new Regex("RES_0603_1%_RES_0603_([^_]*)$");
            Match m = r.Match(value);
            if (m.Groups.Count != 2) return "";

            double val = ParseValue(m.Groups[1].Value);

            String valstring = "";
            if (val < 1000)
            {
                double i = Math.Floor(val);
                double frac = val - i;
                valstring = i + "R";
                if (valstring.Length < 4)
                {
                    for (int k = 0; k < 4 - valstring.Length; k++)
                    {
                        frac = frac * 10;
                    }
                    valstring += Math.Round(frac);
                }
            }
            else if (val < 1000000)
            {
                val /= 1000;
                double i = Math.Floor(val);
                double frac = val - i;
                valstring = i + "K";
                if (valstring.Length < 4)
                {
                    for (int k = 0; k < 4 - valstring.Length; k++)
                    {
                        frac = frac * 10;
                    }
                    valstring += Math.Round(frac);
                }
            }
            else if (val < 1000000000)
            {
                val /= 1000000;
                double i = Math.Floor(val);
                double frac = val - i;
                valstring = i + "M";
                if (valstring.Length < 4)
                {
                    for (int k = 0; k < 4 - valstring.Length; k++)
                    {
                        frac = frac * 10;
                    }
                    valstring += Math.Round(frac);
                }
            }

            string s = "CRCW0603" + valstring + "FK";
            return s;
        }

        public static string SusumuThinFilmResistorPartno(string value)
        {
            var r = new Regex("RES_0603_1%_THIN_FILM_RES_0603_([^_]*)$");
            Match m = r.Match(value);
            if (m.Groups.Count != 2) return "";

            double val = ParseValue(m.Groups[1].Value);

            string tempco = "P";
            if (val < 92) tempco = "Q";

            int n = 0;
            while (val >= 100)
            {
                n++;
                val /= 10;
            }

            string s = "RR0816" + tempco + "-" + val + n + "-D";
            return s;
        }

        public static string XiconThroughHolePartno(string value)
        {
            var r = new Regex("R-US__0204V_([^_]*)$");
            Match m = r.Match(value);
            if (m.Groups.Count != 2) return "";
            string val = m.Groups[1].Value;

            string s = "270-" + val + "-RC";
            return s;
        }

        public static string MakePartNo(string partno, string value)
        {

            if (partno.Equals("%%KEMETC0G%%"))
            {
                return KemetC0GPartno(value);
            }
            else if (partno.Equals("%%MULTICOMPTHICKRES%%"))
            {
                return MulticompThickFilmResistorPartno(value);
            }
            else if (partno.Equals("%%VISHAYTHICKRES%%"))
            {
                return VishayThickFilmResistorPartno(value);
            }
            else if (partno.Equals("%%SUSUMUTHINRES%%"))
            {
                return SusumuThinFilmResistorPartno(value);
            }
            else if (partno.Equals("%%XICONTHROUGHHOLE%%"))
            {
                return XiconThroughHolePartno(value);
            }

            return partno;
        }

        public static Dictionary<string, string> MulticompReels = new Dictionary<string, string>();

        public static Dictionary<string, string> MulticompOrderList = new Dictionary<string, string>();

        private static string MulticompThickFilmResistorPartno(string value)
        {
            value = value.Replace(',', '.');
            var r = new Regex("RES_0603_1%_RES_0603_([^_]*)$");
            Match m = r.Match(value);
            if (m.Groups.Count != 2) return "";
            string valuestring = m.Groups[1].Value;
            double val = ParseValue(valuestring);

            int n = 0;
            while (val > 1000)
            {
                n++;
                val /= 10;
            }
            while (Math.Floor(val / 10.0) < (val / 10.0))
            {
                n--;
                val *= 10;
            }



            string s = "MCWR06X" + Math.Floor(val) + n + "FTL";

            /*           if (AlreadyHave.Contains(valuestring) == false)
                        {
                            MulticompReels[value] = s;
                        }
                        if (needthese.Contains(valuestring))
                        {
                            MulticompOrderList[valuestring] = s;
                        }*/

            return s;


        }

        public void SetPartno(string pattern, string partno)
        {
            var r = new Regex(pattern);
            foreach (var a in DeviceTree)
            {
                foreach (var b in a.Value)
                {
                    string cop = b.Value.Combined().Trim();
                    if (b.Value.MfgPartNumber == null && r.Match(cop).Success)
                    {
                        b.Value.MfgPartNumber = MakePartNo(partno, cop);
                    }
                }
            }
        }

        public void Remap(ProgressLog log, string basefolder)
        {
            log.PushActivity("Remap");

            var D = PartLibrary.CreatePassivesMapping();
            File.WriteAllLines(Path.Combine(basefolder, "genmapping.txt"), (from i in D select i.Key + " " + i.Value).ToList());
            var map = File.ReadAllLines(Path.Combine(basefolder, "bommapping.txt"));
            log.AddString("Loaded bommapping!");
            foreach (var l in map)
            {
                var A = l.Split(' ');
                if (A.Count() >= 2)
                {
                    string from = A[0];
                    string to = A[1];

                    bool dooverride = false;
                    if (A.Count() > 2)
                    {
                        if (A[2] == "override") { dooverride = true; };
                    }
                    RemapPair(log, from, to, dooverride);
                    //Console.WriteLine("remapped {0} to {1}", from, to);

                }
            }
            foreach (var l in D)
            {

                RemapPair(log, l.Key, l.Value, false);
            }
            log.PopActivity();
        }

        private void RemapPair(ProgressLog log, string from, string to, bool overridevalue)
        {
            log.PushActivity("RemapPair");
            if (from != to)
            {

                var F = FindEntry(from, overridevalue);
                var T = FindEntry(to, overridevalue);
                if (F != null)
                {
                    if (T != null)
                    {
                        foreach (var rd in F.RefDes)
                        {
                            T.RefDes.Add(rd);
                        }
                        F.RefDes.Clear();

                        RemoveEntry(F);
                    }
                    else
                    {
                        log.AddString(String.Format("From found, but no To: {0}", from));
                        F.SetCombined(to);
                    }
                }
            }
            log.PopActivity();
        }

        private void RemoveEntry(BOMEntry f)
        {
            foreach (var a in DeviceTree)
            {
                foreach (var item in a.Value.Where(kvp => kvp.Value == f).ToList())
                {
                    a.Value.Remove(item.Key);
                }
            }
        }

        private BOMEntry FindEntry(string q, bool overridevalue = false)
        {
            foreach (var a in DeviceTree)
            {
                //Console.WriteLine(a.Key);
                foreach (var b in a.Value)
                {
                    string cop = b.Value.Combined().Trim();
                    if (cop == q)
                    {
                        return b.Value;
                    }

                }
            }

            return null;
        }

        public void SaveBom(string bomFile)
        {
            var L = PrintBOM(new List<string>(), false);
            File.WriteAllLines(bomFile, L.ToArray());
        }

        public void SaveCentroids(string centroidFile)
        {
            List<String> Output = new List<string>();

            Output.Add("\"REFDES\",\"X\",\"Y\",\"ANGLE\",\"SIDE\"");
            foreach (var a in DeviceTree)
            {
                foreach (var b in a.Value)
                {
                    foreach (var c in b.Value.RefDes)
                    {
                        Output.Add(String.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\"", c.NameOnBoard, c.x.ToString().Replace(',', '.'), c.y.ToString().Replace(',', '.'), c.angle.ToString().Replace(',', '.'), c.Side.ToString()));
                    }
                }
            }
            File.WriteAllLines(centroidFile, Output.ToArray());
        }

        internal int GetPartCount()
        {
            int partcount = 0;
            foreach (var a in DeviceTree)
            {
                foreach (var b in a.Value)
                {
                    partcount += b.Value.RefDes.Count;
                }
            }
            return partcount;
        }

        internal static BOM ScanFolderForBoms(string gerberPath, ProgressLog Log)
        {
            string ultiboardfolder = Path.Combine(gerberPath, "ultiboard");

            if (Directory.Exists(ultiboardfolder))
            {
                BOM R = new BOM();
                var F = Directory.GetFiles(ultiboardfolder);
                string Centroids = "";
                string BOMFile = "";

                foreach (var a in F)
                {
                    if (a.ToLower().Contains("bill of materials"))
                    {
                        BOMFile = a;
                    }
                    if (a.ToLower().Contains("parts centroids"))
                    {
                        Centroids = a;
                    }

                }

                if (File.Exists(Centroids) && File.Exists(BOMFile)) R.LoadUltiboard(BOMFile, Centroids);
                return R;
            }

            string diptracefolder = Path.Combine(gerberPath, "diptraceasc");

            if (Directory.Exists(diptracefolder))
            {
                BOM R = new BOM();
                var F = Directory.GetFiles(diptracefolder, "*.asc");

                foreach (var a in F)
                {
                    R.LoadDiptraceASC(a, Log);
                    return R;
                }

            }
            return null;


        }

        public class SNode
        {
            public override string ToString()
            {
                return Name + String.Format(" ({0} children)", Children.Count);
            }
            public string Print(int tab = 0)
            {
                var res = tab.ToString() + " ";
                for (int i = 0; i < tab; i++)
                {
                    res += "  ";
                }
                res += String.Format("{0}", Name) + "\n";
                foreach (var a in Children)
                {
                    res += a.Print(tab + 1);
                }
                return res;
            }
            public void Load(string s)
            {
                Children = ReadExpression(s);
            }
            public void Read(string s)
            {
                int R = FindWhiteSpace(s);
                if (R == -1) R = s.Length - 1;
                int start = 0;
                while (s[start] == '(') start++;
                Name = s.Substring(start, R - start).Trim();

                int i = s.LastIndexOf(')');
                Load(s.Substring(R, i - R));


            }

            private int FindWhiteSpace(string s)
            {

                int idx = 0;
                while (idx < s.Length)
                {
                    if (char.IsWhiteSpace(s[idx])) return idx;
                    idx++;
                }
                return idx;
            }

            public string Name;
            public List<SNode> Children = new List<SNode>();
            string currenttoken = "";
            int owntokens = 0;

            public int Read(string s, int start)
            {
                int P = start;
                bool quoted = false;
                bool escaped = false;
                int depth = 1;
                int written = 0;
                while (P < s.Length && depth > 0)
                {
                    if (quoted)
                    {
                  //      Console.Write("{0}", s[P]);
                        written++;
                        if (written > 100)
                        {
                    //        Console.WriteLine("");
                            written = 0;
                        }
                    }
                    else
                    {
                        if (written > 0)
                        {
                      //      Console.WriteLine("");
                            written = 0;
                        }
                    }
                    switch (s[P])
                    {
                        case '\\':
                        {
                                if (escaped)
                                {
                                    currenttoken += s[P];
                                    escaped = false;
                                }
                                else
                                {
                                    escaped = true;
                                }
                                P++;
                            }
                            break;
                        case '"':
                            if (quoted)
                            {
                                if (escaped)
                                {
                                    currenttoken += s[P];
                                    escaped = false;
                                }
                                else
                                {
                                    quoted = false;
                                    CheckTokenAndConsume();

                                }
                            }
                            else
                            {
                                if (escaped)
                                {
                                    currenttoken += s[P];
                                    escaped = false;
                                }
                                else
                                {
                               //     Console.Write("escaping:");
                                    for (int R = Math.Max(P - 140, 0); R != P + 1; R++)
                                    {
                                 //       Console.Write("{0}", s[R]);
                                    }
                                    //Console.WriteLine("-- escaping {0}", P);

                                    quoted = true;
                                    escaped = false;
                                }
                            }
                            P++;
                            break;
                        case '\r':
                        case '\n':
                        case '\t':
                        case ' ':
                            if (quoted)
                            {
                                currenttoken += s[P];
                            }
                            else
                            {
                                CheckTokenAndConsume();

                            }
                            P++;
                            break;
                        case '(':
                            if (!quoted)
                            {
                                CheckTokenAndConsume();
                                SNode child = new SNode();
                                P = child.Read(s, P + 1);
                                Children.Add(child);
                            }
                            else
                            {
                                currenttoken += s[P];
                                P++;
                            }
                            break;
                        case ')':
                            if (!quoted)
                            {
                                CheckTokenAndConsume();
                                return P + 1;
                            }
                            else
                            {
                                currenttoken += s[P];
                                P++;
                            }break;

                        default:
                            currenttoken += s[P];
                            P++;
                            escaped = false;
                            break;

                    }
                    

                }

                return P;
            }

            private void CheckTokenAndConsume()
            {
                if (currenttoken.Length > 0)
                {
                    if (owntokens == 0)
                    {
                        Name = currenttoken;

                    }
                    else
                    {
                        Children.Add(new SNode() { Name = currenttoken });
                    }

                    owntokens++;
                    currenttoken = "";
                }
            }

            public static List<SNode> ReadExpression(string s)
            {
                List<SNode> res = new List<SNode>();
                string currenttoken = "";
                int R = 0;
                bool escaped =false ;
                while (R < s.Length)
                {
                    if (escaped)
                    {
                        switch (s[R])
                        {
                            case '"':
                                escaped = false;
                                break;

                        }
                        R++;
                    }
                    else

                    {
                        switch (s[R])
                        {
                            /*case '"':
                                escaped = true;
                                R++;
                                break;*/
                            case '\r':
                            case '\n':
                            case '\t':
                            case ' ':
                                currenttoken = "";

                                R++;
                                break;
                            default:
                                currenttoken += s[R];
                                R++;
                                break;
                            case '(':
                                {
                                    SNode child = new SNode();
                                    R = child.Read(s, R + 1);
                                    res.Add(child);
                                }
                                break;
                        }
                    }
                    
                }
                return res;

            }
            public static List<SNode> aReadExpression(string s)
            {
                List<SNode> res = new List<SNode>();

                int R = 0;

                while (R < s.Length)
                {
                    switch (s[R])
                    {
                        case '\r':
                        case '\n':
                        case '\t':
                        case ' ':
                            R++;
                            break;
                        default:
                            {
                                int E = R + 1;
                                while (E < s.Length && digits(s[E]))
                                {
                                    E++;
                                }

                                SNode c = new SNode();
                                c.Name = s.Substring(R, E - R);
                                res.Add(c);
                                R = E + 1;

                            }

                            break;

                        case '(':
                            {
                                int depth = 1;
                                int pos = R + 1;
                                while (depth > 0 && pos < s.Length)
                                {
                                    if (s[pos] == '(')
                                    {
                                        depth++;
                                    }
                                    if (s[pos] == ')')
                                    {
                                        depth--;
                                    }
                                    pos++;
                                }
                                int E = pos + 1;



                                SNode c = new SNode();
                                c.Read(s.Substring(R, E - R - 1));
                                res.Add(c);

                                R = E + 1;
                            }
                            break;
                        case '"':
                            {
                                int E = R + 1;
                                while (s[E] != '"')
                                {
                                    E++;
                                }

                                SNode c = new SNode();
                                c.Name = s.Substring(R + 1, E - R - 1);
                                res.Add(c);
                                R = E + 1;

                            }
                            break;
                    }
                }
                // Name = s.Substring(1, R);



                return res;
            }

            private static bool digits(char v)
            {
                if (char.IsWhiteSpace(v)) return false;

                return true;
            }

            public List<SNode> Find(string v, bool recurse = true)
            {
                List<SNode> R = new List<SNode>();
                foreach (var a in Children)
                {
                    if (a.Name == v)
                    {
                        R.Add(a);
                    }
                    if (recurse) R.AddRange(a.Find(v, true));
                }
                return R;
            }
        }

        static void DipRotateAndAdjust(double X, double Y, double OriginX, double OriginY, double Angle, int Orientation, int PatternOrientation, bool flipped, BOMEntry.RefDesc RD, double fileoriginX, double fileoriginY, double UnitMult)
        {
            if (flipped) OriginX = -OriginX;

            double pat_rot = ((Orientation) * 90) * 3.141592654 / 180.0;
            double r_orig_x = 0;

            double r_orig_y = 0;

            if (flipped)
            {
                r_orig_x = OriginX * Math.Cos(pat_rot) - OriginY * Math.Sin(pat_rot);
                r_orig_y = -OriginX * Math.Sin(pat_rot) - OriginY * Math.Cos(pat_rot);
            }
            else
            {
                r_orig_x = -OriginX * Math.Cos(pat_rot) - OriginY * Math.Sin(pat_rot);
                r_orig_y = OriginX * Math.Sin(pat_rot) - OriginY * Math.Cos(pat_rot);
            }

            double p_x = (X - r_orig_x);
            double p_y = (Y - r_orig_y);

            //     p_y = -p_y;
            double p_angle = ((Orientation) * 90) + Angle;

            if (flipped) //: # maybe this should check BottomSide instead
            {
                p_angle = 360 - p_angle;
            }

            p_angle = (p_angle + 10 * 360) % 360;

            RD.x = (p_x - fileoriginX) * UnitMult;
            RD.y = -(p_y - fileoriginY) * UnitMult;
            RD.angle = -p_angle;

        }



        public void LoadDiptraceASC(string InputFile, ProgressLog Log)
        {
                Log.PushActivity("DIPTrace BOM Loader");

                Log.AddString(String.Format("Loading Diptrace module: {0}", Path.GetFileNameWithoutExtension(InputFile)));

                string S = System.IO.File.ReadAllText(InputFile);
                S = S.Replace("\n", "");
                var Root = new SNode();
                Root.Load(S);
                var Src = Root.Find("Source");
                float AngleMult = 1.0f;
                if (Src.Count > 0)
                {
                    if (Src.First().Children[0].Name == "DipTrace-PCBN")
                    {
                        Log.AddString("New Dip Asc");
                        AngleMult = 360.0f / ((float)Math.PI * 2.0f);
                    }
                    else
                    {
                        if (Src.First().Children[0].Name == "DipTrace-PCB")
                        {
                            Log.AddString("Old Dip Asc");
                            AngleMult = 1.0f;
                        }
                    }
                }
                //    string Out = Root.Print();
                float UnitMult = 10.0f / 25.4f;
                UnitMult = 10.0f / 30.0f;

                var C = Root.Find("Board", false).First().Find("Components", false);
                var Shapes = Root.Find("Board", false).First().Find("Shapes", false);


                float originx = 0;
                float originy = 0;
                var ox = Root.Find("OriginX");
                var oy = Root.Find("OriginY");
                if (ox.Count > 0)
                {
                    originx = float.Parse(ox.First().Children[0].Name.Replace('.', ','));
                }
                if (oy.Count > 0)
                {
                    originy = float.Parse(oy.First().Children[0].Name.Replace('.', ','));
                }
                var Um = Root.Find("mm");
                if (Um.Count > 0) UnitMult = 10.0f / 30.0f;
                var Umil = Root.Find("mil");
                if (Umil.Count > 0) UnitMult = 10.0f / 30.0f;

/*                var Pts = Root.Find("Board", false).First().Find("Points", false);
                
                List<PointF> Outline = new List<PointF>();
                foreach (var p in Pts.First().Children)
                {
                    Outline.Add(new PointF((float.Parse(p.Children[0].Name.Replace('.', ',')) - originx) * UnitMult, -(float.Parse(p.Children[1].Name.Replace('.', ',')) - originy) * UnitMult));

                }
                for (int i = 0; i < Outline.Count; i++)
                {
                    int idx2 = (i + 1) % Outline.Count;
                    AddWire(Outline[i].X, Outline[i].Y, Outline[idx2].X, Outline[idx2].Y);
                }



                foreach (var s in Shapes[0].Children)
                {
                    var ShapeNmae = s.Find("Name", false).FirstOrDefault();
                    if (ShapeNmae != null)
                    {
                        if (ShapeNmae.Children[0].Name == "Panel Area")
                        {

                            var pts = s.Find("Points", false);
                            if (pts != null && pts[0].Children.Count == 2)
                            {

                                var c1 = pts[0].Children[0];
                                var c2 = pts[0].Children[1];
                                var x1 = (float.Parse(c1.Children[0].Name.Replace('.', ',')) - originx) * UnitMult;
                                var y1 = -(float.Parse(c1.Children[1].Name.Replace('.', ',')) - originy) * UnitMult;
                                var x2 = (float.Parse(c2.Children[0].Name.Replace('.', ',')) - originx) * UnitMult;
                                var y2 = -(float.Parse(c2.Children[1].Name.Replace('.', ',')) - originy) * UnitMult;
                                this.HasGrid = true;
                                this.GridOffset = new vec2(Math.Min(x1, x2), Math.Max(y1, y2));
                                this.HasEnd = true;
                                this.EndPos = new vec2(Math.Max(x1, x2), Math.Min(y1, y2));
                            }

                        }

                    }

                }

                */

                foreach (var a in C[0].Children)
                {


                    float partoriginx = 0;
                    float partoriginy = 0;
                    var pox = a.Find("OriginX");
                    var poy = a.Find("OriginY");
                    if (pox.Count > 0)
                    {
                        partoriginx = float.Parse(pox.First().Children[0].Name.Replace('.', ','));
                    }
                    if (poy.Count > 0)
                    {
                        partoriginy = float.Parse(poy.First().Children[0].Name.Replace('.', ','));
                    }


                    float x = -1;
                    float y = -1;
                    string name = "unknown";
                    string id = "id";
                    // try
                    {
                        name = a.Children[0].Name;
                        id = a.Children[1].Name;
                        string descattrib = "";
                        string sizeattrib = "";
                        string value = "";
                        var vV = a.Find("Value", false).FirstOrDefault();
                        var pP = a.Find("Pattern", false).FirstOrDefault();


                        if (vV != null && vV.Children.Count > 0)
                        {
                            descattrib = vV.Children[0].Name;
                            value = vV.Children[0].Name;
                        }

                        BoardSide side = BoardSide.Top;
                        var BottomSide = a.Find("BottomSide", false).FirstOrDefault();
                        if (BottomSide != null && BottomSide.Children.Count > 0)
                        {
                            string V = BottomSide.Children[0].Name;
                            if (V == "Y") side = BoardSide.Bottom;
                        }

                       
                        string pattern = "";

                        if (pP != null && pP.Children.Count > 0)
                        {
                            pattern = pP.Children[0].Name;
                        }
                        var X = a.Find("X", false).FirstOrDefault();
                        if (X != null && X.Children.Count > 0)
                        {
                            string V = X.Children[0].Name;
                            if (V[0] == '.') V = "0" + V;
                            V = V.Replace('.', ',');
                            x = float.Parse(V);

                        }
                        var Y = a.Find("Y", false).FirstOrDefault();
                        if (Y != null && Y.Children.Count > 0)
                        {
                            string V = Y.Children[0].Name;
                            if (V[0] == '.') V = "0" + V;
                            V = V.Replace('.', ',');

                            y = float.Parse(V);
                        }
                        float angle = 0;
                        float orientangle = 0;
                        int orientidx = 0;
                        var O = a.Find("Orientation", false).FirstOrDefault();
                        if (O != null && O.Children.Count > 0)
                        {
                            string V = O.Children[0].Name;
                            if (V[0] == '0') { orientangle = 0; orientidx = 0; };
                            if (V[0] == '1') { orientangle = 90; orientidx = 1; };
                            if (V[0] == '2') { orientangle = 180; orientidx = 2; };
                            if (V[0] == '3') { orientangle = 270; orientidx = 3; };
                        }
                        float pangle = 0;
                        int partorientidx = 0;
                        var PO = a.Find("PatternOrientation", false).FirstOrDefault();
                        if (PO != null && PO.Children.Count > 0)
                        {
                            string V = PO.Children[0].Name;
                            if (V[0] == '0') { pangle = 0; partorientidx = 0; }
                            if (V[0] == '1') { pangle = 90; partorientidx = 1; }
                            if (V[0] == '2') { pangle = 180; partorientidx = 2; }
                            if (V[0] == '3') { pangle = 270; partorientidx = 3; }

                        }


                        var A = a.Find("Angle", false).FirstOrDefault();
                        if (A != null && A.Children.Count > 0)
                        {
                            angle = float.Parse(A.Children[0].Name.Replace('.', ',')) * AngleMult;
                        }

                        bool Flipped = false;
                        var nFlipped = a.Find("Flipped", false).FirstOrDefault();
                        if (nFlipped != null && BottomSide.Children.Count > 0)
                        {
                            string V = nFlipped.Children[0].Name;
                            if (V == "Y")
                            {
                                Flipped = true;
                            }
                        }

                        var placement = AddBOMItemExt(pattern, name, value, id, null, System.IO.Path.GetFileNameWithoutExtension(InputFile), 0, 0, 0, side);
                        var BE = GetBOMEntry(placement);
                    float DipComp = 0;// DipCompensationAngle(BE.Combined(), side);

                        // orientangle += DipComp;
                        var RD = GetRefDes(placement);

                        DipRotateAndAdjust(x, y, partoriginx, partoriginy, angle, orientidx, partorientidx, Flipped, RD, originx, originy, UnitMult);
                        if (side == BoardSide.Bottom) RD.angle = -RD.angle;
                        RD.angle += DipComp;
                        if (id == "C10")
                        {
                            Console.WriteLine(RD);
                        }
                       
                        
                        
                    }

                }
                Log.PopActivity();

        }

        class CSVLine
        {
            public List<string> columns = new List<string>();
            public void Parse(string inp)
            {
                const char splitter = ',';
                const char escaper = '"';
                bool escaped = false;
                string current = "";
                for (int i = 0; i < inp.Length; i++)
                {
                    switch (inp[i])
                    {
                        case escaper:

                            if (escaped)
                            {
                                escaped = false;
                            }
                            else
                            {
                                escaped = true;
                            }
                            break;
                        case splitter:
                            if (escaped)
                            {
                                current += splitter;
                            }
                            else
                            {
                                columns.Add(current);
                                current = "";
                            }
                            break;
                        default:
                            current += inp[i];
                            break;
                    }
                }
                if (current.Length > 0) columns.Add(current);
            }
        }

        class CSVLoader
        {
            public List<CSVLine> Lines = new List<CSVLine>();
            public void LoadFile(string file)
            {
                var L = File.ReadAllLines(file);
                foreach (var l in L)
                {
                    string S = l.Trim();
                    if (S.Length > 0)
                    {
                        var CSVL = new CSVLine();
                        CSVL.Parse(S);
                        Lines.Add(CSVL);
                    }
                }
            }
        }

        private void LoadUltiboard(string bOMFile, string centroids)
        {
            Dictionary<string, BOMEntry.RefDesc> positions = new Dictionary<string, BOMEntry.RefDesc>();



            CSVLoader centroidloader = new CSVLoader();
            centroidloader.LoadFile(centroids);


            int FirstRow = -1;
            for (int i = 0; i < centroidloader.Lines.Count; i++)
            {
                if (centroidloader.Lines[i].columns[0] == "REFDES")
                {
                    FirstRow = i + 1;

                }
            }

            if (FirstRow > -1)
            {
                for (int i = FirstRow; i < centroidloader.Lines.Count; i++)
                {
                    var Line = centroidloader.Lines[i];
                    string rd = Line.columns[0];
                    double X = 0, Y = 0, Angle = 0;

                    X = Double.Parse(Line.columns[3]) / 1000000.0;
                    Y = Double.Parse(Line.columns[4]) / 1000000.0;
                    Angle = Double.Parse(Line.columns[5]);

                    BoardSide Side = BoardSide.Top;
                    if (Line.columns[6] != "TOP") Side = BoardSide.Bottom;

                    positions[rd] = new BOMEntry.RefDesc() { angle = Angle, x = X, y = Y, OriginalName = rd, NameOnBoard = rd, SourceBoard = bOMFile, Side = Side };
                }
            }


            BOMNumberSet Set = new BOMNumberSet();
            CSVLoader bomloader = new CSVLoader();
            bomloader.LoadFile(bOMFile);

            FirstRow = -1;
            for (int i = 0; i < bomloader.Lines.Count; i++)
            {
                if (bomloader.Lines[i].columns[0] == "VALUE")
                {
                    FirstRow = i + 1;

                }
            }

            if (FirstRow > -1)
            {
                for (int i = FirstRow; i < bomloader.Lines.Count; i++)
                {
                    string value = bomloader.Lines[i].columns[0];
                    string package = bomloader.Lines[i].columns[1];
                    var refdes = bomloader.Lines[i].columns[3].Split(',');
                    foreach (var rd in refdes)
                    {
                        var S = positions[rd];

                        AddBOMItemExt(package, "", value, rd, Set, bOMFile, S.x, S.y, S.angle, S.Side);
                    }
                }
            }



        }

        /*       public void WriteJLCXLS(string BaseFolder, string Name)
        {

            Workbook BOMworkbook = new Workbook();
            Worksheet BOMworksheet = new Worksheet(String.Format("{0} BOM", Name));

            

            BOMworksheet.Cells[0, 0] = new Cell("Comment");
            BOMworksheet.Cells[0, 1] = new Cell("Designator");
            BOMworksheet.Cells[0, 2] = new Cell("Footprint");
            BOMworksheet.Cells[0, 3] = new Cell("LCSC Part #");
            int currentrow = 1;
            foreach (var ds in DeviceTree)
            {

                foreach (var v in ds.Value.Values)
                {
                    BOMworksheet.Cells[currentrow, 0] = new Cell(v.Value);

                    string refdescs = v.RefDes[0].NameOnBoard;
                    for (int i = 1; i < v.RefDes.Count; i++)
                    {
                        refdescs += ", " + v.RefDes[i].NameOnBoard;
                    }

                    BOMworksheet.Cells[currentrow, 1] = new Cell(refdescs);
                    BOMworksheet.Cells[currentrow, 2] = new Cell(v.PackageName);
                    BOMworksheet.Cells[currentrow, 3] = new Cell(v.MfgPartNumber);
                    currentrow++;
                }
            }

            BOMworkbook.Worksheets.Add(BOMworksheet);
            BOMworkbook.Save(BaseFolder + "\\" + Name + "_BOM.xls");

            Workbook PNPworkbook = new Workbook();
            Worksheet PNPworksheet = new Worksheet(String.Format("{0} PnP", Name));

            PNPworksheet.Cells[0, 0] = new Cell("Designator");
            PNPworksheet.Cells[0, 1] = new Cell("Mid X");
            PNPworksheet.Cells[0, 2] = new Cell("Mid Y");
            PNPworksheet.Cells[0, 3] = new Cell("Layer");
            PNPworksheet.Cells[0, 4] = new Cell("Rotation");
            currentrow = 1;
            foreach (var ds in DeviceTree)
            {

                foreach (var v in ds.Value.Values)
                {
                    foreach (var p in v.RefDes)
                    {
                        PNPworksheet.Cells[currentrow, 0] = new Cell(p.NameOnBoard);
                        PNPworksheet.Cells[currentrow, 1] = new Cell(p.x.ToString().Replace(",",".") + "mm");
                        PNPworksheet.Cells[currentrow, 2] = new Cell(p.y.ToString().Replace(",", ".") + "mm");
                        PNPworksheet.Cells[currentrow, 3] = new Cell(p.Side == BoardSide.Top ? "T" : "B");
                        PNPworksheet.Cells[currentrow, 4] = new Cell(p.angle);

                        currentrow++;
                    }
                }
            }

            PNPworkbook.Worksheets.Add(PNPworksheet);
            PNPworkbook.Save(BaseFolder + "\\" + Name + "_PNP.xls");


        }
        */
        public void WriteJLCCSV(string BaseFolder, string Name, bool EllagePreference)
        {
            WriteJLCBom(BaseFolder, Name);

            WriteJLCPnpFile(BaseFolder, Name, EllagePreference);

        }

        public void WriteJLCBom(string BaseFolder, string Name)
        {
            List<string> outlinesBOM = new List<string>();
            List<string> OutlinesRotations = new List<string>();


            outlinesBOM.Add("Comment,Designator,Footprint,LCSC Part #,combinedname,name");
            foreach (var ds in DeviceTree)
            {

                foreach (var v in ds.Value.Values)
                {
                    SetRotationOffset(v.Combined(), GetRotationOffset(v.Combined()));

                    string refdescs = "\"" + v.RefDes[0].NameOnBoard;
                    for (int i = 1; i < v.RefDes.Count; i++)
                    {
                        refdescs += ", " + v.RefDes[i].NameOnBoard;
                    }
                    refdescs += "\"";
                    string V = v.Value;
                    if (V.Trim().Length == 0)
                    {
                        V = v.MfgPartNumber;
                    }

                    string mfg = "";
                    if (v.MfgPartNumber != null)
                    {
                        mfg = v.MfgPartNumber.Replace(',', '.');
                    }
                    string line = String.Format("{0},{1},{2},{3},{4},{5}", V, refdescs, v.PackageName, mfg, v.Combined(), v.Name);
                    outlinesBOM.Add(line);

                }
            }
            File.WriteAllLines(BaseFolder + "\\" + Name + "_BOM.csv", outlinesBOM);

            foreach (var a in RotationOffsets)
            {
                OutlinesRotations.Add(String.Format("{0} {1}", a.Key, a.Value));
            }
            OutlinesRotations.Sort();
            File.WriteAllLines(DefaultRotationFile, OutlinesRotations);

        }

        public static String EllageFormat(double inp, int digits = 2)
        {
            string fmt = "{0:F" + digits.ToString() + "}";
            return String.Format(fmt, inp).Replace(",", ".");
        }
        public static String QuantFormat(double inp, int digits = 2)
        {
            string fmt = "{0:F" + digits.ToString() + "}";
            return String.Format(fmt, inp).Replace(".", ",");
        }
        public void WriteJLCPnpFile(string BaseFolder, string Name, bool ellagepreference)
        {
            List<string> outlinesPNP = new List<string>();
            if (ellagepreference)
            {
                outlinesPNP.Add("Designator;Mid X mm;Mid Y mm;Rotation deg");
            }
            else
            {


                outlinesPNP.Add("Designator,Mid X,Mid Y,Layer,Rotation");
            }
            foreach (var ds in DeviceTree)
            {

                foreach (var v in ds.Value.Values)
                {
                    foreach (var p in v.RefDes)
                    {
                        if (ellagepreference)
                        {

                            double A = (p.angle + 360 + GetRotationOffset(v.Combined())) % 360;
                            while (A > 180) A -= 360;
                            while (A < -180) A += 360;
                            string line = String.Format("{0};{1};{2};{3}",
                            p.NameOnBoard,
                            EllageFormat(p.x,2),
                            EllageFormat(p.y,2),
                             EllageFormat(A,0));
                            outlinesPNP.Add(line);

                        }
                        else
                        {


                            string line = String.Format("{0},{1},{2},{3},{4}",
                            p.NameOnBoard,
                            p.x.ToString().Replace(",", ".") + "mm",
                            p.y.ToString().Replace(",", ".") + "mm",
                            p.Side == BoardSide.Top ? "T" : "B",
                               (p.angle + 360 + GetRotationOffset(v.Combined())) % 360);
                            outlinesPNP.Add(line);
                        }

                    }
                }
            }

            File.WriteAllLines(BaseFolder + "\\" + Name + "_PNP.csv", outlinesPNP);
        }

        public static Dictionary<string, int> RotationOffsets = new Dictionary<string, int>();
        
        public static int GetRotationOffset(string name)
        {
            if (RotationOffsets.ContainsKey(name)) return RotationOffsets[name];
            return 0;

        }
        
        public static void SetRotationOffset(string name, int off)
        {
            RotationOffsets[name] = off;
        }

        public static string DefaultRotationFile = "RotationOffsets.txt";
        
        public static void LoadRotationOffsets(string v = "")
        {
            if (v.Length == 0) v = DefaultRotationFile; else DefaultRotationFile = v;
            Console.WriteLine("Loading rotations from {0}", Path.GetFullPath(v));
            try
            {
                var L = File.ReadAllLines(v);
                foreach (var s in L)
                {
                    try
                    {
                        var items = s.Split(' ');
                        string t = items[0];
                        int rot = int.Parse(items[1]);
                        RotationOffsets[t] = rot;
                    }
                    catch (Exception)
                    {

                    }
                }
            }
            catch (Exception)
            {

            }
        }

        public string OriginalBasefolder = "";
        public string OriginalPnpName = "";
        public string OriginalBomName = "";
        
        public void LoadJLC(string bOMFile, string pnPFile)
        {
            OriginalBasefolder = Path.GetDirectoryName(bOMFile);
            OriginalPnpName = Path.GetFileNameWithoutExtension(pnPFile);
            OriginalBomName = Path.GetFileNameWithoutExtension(bOMFile);

            Dictionary<string, BOMEntry.RefDesc> positions = new Dictionary<string, BOMEntry.RefDesc>();


            var bomlines = File.ReadAllLines(bOMFile);

            var regex = new Regex("(?<=^|,)(\"(?:[^\"]|\"\")*\"|[^,]*)");

            if (pnPFile != null)
            {
                var pnplines = File.ReadAllLines(pnPFile);

                //outlinesBOM.Add("Comment,Designator,Footprint,LCSC Part #");
             
                int RefdesIdx = 0;
                int Xidx = 1;
                int Yidx = 2;
                int Sideidx = 3;
                int Angleidx = 4;

                var Headers = pnplines[0].Split(',');
                for (int i = 0; i < Headers.Count(); i++)
                {
                    var H = StripCSV(Headers[i].ToLower());
                    switch (H)
                    {
                        case "x": Xidx = i; break;
                        case "y": Yidx = i; break;
                        case "mid x": Xidx = i; break;
                        case "mid y": Yidx = i; break;
                        case "refdes": RefdesIdx = i; break;
                        case "designator": RefdesIdx = i; break;
                        case "side": Sideidx = i; break;
                        case "angle": Angleidx = i; break;
                    }
                }

                //outlinesPNP.Add("Designator,Mid X,Mid Y,Layer,Rotation");
                for (int i = 1; i < pnplines.Count(); i++)
                {
                    var s = pnplines[i];
                    List<string> items = new List<string>();
                    foreach (Match m in regex.Matches(s))
                    {
                        items.Add(m.Value);
                    }

                    var rd = StripCSV(items[RefdesIdx]);
                    var X = ConvertDimension(items[Xidx]);
                    var Y = ConvertDimension(items[Yidx]);
                    var Side = GetSide(StripCSV(items[Sideidx]));
                    var Angle = Double.Parse(StripCSV(items[Angleidx]));

                    positions[rd] = new BOMEntry.RefDesc() { angle = Angle, x = X, y = Y, OriginalName = rd, NameOnBoard = rd, SourceBoard = bOMFile, Side = Side };
                }
            }
            BOMNumberSet Set = new BOMNumberSet();
            var headers = bomlines[0].Split(',');

            for (int i = 1; i < bomlines.Count(); i++)
            {
                var s = bomlines[i];
                List<string> items = new List<string>();
                foreach (Match m in regex.Matches(s))
                {
                    items.Add(StripCSV(m.Value.Trim()));
                }

                var refdesc = items[1].Trim(); ;
                if (refdesc.StartsWith("\""))
                {
                    refdesc = refdesc.Substring(1, refdesc.Length - 2);
                }
                var rd = refdesc.Split(',');

                var value = items[0];
                var package = items[2];


                foreach (var rd_ in rd)
                {
                    if (positions.ContainsKey(rd_.Trim()))
                    {
                        var S = positions[rd_.Trim()];

                        if (headers.Count() > 5)
                        {
                            OptionalOut<BOMEntry> Entr = new OptionalOut<BOMEntry>(); ;
                            string name = items[5];
                            string combined = items[4];
                            AddBOMItemInt(package, name, value, rd_.Trim(), Set, bOMFile, S.x, S.y, S.angle, S.Side, Entr);
                            Entr.Result.SetCombined(combined);
                            //AddBOMItemExt(package, name, value, rd_, Set, bOMFile, S.x, S.y, S.angle, S.Side);

                        }
                        else
                        {
                            AddBOMItemExt(package, "", value, rd_.Trim(), Set, bOMFile, S.x, S.y, S.angle, S.Side);
                        }
                    }
                    else
                    {
                        Console.WriteLine("wtf!");
                    }

                }


            }

        }

        private string StripCSV(string H)
        {
            if (H.Length > 2 && H[0] == '"' && H[H.Length - 1] == '"')
            {
                return H.Substring(1, H.Length - 2);
            }
            return H;
        }

        private BoardSide GetSide(string v)
        {
            switch (v.ToLower())
            {
                case "t":
                case "top":
                    return BoardSide.Top;
                case "b":
                case "bottom":
                    return BoardSide.Bottom;
            }
            return BoardSide.Unknown;
        }

        private double ConvertDimension(string v)
        {
            v = v.Trim();
            if (v.EndsWith("mm"))
            {
                v = v.Substring(0, v.Length - 2).Trim(); ;

            }
            if (v[0] == '"' && v[v.Length -1] == '"')
            {
                v = v.Substring(1, v.Length - 2);
            }
            return double.Parse(v.Replace('.', ','));
        }

        public int GetSolderedPartCount(List<string> toIgnore)
        {
            int partcount = 0;
            foreach (var a in DeviceTree)
            {
                //Console.WriteLine(a.Key);
                foreach (var b in a.Value)
                {
                    if (toIgnore.Contains(b.Value.PackageName) == false)
                    {
                        if (b.Value.Soldered == true) partcount += b.Value.RefDes.Count;
                    }
                }
            }

            return partcount;
        }

        public BOMEntry.RefDesc GetRefDes(string rdi)
        {
            foreach (var t in DeviceTree)
            {
                foreach (var b in t.Value.Values)
                {
                    foreach (var rd in b.RefDes)
                    {
                        if (rd.NameOnBoard == rdi)
                        {
                            return rd;
                        }
                    }
                }
            }
            return null;
        }

        public BOMEntry GetBOMEntry(string rdi)
        {
            foreach (var t in DeviceTree)
            {
                foreach (var b in t.Value.Values)
                {
                    foreach (var rd in b.RefDes)
                    {
                        if (rd.NameOnBoard == rdi)
                        {
                            return b;
                        }
                    }
                }
            }
            return null;
        }

        public void FixupAngles(StockDocument doc)
        {
            foreach (var t in DeviceTree)
            {
                foreach (var b in t.Value.Values)
                {
                    var partname = b.Combined();
                    var p = doc.FindPart(partname);
                    bool symmetric = false;
                    if (p != null) symmetric = p.IsPolarized ? false : true;
                    foreach (var rd in b.RefDes)
                    {
                        if (rd.angle > 180)
                        {
                            rd.angle -= 360;
                        }
                        if (symmetric)
                        {
                            rd.angle = (rd.angle + 180.0 * 10.0) % 180;
                            if (rd.angle >= 90) rd.angle -= 180;
                        }

                    }
                }
            }

        }

        public void SwapXY()
        {
            foreach (var t in DeviceTree)
            {
                foreach (var b in t.Value.Values)
                {
                    foreach (var rd in b.RefDes)
                    {
                        var tt = rd.x;
                        rd.x = rd.y;
                        rd.y = tt;
                        rd.angle = 45 + (45 - rd.angle);
                    }
                }
            }
        }

        public void FlipX()
        {
            foreach (var t in DeviceTree)
            {
                foreach (var b in t.Value.Values)
                {
                    foreach (var rd in b.RefDes)
                    {
                        
                        rd.x = -rd.x;
                        rd.angle = -rd.angle;
                    }
                }
            }
        }

        public void FlipSides()
        {
            foreach (var t in DeviceTree)
            {
                foreach (var b in t.Value.Values)
                {
                    foreach (var rd in b.RefDes)
                    {
                        switch (rd.Side)
                        {
                            case BoardSide.Bottom:
                                rd.Side = BoardSide.Top;
                                break;
                            case BoardSide.Top:
                                rd.Side = BoardSide.Bottom;
                                break;
                        }
                    }
                }
            }
        }

        public static void RenderPackage(Graphics g, double x, double y, double angle, string packageName, BoardSide side)
        {


            var t = g.Transform.Clone();
            g.TranslateTransform((float)x, (float)y);
            if (side == BoardSide.Bottom)
            {
                g.MultiplyTransform(new System.Drawing.Drawing2D.Matrix(-1, 0, 0, 1, 0, 0));
                g.RotateTransform((float)angle);
            }
            else
            {

                g.RotateTransform((float)-angle);
            }

            switch(packageName.Trim())
            {
                case "3MMLED":
                    RenderLed(g);
                    break;
                case "TC33X":
                case "MINITRIM":
                    RenderTrimPot(g);
                    break;
                case "SOT363":
                    RenderSOT363(g);
                    break;
                case "SOT-223":
                case "SOT223":
                case "SOT223-4":
                    RenderSOT223(g);
                    break;
                case "POL3528":
                case "CASE-A_3216":
                    RenderTantalum(g);
                    break;

                case "SOT-323-ZENER":
                case "SOT-323":
                case "SOT323":
                    RenderSot323(g);
                    break;

                case "SOT23-BEC":
                case "SOT-23":
                case "SOT23":
                    RenderSot23(g);
                    break;

                case "DO214AC":
                case "DO-214AC":
                    RenderDO214Diode(g);
                    break;
                case "CAPAE-5.3x5.3h5.4":
                    RenderECap(g, 5.3f);
                    break;
                case "C5B4.5":
                case "CAP-5.08/7.6x5":
                    RenderBigCap(g,7, 4.0f, Color.FromArgb(200, 60, 60));
                    
                    break;

                case "RES_2512":
                    RenderSMD2Pin(g, 25, 12, Color.FromArgb(60, 60, 60));                    
                    break;
                case "R0603":
                case "0603":
                    RenderSMD2Pin(g, 6, 3, Color.FromArgb(60, 60, 60));
                    break;
                case "L0805":
                    RenderSMD2Pin(g, 8, 5, Color.FromArgb(60, 70, 80));
                    break;
                case "0402":
                    RenderSMD2Pin(g, 4, 2, Color.FromArgb(160, 130, 60));
                    break;
                case "C0603":
                    RenderSMD2Pin(g, 6, 3, Color.FromArgb(160, 130, 60));
                    break;
                case "C3225":
                    RenderSMD2Pin(g, 32*10/25.4f, 25*10/25.4f, Color.FromArgb(160, 130, 60));
                    break;
                case "FERRITE_1812":
                    RenderSMD2Pin(g, 18, 12, Color.FromArgb(20, 20, 20));
                    break;
                case "RES_0805":
                case "R0805":
                case "0805":
                    RenderSMD2Pin(g, 8, 5, Color.FromArgb(60, 60, 60));
                    break;
                case "C0805":
                    RenderSMD2Pin(g, 8, 5, Color.FromArgb(160, 130, 60));
                    break;
                case "SO14":
                case "SOIC14":
                case "SOIC-14/150mil":
                    RenderSOIC(g, 14);
                    break;
                case "SO16":
                case "SOIC16":
                case "SOIC-16/150mil":
                    RenderSOIC(g, 16);
                    break;
                case "SO8":
                case "SOIC-8/150mil":
                case "SO08":
                case "SOIC8-N_MC":
                case "SOIC8":
                    RenderSOIC(g, 8);
                    break;
                default:
                    Console.WriteLine("unknown package: {0}", packageName);
                    break;
                case "TSSOP14":
                case "TSSOP-14":
                    RenderSSOP(g, 14);
                    break;
                case "TSSOP8":
                case "TSSOP-8":
                    RenderSSOP(g, 8);
                    break;
                case "UMAX8":
                    RenderSSOP(g, 8,3.0f);
                    break;
                case "TSSOP16":
                case "TSSOP-16":
                    RenderSSOP(g, 16);
                    break;
                case "SSOP24":
                    RenderSSOP(g, 24);
                    break;
                case "PANASONIC_C":
                    RenderECap(g, 5.3f);                    
                    break;
                case "CASE_B":
                    RenderECap(g, 4.3f);
                    break;
                case "CAPAE-6.6x6.6h5.4":
                case "PANASONIC_D":
                    RenderECap(g, 6);
                    break;
                case "LMZM33606":
                    RenderQFN(g, 10.15f, 16.15f, 9,15, 0.8f);
                    break;
                case "QFN-32/5x5x0.5":
                    RenderQFN(g, 5, 5, 8, 8, 0.5f);
                    break;
                case "QFN50P400X400X100-25N":
                    RenderQFN(g, 4,4,6,6, 0.5f);
                    break;
                case "PTV09A":
                case "ALPS_POT_SQUAREHOLES":
                    RenderPot(g);
                    break;
                case "PB6149L":
                    RenderButton(g);
                    break;
                case "TOOLINGHOLE":
                case "SWDPADS":
                case "SQUAREFIDUCIAL":
                case "JUSTATINYROCKET_DATECODE":
                case "FENIXPOWERBACKPACK_BOARD":
                case "FENIXPOWERBACKPACK":
                case "POWERTESTPAD_MEDIUM":
                case "POWERTESTPAD":
                case "POWERTESTPAD_LARGE":
                case "GROUNDLOOP":
                case "MINISWD-SMD":
                case "IPSLCD1.3":
                case "FRONTPANELSTANDOFF_SCREWONONESIDE":
                case "3":
                case "":
                    break;
                case "3.5MM-JACK-SWITCH-13MM-NOHOLES":
                    RenderJack(g);
                        break;
                case "TS-23E01":
                    RenderToggleSwitch(g);
                    break;
                case "TRIMPOT":
                    RenderBigTrim(g);
                    break;

                case "SOT23-5":
                case "SOT-23-5":
                    RenderSot23_5(g);
                    break;
                case "HDR-1x2T/2.54/5x2":
                    RenderPinHeader(g, 2, 1, 2.54f);
                    break;
                case "M50-3600542":
                    RenderPinHeader(g, 5, 2, 1.27f);
                    break;
                case "2X3-SHROUDED":
                    RenderPinHeader(g, 2, 3, 2.54f, true);
                    break;
                case "SPU0410HR5H-PB":
                    DrawMic(g);
                    break;
            }

            g.Transform = t;
        }

        private static void DrawMic(Graphics g)
        {
            float L = 2.95f;
            float W = 3.76f;

            g.FillRectangle(new SolidBrush(Color.FromArgb(200,180, 20)), -L / 2, -W / 2, L, W);

            g.FillEllipse(new SolidBrush(Color.Black), new RectangleF(-0.25f, W / 2 - 1.184f + 0.25f, 0.5f, 0.5f));

        }

        private static void RenderPinHeader(Graphics g, int wpin, int hpin, float pinspacing, bool shroud = false)
        {
            float L = (wpin ) * pinspacing;
            float W = (hpin ) * pinspacing; ;

            g.FillRectangle(new SolidBrush(Color.FromArgb(30, 30, 30)), -L / 2, -W / 2, L, W);


            
            RectangleF pinrect = new RectangleF(0, 0, 0.4f, 0.4f);
            float woffs = (wpin/ 2);
            if ((wpin% 2) != 1) woffs -= 0.5f;

            float hoffs = (hpin / 2);
            if ((hpin % 2) != 1) hoffs -= 0.5f;

            float pinw = 0.6f;
            for (int p = 0; p < wpin; p++)
            {
                for (int q = 0; q < hpin; q++)
                {
                    float x = (p - woffs) * pinspacing - pinw /2; 
                    float y = (q - hoffs) * pinspacing - pinw / 2; 
                    g.FillRectangle(new SolidBrush(Color.FromArgb(160, 160, 130)), x,y,pinw,pinw);
                }

            }
            W += 5.45f;
            L += 0.05f+pinspacing;
            if (shroud)
            {
                g.DrawRectangle(new Pen(Color.FromArgb(40, 40, 40),1.4f), -L / 2, -W / 2, L, W);

            }
        }
        private static void RenderSot23_5(Graphics g)
        {
            float L = 3;
            float W = 1.4f;

            g.FillRectangle(new SolidBrush(Color.FromArgb(60, 60, 60)), -L / 2, -W / 2, L, W);

            float pinspacing = 0.8f;
            int sidepins = 6 / 2;
            RectangleF pinrect = new RectangleF(0, 0, 0.4f, 0.4f);
            float offs = (sidepins / 2);
            if ((sidepins % 2) != 1) offs -= 0.5f;

            List<bool> top = new List<bool>() { true, true, true};
            List<bool> bottom = new List<bool>() { true, false, true };

            for (int p = 0; p < sidepins; p++)
            {
                pinrect.X = (p - offs) * pinspacing - 0.2f;
                pinrect.Y = -1.25f;
                if (top[p]) g.FillRectangle(new SolidBrush(Color.FromArgb(160, 160, 160)), pinrect);

                pinrect.Y = 1.25f - 0.5f;
                if (bottom[p]) g.FillRectangle(new SolidBrush(Color.FromArgb(160, 160, 160)), pinrect);
            }

            pinrect.X = (0 - offs) * pinspacing - pinrect.Width/2;
            pinrect.Y = -0.4f;

            g.FillEllipse(new SolidBrush(Color.FromArgb(190, 190, 190)), pinrect);
        }

        private static void RenderBigTrim(Graphics g)
        {
            float W = 2.54f*4;
            float H = 2.54f*2;
            g.FillRectangle(new SolidBrush(Color.FromArgb(20, 20, 160)), -W / 2, -H / 2, W, H);
            float L = 2.54f;
            g.FillEllipse(new SolidBrush(Color.FromArgb(145, 145, 10)), -W / 2 , H / 2 - L, L, L);

            g.FillRectangle(new SolidBrush(Color.FromArgb(120, 120, 0)), -W / 2, H /2 - L + L/2 -0.1f, L, 0.2f);
        }

        private static void RenderToggleSwitch(Graphics g)
        {
            float W = 23;
            float H = 11;
            g.FillRectangle(new SolidBrush(Color.FromArgb(130, 130, 130)), -W / 2, -H / 2, W, H);
            float L = 5.0f;
            g.FillEllipse(new SolidBrush(Color.FromArgb(145, 145, 145)), -L / 2, -L / 2, L, L);
        }

        private static void RenderJack(Graphics g)
        {
            float W = 10;
            float H = 10;
            g.FillRectangle(new SolidBrush(Color.FromArgb(30, 30, 30)), -W / 2, -H / 2, W, H);
            float L = 9.5f;
            g.FillEllipse(new SolidBrush(Color.FromArgb(35, 35, 35)), -L / 2, -L / 2, L, L);
            L = 3.5f;
            g.FillEllipse(new SolidBrush(Color.FromArgb(2, 2, 2)), -L / 2, -L / 2, L, L);
        }

        private static void RenderButton(Graphics g)
        {
            float W = 6;
            float H = 6;
            g.FillRectangle(new SolidBrush(Color.FromArgb(120, 120, 120)), -W / 2, -H / 2, W, H);
            float L = 6;
            g.FillEllipse(new SolidBrush(Color.FromArgb(200, 180, 0)), -L / 2, -L / 2, L, L);

        }

        private static void RenderPot(Graphics g)
        {
            float W = 9;
            float H = 9;
            g.FillRectangle(new SolidBrush(Color.FromArgb(120, 120, 120)), -W / 2, -H / 2, W, H);
        }

        private static void RenderQFN(Graphics g, float W, float H, int wpins, int hpins, float spacingbetweenpins)
        {

            g.FillRectangle(new SolidBrush(Color.FromArgb(50, 50, 50)), -W / 2, -H / 2, W, H);

            float woffs = (wpins/ 2);
            if ((wpins % 2) != 1) woffs -= 0.5f;
            float hoffs = (hpins / 2);
            if ((hpins % 2) != 1) hoffs -= 0.5f;
            RectangleF hpin = new RectangleF(0, 0, spacingbetweenpins * 0.8f, spacingbetweenpins * 0.2f);
            RectangleF vpin = new RectangleF(0, 0, spacingbetweenpins * 0.2f, spacingbetweenpins * 0.8f);
            SolidBrush b = new SolidBrush(Color.FromArgb(160, 160, 160));
            for (int p = 0; p < wpins; p++)
            {
                hpin.X = (p - woffs) * spacingbetweenpins - hpin.Width/2;

                hpin.Y = -H / 2 - hpin.Height;
                g.FillRectangle(b, hpin);
                hpin.Y = H / 2;
                g.FillRectangle(b, hpin);


            }
            for (int p = 0; p < hpins; p++)
            {

                vpin.Y = (p - hoffs) * spacingbetweenpins - vpin.Height / 2;
            vpin.X = -W / 2 - vpin.Width;
            g.FillRectangle(b, vpin);
            vpin.X = W / 2;
            g.FillRectangle(b, vpin);

        }

        float dotsize = 0.5f;
            RectangleF dot = new RectangleF();
            dot.X = -W / 2 + 0.2f;
            dot.Y = H / 2 - 0.2f - dotsize; ;
            dot.Width = dotsize;
            dot.Height = dotsize;

            g.FillEllipse(new SolidBrush(Color.FromArgb(190, 190, 190)), dot);


        }
        public static void RenderECap(Graphics g, float candiameter)
        {
            float L = candiameter*0.9f;
            float L2 = L*1.1f;
            float L23 = L2 / 3;
            List<PointF> polyvert = new List<PointF>();

            polyvert.Add(new PointF( L2 / 2, L23 / 2));
            polyvert.Add(new PointF( L23 / 2, L2/2));
            polyvert.Add(new PointF( -L2  / 2, L2 / 2));
            polyvert.Add(new PointF( -L2  / 2, -L2 / 2));
            polyvert.Add(new PointF( L23 / 2, -L2 / 2));
            polyvert.Add(new PointF( L2 / 2, -L23 / 2));

            g.FillPolygon(new SolidBrush(Color.FromArgb(76,80,85)), polyvert.ToArray());
            g.FillEllipse(new SolidBrush(Color.FromArgb(110, 110, 110)), -L / 2, -L / 2, L, L);

        }
        private static void RenderDO214Diode(Graphics g)
        {
            RenderSMD2Pin(g, 3.2f * 100 / 25.4f, 1.6f * 100 / 25.4f, Color.FromArgb(60, 60, 60), Color.FromArgb(180, 180, 180), Color.FromArgb(60, 60, 60));
        }

        private static void RenderLed(Graphics g)
        {
            float L = 3;
            g.FillEllipse(new SolidBrush(Color.FromArgb(200, 180, 0)), -L / 2, -L / 2, L, L);
        }

        private static void RenderTrimPot(Graphics g)
        {
            float L = 3.0f;
            float W = 3.8f;
            float extendH = 4.1f / 2;
            g.FillRectangle(new SolidBrush(Color.FromArgb(120, 120, 120)), -L / 2, -W / 2, L, W);

            g.FillEllipse(new SolidBrush(Color.FromArgb(110, 110, 110)), -L / 2, -L / 2, L, L);

            g.FillRectangle(new SolidBrush(Color.FromArgb(90, 90, 90)), -L*0.6f / 2, -L * 0.1f / 2, L * 0.6f, L * 0.1f);
            g.FillRectangle(new SolidBrush(Color.FromArgb(90, 90, 90)), -L * 0.1f / 2, -L * 0.6f / 2, L * 0.1f, L * 0.6f);

            float pinspacing = 0.9f;
            int sidepins = 6 / 2;
            RectangleF pinrect = new RectangleF(0, 0, 0.65f, 0.1f);
            RectangleF pinrectpad = new RectangleF(0, 0,0.65f, 0.1f);
            float offs = (sidepins / 2);
            if ((sidepins % 2) != 1) offs -= 0.5f;

            List<bool> bottom = new List<bool>() { false, true, false };
            List<bool> top = new List<bool>() { true, false, true };

            for (int p = 0; p < sidepins; p++)
            {
                pinrect.X = (p - offs) * pinspacing - 0.35f;
                pinrect.Y = -extendH;
                if (top[p]) g.FillRectangle(new SolidBrush(Color.FromArgb(160, 160, 160)), pinrect);

                pinrectpad.X = (p - offs) * pinspacing - 0.35f;
                pinrectpad.Y = extendH - pinrect.Height;
                if (bottom[p]) g.FillRectangle(new SolidBrush(Color.FromArgb(160, 160, 160)), pinrectpad);
            }
        }

        private static void RenderSOT363(Graphics g)
        {
            float L = 2;
            float W = 1.25f;
            float extendH = 2.1f / 2;
            g.FillRectangle(new SolidBrush(Color.FromArgb(60, 60, 60)), -L / 2, -W / 2, L, W);

            float pinspacing = 0.65f;
            int sidepins = 6 / 2;
            RectangleF pinrect = new RectangleF(0, 0, 0.2f, extendH-W / 2);
            float offs = (sidepins / 2);
            if ((sidepins % 2) != 1) offs -= 0.5f;

            List<bool> bottom = new List<bool>() { true , true, true };
            List<bool> top = new List<bool>() { true, true, true };

            for (int p = 0; p < sidepins; p++)
            {
                pinrect.X = (p - offs) * pinspacing - pinrect.Width/2;
                pinrect.Y = -extendH;
                if (top[p]) g.FillRectangle(new SolidBrush(Color.FromArgb(160, 160, 160)), pinrect);

                pinrect.Y = extendH - pinrect.Height;
                if (bottom[p]) g.FillRectangle(new SolidBrush(Color.FromArgb(160, 160, 160)), pinrect);
            }

            pinrect.Width = 0.3f;
            pinrect.Height = 0.3f;
            pinrect.X = (0 - offs) * pinspacing - pinrect.Width / 2;
            pinrect.Y = -0.4f;
            

            g.FillEllipse(new SolidBrush(Color.FromArgb(190, 190, 190)), pinrect);
        }

        private static void RenderSOT223(Graphics g)
        {
            float L = 6.5f;
            float W = 3.5f;

            g.FillRectangle(new SolidBrush(Color.FromArgb(60, 60, 60)), -L / 2, -W / 2, L, W);

            float pinspacing = 2.3f;
            int sidepins = 6 / 2;
            RectangleF pinrect = new RectangleF(0, 0, 0.7f, 1.75f);
            RectangleF pinrectpad = new RectangleF(0, 0, 3, 1.75f);
            float offs = (sidepins / 2);
            if ((sidepins % 2) != 1) offs -= 0.5f;

            List<bool> bottom = new List<bool>() { false, true, false };
            List<bool> top = new List<bool>() { true, true, true };

            for (int p = 0; p < sidepins; p++)
            {
                pinrect.X = (p - offs) * pinspacing - 0.35f;
                pinrect.Y = -3.5f;
                if (top[p]) g.FillRectangle(new SolidBrush(Color.FromArgb(160, 160, 160)), pinrect);

                pinrectpad.X = (p - offs) * pinspacing - 1.5f;
                pinrectpad.Y = 3.5f - 1.75f;
                if (bottom[p]) g.FillRectangle(new SolidBrush(Color.FromArgb(160, 160, 160)), pinrectpad);
            }

        }

        private static void RenderSot23(Graphics g)
        {
            float L = 3;
            float W = 1.4f;

            g.FillRectangle(new SolidBrush(Color.FromArgb(60, 60, 60)), -L / 2, -W / 2, L, W);

            float pinspacing = 0.8f;
            int sidepins = 6 / 2;
            RectangleF pinrect = new RectangleF(0, 0, 0.4f, 0.4f);
            float offs = (sidepins / 2);
            if ((sidepins % 2) != 1) offs -= 0.5f;

            List<bool> bottom = new List<bool>() { false, true, false };
            List<bool> top = new List<bool>() { true, false, true };

            for (int p = 0; p < sidepins; p++)
            {
                pinrect.X = (p - offs) * pinspacing - 0.2f;
                pinrect.Y = -1.25f;
                if (top[p]) g.FillRectangle(new SolidBrush(Color.FromArgb(160, 160, 160)), pinrect);

                pinrect.Y = 1.25f - 0.5f;
                if (bottom[p]) g.FillRectangle(new SolidBrush(Color.FromArgb(160, 160, 160)), pinrect);
            }

        }

        private static void RenderTantalum(Graphics g)
        {
            RenderSMD2Pin(g, 3.2f * 100 / 25.4f, 1.6f* 100 / 25.4f, Color.FromArgb(160,100,30), Color.FromArgb(160,10,10));
        }

        private static void RenderSot323(Graphics g)
        {
            float L = 2;
            float W = 1.25f;

            g.FillRectangle(new SolidBrush(Color.FromArgb(60,60,60)), -L / 2, -W / 2, L, W);

            float pinspacing = 0.65f;
            int sidepins = 6 / 2;
            RectangleF pinrect = new RectangleF(0, 0, 0.4f, 0.4f);
            float offs = (sidepins / 2);
            if ((sidepins % 2) != 1) offs -= 0.5f;

            List<bool> bottom = new List<bool>() { false, true, false };
            List<bool> top = new List<bool>() {true, false, true };

            for (int p = 0; p < sidepins; p++)
            {
                pinrect.X = (p - offs) * pinspacing - 0.2f;
                pinrect.Y = -1;
                if (top[p]) g.FillRectangle(new SolidBrush(Color.FromArgb(160, 160, 160)), pinrect);

                pinrect.Y = 1-0.5f;
                if (bottom[p]) g.FillRectangle(new SolidBrush(Color.FromArgb(160, 160, 160)), pinrect);
            }


        }

        private static void RenderBigCap(Graphics g, float L, float W, Color color)
        {
            g.FillRectangle(new SolidBrush(color), -L/ 2 , -W / 2, L,W);

        }

        public static void RenderSMD2Pin(Graphics g, float w, float h, Color BodyColor, Color ?pin1mark = null, Color? pin2mark = null)
        {
            w *=  25.4f/100.0f;
            h *=  25.4f/100.0f;

            g.FillRectangle(new SolidBrush(Color.FromArgb(160, 160, 160)), -w / 2, -h / 2, w ,h);
            g.FillRectangle(new SolidBrush(BodyColor), -(w*0.8f) / 2, -h / 2, (w*0.8f), h);

            if (pin1mark !=null)
            {
                g.FillRectangle(new SolidBrush((Color)pin1mark), (-w * 0.8f) / 2, -h / 2, (w * 0.2f), h);
            }

            if (pin2mark != null)
            {
                g.FillRectangle(new SolidBrush((Color)pin2mark), (w * 0.4f) / 2, -h / 2, (w * 0.2f), h);
            }

        }
        public static void RenderSOIC(Graphics g, int v)
        {

            int sidepins = v / 2;
            float length = (sidepins - 1) * 1.27f;
            g.FillRectangle(new SolidBrush(Color.FromArgb(60, 60, 60)), -length / 2 - 0.5f, -3.9f / 2, length + 1, 3.9f);
            RectangleF pinrect = new RectangleF(0,0,0.4f,1);
            float offs = (sidepins / 2) ;
            if ((sidepins % 2) != 1) offs -= 0.5f;
            
            for(int p = 0;p<sidepins ;p++)
            {
                pinrect.X = (p - offs)  * 1.27f - 0.2f;
                pinrect.Y = -3;
                g.FillRectangle(new SolidBrush(Color.FromArgb(160, 160, 160)), pinrect);

                pinrect.Y = 2;
                g.FillRectangle(new SolidBrush(Color.FromArgb(160, 160, 160)), pinrect);
            }
            pinrect.X = (0 - offs) * 1.27f - 0.2f;
            pinrect.Y = -1.4f;
            pinrect.Width = 0.4f;
            pinrect.Height = 0.4f;

            g.FillEllipse(new SolidBrush(Color.FromArgb(190, 190, 190)), pinrect);

        }

        public static void RenderSSOP(Graphics g, int v, float W = 3.9f)
        {

            float pinspacing = 0.65f;
            int sidepins = v / 2;
            float length = (sidepins - 1) * pinspacing;
            

            g.FillRectangle(new SolidBrush(Color.FromArgb(60, 60, 60)), -length / 2 - 0.5f, -W / 2, length + 1, W);
            RectangleF pinrect = new RectangleF(0, 0, 0.4f, 1);
            float offs = (sidepins / 2);
            if ((sidepins % 2) != 1) offs -= 0.5f;

            for (int p = 0; p < sidepins; p++)
            {
                pinrect.X = (p - offs) * pinspacing - 0.2f;
                pinrect.Y = -(W/2)-pinrect.Height;
                g.FillRectangle(new SolidBrush(Color.FromArgb(160, 160, 160)), pinrect);

                pinrect.Y = (W / 2 );
                g.FillRectangle(new SolidBrush(Color.FromArgb(160, 160, 160)), pinrect);
            }

            pinrect.X = (0 - offs) * pinspacing - 0.2f;
            pinrect.Y = -1.4f;
            pinrect.Width = 0.4f;
            pinrect.Height = 0.4f;

            g.FillEllipse(new SolidBrush(Color.FromArgb(190, 190, 190)), pinrect);

        }

        public void WriteQuantPnPFiles(string BaseFolder, string Name)
        {
            List<string> outlinesPNP = new List<string>();


            outlinesPNP.Add("Variant:    No variations");
            outlinesPNP.Add("Units used: mm");
            outlinesPNP.Add("");
            outlinesPNP.Add("\"Designator,\"\"Comment\"\",\"\"Layer\"\",\"\"Footprint\"\",\"\"Center-X(mm)\"\",\"\"Center-Y(mm)\"\",\"\"Rotation\"\",\"\"Part Number\"\",\"\"Description\"\",\"\"Variation\"\",\"\"Mounting\"\"\"");;
            
            foreach (var ds in DeviceTree)
            {

                foreach (var v in ds.Value.Values)
                {
                    foreach (var p in v.RefDes)
                    {

                            double A = (p.angle + 360 + GetRotationOffset(v.Combined())) % 360;
                            while (A > 180) A -= 360;
                            while (A < -180) A += 360;
                            string line = String.Format("\"{0}\",\"\"{1}\"\",\"\"{2}\"\",\"\"{3}\"\",\"\"{4}\"\",\"\"{5}\"\",\"\"{6}\"\",\"\"{7}\"\",\"\"{8}\"\",\"\"{9}\"\",\"\"{10}\"\"\"",
                            p.NameOnBoard,
                            v.Value, 
                            p.Side== BoardSide.Top?"TopLayer":"BottomLayer",
                            v.PackageName,
                            QuantFormat(p.x, 4),
                            QuantFormat(p.y, 4),
                            QuantFormat(A, 0),                              
                             v.CombinedName,
                             v.Value,
                             "",
                             QuantMounting(v.Mounting));

                            outlinesPNP.Add(line);

                      

                    }
                }
            }

            File.WriteAllLines(BaseFolder + "\\" + Name + "_PNP.csv", outlinesPNP);
        }

        private string QuantMounting(MountingType mounting)
        {
        switch(mounting)
            {
                case MountingType.Meta: return "";
                case MountingType.SMD: return "SMD";
                case MountingType.Throughhole: return "TH";
            }
            return "";
        }

        public void UpdateMountingTypesFromStock(StockDocument stockDoc)
        {
            foreach (var ds in DeviceTree)
            {

                foreach (var v in ds.Value.Values)
                {
                    foreach (var p in stockDoc.Parts)
                    {
                       if ( p.name == v.Combined())
                        {
                            v.Mounting = p.Mounting;
                        }
                    }
                }
            }
        }

        public void WriteQuantBOMFile(string BaseFolder, string Name)
        {
            List<string> outlinesBOM = new List<string>();

            outlinesBOM.Add("Designator,Comment,Characteristic,Outline,Manufacturer + Part Number,Fitted,Quantity,Mounting");

            foreach (var ds in DeviceTree)
            {

                foreach (var v in ds.Value.Values)
                {
                    SetRotationOffset(v.Combined(), GetRotationOffset(v.Combined()));

                    string refdescs = "\"" + v.RefDes[0].NameOnBoard;
                    for (int i = 1; i < v.RefDes.Count; i++)
                    {
                        refdescs += ", " + v.RefDes[i].NameOnBoard;
                    }
                    refdescs += "\"";
                    string V = v.Value;
                    if (V.Trim().Length == 0)
                    {
                        V = v.MfgPartNumber;
                    }

                    string mfg = "";
                    if (v.MfgPartNumber != null)
                    {
                        mfg = v.MfgPartNumber.Replace(',', '.');
                    }
                    string line = String.Format("{0},{1},{2},{3},{4},{5},{6},{7}", refdescs, V,v.Value, v.PackageName, mfg,"Fitted", v.RefDes.Count.ToString(), v.Mounting);
                    outlinesBOM.Add(line);

                }
            }
            File.WriteAllLines(BaseFolder + "\\" + Name + "_QUANT_BOM.csv", outlinesBOM);

          
        }

        public void LoadKicad(string kicadschname, string kicadpcbname, StandardConsoleLog Log)
        {
                Log.PushActivity("KiCAD BOM Loader");

                Log.AddString(String.Format("Loading Kicad board files for: {0}", Path.GetFileNameWithoutExtension(kicadschname)));

                string S = System.IO.File.ReadAllText(kicadschname);
                S = S.Replace("\n", "");
                string S2 = System.IO.File.ReadAllText(kicadpcbname);
                S2 = S2.Replace("\n", "");
                var SchRoot = new SNode();
                SchRoot.Load(S);
                 var PCBRoot = new SNode();
                PCBRoot.Load(S2);

                float originx = 0, originy= 0, UnitMult = 1;
                var pcb = PCBRoot.Find("kicad_pcb");
                float AngleMult = 1.0f;

                var C = pcb[0].Find("footprint");

                foreach (var a in C)
                {
                    BoardSide side = BoardSide.Top;
                    var AT = a.Find("at");
                    var FP = a.Children[0].Name;

                if (FP.Contains(":"))
                {
                    FP = FP.Split(':').Last();
                }
                    int layerdesc = 1;
                string siden = "";
                for (int i =0;i<a.Children.Count();i++)
                {
                    if (a.Children[i].Name == "layer")
                    {
                            siden = a.Children[i].Children[0].Name;
                    }
                }
                    if (a.Children[1].Name == "locked") layerdesc++;
                   
                    if (siden != "F.Cu") side = BoardSide.Bottom;
                    var fp_text = a.Find("fp_text");
                    bool skip = false;
                    var attr = a.Find("attr");
                    float Angle = 0;
                    float X = float.Parse(AT[0].Children[0].Name.Replace('.', ','));
                    float Y = float.Parse(AT[0].Children[1].Name.Replace('.', ','));
                    if (AT[0].Children.Count > 2)
                    {
                        Angle = float.Parse(AT[0].Children[2].Name.Replace('.', ','));
                    }
                    string reference = "";
                    string value = "";
                foreach (var t in attr)
                {
                    foreach (var c in t.Children)
                    {
                        if (c.Name == "exclude_from_pos_files") skip = true;
                        if (c.Name == "exclude_from_bom") skip = true;
                    }
                    


                }
                foreach (var t in fp_text)
                    {
                        if (t.Children[0].Name == "reference")
                        {
                            reference = t.Children[1].Name;
                        }
                        if (t.Children[0].Name == "value")
                        {
                            value= t.Children[1].Name;
                        }

                    }

                    Console.WriteLine("{0} {1} {2} {3}", reference, value, X, Y, Angle);
                if (!skip)
                {
                    var placement = AddBOMItemExt(FP, FP, value, reference, null, System.IO.Path.GetFileNameWithoutExtension(kicadschname), X, Y, Angle, side);
                    //    var BE = GetBOMEntry(placement);
                    //    float DipComp = 0;// DipCompensationAngle(BE.Combined(), side);

                    //    // orientangle += DipComp;
                    var RD = GetRefDes(placement);
                }
                //    DipRotateAndAdjust(x, y, partoriginx, partoriginy, angle, orientidx, partorientidx, Flipped, RD, originx, originy, UnitMult);
                //    if (side == BoardSide.Bottom) RD.angle = -RD.angle;
                //    RD.angle += DipComp;


                //float partoriginx = 0;
                //float partoriginy = 0;
                //var pox = a.Find("OriginX");
                //var poy = a.Find("OriginY");
                //if (pox.Count > 0)
                //{
                //    partoriginx = float.Parse(pox.First().Children[0].Name.Replace('.', ','));
                //}
                //if (poy.Count > 0)
                //{
                //    partoriginy = float.Parse(poy.First().Children[0].Name.Replace('.', ','));
                //}


                //float x = -1;
                //float y = -1;
                //string name = "unknown";
                //string id = "id";
                //// try
                //{
                //    name = a.Children[0].Name;
                //    id = a.Children[1].Name;
                //    string descattrib = "";
                //    string sizeattrib = "";
                //    string value = "";
                //    var vV = a.Find("Value", false).FirstOrDefault();
                //    var pP = a.Find("Pattern", false).FirstOrDefault();


                //    if (vV != null && vV.Children.Count > 0)
                //    {
                //        descattrib = vV.Children[0].Name;
                //        value = vV.Children[0].Name;
                //    }

                //    BoardSide side = BoardSide.Top;
                //    var BottomSide = a.Find("BottomSide", false).FirstOrDefault();
                //    if (BottomSide != null && BottomSide.Children.Count > 0)
                //    {
                //        string V = BottomSide.Children[0].Name;
                //        if (V == "Y") side = BoardSide.Bottom;
                //    }


                //    string pattern = "";

                //    if (pP != null && pP.Children.Count > 0)
                //    {
                //        pattern = pP.Children[0].Name;
                //    }
                //    var X = a.Find("X", false).FirstOrDefault();
                //    if (X != null && X.Children.Count > 0)
                //    {
                //        string V = X.Children[0].Name;
                //        if (V[0] == '.') V = "0" + V;
                //        V = V.Replace('.', ',');
                //        x = float.Parse(V);

                //    }
                //    var Y = a.Find("Y", false).FirstOrDefault();
                //    if (Y != null && Y.Children.Count > 0)
                //    {
                //        string V = Y.Children[0].Name;
                //        if (V[0] == '.') V = "0" + V;
                //        V = V.Replace('.', ',');

                //        y = float.Parse(V);
                //    }
                //    float angle = 0;
                //    float orientangle = 0;
                //    int orientidx = 0;
                //    var O = a.Find("Orientation", false).FirstOrDefault();
                //    if (O != null && O.Children.Count > 0)
                //    {
                //        string V = O.Children[0].Name;
                //        if (V[0] == '0') { orientangle = 0; orientidx = 0; };
                //        if (V[0] == '1') { orientangle = 90; orientidx = 1; };
                //        if (V[0] == '2') { orientangle = 180; orientidx = 2; };
                //        if (V[0] == '3') { orientangle = 270; orientidx = 3; };
                //    }
                //    float pangle = 0;
                //    int partorientidx = 0;
                //    var PO = a.Find("PatternOrientation", false).FirstOrDefault();
                //    if (PO != null && PO.Children.Count > 0)
                //    {
                //        string V = PO.Children[0].Name;
                //        if (V[0] == '0') { pangle = 0; partorientidx = 0; }
                //        if (V[0] == '1') { pangle = 90; partorientidx = 1; }
                //        if (V[0] == '2') { pangle = 180; partorientidx = 2; }
                //        if (V[0] == '3') { pangle = 270; partorientidx = 3; }

                //    }


                //    var A = a.Find("Angle", false).FirstOrDefault();
                //    if (A != null && A.Children.Count > 0)
                //    {
                //        angle = float.Parse(A.Children[0].Name.Replace('.', ',')) * AngleMult;
                //    }

                //    bool Flipped = false;
                //    var nFlipped = a.Find("Flipped", false).FirstOrDefault();
                //    if (nFlipped != null && BottomSide.Children.Count > 0)
                //    {
                //        string V = nFlipped.Children[0].Name;
                //        if (V == "Y")
                //        {
                //            Flipped = true;
                //        }
                //    }

                //    var placement = AddBOMItemExt(pattern, name, value, id, null, System.IO.Path.GetFileNameWithoutExtension(kicadschname), 0, 0, 0, side);
                //    var BE = GetBOMEntry(placement);
                //    float DipComp = 0;// DipCompensationAngle(BE.Combined(), side);

                //    // orientangle += DipComp;
                //    var RD = GetRefDes(placement);

                //    DipRotateAndAdjust(x, y, partoriginx, partoriginy, angle, orientidx, partorientidx, Flipped, RD, originx, originy, UnitMult);
                //    if (side == BoardSide.Bottom) RD.angle = -RD.angle;
                //    RD.angle += DipComp;
                //    if (id == "C10")
                //    {
                //        Console.WriteLine(RD);
                //    }



            }

            Log.PopActivity();
        }
    }


}
