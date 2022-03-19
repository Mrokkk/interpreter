using System;

public class TypedExpression : Expression
{
    public Identifier type;
    public Expression expression;

    public TypedExpression(Identifier type, Expression expression)
    {
        this.type = type;
        this.expression = expression;
    }

    public override void Prepare(ExecutionContext context)
    {
        context.callStack.Push(this);
        expression.Prepare(context);
    }

    public override void Execute(ExecutionContext context)
    {
    }
}
