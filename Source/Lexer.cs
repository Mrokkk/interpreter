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
    Logger logger;

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
        logger = Logging.CreateLogger(this.GetType().Name);
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
        var previous = Tokens.Newline;

        foreach (var pair in sorted)
        {
            var tokenPosition = pair.Key;
            var token = pair.Value;

            if (tokenPosition < index)
            {
                continue;
            }

            if (pair.Value.type == Tokens.Newline)
            {
                lineNumber++;
                lastNewlineIndex = tokenPosition + 1;
                index = tokenPosition + 1;
                previous = Tokens.Newline;
                continue;
            }
            else if (pair.Value.type == Tokens.Comment)
            {
                index = tokenPosition + token.value.Length;
                previous = Tokens.Comment;
                continue;
            }

            var nextNewLineIndex = FindNextNewline(code, index);
            token.lineNumber = lineNumber;
            token.rowNumber = index - lastNewlineIndex; // FIXME: it does not work correctly
            token.line = code.Substring(lastNewlineIndex, nextNewLineIndex - lastNewlineIndex);

            if (previous == Tokens.Newline)
            {
                logger.Debug("{0}\t | line \"{1}\"", lineNumber, pair.Value.line);
            }

            logger.Debug("{0}:{1}\t | \t\t{2} : {3}",
                lineNumber,
                pair.Value.rowNumber,
                pair.Value.type,
                pair.Value.value);

            list.AddLast(token);
            index = tokenPosition + token.value.Length;
            previous = token.type;
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
