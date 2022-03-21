public class DebugInfo
{
    public int lineNumber;
    public int linePosition;
    public string line;
    public string fileName;

    public DebugInfo(int lineNumber, int linePosition, string line, string fileName)
    {
        this.lineNumber = lineNumber;
        this.linePosition = linePosition;
        this.line = line.Trim();
        this.fileName = fileName;
    }
}
