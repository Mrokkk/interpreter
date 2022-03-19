public class DebugInfo
{
    public int lineNumber;
    public string line;
    public string fileName;

    public DebugInfo(int lineNumber, string line, string fileName)
    {
        this.lineNumber = lineNumber;
        this.line = line.Trim();
        this.fileName = fileName;
    }
}
