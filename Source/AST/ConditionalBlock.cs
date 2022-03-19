using System;
using System.Collections.Generic;

public class ConditionalBlock : Statement
{
    public Expression condition;
    public Block block;

    public ConditionalBlock(Expression condition, Block block)
    {
        this.condition = condition;
        this.block = block;
    }

    protected LinkedListNode<Statement> FindNextNotConditionalStatement(
        LinkedListNode<Statement> counter)
    {
        while (counter != null && (counter.Value is ElseIfBlock || counter.Value is ElseBlock))
        {
            counter = counter.Next;
        }
        return counter;
    }
}
