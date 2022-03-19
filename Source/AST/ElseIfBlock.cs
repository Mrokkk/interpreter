using System;

public class ElseIfBlock : ConditionalBlock
{
    public ElseIfBlock(Expression condition, Block block)
        : base(condition, block)
    {
    }

    public override void Prepare(ExecutionContext context)
    {
        if (context.programCounter.Previous is null ||
            (context.programCounter.Previous.Value.GetType() != typeof(IfBlock) &&
             context.programCounter.Previous.Value.GetType() != typeof(ElseIfBlock)))
        {
            throw new Exception("Unexpected elseif");
        }
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
