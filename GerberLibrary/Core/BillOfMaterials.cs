using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GerberLibrary.Core
{
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
        public class RefDesc
        {
            public string NameOnBoard;
            public string SourceBoard;
            public string OriginalName;
            public double x;
            public double y;
            public double angle;
        }
        public List<RefDesc> RefDes = new List<RefDesc>();
        public bool Soldered = false;

        internal string AddRef(string refdes, string source, BOMNumberSet set, double x, double y, double angle)
        {
            if (set == null)
            {
                RefDes.Add(new RefDesc() { NameOnBoard = refdes, angle = angle, OriginalName = refdes, SourceBoard = source, x = x, y = y });
                return refdes;
            }
            string newref = set.GetNew(refdes);
            RefDes.Add(new RefDesc() { NameOnBoard = newref, angle = angle, OriginalName = refdes, SourceBoard = source, x = x, y = y });
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
            return (Name + "_" + PackageName + "_" + Value).Replace(' ', '_');
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
        None
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
                    string T = "RES_0603_1%_RES_0603_" + unit;
                    Res[F] = T;
                    Res[F2] = T;
                }

                foreach (var V in Helpers.E24)
                {
                    string unit = Helpers.MakeNiceUnitString(V * R, Units.None).Replace(" ", "").Replace(',', '.');
                    string F = "R*_R0603_" + unit.ToUpper();
                    string F2 = "R*_R0603_" + unit.ToLower();
                    string T = "RES_0603_1%_RES_0603_" + unit;
                    Res[F] = T;
                    Res[F2] = T;
                }

                foreach (var V in Helpers.E48)
                {
                    string unit = Helpers.MakeNiceUnitString(V * R, Units.None).Replace(" ", "").Replace(',', '.');
                    string F = "R*_R0603_" + unit.ToUpper();
                    string F2 = "R*_R0603_" + unit.ToLower();
                    string T = "RES_0603_1%_RES_0603_" + unit;
                    Res[F] = T;
                    Res[F2] = T;
                }
            }
            foreach (var R in Helpers.CapacitorRanges)
            {
                foreach (var V in Helpers.E24)
                {
                    string unit = Helpers.MakeNiceUnitString(V * R, Units.None).Replace(" ", "").Replace(',', '.');
                    string F = "C*_C0603_" + unit.ToUpper();
                    string F2 = "C*_C0603_" + unit.ToLower();

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
        public string AddBOMItem(string package, string device, string value, string refdes, BOMNumberSet set, string SourceBoard, double x, double y, double angle)
        {



            string ID = package + device;
            if (refdes == device) ID = package;

            if (DeviceTree.ContainsKey(ID) == false) DeviceTree[ID] = new Dictionary<string, BOMEntry>();
            if (DeviceTree[ID].ContainsKey(value) == false) DeviceTree[ID][value] = new BOMEntry() { Name = device, Value = value, PackageName = package };
            BOMEntry BE = DeviceTree[ID][value];
            return BE.AddRef(refdes, SourceBoard, set, x, y, angle);




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
                        AddBOMItem(b.Value.PackageName, b.Value.Name, b.Value.Value, c.OriginalName, set, c.SourceBoard, X, Y, (c.angle + angle) % 360);
                    }
                }
            }
        }

        public List<string> PrintBOM(List<String> IgnoreList)
        {
            List<string> ToIgnore = new List<string>() { "FENIXINTERNALCONNECTIONPAD" };
            ToIgnore.AddRange(IgnoreList);
            List<Tuple<string, string, string>> Lines = new List<Tuple<string, string, string>>();
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

            return partno;
        }

        public static Dictionary<string, string> MulticompReels = new Dictionary<string, string>();

        public static Dictionary<string, string> MulticompOrderList = new Dictionary<string, string>();

        private static string MulticompThickFilmResistorPartno(string value)
        {
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

        public  void SetPartno(string pattern, string partno)
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

        public void Remap(string basefolder)
        {
            var D = PartLibrary.CreatePassivesMapping();
            File.WriteAllLines(Path.Combine(basefolder, "genmapping.txt"), (from i in D select i.Key + " " + i.Value).ToList());
            var map = File.ReadAllLines(Path.Combine(basefolder, "bommapping.txt"));

            foreach (var l in map)
            {
                var A = l.Split(' ');
                if (A.Count() == 2)
                {
                    string from = A[0];
                    string to = A[1];

                    RemapPair(from, to);

                }
            }
            foreach (var l in D)
            {
                RemapPair(l.Key, l.Value);
            }

        }

        private void RemapPair(string from, string to)
        {
            if (from != to)
            {

                var F = FindEntry(from);
                var T = FindEntry(to);
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
                        Console.WriteLine("From found, but no To: {0}", from);
                        F.SetCombined(to);
                    }
                }
            }
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

        private BOMEntry FindEntry(string q)
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
    }
}
