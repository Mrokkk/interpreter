using System.Collections.Generic;

public class ExecutionContext
{
    public Stack<Frame> stackFrame;
    public Stack<dynamic> valueStack;
    public LinkedListNode<Statement> programCounter;
    public Builtins builtins;

    public Frame currentFrame => stackFrame.Peek();
    public Stack<Statement> callStack => stackFrame.Peek().callStack;

    public ExecutionContext(Stack<Frame> frameStack, Builtins builtins)
    {
        this.stackFrame = frameStack;
        this.builtins = builtins;
        this.valueStack = new Stack<dynamic>();
        this.programCounter = null;
    }

    public Symbol FindSymbol(string name)
    {
        foreach (var frame in stackFrame)
        {
            if (frame.symbols.ContainsKey(name))
            {
                return frame.symbols[name];
            }
        }
        return null;
    }

    public Frame FindFrameWithSymbol(string name, out Symbol symbol)
    {
        foreach (var frame in stackFrame)
        {
            if (frame.symbols.ContainsKey(name))
            {
                symbol = frame.symbols[name];
                return frame;
            }
        }
        symbol = null;
        return null;
    }
}
