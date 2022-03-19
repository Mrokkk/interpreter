using System;
using System.Collections.Generic;

public class Import : Statement
{
    LinkedList<Statement> statements;

    public Import(LinkedList<Statement> statements)
    {
        this.statements = statements;
    }

    public override void Execute(ExecutionContext context)
    {
        bool isInitialized = false;
        var programCounter = statements.First;
        while (programCounter != null)
        {
            if (programCounter.Value is Function function)
            {
                if (function.call.name == "init")
                {
                    if (isInitialized)
                    {
                        throw new Exception("init already declared");
                    }
                    if (function.call.arguments.Count == 0)
                    {
                        isInitialized = true;
                        ExecuteBlock(function.functionBody, context.programCounter.Next, context);
                    }
                }
                else
                {
                    function.Execute(context);
                }
            }
            programCounter = programCounter.Next;
        }
    }
}
