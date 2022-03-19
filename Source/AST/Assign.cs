using System;

public class Assign : Statement
{
    public Expression left;
    public Expression right;

    public Assign(Expression left, Expression right)
    {
        this.left = left;
        this.right = right;
    }

    public override void Prepare(ExecutionContext context)
    {
        var leftSide = left as Identifier ?? throw new Exception("Identifier expected for assignment");
        var rightSide = right;

        context.callStack.Push(this);
        rightSide.Prepare(context);
    }

    public override void Execute(ExecutionContext context)
    {
        var left = this.left as Identifier;
        var right = context.valueStack.Pop() as Symbol;
        var frame = context.FindFrameWithSymbol(left.name, out var symbol) ?? context.currentFrame;

        var rightType = right.type;

        if (symbol != null && symbol.readOnly)
        {
            if (frame == context.currentFrame)
            {
                throw new Exception($"{left.name} is read only");
            }
            else
            {
                frame = context.currentFrame;
            }
        }

        if (symbol != null && symbol.type != rightType)
        {
            symbol.value = CastingSystem.Cast(rightType, symbol.type, right.value);
        }
        else
        {
            frame.symbols[left.name] = new Symbol(right.value, right.type);
        }
    }
}
