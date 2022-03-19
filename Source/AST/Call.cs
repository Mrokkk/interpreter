using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

public class Call : Expression
{
    public string name;
    public List<Expression> arguments;

    public Call(string name, List<Expression> arguments)
    {
        this.name = name;
        this.arguments = arguments;
    }

    public override void Prepare(ExecutionContext context)
    {
        context.callStack.Push(this);
        foreach (var arg in arguments)
        {
            arg.Prepare(context);
        }
    }

    public override void Execute(ExecutionContext context)
    {
        var type = CastingSystem.GetTypeInfo(name);

        if (type != null)
        {
            if (this.arguments.Count != 1)
            {
                throw new Exception($"Invalid parameters passed to \"{name}\" number; expected 1");
            }

            var desiredType = CastingSystem.GetTypeInfo(name).type;
            var expressionSymbol = context.valueStack.Pop();
            if (expressionSymbol.type != desiredType)
            {
                var castedValue = CastingSystem.Cast(expressionSymbol.type, desiredType, expressionSymbol.value);
                expressionSymbol.value = castedValue;
                expressionSymbol.type = desiredType;
            }
            context.valueStack.Push(expressionSymbol);

            return;
        }

        var symbol = context.FindSymbol(name) ?? throw new Exception($"No such function: \"{name}\"");

        var function = (symbol as FunctionObject) ?? throw new Exception($"{name} is not a function");

        var definedArguments = function.argumentNames;

        if (this.arguments.Count() < definedArguments.Count)
        {
            throw new Exception($"{name} expects {definedArguments.Count} arguments; {this.arguments.Count()} passed");
        }

        var arguments = new dynamic[this.arguments.Count];
        for (var i = 0; i < this.arguments.Count; ++i)
        {
            var value = context.valueStack.Pop();
            arguments[i] = value.value;
        }

        if (function.value is Block block)
        {
            if (context.programCounter.Next is null)
            {
                context.programCounter.List.AddLast(new Nop()); // FIXME: hack for being able to find out the backtrace
            }

            context.currentFrame.returnAddress = context.programCounter.Next;

            var newFrame = new Frame();
            for (var i = 0; i < definedArguments.Count; ++i)
            {
                newFrame.symbols[definedArguments[i]] = new Symbol(
                    arguments[i],
                    CastingSystem.FromNative(arguments[i].GetType()));
            }

            context.stackFrame.Push(newFrame);
            context.programCounter = block.statements.First;
        }
        else // builtin
        {
            // FIXME: it will not be seen in the backtrace
            arguments.Reverse();
            var builtins = context.builtins;
            var method = typeof(Builtins).GetMethod(name, BindingFlags.Instance | BindingFlags.Public)
                ?? throw new Exception($"Internal error: Cannot find method: {name}");

            var expectedParameters = method.GetParameters();
            if (expectedParameters.Count() != arguments.Count())
            {
                throw new Exception($"{name} expects {expectedParameters.Count()} arguments; {arguments.Count()} passed");
            }

            dynamic returnValue = null;
            try
            {
                returnValue = method.Invoke(builtins, arguments);
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }

            if (returnValue is null)
            {
                context.valueStack.Push(new Symbol(null, Types.Null));
            }
            else
            {
                context.valueStack.Push(new Symbol(returnValue, CastingSystem.FromNative(returnValue.GetType())));
            }
        }
    }
}
