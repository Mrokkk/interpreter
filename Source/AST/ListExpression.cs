using System;
using System.Linq;
using System.Collections.Generic;

public class ListExpression : Expression
{
    Identifier type;
    List<Expression> expressions;

    public ListExpression(Identifier type)
    {
        this.type = type;
        expressions = new List<Expression>();
    }

    public void Add(Expression expression)
    {
        expressions.Add(expression);
    }

    public override void Prepare(ExecutionContext context)
    {
        context.callStack.Push(this);
        expressions.ForEach(e => e.Prepare(context));
    }

    public override void Execute(ExecutionContext context)
    {
        dynamic list = null;
        Types? listType = null;
        var typeInfo = CastingSystem.GetTypeInfo(type.name);
        switch (typeInfo.type)
        {
            case Types.Int:
            {
                listType = Types.IntList;
                list = new List<int>();
                break;
            }
            case Types.Float:
            {
                listType = Types.FloatList;
                list = new List<float>();
                break;
            }
            case Types.String:
            {
                listType = Types.StringList;
                list = new List<string>();
                break;
            }
        }
        for (var i = 0; i < expressions.Count; ++i)
        {
            list.Add(context.valueStack.Pop().value);
        }
        if (listType is null)
        {
            throw new Exception($"Invalid type given: {type.name}");
        }
        context.valueStack.Push(new Symbol(list, listType.Value));
    }
}
