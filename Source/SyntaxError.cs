[System.Serializable]
class SyntaxError : System.Exception
{
    public Lexer.Token token;

    public SyntaxError(string message, Lexer.Token token)
        : base(FormatException(message, token))
    {
        this.token = token;
    }

    public SyntaxError(string message, System.Exception inner)
        : base(message, inner)
    {
    }

    protected SyntaxError(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context)
        : base(info, context)
    {
    }

    static string FormatException(string message, Lexer.Token token)
    {
        return $"{token.lineNumber}:{token.rowNumber} : {message}: {token.value}\n\t{token.line}";
    }
}
