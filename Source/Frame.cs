using System.Collections.Generic;

public class Frame
{
    public Stack<Statement> callStack;
    public Dictionary<string, Symbol> symbols;
    public LinkedListNode<Statement> returnAddress;

    public Frame()
    {
        callStack = new Stack<Statement>();
        symbols = new Dictionary<string, Symbol>();
        returnAddress = null;
    }
}
