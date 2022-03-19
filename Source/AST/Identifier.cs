using System;

public class Identifier : Expression
{
    public string name;

    public Identifier(string name)
    {
        this.name = name;
    }

    public override void Execute(ExecutionContext context)
    {
        var symbol = context.FindSymbol(name) ?? throw new Exception($"No such symbol: \"{name}\"");
        context.valueStack.Push(symbol);
    }
}
