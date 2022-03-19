using System;
using System.Linq;
using System.Collections.Generic;

public class Function : Statement
{
    public Call call;
    public Block functionBody;

    public Function(Call call, Block functionBody)
    {
        this.call = call;
        this.functionBody = functionBody;
    }

    public override void Execute(ExecutionContext context)
    {
        if (context.currentFrame.symbols.ContainsKey(call.name))
        {
            throw new Exception($"{call.name} is already defined");
        }

        context.currentFrame.symbols[call.name] = new FunctionObject(
            functionBody,
            call.arguments.Select(arg => (arg as Identifier).name).ToList());
    }
}
