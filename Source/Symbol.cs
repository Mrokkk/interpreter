using System;
using System.Collections.Generic;

public enum Types
{
    Null,
    Int,
    Float,
    Double,
    String,
    Bool,
    Function,
    Exception,
    List,
    IntList,
    FloatList,
    StringList,
}

public class Symbol
{
    public dynamic value;
    public Types type;
    public bool readOnly;

    public Symbol(dynamic value, Types type, bool readOnly = false)
    {
        this.value = value;
        this.type = type;
        this.readOnly = readOnly;
    }
}

public class FunctionObject : Symbol
{
    public List<string> argumentNames;

    public FunctionObject(object callable, List<string> argumentNames)
        : base(callable, Types.Function, true)
    {
        this.argumentNames = argumentNames ?? new List<string>();
    }
}

public class TypeInfo
{
    public Types type;
    public Type nativeType;
    public string name;

    public TypeInfo(Types type, Type nativeType, string name)
    {
        this.type = type;
        this.nativeType = nativeType;
        this.name = name;
    }

    public override string ToString()
    {
        return name;
    }
}
