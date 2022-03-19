public class Null : Expression
{
    public override void Execute(ExecutionContext context)
    {
        context.valueStack.Push(new Symbol(null, Types.Null, true));
    }
}
