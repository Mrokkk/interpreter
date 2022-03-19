using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class Executor
{
    Action<string> print;
    ExecutionContext context;
    Builtins builtins;

    public Executor(Action<string> print)
    {
        this.print = print;

        CastingSystem.Initialize(); // FIXME: make class non-static

        var stackFrame = new Stack<Frame>();
        var globalFrame = new Frame();
        stackFrame.Push(globalFrame);

        builtins = new Builtins(print);
        context = new ExecutionContext(stackFrame, builtins);
        foreach (var symbolName in builtins.GetFunctions())
        {
            context.currentFrame.symbols[symbolName] = new FunctionObject(null, null); // FIXME
        }
    }

    public void Execute(LinkedList<Statement> statements)
    {
        context.programCounter = statements.First;

        try
        {
            MainLoop();
        }
        catch (UnhandledException e)
        {
            PrintBacktrace(e.backtrace);
            print($"{e.originalException}");
        }
    }

    void MainLoop()
    {
        while (true)
        {
            if (AlignProgramCounter())
            {
                // If programCounter after dropping frames is still null, break
                break;
            }

            context.programCounter.Value.Prepare(context);

            var initialProgramCounter = context.programCounter;

            while (context.callStack.Count > 0)
            {
                try
                {
                    context.callStack.Pop().Execute(context);
                }
                catch (Exception e)
                {
                    HandleException(e);
                }
            }

            // If jump was performed, do not increment counter, as it already points to next instruction
            if (context.programCounter == initialProgramCounter)
            {
                // FIXME: how to align valueStack after dropping frame (e.g. after
                // exception or after calling function without using its return value)
                //context.valueStack.Clear();
                context.programCounter = context.programCounter.Next;
            }
        }
    }

    bool AlignProgramCounter()
    {
        // Pop frames until the one with returnAddress is found
        while (context.programCounter == null && context.stackFrame.Count > 1)
        {
            context.stackFrame.Pop();
            if ((context.programCounter = context.currentFrame.returnAddress) != null)
            {
                return false;
            }
        }
        return context.programCounter == null;
    }

    void HandleException(Exception exception)
    {
        var backtrace = new List<Statement>();
        backtrace.Add(context.programCounter.Value);
        while (context.stackFrame.Count > 1)
        {
            context.stackFrame.Pop();
            if (context.currentFrame.returnAddress != null &&
                context.currentFrame.returnAddress.Previous != null)
            {
                if (context.currentFrame.returnAddress.Previous.Value is Call call)
                {
                    backtrace.Add(call);
                }
            }
            if (context.currentFrame.returnAddress != null &&
                context.currentFrame.returnAddress.Value is CatchBlock)
            {
                context.programCounter = context.currentFrame.returnAddress;
                context.valueStack.Push(exception);
                return;
            }
        }

        context.valueStack.Clear();
        context.callStack.Clear();
        throw new UnhandledException(exception, backtrace);
    }

    void PrintBacktrace(List<Statement> backtrace)
    {
        print("Backtrace (most recent call first):");
        foreach (var symbol in backtrace)
        {
            if (symbol.debugInfo != null)
            print($"    {symbol.debugInfo.line} at {symbol.debugInfo.fileName}:{symbol.debugInfo.lineNumber}");
        }
    }
}
