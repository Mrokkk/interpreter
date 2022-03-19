using System;

public class TryBlock : Statement
{
    public Block block;

    public TryBlock(Block block)
    {
        this.block = block;
    }

    public override void Execute(ExecutionContext context)
    {
        if (context.programCounter.Next == null || !(context.programCounter.Next.Value is CatchBlock))
        {
            throw new Exception("Expected catch block after try"); // FIXME: should be syntax error
        }

        ExecuteBlock(block, context.programCounter.Next, context);
    }
}
