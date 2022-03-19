using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using TokenNode = System.Collections.Generic.LinkedListNode<Lexer.Token>;
using ParseTree = System.Collections.Generic.LinkedList<Statement>;

public class Parser
{
    class TokenSelector<T>
        where T : class
    {
        TokenHead head;
        Func<TokenHead, T> action;

        public TokenSelector(TokenHead head)
        {
            this.head = head;
        }

        public TokenSelector<T> Case(
            Lexer.Tokens tokenType,
            Func<TokenHead, T> action)
        {
            if (head.Current != null && head.Current.type == tokenType)
            {
                this.action = action;
            }
            return this;
        }

        public T Hit()
        {
            return action != null
                ? action(head)
                : null;
        }
    }

    class TokenHead
    {
        TokenNode previous;
        TokenNode current;
        public Lexer.Token Current => current?.Value;
        public Lexer.Token Previous => previous?.Value;

        public TokenHead(TokenNode token)
        {
            this.previous = null;
            this.current = token;
        }

        public TokenHead MoveForward()
        {
            previous = current;
            current = current.Next;
            return this;
        }

        public TokenHead Assert(string message)
        {
            if (current is null)
            {
                throw new SyntaxError(message, Previous);
            }
            return this;
        }

        public TokenHead Assert(Lexer.Tokens type, string message)
        {
            if (current is null || Current.type != type)
            {
                throw new SyntaxError(message, Previous);
            }
            return this;
        }

        public TokenSelector<T> Switch<T>()
            where T : class
        {
            return new TokenSelector<T>(this);
        }
    }

    Action<string> print;
    SystemPath path;
    string fileName;
    Lexer lexer;
    TokenBuffer buffer;
    Stack<Lexer.Tokens> parenthesisStack;
    ParseTree parseTree;

    public Parser(Action<string> print, SystemPath path, string fileName)
    {
        this.print = print;
        this.path = path;
        this.fileName = fileName;

        lexer = new Lexer();
        buffer = new TokenBuffer(true, print);

        parenthesisStack = new Stack<Lexer.Tokens>();
    }

    public ParseTree Parse(string code)
    {
        buffer.Add(lexer.Parse(code));
        var token = buffer.GetFirstNode();

        parseTree = new ParseTree();
        if (token is null)
        {
            return null;
        }

        try
        {
            var head = new TokenHead(token);
            while (head.Current != null)
            {
                if (head.Current.type == Lexer.Tokens.Semicolon)
                {
                    head.MoveForward();
                    continue;
                }
                var statement = ParseStatement(head);
                parseTree.AddLast(statement);
            }
        }
        catch (SyntaxError e)
        {
            print($"{e.GetType()}: {e.Message}");
            buffer.Clear();
            parseTree.Clear();
            return parseTree;
        }

        buffer.Clear();
        return parseTree;
    }

    Statement ParseStatement(TokenHead head, string message = "Unexpected token after")
    {
        var token = head.Current;
        return WrapWithDebugInfo(
            head.Switch<Statement>()
                .Case(Lexer.Tokens.Identifier, ParseIdentifier)
                .Case(Lexer.Tokens.If, ParseIf)
                .Case(Lexer.Tokens.ElseIf, ParseElseIf)
                .Case(Lexer.Tokens.Else, ParseElse)
                .Case(Lexer.Tokens.While, ParseWhile)
                .Case(Lexer.Tokens.Break, ParseBreak)
                .Case(Lexer.Tokens.LeftBrace, ParseBlock)
                .Case(Lexer.Tokens.Function, ParseFunction)
                .Case(Lexer.Tokens.Return, ParseReturn)
                .Case(Lexer.Tokens.Try, ParseTry)
                .Case(Lexer.Tokens.Catch, ParseCatch)
                .Case(Lexer.Tokens.Import, ParseImport)
                .Hit() ?? throw new SyntaxError(message, head.Previous),
            token);
    }

    Statement ParseIdentifier(TokenHead head)
    {
        Expression identifier = new Identifier(head.Current.value);

        head.MoveForward();

        if (head.Current != null && head.Current.type == Lexer.Tokens.LeftBracket)
        {
            identifier = ParseIndexedIdentifier(identifier as Identifier, head);
        }

        return head.Switch<Statement>()
            .Case(Lexer.Tokens.Assign, head => ParseAssign(identifier, head))
            .Case(Lexer.Tokens.LeftParenth, head =>
            {
                var call = ParseCall(identifier as Identifier, head);
                ParseSemicolon(head);
                return call;
            })
            .Hit() ?? throw new SyntaxError("Expected assignment or function call after", head.Previous);
    }

    Statement ParseAssign(Expression identifier, TokenHead head)
    {
        parenthesisStack.Push(head.Current.type);

        head.MoveForward();

        var rightExpression = ParseExpression(head) ?? throw new SyntaxError("Expected expression after", head.Previous);
        ParseSemicolon(head, "Expected \";\" or operator after");

        parenthesisStack.Pop();

        return new Assign(identifier as Identifier, rightExpression);
    }

    Expression ParseCall(Identifier identifier, TokenHead head)
    {
        var call = ParseCallInternal(identifier, head);

        if (parenthesisStack.Count == 0 && (head.Current is null || (head.Current.type != Lexer.Tokens.Semicolon && TokenToOperator(head.Current) is null)))
        {
            throw new SyntaxError("Expected \";\" or operator after", head.Previous);
        }

        return call;
    }

    Expression ParseCallInternal(Identifier identifier, TokenHead head)
    {
        parenthesisStack.Push(head.Current.type);

        head.MoveForward();

        var arguments = new List<Expression>();
        while (head.Current != null && head.Current.type != Lexer.Tokens.RightParenth)
        {
            arguments.Add(ParseExpression(head));
            if (head.Current != null && head.Current.type == Lexer.Tokens.Comma)
            {
                head.MoveForward();
            }
        }

        head.Assert(Lexer.Tokens.RightParenth, "Expected \")\" after");
        var leftParenthesis = head.Current;
        head.MoveForward();

        parenthesisStack.Pop();

        return new Call(identifier.name, arguments);
    }

    Expression ParseIndexedIdentifier(Identifier identifier, TokenHead head)
    {
        head.MoveForward();

        var expression = ParseExpression(head) ?? throw new SyntaxError("Expression expected after", head.Previous);

        head.Assert(Lexer.Tokens.RightBracket, "Expected \"]\" after");
        head.MoveForward();

        return new IndexedIdentifier(identifier.name, expression);
    }

    Statement ParseIf(TokenHead head)
    {
        head.MoveForward();

        var expression = ParseExpression(head) ?? throw new SyntaxError("Expected expression after", head.Previous);

        head.Assert(Lexer.Tokens.LeftBrace, "Block expected after");

        var block = ParseBlock(head);

        return new IfBlock(expression, block as Block);
    }

    Statement ParseElseIf(TokenHead head)
    {
        head.MoveForward();

        var expression = ParseExpression(head) ?? throw new SyntaxError("Expected expression after", head.Previous);

        head.Assert(Lexer.Tokens.LeftBrace, "Block expected after");

        var block = ParseBlock(head);

        return new ElseIfBlock(expression, block as Block);
    }

    Statement ParseElse(TokenHead head)
    {
        head.MoveForward();
        head.Assert(Lexer.Tokens.LeftBrace, "Block expected after");

        var block = ParseBlock(head);

        return new ElseBlock(block as Block);
    }

    Statement ParseWhile(TokenHead head)
    {
        head.MoveForward();

        var expression = ParseExpression(head) ?? throw new SyntaxError("Expected expression after", head.Previous);

        if (head.Current.type != Lexer.Tokens.LeftBrace)
        {
            throw new SyntaxError("Block expected after", head.Previous);
        }

        var statement = ParseBlock(head);

        return new WhileBlock(expression, statement);
    }

    Statement ParseBreak(TokenHead head)
    {
        head.MoveForward();
        ParseSemicolon(head);
        return new Break();
    }

    Statement ParseBlock(TokenHead head)
    {
        head.MoveForward();

        var block = new Block();

        while (head.Current != null && head.Current.type != Lexer.Tokens.RightBrace)
        {
            var statement = ParseStatement(head, "Expected statement after");
            if (!(statement is Nop))
            {
                block.Add(statement);
            }
        }

        head.Assert(Lexer.Tokens.RightBrace, "Expected right brace after");
        head.MoveForward();

        return block;
    }

    Statement ParseFunction(TokenHead head)
    {
        head.MoveForward();
        head.Assert(Lexer.Tokens.Identifier, "Expected identifier after");

        var identifier = new Identifier(head.Current.value);

        head.MoveForward();

        var call = ParseCallInternal(identifier, head);

        head.Assert(Lexer.Tokens.LeftBrace, "Expected block after");

        return new Function(call as Call, ParseBlock(head) as Block);
    }

    Statement ParseReturn(TokenHead head)
    {
        var returnToken = head.Current;
        head.MoveForward();
        head.Assert("Expected \";\" or expression after");

        if (head.Current.type == Lexer.Tokens.Semicolon)
        {
            head.MoveForward();
            return new Return(new Identifier("null"));
        }

        var expression = ParseExpression(head);
        ParseSemicolon(head);

        return new Return(expression);
    }

    Statement ParseTry(TokenHead head)
    {
        head.MoveForward();
        head.Assert(Lexer.Tokens.LeftBrace, "Expected block after");
        return new TryBlock(ParseBlock(head) as Block);
    }

    Statement ParseCatch(TokenHead head)
    {
        head.MoveForward();

        head.Assert("Expected block or expression after");

        Identifier identifier = null;
        if (head.Current.type == Lexer.Tokens.LeftParenth)
        {
            head.MoveForward();
            head.Assert(Lexer.Tokens.Identifier, "Expected identifier after");

            identifier = new Identifier(head.Current.value);

            head.MoveForward();
            head.Assert(Lexer.Tokens.RightParenth, "Missing closing parenthesis after");
            head.MoveForward();
        }

        head.Assert(Lexer.Tokens.LeftBrace, "Expected block after");

        return new CatchBlock(identifier, ParseBlock(head) as Block);
    }

    Statement ParseImport(TokenHead head)
    {
        head.MoveForward();
        head.Assert(Lexer.Tokens.Identifier, "Expected module name after");

        var moduleName = head.Current.value;

        var pathToModule = path.Find(moduleName) ?? throw new Exception($"Cannot find {moduleName}");

        head.MoveForward();

        var parser = new Parser(this.print, this.path, pathToModule);
        var moduleParseTree = parser.Parse(File.ReadAllText(pathToModule));

        return new Import(moduleParseTree);
    }

    void ParseSemicolon(TokenHead head, string message = "Expected \";\" after")
    {
        head.Assert(Lexer.Tokens.Semicolon, message);
        head.MoveForward();
    }

    delegate Expression ParseDelagate(TokenHead head);

    Expression ParseExpression(TokenHead head)
    {
        return ParseGenericExpression(
            head,
            ParseTerm,
            Lexer.Tokens.Add,
            Lexer.Tokens.Sub,
            // FIXME: below operators should not be here
            Lexer.Tokens.Eq,
            Lexer.Tokens.NotEq,
            Lexer.Tokens.Less,
            Lexer.Tokens.LessEq,
            Lexer.Tokens.Greater,
            Lexer.Tokens.GreaterEq);
   }

    Expression ParseTerm(TokenHead head)
    {
        return ParseGenericExpression(
            head,
            ParseFactor,
            Lexer.Tokens.Mul,
            Lexer.Tokens.Div,
            Lexer.Tokens.Mod);
    }

    Expression ParseGenericExpression(
        TokenHead head,
        ParseDelagate func,
        params Lexer.Tokens[] parsedTokens)
    {
        var node = func(head);

        if (node is null)
        {
            return null;
        }

        if (head.Current is null)
        {
            return node;
        }

        while (head.Current.type.IsAnyOf(parsedTokens))
        {
            var op = TokenToOperator(head.Current) ?? throw new SyntaxError("No valid operator given after", head.Previous);

            head.MoveForward();
            head.Assert("Expected expression after");

            node = new MathOperation(op, node, func(head));

            if (head.Current is null)
            {
                break;
            }
        }

        return node;
    }

    Expression ParseFactor(TokenHead head)
    {
        if (head.Current is null)
        {
            return null;
        }

        if (head.Current.type == Lexer.Tokens.LeftParenth)
        {
            parenthesisStack.Push(head.Current.type);
            head.MoveForward();
            var node = ParseExpression(head);
            head.Assert(Lexer.Tokens.RightParenth, "Missing closing parenthesis after");
            parenthesisStack.Pop();
            head.MoveForward();

            return node;
        }

        var expression = ReadCurrentToken(head.Current) ?? throw new SyntaxError("Expected identifier or literal instead of", head.Current);
        head.MoveForward();
        head.Assert("Expected \";\" or expression");

        if (expression != null && expression is Identifier && head.Current.type == Lexer.Tokens.LeftBracket)
        {
            head.MoveForward();
            var list = new ListExpression(expression as Identifier);
            while (head.Current != null && head.Current.type != Lexer.Tokens.RightBracket)
            {
                list.Add(ParseExpression(head));
                if (head.Current is null || (head.Current.type != Lexer.Tokens.Comma && head.Current.type != Lexer.Tokens.RightBracket))
                {
                    throw new SyntaxError("Expected \",\" or \"]\"", head.Current);
                }
                if (head.Current.type == Lexer.Tokens.Comma)
                {
                    head.MoveForward();
                }
            }

            head.Assert(Lexer.Tokens.RightBracket, "Expected \"]\" after");
            head.MoveForward();

            return list;
        }

        if (head.Current != null && head.Current.type == Lexer.Tokens.LeftParenth)
        {
            if (!(expression is Identifier))
            {
                throw new SyntaxError("Expected identifier before", head.Current);
            }
            return ParseCall(expression as Identifier, head);
        }
        else if (head.Current != null && head.Current.type == Lexer.Tokens.Lambda)
        {
            if (!(expression is Identifier))
            {
                throw new SyntaxError("Expected identifier before", head.Current);
            }

            head.MoveForward();
            head.Assert(Lexer.Tokens.LeftBrace, "Expected block in lambda expression");

            var block = ParseBlock(head);

            return new Lambda(new List<Expression>{expression}, block as Block); // FIXME: allow multiple or zero args
        }

        return expression;
    }

    Expression ReadCurrentToken(Lexer.Token token) => token switch
    {
        var t when t.type == Lexer.Tokens.Identifier => new Identifier(token.AsIdentifier()),
        var t when t.type == Lexer.Tokens.IntLiteral => new Literal<int>(token.AsInt()),
        var t when t.type == Lexer.Tokens.BoolLiteral => new Literal<bool>(token.AsBool()),
        var t when t.type == Lexer.Tokens.FloatLiteral => new Literal<float>(token.AsFloat()),
        var t when t.type == Lexer.Tokens.StringLiteral => new Literal<string>(token.AsString()),
        var t when t.type == Lexer.Tokens.Null => new Null(),
        _ => null
    };

    Operator? TokenToOperator(Lexer.Token token)
    {
        Enum.TryParse<Operator>(token.type.ToString(), false, out var op);
        return op;
    }

    T WrapWithDebugInfo<T>(T statement, Lexer.Token token)
        where T : Statement
    {
        statement.debugInfo = new DebugInfo(
            token.lineNumber,
            token.line,
            fileName);
        return statement;
    }
}
