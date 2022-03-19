using System;

public class WhileBlock : Statement
{
    public Expression condition;
    public Statement statement;

    public WhileBlock(Expression condition, Statement statement)
    {
        this.condition = condition;
        this.statement = statement;
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
                statement as Block,
                context.programCounter,
                context);
        }
        else
        {
            context.programCounter = context.programCounter.Next;
        }
    }
}
