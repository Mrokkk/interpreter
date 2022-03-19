using System;

public class Return : Statement
{
    public Expression returnValue;

    public Return(Expression returnValue)
    {
        this.returnValue = returnValue;
    }

    public override void Prepare(ExecutionContext context)
    {
        context.callStack.Push(this);
        returnValue.Prepare(context);
    }

    public override void Execute(ExecutionContext context)
    {
        context.stackFrame.Pop();
        context.programCounter = context.currentFrame.returnAddress;
    }
}
