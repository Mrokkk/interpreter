using System;
using System.IO;
using System.Collections.Generic;

using ParseTree = System.Collections.Generic.LinkedList<Statement>;

public class Interpreter
{
    Action<string> print;
    SystemPath path;
    string fileName;
    Parser parser;
    Executor executor;
    Logger logger;

    public Interpreter(Action<string> print, SystemPath path, string fileName)
    {
        this.print = print;
        this.path = path;
        this.fileName = fileName ?? "<unnamed>";
        this.parser = new Parser(print, path, this.fileName);
        this.executor = new Executor(print);
        this.logger = Logging.CreateLogger(this.GetType().Name);
    }

    public void Start(TextReader reader, TextWriter writer)
    {
        if (reader == Console.In)
        {
            logger.Debug("interactive mode selected");
            Interactive(reader, writer);
        }
        else
        {
            logger.Debug("non-interactive mode selected");
            var parseTree = parser.Parse(reader.ReadToEnd());
            executor.Execute(parseTree);
        }
    }

    void Interactive(TextReader reader, TextWriter writer)
    {
        writer.WriteLine("Interactive console");
        writer.Write(">>> ");
        Console.CancelKeyPress += delegate
        {
            Console.WriteLine("KeyboardInterruption");
        };

        string line;
        while ((line = reader.ReadLine()) != null)
        {
            var parseTree = parser.Parse(line + '\n');
            if (parseTree is null || parseTree.Count == 0)
            {
                writer.Write("... ");
            }
            else
            {
                executor.Execute(parseTree);
                writer.Write(">>> ");
            }
        }
    }
}
