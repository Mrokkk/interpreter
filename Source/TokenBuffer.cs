using System;
using System.Collections;
using System.Collections.Generic;

using TokenNode = System.Collections.Generic.LinkedListNode<Lexer.Token>;

public class TokenBuffer
{
    bool isInteractive;
    Action<string> print;
    LinkedList<Lexer.Token> tokens;
    Stack<Lexer.Tokens> bracesStack;

    public TokenBuffer(bool isInteractive, Action<string> print)
    {
        this.isInteractive = isInteractive;
        this.print = print;
        tokens = new LinkedList<Lexer.Token>();
        bracesStack = new Stack<Lexer.Tokens>();
    }

    public void Add(LinkedList<Lexer.Token> newTokens)
    {
        if (!isInteractive)
        {
            tokens = newTokens;
            return;
        }

        try
        {
            Preprocess(newTokens);
        }
        catch (SyntaxError e)
        {
            print($"{typeof(SyntaxError)}:{e.token.lineNumber}:{e.token.rowNumber} : {e.Message}: \"{e.token.value}\"");
            print($"\t{e.token.line}");
            tokens.Clear();
            bracesStack.Clear();
        }
    }

    public TokenNode GetFirstNode()
    {
        return bracesStack.Count > 0
            ? null
            : tokens.First;
    }

    void Preprocess(LinkedList<Lexer.Token> newTokens)
    {
        foreach (var tempToken in newTokens)
        {
            var type = tempToken.type;
            Lexer.Tokens? firstTokenOnStack = null;

            if (bracesStack.Count > 0)
            {
                firstTokenOnStack = bracesStack.Peek();
            }

            if (type == Lexer.Tokens.LeftBrace || type == Lexer.Tokens.LeftParenth)
            {
                if (type == Lexer.Tokens.LeftBrace && firstTokenOnStack.IsAnyOf(Lexer.Tokens.If, Lexer.Tokens.Function))
                {
                    bracesStack.Pop();
                }
                bracesStack.Push(type);
            }
            else if (type == Lexer.Tokens.If || type == Lexer.Tokens.Function)
            {
                bracesStack.Push(type);
            }
            else if (type == Lexer.Tokens.RightBrace || type == Lexer.Tokens.RightParenth)
            {
                if (bracesStack.Count == 0 || bracesStack.Peek() != GetOppositeToken(type))
                {
                    throw new SyntaxError("Unexpected token", tempToken);
                }
                else
                {
                    bracesStack.Pop();
                }
            }

            tokens.AddLast(tempToken);
        }
    }

    public void Clear()
    {
        tokens.Clear();
        bracesStack.Clear();
    }

    Lexer.Tokens GetOppositeToken(Lexer.Tokens type)
    {
        switch (type)
        {
            case Lexer.Tokens.RightBrace: return Lexer.Tokens.LeftBrace;
            case Lexer.Tokens.RightParenth: return Lexer.Tokens.LeftParenth;
        }
        throw new Exception($"Unexpected token given to the GetOppositeToken: {type.ToString()}");
    }
}
