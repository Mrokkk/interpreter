using System;

public class ElseBlock : Statement
{
    public Block block;

    public ElseBlock(Block block)
    {
        this.block = block;
    }

    public override void Prepare(ExecutionContext context)
    {
        if (context.programCounter.Previous is null ||
            (context.programCounter.Previous.Value.GetType() != typeof(IfBlock) &&
             context.programCounter.Previous.Value.GetType() != typeof(ElseIfBlock)))
        {
            throw new Exception("Unexpected else");
        }
        context.callStack.Push(this);
    }

    public override void Execute(ExecutionContext context)
    {
        ExecuteBlock(
            block,
            context.programCounter.Next,
            context);
    }
}
