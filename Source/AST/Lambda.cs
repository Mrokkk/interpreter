using System.Linq;
using System.Collections.Generic;

public class Lambda : Expression
{
    public List<Expression> arguments;
    public Block functionBody;

    public Lambda(List<Expression> arguments, Block functionBody)
    {
        this.arguments = arguments;
        this.functionBody = functionBody;
    }

    public override void Execute(ExecutionContext context)
    {
        context.valueStack.Push(new FunctionObject(
            functionBody,
            arguments.Select(arg => (arg as Identifier).name).ToList()));
    }
}
