using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class Lexer
{
    public enum Tokens
    {
        // Comment
        Comment,
        // Literals
        StringLiteral,
        FloatLiteral,
        IntLiteral,
        BoolLiteral,
        // Keywords
        Function,
        If,
        ElseIf,
        Else,
        ForEach,
        For,
        While,
        Break,
        Return,
        Try,
        Catch,
        Import,
        Null,
        // Brackets
        LeftBrace,
        RightBrace,
        LeftBracket,
        RightBracket,
        LeftParenth,
        RightParenth,
        // Operators
        Lambda,
        Add,
        Sub,
        Div,
        Mod,
        Mul,
        LessEq,
        Less,
        GreaterEq,
        Greater,
        Eq,
        Assign,
        NotEq,
        Comma,
        Dot,
        Identifier,
        // Separators
        Semicolon,
        Newline,
    }

    public class Token
    {
        public Tokens type;
        public string value;
        public int lineNumber;
        public int rowNumber;
        public string line;

        public Token(Tokens type, string value)
        {
            this.type = type;
            this.value = value;
        }

        public string AsIdentifier()
        {
            return value;
        }

        public int AsInt()
        {
            return Int32.Parse(value);
        }

        public float AsFloat()
        {
            return float.Parse(
                // FIXME: ugly hack for removing "f" and to have "." instead of ","
                value.Substring(0, value.Length - 1),
                System.Globalization.CultureInfo.InvariantCulture);
        }

        public string AsString()
        {
            return value.Trim('\"');
        }

        public bool AsBool()
        {
            return value == "true";
        }
    }

    Dictionary<Tokens, string> tokenRegexMap;

    public Lexer()
    {
        tokenRegexMap = new Dictionary<Tokens, string>{
            {Tokens.Comment, "\\/\\/.*"},
            {Tokens.StringLiteral, "\"[^\"]*\""},
            {Tokens.FloatLiteral, "\\d\\d*\\.?\\d*f"},
            {Tokens.IntLiteral, "\\d\\d*"},
            {Tokens.BoolLiteral, "true|false"},
            {Tokens.Function, "function(?![_a-zA-Z][_a-zA-Z\\d]*)"},
            {Tokens.If, "if(?![_a-zA-Z][_a-zA-Z\\d]*)"},
            {Tokens.ElseIf, "elseif(?![_a-zA-Z][_a-zA-Z\\d]*)"},
            {Tokens.Else, "else(?![_a-zA-Z][_a-zA-Z\\d]*)"},
            {Tokens.ForEach, "foreach(?![_a-zA-Z][_a-zA-Z\\d]*)"},
            {Tokens.For, "for(?![_a-zA-Z][_a-zA-Z\\d]*)"},
            {Tokens.While, "while(?![_a-zA-Z][_a-zA-Z\\d]*)"},
            {Tokens.Break, "break(?![_a-zA-Z][_a-zA-Z\\d]*)"},
            {Tokens.Return, "return(?![_a-zA-Z][_a-zA-Z\\d]*)"},
            {Tokens.Try, "try(?![_a-zA-Z][_a-zA-Z\\d]*)"},
            {Tokens.Catch, "catch(?![_a-zA-Z][_a-zA-Z\\d]*)"},
            {Tokens.Import, "import(?![_a-zA-Z][_a-zA-Z\\d]*)"},
            {Tokens.Null, "null(?![_a-zA-Z][_a-zA-Z\\d]*)"},
            {Tokens.LeftBrace, "\\{"},
            {Tokens.RightBrace, "\\}"},
            {Tokens.LeftBracket, "\\["},
            {Tokens.RightBracket, "\\]"},
            {Tokens.LeftParenth, "\\("},
            {Tokens.RightParenth, "\\)"},
            {Tokens.Lambda, "\\=>"},
            {Tokens.Add, "\\+"},
            {Tokens.Sub, "\\-"},
            {Tokens.Div, "\\/"},
            {Tokens.Mod, "\\%"},
            {Tokens.Mul, "\\*"},
            {Tokens.LessEq, "<\\="},
            {Tokens.Less, "<"},
            {Tokens.GreaterEq, ">\\="},
            {Tokens.Greater, ">"},
            {Tokens.Eq, "\\=="},
            {Tokens.Assign, "\\="},
            {Tokens.NotEq, "\\!="},
            {Tokens.Comma, "\\,"},
            {Tokens.Dot, "\\."},
            {Tokens.Identifier, "[_a-zA-Z][_a-zA-Z\\d]*"},
            {Tokens.Semicolon, "\\;"},
            {Tokens.Newline, "\\\n"}
        };
    }

    public LinkedList<Token> Parse(string code)
    {
        var matchMap = new SortedDictionary<Tokens, MatchCollection>();

        foreach (var pair in tokenRegexMap)
        {
            matchMap.Add(pair.Key, Regex.Matches(code, pair.Value));
        }

        var sorted = CreateSortedDict(matchMap);

        var list = new LinkedList<Token>();
        int index = 0;
        int lineNumber = 1;
        int lastNewlineIndex = 0;

        foreach (var pair in sorted)
        {
            if (pair.Key < index)
            {
                continue;
            }

            if (pair.Value.type == Tokens.Newline)
            {
                //Console.WriteLine($"{pair.Key.ToString()} : {pair.Value.type}");
                lineNumber++;
                lastNewlineIndex = pair.Key + 1;
                index = pair.Key + 1;
                continue;
            }
            else if (pair.Value.type == Tokens.Comment)
            {
                index = pair.Key + pair.Value.value.Length;
                continue;
            }

            pair.Value.lineNumber = lineNumber;
            pair.Value.rowNumber = index - lastNewlineIndex;
            var nextNewLineIndex = FindNextNewline(code, index);
            pair.Value.line = code.Substring(lastNewlineIndex, nextNewLineIndex - lastNewlineIndex);

            //Console.WriteLine($"{pair.Key.ToString()} : {pair.Value.type} : {pair.Value.value} : \"{pair.Value.line}\"");
            list.AddLast(pair.Value);
            index = pair.Key + pair.Value.value.Length;
        }

        return list;
    }

    SortedDictionary<int, Token> CreateSortedDict(SortedDictionary<Tokens, MatchCollection> matchMap)
    {
        var sorted = new SortedDictionary<int, Token>();

        foreach (var pair in matchMap)
        {
            foreach (Match match in pair.Value)
            {
                if (sorted.ContainsKey(match.Index))
                {
                    continue;
                }

                sorted.Add(match.Index, new Token(pair.Key, match.Value));
            }
        }

        return sorted;
    }

    int FindNextNewline(string code, int index)
    {
        for (var i = index; i < code.Length; i++)
        {
            if (code[i] == '\n')
            {
                return i;
            }
        }
        return code.Length;
    }
}
