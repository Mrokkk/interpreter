using System;
using System.Text;

public class Formatter
{
    StringBuilder line = new StringBuilder();

    // TODO: use .NET's Console.ForegroundColor as ANSI escape codes
    // are not supported on Windows (neither powershell nor cmd); it
    // will be harder to implement in-line color changing though; maybe
    // just fallback to whole line coloring?
    public enum Color
    {
        Default = 0,
        Black = 30,
        Red = 31,
        Green = 32,
        Yellow = 33,
        Blue = 34,
        Purple = 35,
        Cyan = 36,
        White = 37,
    }

    public Formatter Add(string text, Color color = Color.Default)
    {
        if (color != Color.Default)
        {
            line.AppendFormat("\u001b[{0};1m{1}\u001b[0m", color.ToString("d"), text);
        }
        else
        {
            line.Append(text);
        }
        return this;
    }

    public Formatter AddFormat(string text, Color color, params object[] args)
    {
        if (color != Color.Default)
        {
            line.AppendFormat("\u001b[{0};1m", color.ToString("d"))
                .AppendFormat(text, args)
                .Append("\u001b[0m");
        }
        else
        {
            line.AppendFormat(text, args);
        }
        return this;
    }

    public string Get()
    {
        return line.ToString();
    }
}
