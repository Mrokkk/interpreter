using System;
using System.Collections;
using System.Collections.Generic;

public class Break : Statement
{
    public override void Execute(ExecutionContext context)
    {
        int framesToDrop = 0;
        bool found = false;
        foreach (var frame in context.stackFrame)
        {
            if (frame.returnAddress != null && frame.returnAddress.Value is WhileBlock)
            {
                found = true;
                break;
            }
            framesToDrop++;
        }

        if (!found)
        {
            throw new Exception("Unexpected break outside of loop");
        }

        context.stackFrame.Pop(framesToDrop);

        context.programCounter = context.currentFrame.returnAddress.Next;
    }
}
