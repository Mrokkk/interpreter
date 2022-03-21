using System;
using System.IO;
using System.Diagnostics;

public class Logging
{
    static Verbosity verbosity;
    static bool useColors;
    static TextWriter writer;

    static public void Initialize(Verbosity verbosity, bool useColors, TextWriter writer)
    {
        Logging.verbosity = verbosity;
        Logging.useColors = useColors;
        Logging.writer = writer;
    }

    static public Logger CreateLogger(string header)
    {
        if (verbosity == Verbosity.None)
        {
            return new LoggerStub();
        }
        return new LoggerImpl(header, verbosity, writer);
    }

    private class LoggerStub : Logger
    {
        public Logger Debug(string log, params object[] args)
        {
            return this;
        }

        public Logger Info(string log, params object[] args)
        {
            return this;
        }

        public Logger Error(string log, params object[] args)
        {
            return this;
        }
    }

    private class LoggerImpl : Logger
    {
        string header;
        Verbosity logLevel;
        TextWriter writer;

        public LoggerImpl(string header, Verbosity logLevel, TextWriter writer)
        {
            this.header = header;
            this.logLevel = logLevel;
            this.writer = writer;
        }

        public Logger Debug(string log, params object[] args)
        {
            if (logLevel != Verbosity.Debug)
            {
                return this;
            }

            return Log(log, LogLevel.DEBUG, args);
        }

        public Logger Info(string log, params object[] args)
        {
            return Log(log, LogLevel.INFO, args);
        }

        public Logger Error(string log, params object[] args)
        {
            return Log(log, LogLevel.ERROR, args);
        }

        // https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.defaultinterpolatedstringhandler?view=net-6.0
        // https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/tutorials/interpolated-string-handler
        private Logger Log(string log, LogLevel level, params object[] args)
        {
            var stackTrace = new StackTrace();

            writer.WriteLine(LogFormatter.Create(level)
                .AddTime(DateTime.Now)
                .AddLogLevel()
                .AddHeader(header, stackTrace.GetFrame(2).GetMethod().Name)
                .AddLog(log, args)
                .Get());

            return this;
        }
    }
}
