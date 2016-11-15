using GerberLibrary.Core.Primitives;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace GerberLibrary
{
    public class GerberNumberPair
    {
        public string Orig = "";
        public double Number;
        public string Command = "";

        public void Parse(string running, GerberNumberFormat form, bool hasdecimalpoint = false)
        {
            if (Command == "G" || Command == "D" || Command == "M")
            {
                Number = (double)Int32.Parse(running);
            }
            else
            {
                Number = form.Decode(running, hasdecimalpoint);
            }
        }
    }

    public class GerberNumberPairList
    {
        public string Command = "";
        public string Orig = "";

        public List<GerberNumberPair> Numbers = new List<GerberNumberPair>();
        public void Parse(string running, GerberNumberFormat form)
        {
            GerberNumberPair GNP = new GerberNumberPair();
            GNP.Orig = Orig;
            GNP.Command = Command;
            GNP.Parse(running, form);
            Numbers.Add(GNP);
        }
    }

    public class GerberSplitter
    {
        GerberNumberPair GNP = new GerberNumberPair();
        List<String> CommandsInOrder = new List<string>();
        public Dictionary<string, GerberNumberPair> Pairs = new Dictionary<string, GerberNumberPair>();
        public double Get(string n)
        {
            if (Pairs.ContainsKey(n)) return Pairs[n].Number;
            return -1;
        }

        public bool Has(string n)
        {
            return Pairs.ContainsKey(n);
        }

        public void Split(string p, GerberNumberFormat form, bool finddecimalpoint = false)
        {
            if (p.Length < 2) return;
            try
            {
                if (p.Substring(0, 3) == "G04")
                {
                    GNP.Command = "G";
                    GNP.Number = 4;
                    Pairs[GNP.Command] = GNP;
                    CommandsInOrder.Add(GNP.Command);
                    // line is a comment!
                    //           Console.WriteLine("comment: {0}", p.Substring(3));
                    return;
                }
                bool wasnumber = false;
                bool isnumber = false;
//                bool hasdecimalpoint = false;
                string running = "";
                for (int i = 0; i < p.Length; i++)
                {
                    char current = p[i];
                    if (char.IsNumber(current) || current == '+' || current == '-' || current == '.')
                    {
                        isnumber = true;
                    }
                    else
                    {
                        isnumber = false;
                    }

                    if (isnumber != wasnumber)
                    {
                        if (isnumber)
                        {
                            GNP.Command = running;
                            GNP.Orig = running;
                        }
                        else
                        {
                            GNP.Orig += running;
                            GNP.Parse(running, form, finddecimalpoint);
                            Pairs[GNP.Command] = GNP;
                            CommandsInOrder.Add(GNP.Command);
                            GNP = new GerberNumberPair();
                        }
                        wasnumber = isnumber;
                        running = "";
                    }
                    if (current != '+') running += current;
                }

                if (GNP.Command.Length > 0)
                {
                    GNP.Parse(running, form, finddecimalpoint);
                    Pairs[GNP.Command] = GNP;
                    CommandsInOrder.Add(GNP.Command);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("this line does not seem to contain gerber: {0}", p);
            }
        }

        internal void Set(string name, double val)
        {
            if (Has(name) == false)
            {
                Pairs[name] = new GerberNumberPair();
                Pairs[name].Command = name;
                CommandsInOrder.Add(name);

            }
            Pairs[name].Number = val;
        }

        public string Rebuild(GerberNumberFormat form)
        {
            string Res = "";
            if ((Has("X") || Has("Y")) && Has("D"))
            {
                CommandsInOrder.Remove("X");
                CommandsInOrder.Remove("Y");
                CommandsInOrder.Remove("I");
                CommandsInOrder.Remove("J");
                CommandsInOrder.Remove("D");

                if (Has("X")) CommandsInOrder.Add("X");
                if (Has("Y")) CommandsInOrder.Add("Y");
                if (Has("I")) CommandsInOrder.Add("I");
                if (Has("J")) CommandsInOrder.Add("J");
                CommandsInOrder.Add("D");
            }
            else
            {
                if (Has("D") && Get("D") < 10)
                {
                    CommandsInOrder.Remove("D");
                    CommandsInOrder.Add("D");
                }
            }
            foreach (var a in CommandsInOrder)
            {
                if (a == "D" || a == "G" || a == "M")
                {
                    Res += a;
                    Res += ((int)Get(a)).ToString("D2");
                }
                else
                {
                    bool Write = true;
                    if (a == "J" || a == "I")

                    {
                        string R = form.Format(Get(a));
                        int zerocount = 0;
                        for (int i = 0; i < R.Count(); i++) if (R[i] == '0') zerocount++;
                        if (zerocount == R.Count()) Write = false;
                    }
                    if (Write)
                    {
                        Res += a;
                        Res += form.Format(Get(a));
                    }
                }
            }
            return Res + "*";
        }

        internal void ScaleToMM(GerberNumberFormat gerberNumberFormat)
        {
            foreach (var a in Pairs)
            {
                switch (a.Value.Command)
                {
                    case "X":
                    case "Y":
                    case "I":
                    case "J":
                        a.Value.Number = gerberNumberFormat.ScaleFileToMM(a.Value.Number);

                        break;
                }
            }
        }

        internal void ScaleToFile(GerberNumberFormat GNF)
        {
            foreach (var a in Pairs)
            {
                switch (a.Value.Command)
                {
                    case "X":
                    case "Y":
                    case "I":
                    case "J":
                        a.Value.Number = GNF._ScaleMMToFile(a.Value.Number);

                        break;
                }
            }
        }

        public double GetByLastChar(string p)
        {
            foreach (var a in Pairs)
            {
                if (a.Key.Last() == p.Last())
                {
                    return a.Value.Number;
                }
            }
            return -1;
        }
    }

    public class GerberListSplitter
    {
        GerberNumberPair GNP = new GerberNumberPair();
        List<String> CommandsInOrder = new List<string>();
        Dictionary<string, GerberNumberPairList> Pairs = new Dictionary<string, GerberNumberPairList>();
        public double Get(string n, int i = 0)
        {
            if (Pairs.ContainsKey(n)) return Pairs[n].Numbers[i].Number;
            return -1;
        }

        public bool Has(string n)
        {
            return Pairs.ContainsKey(n);
        }
        internal void Split(string p, GerberNumberFormat form, bool hasdecimalpoint = false)
        {
            try
            {
                bool wasnumber = false;
                bool isnumber = false;
                string running = "";
                for (int i = 0; i < p.Length; i++)
                {
                    char current = p[i];
                    if (char.IsNumber(current) || current == '+' || current == '-')
                    {
                        isnumber = true;
                    }
                    else
                    {
                        isnumber = false;
                    }

                    if (isnumber != wasnumber)
                    {
                        if (isnumber)
                        {
                            GNP.Command = running;
                            GNP.Orig = running;
                        }
                        else
                        {
                            GNP.Orig += running;
                            GNP.Parse(running, form, hasdecimalpoint);
                            if (Pairs.ContainsKey(GNP.Command) == false)
                            {
                                GerberNumberPairList GNPL = new GerberNumberPairList();
                                GNPL.Command = GNP.Command;
                                GNPL.Orig = GNP.Orig;
                                Pairs[GNP.Command] = GNPL;
                            }
                            Pairs[GNP.Command].Numbers.Add(GNP);
                            CommandsInOrder.Add(GNP.Command);
                            GNP = new GerberNumberPair();
                        }
                        wasnumber = isnumber;
                        running = "";
                    }
                    if (current != '+') running += current;
                }

                if (GNP.Command.Length > 0)
                {
                    GNP.Parse(running, form, hasdecimalpoint);
                    if (Pairs.ContainsKey(GNP.Command) == false)
                    {
                        GerberNumberPairList GNPL = new GerberNumberPairList();
                        GNPL.Command = GNP.Command;
                        GNPL.Orig = GNP.Orig;
                        Pairs[GNP.Command] = GNPL;
                    }
                    Pairs[GNP.Command].Numbers.Add(GNP);

                    CommandsInOrder.Add(GNP.Command);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("this line does not seem to contain gerber: {0}", p);
            }
        }

        internal void Set(string name, double val)
        {
            if (Has(name) == false)
            {
                Pairs[name] = new GerberNumberPairList();
                Pairs[name].Command = name;
            }
            Pairs[name].Numbers.Add(new GerberNumberPair() { Number = val, Command = name, Orig = name });
        }

        public string Rebuild(GerberNumberFormat form)
        {
            string Res = "";
            if ((Has("X") || Has("Y")) && Has("D"))
            {
                CommandsInOrder.Remove("X");
                CommandsInOrder.Remove("Y");
                CommandsInOrder.Remove("I");
                CommandsInOrder.Remove("J");
                CommandsInOrder.Remove("D");

                if (Has("X")) CommandsInOrder.Add("X");
                if (Has("Y")) CommandsInOrder.Add("Y");
                if (Has("I")) CommandsInOrder.Add("I");
                if (Has("J")) CommandsInOrder.Add("J");
                CommandsInOrder.Add("D");
            }
            foreach (var a in CommandsInOrder)
            {
                Res += a;
                if (a == "D" || a == "G" || a == "M")
                {
                    Res += ((int)Get(a)).ToString("D2");
                }
                else
                {
                    Res += form.Format(Get(a));
                }
            }
            return Res + "*";
        }

        internal void ScaleToMM(GerberNumberFormat gerberNumberFormat)
        {
            foreach (var a in Pairs)
            {
                switch (a.Value.Command)
                {
                    case "X":
                    case "Y":
                    case "I":
                    case "J":
                        foreach (var p in a.Value.Numbers)
                        {
                            p.Number = gerberNumberFormat.ScaleFileToMM(p.Number);
                        }
                        break;
                }
            }
        }

        internal void ScaleToFile(GerberNumberFormat GNF)
        {
            foreach (var a in Pairs)
            {
                switch (a.Value.Command)
                {
                    case "X":
                    case "Y":
                    case "I":
                    case "J":
                        foreach (var p in a.Value.Numbers)
                        {
                            p.Number = GNF._ScaleMMToFile(p.Number);
                        }
                        break;
                }
            }
        }

        public double GetByLastChar(string p)
        {
            foreach (var a in Pairs)
            {
                if (a.Key.Last() == p.Last())
                {
                    return Get(a.Key);
                }
            }
            return -1;
        }

        int GetCommandIndex(string p1)
        {
            for (int i = 0; i < CommandsInOrder.Count; i++)
            {
                if (CommandsInOrder[i] == p1) return i;
            }
            return -1;
        }

        internal double GetBefore(string p1, string p2)
        {
            if (HasBefore(p1, p2)) return Get(p2);//

            return -1;

        }

        internal bool HasBefore(string p1, string p2)
        {

            int i2 = GetCommandIndex(p2);
            if (i2 == -1) return false;

            int i1 = GetCommandIndex(p1);
            if (i1 == -1) return true;
            if (i2 < i1) return true;
            return false;
        }

        internal double GetAfter(string p1, string p2)
        {
            if (HasAfter(p1, p2) == false) return -1;

            int i1 = GetCommandIndex(p1);

            int index = 0;

            for (int i = 0; i < CommandsInOrder.Count; i++)
            {
                if (CommandsInOrder[i] == p2)
                {
                    if (i > i1) return Get(p2, index);
                    index++;

                }
            }
            return -1;
        }

        internal bool HasAfter(string p1, string p2)
        {
            int i1 = GetCommandIndex(p1);
            if (i1 == -1) return false;


            for (int i = i1; i < CommandsInOrder.Count; i++)
            {
                if (CommandsInOrder[i] == p2)
                {
                    return true;
                }
            }
            return false;
        }
    }

}
