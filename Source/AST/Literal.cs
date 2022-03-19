using System;

public class Literal<T> : Expression
{
    public T value;

    public Literal(T value)
    {
        this.value = value;
    }

    public override void Execute(ExecutionContext context)
    {
        context.valueStack.Push(new Symbol(value, CastingSystem.FromNative(value.GetType())));
    }
}
