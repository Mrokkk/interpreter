using System;
using System.Collections.Generic;

public class Block : Statement
{
    public LinkedList<Statement> statements;

    public Block()
    {
        statements = new LinkedList<Statement>();
    }

    public void Add(Statement statement)
    {
        statements.AddLast(statement);
    }

    public override void Execute(ExecutionContext context)
    {
        context.currentFrame.returnAddress = context.programCounter.Next;
        var newFrame = new Frame();
        // Debug.Log("Pushing frame");
        context.stackFrame.Push(newFrame);
        context.programCounter = statements.First;
    }
}
