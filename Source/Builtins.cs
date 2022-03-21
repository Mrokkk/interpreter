using System;
using System.Linq;
using System.Collections.Generic;

public class Builtins
{
    Action<string> printFunction;

    public Builtins(Action<string> printFunction)
    {
        this.printFunction = printFunction;
    }

    public string[] GetFunctions()
    {
        return new string[]{"print", "toString", "random", "throw"};
    }

    public string toString(dynamic argument)
    {
        if (argument is null)
        {
            throw new Exception("Empty argument list passed");
        }

        if (argument == null)
        {
            return "<null>";
        }
        else if (argument is List<int> intList)
        {
            return FormatArguments(intList);
        }
        else if (argument is List<string> stringList)
        {
            return FormatArguments(stringList);
        }
        else
        {
            return argument.ToString();
        }
    }

    string FormatArguments<T>(List<T> list)
    {
        string s = "";
        list.ForEach(e => s += s.Length == 0 ? e.ToString() : $", {e.ToString()}");
        return $"[{s}]";
    }

    public object print(dynamic argument)
    {
        printFunction(toString(argument));
        return null;
    }

    public object random()
    {
        var rnd = new Random();
        return rnd.Next();
    }

    public object @throw(string exception)
    {
        throw new Exception(exception);
    }
}
