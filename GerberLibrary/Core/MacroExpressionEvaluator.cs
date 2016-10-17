
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GerberLibrary
{
    public class MacroExpressionEvaluator
    {
        public enum AstOperator
        {
            NOP,
            Add,
            Sub,
            Div,
            Mult,
            Pow,
            Symbol,
            Number,
            Expr,
            EndExpr
        }
        public class AstSymbolTable
        {
            public double Get(string name) { return Table[name]; }
            public void Set(string name, double value) { Table[name] = value; }

            public Dictionary<string, double> Table = new Dictionary<string, double>();
        }
        public class AstNode
        {
            public AstNode Left;
            public AstNode Right;
            public AstOperator Operator = AstOperator.NOP;
            public string SymbolName = "";
            public double Number = 0;

            public double GetL(AstSymbolTable T)
            {
                if (Left != null) return Left.Run(T); return 0;
            }
            public double GetR(AstSymbolTable T)
            {
                if (Right != null) return Right.Run(T); return 0;
            }
            public Double Run(AstSymbolTable T)
            {
                switch (Operator)
                {
                    case AstOperator.NOP: return 0;
                    case AstOperator.Add: return GetL(T) + GetR(T);
                    case AstOperator.Sub: return GetL(T) - GetR(T);
                    case AstOperator.Mult: return GetL(T) * GetR(T);
                    case AstOperator.Pow: return Math.Pow(GetL(T), GetR(T));
                    case AstOperator.Div: return GetL(T) / GetR(T);
                    case AstOperator.Number: return Number;
                    case AstOperator.Symbol: return T.Get(SymbolName);


                }
                return 0;

            }

            public override string ToString()
            {
                switch (Operator)
                {

                    case AstOperator.Symbol: return SymbolName;
                    case AstOperator.Number: return Number.ToString("N2");
                    case AstOperator.Add: return "+";
                    case AstOperator.Pow: return "^";
                    case AstOperator.Sub: return "-";
                    case AstOperator.Mult: return "*";
                    case AstOperator.Div: return "/";
                }
                return "Node!";
            }

        }

        public enum TokenType
        {
            Number,
            Symbol,
            Operator,
            BracketOpen,
            BracketClose,
            None
        }

        public class Token
        {
            public TokenType Type = TokenType.None;
            public double Number;
            public string Name;
            public AstOperator Operator;
            public override string ToString()
            {
                if (Type == TokenType.Operator)
                {
                    return Enum.GetName(typeof(AstOperator), Operator);
                }
                if (Type == TokenType.BracketOpen) return "(";
                if (Type == TokenType.BracketClose) return ")";
                if (Type == TokenType.Number) return Number.ToString("N3");
                if (Type == TokenType.Symbol) return Name;

                return Enum.GetName(typeof(TokenType), Type) + " " + Name + " " + Number.ToString();
            }
        }

        public AstNode BuildAST(List<Token> Tokens, int start, out int end)
        {
            List<Token> T2 = Tokens;


            //          Console.WriteLine("Tokens after unary fix:");
            // TokenStream.PrintTokens(T2);

            int bracklevel = 0;
            AstNode CurrentOperatorNode = null;
            AstNode CurrentLeftNode = null;
            for (int i = start; i < T2.Count; i++)
            {
                var Cur = T2[i];

                switch (Cur.Type)
                {
                    case TokenType.Operator:
                        var NewOperatorNode = NodeFromToken(Cur);
                        NewOperatorNode.Left = CurrentOperatorNode != null ? CurrentOperatorNode : CurrentLeftNode;
                        CurrentOperatorNode = NewOperatorNode;

                        break;
                    case TokenType.Number:
                    case TokenType.Symbol:
                        if (CurrentOperatorNode != null)
                        {
                            CurrentOperatorNode.Right = NodeFromToken(Cur);
                        }
                        else
                        {
                            CurrentLeftNode = NodeFromToken(Cur);
                        }
                        break;

                    case TokenType.BracketOpen:
                        bracklevel++;
                        var N = BuildAST(T2, i + 1, out i);
                        if (CurrentOperatorNode != null)
                        {
                            CurrentOperatorNode.Right = N;
                        }
                        else
                        {
                            CurrentLeftNode = N;
                        }
                        break;
                    case TokenType.BracketClose:
                        bracklevel--;
                        if (bracklevel < 0)
                        {
                            end = i;
                            return CurrentOperatorNode;
                        }
                        break;
                }
            }

            end = T2.Count;
            if (CurrentOperatorNode == null && CurrentLeftNode != null) return CurrentLeftNode;
            return CurrentOperatorNode;
        }

        private List<Token> CreateOrderBrackets(List<Token> T2)
        {
            List<Token> Res = new List<Token>();
            int bracketlevel = 0;
            List<int> bracketlevelstoclose = new List<int>();

            for (int i = 0; i < T2.Count; i++)
            {
                var T = T2[i];

                if (T.Type == TokenType.Operator && (T.Operator == AstOperator.Mult || T.Operator == AstOperator.Div))
                {
                    //backtrack and insert!
                    int idx = Res.Count - 1;
                    int resbracketlevel = bracketlevel;
                    bool done = false;
                    while (!done && idx > -1)
                    {
                        var BT = Res[idx];
                        bool validinsertionpoint = false;
                        if (BT.Type == TokenType.BracketClose)
                        {
                            resbracketlevel++;
                        }
                        if (BT.Type == TokenType.BracketOpen)
                        {
                            resbracketlevel--;
                            validinsertionpoint = true;

                        }
                        if (BT.Type == TokenType.Number || BT.Type == TokenType.Symbol)
                        {
                            validinsertionpoint = true;
                        }

                        if (resbracketlevel == bracketlevel && validinsertionpoint)
                        {
                            done = true;
                        }
                        else
                        {
                            idx--;
                        }
                    }

                    Res.Insert(idx, new Token() { Type = TokenType.BracketOpen });
                    bracketlevelstoclose.Add(bracketlevel);
                }
                else
                {
                    if (T.Type == TokenType.BracketOpen) { bracketlevel++; }
                    else
                    {
                        if (T.Type == TokenType.BracketClose) { bracketlevel--; }
                    }
                }
                Res.Add(T);
                if (T.Type == TokenType.Symbol || T.Type == TokenType.Number || T.Type == TokenType.BracketClose)
                {
                    bracketlevelstoclose.Sort();
                    while (bracketlevelstoclose.Count > 0 && bracketlevelstoclose[0] == bracketlevel)
                    {
                        bracketlevelstoclose.Remove(bracketlevel);
                        Res.Add(new Token() { Type = TokenType.BracketClose });
                    }

                }

            }

            return Res;

        }

        public static void PrintAST(AstNode Node, int spaces = 0)
        {
            //if (spaces == 0) Console.WriteLine("**** AST");
            if (Node == null) return;

            PrintAST(Node.Left, spaces + 3);
            //for (var i = 0; i < spaces; i++) Console.Write(" ");
            //Console.WriteLine(Node);
            PrintAST(Node.Right, spaces + 3);

        }

        private static List<Token> CreateUnaryNodes(List<Token> Tokens)
        {
            List<Token> T2 = new List<Token>();
            Token LastTok = null;
            foreach (var T in Tokens)
            {
                if (T.Type == TokenType.Operator && T.Operator == AstOperator.Sub)
                {
                    if (LastTok == null || LastTok.Type == TokenType.Operator)
                    {
                        // is unary! transform next number by -1!
                        T2.Add(new Token() { Number = -1.0, Type = TokenType.Number });
                        T2.Add(new Token() { Operator = AstOperator.Mult, Type = TokenType.Operator });
                    }
                    else
                    {
                        // normal minus
                        T2.Add(T);
                    }
                }
                else
                {
                    T2.Add(T);
                }
                LastTok = T;

            }
            return T2;
        }

        private AstNode NodeFromToken(Token CurrentToken)
        {
            AstNode Node = new AstNode();
            switch (CurrentToken.Type)
            {
                case TokenType.Number: Node.Operator = AstOperator.Number; Node.Number = CurrentToken.Number; break;
                case TokenType.Symbol: Node.Operator = AstOperator.Symbol; Node.SymbolName = CurrentToken.Name; break;
                case TokenType.Operator: Node.Operator = TokenToAst(CurrentToken.Operator); break;
                case TokenType.BracketOpen: Node.Operator = AstOperator.Expr; break;
                case TokenType.BracketClose: Node.Operator = AstOperator.EndExpr; break;
            }

            return Node;
        }

        private AstOperator TokenToAst(AstOperator astOperator)
        {
            return astOperator;
        }

        AstSymbolTable TheTable = new AstSymbolTable();

        public double Evaluate(string source)
        {
            TokenStream TS = new TokenStream();
            List<Token> TheTokenStream = TS.BuildTokenStream(source);
            int end = 0;

            TheTokenStream = CreateUnaryNodes(TheTokenStream);
            TheTokenStream = CreateOrderBrackets(TheTokenStream);

            //TokenStream.PrintTokens(TheTokenStream);

            AstNode Root = BuildAST(TheTokenStream, 0, out end);
            PrintAST(Root);

            if (Root == null) return 0;
            return Root.Run(TheTable);
        }

        public class TokenStream
        {
            List<Token> Res = new List<Token>();
            AstOperator CurrentOperator = AstOperator.NOP;
            TokenType CurrentTokenType = TokenType.None;
            double num = 0;
            bool numberbusy = false;
            bool symbolbusy = false;
            int behinddecimalpoint = -1;
            string CurrentSymbol;

            public List<Token> BuildTokenStream(string source)
            {
                for (int i = 0; i < source.Length; i++)
                {
                    char c = source[i];
                    if (c == '(') SetTokenType(TokenType.BracketOpen);
                    else
                        if (c == ')') SetTokenType(TokenType.BracketClose);
                        else

                            if (c == ' ' || c == '\t') SetTokenType(TokenType.None);
                            else
                                if (char.IsNumber(c) && !symbolbusy)
                                {
                                    SetTokenType(TokenType.Number);

                                    if (numberbusy)
                                    {
                                        if (behinddecimalpoint > -1)
                                        {
                                            behinddecimalpoint++;
                                            num += char.GetNumericValue(c) / Math.Pow(10, behinddecimalpoint);
                                        }
                                        else
                                        {
                                            num *= 10.0;
                                            num += char.GetNumericValue(c);
                                        }

                                    }
                                    else
                                    {
                                        num = char.GetNumericValue(c);

                                    }
                                    numberbusy = true;
                                }
                                else
                                {
                                    if (c == '.')
                                    {
                                        SetTokenType(TokenType.Number);
                                        behinddecimalpoint = 0;
                                    }
                                    else
                                    {
                                        if (char.IsLetter(c) || (char.IsNumber(c) && symbolbusy))
                                        {
                                            SetTokenType(TokenType.Symbol);
                                            if (!symbolbusy)
                                            {
                                                CurrentSymbol = "";
                                            }
                                            CurrentSymbol += c;
                                            symbolbusy = true;
                                        }
                                        else
                                        {
                                            string operators = "-+/*^";

                                            if (operators.Contains(c))
                                            {
                                                SetTokenType(TokenType.Operator);
                                                if (c == '+') CurrentOperator = AstOperator.Add;
                                                if (c == '-') CurrentOperator = AstOperator.Sub;
                                                if (c == '/') CurrentOperator = AstOperator.Div;
                                                if (c == '*') CurrentOperator = AstOperator.Mult;
                                                if (c == '^') CurrentOperator = AstOperator.Pow;
                                                SetTokenType(TokenType.None);

                                            }
                                        }
                                    }
                                }

                }

                SetTokenType(TokenType.None);
                //  PrintTokens(Res);
                return Res;
            }

            public static void PrintTokens(List<Token> Res)
            {
                for (int i = 0; i < Res.Count(); i++)
                {
                    Console.WriteLine("   {0}: {1}", i, Res[i]);
                }

            }

            private void SetTokenType(TokenType tokenType)
            {
                if (CurrentTokenType != tokenType)
                {
                    AddToken(CurrentTokenType);
                }
                CurrentTokenType = tokenType;

            }

            private void AddToken(TokenType CurrentTokenType)
            {
                switch (CurrentTokenType)
                {
                    case TokenType.Number: Res.Add(new Token() { Type = TokenType.Number, Number = num }); break;
                    case TokenType.BracketClose: Res.Add(new Token() { Type = TokenType.BracketClose }); break;
                    case TokenType.BracketOpen: Res.Add(new Token() { Type = TokenType.BracketOpen }); break;
                    case TokenType.Operator: Res.Add(new Token() { Type = TokenType.Operator, Operator = CurrentOperator }); break;
                    case TokenType.Symbol: Res.Add(new Token() { Type = TokenType.Symbol, Name = CurrentSymbol }); break;
                }

                CurrentSymbol = "";
                num = 0;
                symbolbusy = false;
                numberbusy = false;
            }

        }


        public void Set(string name, double value)
        {
            TheTable.Set(name, value);
        }

    }

}