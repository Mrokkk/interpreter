using System;

public class CatchBlock : Statement
{
    public Identifier exception;
    public Block block;

    public CatchBlock(Identifier exception, Block block)
    {
        this.exception = exception;
        this.block = block;
    }

    public override void Execute(ExecutionContext context)
    {
        if (context.valueStack.Count == 0)
        {
            return;
        }
        var valueFromStack = context.valueStack.Peek();
        // FIXME: it is not a good solution
        if (valueFromStack is Exception caughtException)
        {
            context.valueStack.Pop();
            context.currentFrame.returnAddress = context.programCounter.Next;
            var newFrame = new Frame();
            if (exception != null)
            {
                newFrame.symbols[exception.name] = new Symbol(
                    caughtException.Message,
                    Types.String); // FIXME
            }
            context.stackFrame.Push(newFrame);
            context.programCounter = block.statements.First;
        }
    }
}
