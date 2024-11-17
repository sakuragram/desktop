using System;
using System.Diagnostics;
using static sakuragram.Services.Core.Logger.LogType;
using Debug = System.Diagnostics.Debug;

namespace sakuragram.Services.Core;

public class Logger
{
    public enum LogType
    {
        Error,
        Info,
        Warning,
        Debug,
        Fatal
    }
    
    public static void Log(LogType type, object value)
    {
        Console.ForegroundColor = type switch
        {
            Error => ConsoleColor.Red,
            Info => ConsoleColor.Cyan,
            Warning => ConsoleColor.Yellow,
            LogType.Debug => ConsoleColor.Magenta,
            Fatal => ConsoleColor.DarkRed,
            _ => Console.ForegroundColor
        };
        
        Debug.WriteLine(type + ": " + value);
        Console.ResetColor();
    }
}