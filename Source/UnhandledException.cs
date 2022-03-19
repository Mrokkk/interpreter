using System;
using System.Collections.Generic;

[Serializable]
class UnhandledException : Exception
{
    public Exception originalException;
    public List<Statement> backtrace;

    public UnhandledException(Exception originalException, List<Statement> backtrace)
        : base(originalException.Message)
    {
        this.originalException = originalException;
        this.backtrace = backtrace;
    }

    public UnhandledException(string message, Exception inner)
        : base(message, inner)
    {
    }

    protected UnhandledException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context)
        : base(info, context)
    {
    }
}
