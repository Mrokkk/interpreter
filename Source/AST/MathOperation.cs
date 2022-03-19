using System;

public class MathOperation : Expression
{
    public Operator op;
    public Expression left;
    public Expression right;

    public MathOperation(Operator op, Expression left, Expression right)
    {
        this.op = op;
        this.left = left;
        this.right = right;
    }

    public override void Prepare(ExecutionContext context)
    {
        var leftSide = left;
        var rightSide = right;

        context.callStack.Push(this);
        rightSide.Prepare(context);
        leftSide.Prepare(context);
    }

    public override void Execute(ExecutionContext context)
    {
        var right = context.valueStack.Pop() as Symbol;
        var left = context.valueStack.Pop() as Symbol;

        var result = PerformOperation(op, left.value, CastingSystem.Cast(right.type, left.type, right.value));
        context.valueStack.Push(new Symbol(result, left.type));
    }

    static public dynamic PerformOperation(Operator op, dynamic left, dynamic right) => op switch
    {
        Operator.Add => left + right,
        Operator.Sub => left - right,
        Operator.Mul => left * right,
        Operator.Div => left / right,
        Operator.Mod => left % right,
        Operator.Eq => left == right,
        Operator.NotEq => left != right,
        Operator.Less => left < right,
        Operator.LessEq => left <= right,
        Operator.Greater => left > right,
        Operator.GreaterEq => left >= right,
        _ => throw new Exception($"Unsupported operation: {op.ToString()}")
    };
}
