using System;

public class LogFormatter : Formatter
{
    LogLevel logLevel;
    Color color;
    string separator = " | ";

    public static LogFormatter Create(LogLevel logLevel)
    {
        return new LogFormatter(logLevel);
    }

    private LogFormatter(LogLevel logLevel)
    {
        this.logLevel = logLevel;
        this.color = ConvertLogLevelToColor(logLevel);
    }

    public LogFormatter AddTime(DateTime time)
    {
        this.Add(String.Format("{0:MM/dd/yy HH:mm:ss zzz}", time))
            .Add(separator);
        return this;
    }

    public LogFormatter AddLogLevel()
    {
        this.Add(logLevel.ToString(), color)
            .Add(separator);
        return this;
    }

    public LogFormatter AddHeader(string header, string methodName)
    {
        this.Add(header)
            .Add(".")
            .Add(methodName)
            .Add(separator);
        return this;
    }

    public LogFormatter AddLog(string log, params object[] args)
    {
        AddFormat(log, color, args);
        return this;
    }

    static Color ConvertLogLevelToColor(LogLevel logLevel) => logLevel switch
    {
        LogLevel.DEBUG => Color.Black,
        LogLevel.INFO => Color.Purple,
        LogLevel.ERROR => Color.Red,
        _ => throw new ArgumentOutOfRangeException()
    };
}
