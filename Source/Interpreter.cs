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

    public Interpreter(Action<string> print, SystemPath path, string fileName)
    {
        this.print = print;
        this.path = path;
        this.fileName = fileName ?? "<unnamed>";
        parser = new Parser(print, path, this.fileName);
        executor = new Executor(print);
    }

    public void Start(TextReader reader, TextWriter writer)
    {
        if (reader == Console.In)
        {
            Interactive(reader, writer);
        }
        else
        {
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
