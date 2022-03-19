using NUnit.Framework;
using System.Collections.Generic;

namespace Tests
{

public class LexerTests
{
    Lexer sut;

    [SetUp]
    public void Setup()
    {
        sut = new Lexer();
    }

    void AssertToken(
        ref LinkedListNode<Lexer.Token> tokenNode,
        Lexer.Tokens tokenType,
        string value)
    {
        Assert.AreEqual(tokenType, tokenNode.Value.type);
        Assert.AreEqual(value, tokenNode.Value.value);
        tokenNode = tokenNode.Next;
    }

    [Test]
    public void CanParseSemicolon()
    {
        var currentToken = sut.Parse(";").First;
        AssertToken(ref currentToken, Lexer.Tokens.Semicolon, ";");
        Assert.AreEqual(currentToken, null);
    }

    [TestCase("identifier")]
    [TestCase("identifier2")]
    [TestCase("_identifier")]
    [TestCase("identifier_")]
    [TestCase("a_a_a")]
    [TestCase("a2a")]
    [TestCase("aif")]
    [TestCase("areturn")]
    [TestCase("ifa")]
    [TestCase("boolz")]
    [TestCase("nulll")]
    [TestCase("whilea")]
    [TestCase("elseifa")]
    [TestCase("elsea")]
    [TestCase("foreacha")]
    [TestCase("breaka")]
    [TestCase("returnb")]
    [TestCase("tryy")]
    [TestCase("catchh")]
    [TestCase("importt")]
    public void CanParseIdentifiers(string identifier)
    {
        var currentToken = sut.Parse(identifier).First;
        AssertToken(ref currentToken, Lexer.Tokens.Identifier, identifier);
        Assert.AreEqual(currentToken, null);
    }

    [TestCase("\"string\"", Lexer.Tokens.StringLiteral)]
    [TestCase("23", Lexer.Tokens.IntLiteral)]
    [TestCase("true", Lexer.Tokens.BoolLiteral)]
    [TestCase("false", Lexer.Tokens.BoolLiteral)]
    [TestCase("2.432f", Lexer.Tokens.FloatLiteral)]
    [TestCase("20.432f", Lexer.Tokens.FloatLiteral)]
    [TestCase("1093f", Lexer.Tokens.FloatLiteral)]
    [TestCase("41.f", Lexer.Tokens.FloatLiteral)]
    public void CanParseLiterals(string input, Lexer.Tokens type)
    {
        var tokens = sut.Parse(input);
        Assert.AreEqual(1, tokens.Count);
        Assert.AreEqual(type, tokens.First.Value.type);
        Assert.AreEqual(input, tokens.First.Value.value);
    }

    [TestCase("=>", Lexer.Tokens.Lambda)]
    [TestCase("+", Lexer.Tokens.Add)]
    [TestCase("-", Lexer.Tokens.Sub)]
    [TestCase("/", Lexer.Tokens.Div)]
    [TestCase("%", Lexer.Tokens.Mod)]
    [TestCase("*", Lexer.Tokens.Mul)]
    [TestCase("<=", Lexer.Tokens.LessEq)]
    [TestCase("<", Lexer.Tokens.Less)]
    [TestCase(">=", Lexer.Tokens.GreaterEq)]
    [TestCase(">", Lexer.Tokens.Greater)]
    [TestCase("==", Lexer.Tokens.Eq)]
    [TestCase("=", Lexer.Tokens.Assign)]
    [TestCase("!=", Lexer.Tokens.NotEq)]
    [TestCase(",", Lexer.Tokens.Comma)]
    [TestCase(".", Lexer.Tokens.Dot)]
    public void CanParseOperators(string input, Lexer.Tokens type)
    {
        var tokens = sut.Parse(input);
        Assert.AreEqual(1, tokens.Count);
        Assert.AreEqual(type, tokens.First.Value.type);
        Assert.AreEqual(input, tokens.First.Value.value);
    }

    [TestCase("if", Lexer.Tokens.If)]
    [TestCase("elseif", Lexer.Tokens.ElseIf)]
    [TestCase("else", Lexer.Tokens.Else)]
    [TestCase("for", Lexer.Tokens.For)]
    [TestCase("while", Lexer.Tokens.While)]
    [TestCase("return", Lexer.Tokens.Return)]
    [TestCase("try", Lexer.Tokens.Try)]
    [TestCase("catch", Lexer.Tokens.Catch)]
    [TestCase("import", Lexer.Tokens.Import)]
    [TestCase("function", Lexer.Tokens.Function)]
    [TestCase("break", Lexer.Tokens.Break)]
    [TestCase("null", Lexer.Tokens.Null)]
    public void CanParseKeywords(string input, Lexer.Tokens type)
    {
        var tokens = sut.Parse(input);
        Assert.AreEqual(1, tokens.Count);
        Assert.AreEqual(type, tokens.First.Value.type);
        Assert.AreEqual(input, tokens.First.Value.value);
    }

    [TestCase("// comment")]
    [TestCase("// multi\n// line\n// comment")]
    public void CanIgnoreComment(string input)
    {
        var tokens = sut.Parse(input);
        Assert.AreEqual(0, tokens.Count);
    }
}

}
