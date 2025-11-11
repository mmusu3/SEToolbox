using System;
using System.IO;
using System.Threading;

namespace SEToolbox.Support;

public static partial class Log
{
    static StreamWriter writer;

    public static void Init(string fileName, bool appendFile = false)
    {
        writer = new StreamWriter(fileName, appendFile);
    }

    enum LogLevel
    {
        DEBUG, INFO, WARN, ERROR, FATAL,
    }

    static void WriteLine(string message, LogLevel level, Exception exception = null)
    {
        var thread = Thread.CurrentThread;
        var threadStr = thread.Name ?? thread.ManagedThreadId.ToString();
        var logStr = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} {level,-5} [{threadStr}] - {message}";
        var exStr = exception == null ? null : $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} {LogLevel.FATAL,-5} [{threadStr}] - {exception}";

        lock (writer)
        {
            writer.WriteLine(logStr);

            if (exStr != null)
                writer.WriteLine(exStr);

            writer.Flush();
        }
    }

    public static void Debug(string message)
    {
        WriteLine(message, LogLevel.DEBUG);
    }

    public static void Info(string message)
    {
        WriteLine(message, LogLevel.INFO);
    }

    public static void Warning(string message)
    {
        WriteLine(message, LogLevel.WARN);
    }

    public static void Warning(string message, Exception exception)
    {
        WriteLine(message, LogLevel.WARN, exception);
    }

    public static void Error(string message)
    {
        WriteLine(message, LogLevel.ERROR);
    }

    public static void Error(string message, Exception exception)
    {
        WriteLine(message, LogLevel.ERROR, exception);
    }
}
