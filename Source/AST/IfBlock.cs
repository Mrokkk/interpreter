using System;

public class IfBlock : ConditionalBlock
{
    public IfBlock(Expression condition, Block block)
        : base(condition, block)
    {
    }

    public override void Prepare(ExecutionContext context)
    {
        context.callStack.Push(this);
        condition.Prepare(context);
    }

    public override void Execute(ExecutionContext context)
    {
        var condition = (context.valueStack.Pop() as Symbol).value;
        if (condition)
        {
            ExecuteBlock(
                block,
                FindNextNotConditionalStatement(context.programCounter.Next),
                context);
        }
        else
        {
            context.programCounter = context.programCounter.Next;
        }
    }
}
