using System;

public class ForBlock : ConditionalBlock
{
    Expression initExpression;
    Expression loopExpression;

    public ForBlock(
        Block block,
        Expression initExpression,
        Expression conditionalExpression,
        Expression loopExpression)
        : base(conditionalExpression, block)
    {
        this.initExpression = initExpression;
        this.loopExpression = loopExpression;
    }

    public override void Prepare(ExecutionContext context)
    {
    }

    public override void Execute(ExecutionContext context)
    {
    }
}
