using System;
using System.Collections.Generic;

public class Statement
{
    public DebugInfo debugInfo;

    public virtual void Prepare(ExecutionContext context)
    {
        context.callStack.Push(this);
    }

    public virtual void Execute(ExecutionContext context)
    {
        throw new Exception($"Execute not implemented by: {this.GetType()}");
    }

    // FIXME: move to another class
    public void ExecuteBlock(Block block, LinkedListNode<Statement> returnAddress, ExecutionContext context)
    {
        context.currentFrame.returnAddress = returnAddress;
        var newFrame = new Frame();
        context.stackFrame.Push(newFrame);
        context.programCounter = block.statements.First;
    }
}
