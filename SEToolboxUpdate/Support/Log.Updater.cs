using System;

namespace SEToolbox.Support;

partial class Log
{
    public static void Fatal(string message, Exception exception)
    {
        WriteLine(message, LogLevel.FATAL, exception);
    }
}
