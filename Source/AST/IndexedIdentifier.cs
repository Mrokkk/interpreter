using System;

public class IndexedIdentifier : Identifier
{
    Expression index;

    public IndexedIdentifier(string name, Expression index)
        : base(name)
    {
        this.name = name;
        this.index = index;
    }
}
