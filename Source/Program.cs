using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

using CommandLine;

public enum Verbosity
{
    Debug,
    Info,
    None
};

public class Options
{
    [Value(index: 0, Required = false, HelpText = "File with source code")]
    public string filename { get; set; }

    [Option(shortName: 'v', longName: "verbose", Required = false, HelpText = "Verbosity level (Debug, Info, None)", Default = Verbosity.None)]
    public Verbosity verbose { get; set; }
}

public class SystemPath
{
    List<string> path;

    public SystemPath()
    {
        path = new List<string>();
    }

    public void Add(string path)
    {
        this.path.Add(path);
    }

    public string Find(string moduleName)
    {
        foreach (var path in this.path)
        {
            var modulePath = Path.Combine(path, moduleName);
            if (File.Exists(modulePath))
            {
                return modulePath;
            }
        }
        return null;
    }
}

class Program
{
    static void Main(string[] args)
    {
        CommandLine.Parser.Default.ParseArguments<Options>(args)
            .WithParsed(RunParser)
            .WithNotParsed(HandleParseError);
    }

    static void RunParser(Options options)
    {
        Logging.Initialize(options.verbose, !Console.IsErrorRedirected, Console.Error);

        string fileName = options.filename != null
            ? Path.GetFullPath(options.filename)
            : null;

        var path = new SystemPath();

        if (fileName != null)
        {
            var directory = Path.GetDirectoryName(Path.GetFullPath(fileName));
            path.Add(directory);
        }

        path.Add(Directory.GetCurrentDirectory());

        var interpreter = new Interpreter(Console.WriteLine, path, fileName);

        using (var reader = GetReader(fileName))
        {
            interpreter.Start(reader, GetWriter());
        }
    }

    static void HandleParseError(IEnumerable<Error> errs)
    {
    }

    static TextReader GetReader(string fileName)
    {
        return fileName is null
            ? Console.In
            : new StreamReader(fileName) as TextReader;
    }

    static TextWriter GetWriter()
    {
        return Console.Out;
    }
}
