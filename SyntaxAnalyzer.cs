using System;
using System.Collections.Generic;

static class SyntaxAnalyzer
{
    private static readonly HashSet<Sym> _varSync = new HashSet<Sym>
    {
        Sym.Semicolon,
        Sym.Beginsy,
        Sym.Varsy,
        Sym.Ident
    };
    private static readonly HashSet<Sym> _statementSync = new HashSet<Sym>
    {
        Sym.Semicolon,
        Sym.Endsy,
        Sym.Untilsy
    };
    private static readonly HashSet<Sym> _expressionSync = new HashSet<Sym>
    {
        Sym.Semicolon,
        Sym.Endsy,
        Sym.Untilsy,
        Sym.Dosy,
        Sym.Tosy,
        Sym.Downtosy,
        Sym.Rightpar,
        Sym.Comma
    };

    public static void Parse()
    {
        LexicalAnalyzer.NextSym();

        if (LexicalAnalyzer.Symbol == Sym.Varsy)
        {
            ParseVar();
        }

        if (LexicalAnalyzer.Symbol == Sym.Beginsy)
        {
            ParseCompoundStatement();
        }
        else
        {
            InputOutput.Error(LexicalAnalyzer.Token, 3);
        }
    }

    private static void Match(Sym expected, byte errorCode, HashSet<Sym> syncSet)
    {
        if (LexicalAnalyzer.Symbol == expected)
        {
            LexicalAnalyzer.NextSym();
        }
        else
        {
            InputOutput.Error(LexicalAnalyzer.Token, errorCode);

            while (!InputOutput.IsEof && LexicalAnalyzer.Symbol != expected && !syncSet.Contains(LexicalAnalyzer.Symbol))
            {
                LexicalAnalyzer.NextSym();
            }

            if (LexicalAnalyzer.Symbol == expected)
            {
                LexicalAnalyzer.NextSym();
            }
        }
    }

    private static void ParseVar()
    {
        Match(Sym.Varsy, 0, _varSync);
        
        List<string> varNames = new List<string>();
        List<TextPosition> varPositions = new List<TextPosition>();
        DataType declType = DataType.Unknown;
        string typeName = "";

        while (LexicalAnalyzer.Symbol == Sym.Ident)
        {

            ParseIdentList(varNames, varPositions);

            Match(Sym.Colon, 14, _varSync);

            declType = DataType.Unknown;
            if (LexicalAnalyzer.Symbol == Sym.Ident)
            {
                typeName = LexicalAnalyzer.AddrName.ToLower();
                switch (typeName)
                {
                    case "integer":
                        declType = DataType.Integer;
                        break;
                    case "real": 
                        declType = DataType.Real; 
                        break;
                    case "boolean": 
                        declType = DataType.Boolean; 
                        break;
                    case "string": 
                        declType = DataType.String; 
                        break;
                    default: 
                        InputOutput.Error(LexicalAnalyzer.Token, 105);
                        break;
                }

                LexicalAnalyzer.NextSym();
            }
            else
            {
                Match(Sym.Ident, 15, _varSync);
            }

            for (int i = 0; i < varNames.Count; i++)
            {
                SemanticAnalyzer.AddVariable(varNames[i], declType, varPositions[i]);
            }

            Match(Sym.Semicolon, 4, _varSync);

            varNames.Clear();
            varPositions.Clear();
        }
    }
    private static void ParseIdentList(List<string> names, List<TextPosition> positions)
    {
        if (LexicalAnalyzer.Symbol == Sym.Ident)
        {
            names.Add(LexicalAnalyzer.AddrName);
            positions.Add(LexicalAnalyzer.Token);
        }
        Match(Sym.Ident, 13, _varSync);

        while (LexicalAnalyzer.Symbol == Sym.Comma)
        {
            LexicalAnalyzer.NextSym();
            if (LexicalAnalyzer.Symbol == Sym.Ident)
            {
                names.Add(LexicalAnalyzer.AddrName);
                positions.Add(LexicalAnalyzer.Token);
            }
            Match(Sym.Ident, 13, _varSync);
        }
    }

    private static void ParseCompoundStatement()
    {
        Match(Sym.Beginsy, 3, _statementSync);

        ParseStatement();
        while (LexicalAnalyzer.Symbol == Sym.Semicolon)
        {
            LexicalAnalyzer.NextSym();
            ParseStatement();
        }

        Match(Sym.Endsy, 2, _statementSync);
    }

    private static void ParseStatement()
    {
        switch (LexicalAnalyzer.Symbol)
        {
            case Sym.Ident:
                ParseAssignment();
                break;
            case Sym.Whilesy:
                ParseWhileLoop();
                break;
            case Sym.Forsy:
                ParseForLoop();
                break;
            case Sym.Repeatsy:
                ParseRepeatLoop();
                break;
            case Sym.Beginsy:
                ParseCompoundStatement();
                break;
            case Sym.Semicolon:
                break;
            case Sym.Endsy:
                break;
            case Sym.Untilsy:
                break;
            default:
                InputOutput.Error(LexicalAnalyzer.Token, 5);
                while (!InputOutput.IsEof && !_statementSync.Contains(LexicalAnalyzer.Symbol))
                {
                    LexicalAnalyzer.NextSym();
                }
                break;
        }
    }

    private static void ParseAssignment()
    {
        string varName = LexicalAnalyzer.AddrName;
        TextPosition varPos = LexicalAnalyzer.Token;

        Match(Sym.Ident, 13, _statementSync);

        DataType leftType = SemanticAnalyzer.GetVariableType(varName, varPos);

        Match(Sym.Assign, 8, _statementSync);

        DataType rightType = ParseExpression();

        if (leftType != DataType.Unknown && rightType != DataType.Unknown)
        {
            if (leftType != rightType)
            {
                if (!(leftType == DataType.Real && rightType == DataType.Integer))
                {
                    InputOutput.Error(varPos, 103);
                }
            }
        }
    }

    private static void ParseWhileLoop()
    {
        TextPosition pos = LexicalAnalyzer.Token;
        Match(Sym.Whilesy, 0, _statementSync);

        DataType condType = ParseExpression();
        if (condType != DataType.Unknown && condType != DataType.Boolean)
        {
            InputOutput.Error(pos, 104);
        }

        Match(Sym.Dosy, 9, _statementSync);
        ParseStatement();
    }

    private static void ParseForLoop()
    {
        Match(Sym.Forsy, 0, _statementSync);

        string iteratorName = LexicalAnalyzer.AddrName;
        TextPosition iterPos = LexicalAnalyzer.Token;
        Match(Sym.Ident, 13, _statementSync);

        DataType iterType = SemanticAnalyzer.GetVariableType(iteratorName, iterPos);

        Match(Sym.Assign, 8, _statementSync);

        DataType startType = ParseExpression();
        if (iterType != DataType.Unknown && startType != DataType.Unknown && iterType != startType)
        {
            InputOutput.Error(iterPos, 103);
        }

        if (LexicalAnalyzer.Symbol == Sym.Tosy || LexicalAnalyzer.Symbol == Sym.Downtosy)
        {
            LexicalAnalyzer.NextSym();
        }
        else
        {
            InputOutput.Error(LexicalAnalyzer.Token, 17);
        }

        DataType endType = ParseExpression();
        if (iterType != DataType.Unknown && endType != DataType.Unknown && iterType != endType)
        {
            InputOutput.Error(iterPos, 103);
        }

        Match(Sym.Dosy, 9, _statementSync);
        ParseStatement();
    }

    private static void ParseRepeatLoop()
    {
        Match(Sym.Repeatsy, 0, _statementSync);

        ParseStatement();
        while (LexicalAnalyzer.Symbol == Sym.Semicolon)
        {
            LexicalAnalyzer.NextSym();
            ParseStatement();
        }

        TextPosition pos = LexicalAnalyzer.Token;
        Match(Sym.Untilsy, 18, _statementSync);

        DataType condType = ParseExpression();
        if (condType != DataType.Unknown && condType != DataType.Boolean)
        {
            InputOutput.Error(pos, 104);
        }
    }

    private static DataType ParseExpression()
    {
        DataType type1 = ParseSimpleExpression();

        if (IsRelationalOp(LexicalAnalyzer.Symbol))
        {
            LexicalAnalyzer.NextSym();
            ParseSimpleExpression();

            return DataType.Boolean;
        }
        return type1;
    }

    private static DataType ParseSimpleExpression()
    {
        if (LexicalAnalyzer.Symbol == Sym.Plus || LexicalAnalyzer.Symbol == Sym.Minus)
        {
            LexicalAnalyzer.NextSym();
        }

        DataType type = ParseTerm();

        while (LexicalAnalyzer.Symbol == Sym.Plus ||
            LexicalAnalyzer.Symbol == Sym.Minus ||
            LexicalAnalyzer.Symbol == Sym.Orsy)
        {
            Sym op = LexicalAnalyzer.Symbol;

            LexicalAnalyzer.NextSym();
            DataType rightType = ParseTerm();
            if (type != DataType.Unknown && rightType != DataType.Unknown)
            {
                if (op == Sym.Plus || op == Sym.Minus)
                {
                    if (type == DataType.String || rightType == DataType.String)
                    {
                        InputOutput.Error(LexicalAnalyzer.Token, 103);
                        type = DataType.Unknown;
                        continue;
                    }
                }
                if (op == Sym.Orsy)
                {
                    if (type != DataType.Boolean || rightType != DataType.Boolean)
                    {
                        InputOutput.Error(LexicalAnalyzer.Token, 103);
                        type = DataType.Unknown;
                        continue;
                    }
                }
            }
            if (type == DataType.Real || rightType == DataType.Real)
            {
                type = DataType.Real;
            }
        }
        return type;
    }

    private static DataType ParseTerm()
    {
        DataType type = ParseFactor();

        while (LexicalAnalyzer.Symbol == Sym.Star ||
            LexicalAnalyzer.Symbol == Sym.Slash ||
            LexicalAnalyzer.Symbol == Sym.Divsy ||
            LexicalAnalyzer.Symbol == Sym.Modsy ||
            LexicalAnalyzer.Symbol == Sym.Andsy)
        {
            Sym op = LexicalAnalyzer.Symbol;
            LexicalAnalyzer.NextSym();
            DataType rightType = ParseFactor();

            if (op == Sym.Slash)
            {
                type = DataType.Real;
            }
            else if (type == DataType.Real || rightType == DataType.Real)
            {
                type = DataType.Real;
            }
        }
        return type;
    }

    private static DataType ParseFactor()
    {
        DataType type = DataType.Unknown;

        switch (LexicalAnalyzer.Symbol)
        {
            case Sym.Ident:
                type = SemanticAnalyzer.GetVariableType(LexicalAnalyzer.AddrName, LexicalAnalyzer.Token);
                LexicalAnalyzer.NextSym();
                break;
            case Sym.Intc:
                type = DataType.Integer;
                LexicalAnalyzer.NextSym();
                break;
            case Sym.Stringc:
                type = DataType.String;
                LexicalAnalyzer.NextSym();
                break;
            case Sym.Leftpar:
                LexicalAnalyzer.NextSym();
                type = ParseExpression();
                Match(Sym.Rightpar, 6, _expressionSync);
                break;
            case Sym.Notsy:
                LexicalAnalyzer.NextSym();
                type = ParseFactor();
                break;
            default:
                InputOutput.Error(LexicalAnalyzer.Token, 19);
                if (!_expressionSync.Contains(LexicalAnalyzer.Symbol))
                {
                    LexicalAnalyzer.NextSym();
                }
                break;
        }
        return type;
    }

    private static bool IsRelationalOp(Sym sym)
    {
        return sym == Sym.Equal || sym == Sym.Latergreater ||
               sym == Sym.Later || sym == Sym.Laterequal ||
               sym == Sym.Greater || sym == Sym.Greaterequal;
    }
}