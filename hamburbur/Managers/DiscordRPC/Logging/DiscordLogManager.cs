using System;
using UnityEngine;

namespace hamburbur.Managers.DiscordRPC.Logging;

public class DiscordDebug : ILogger
{
    public DiscordDebug()
    {
        Level    = LogLevel.Info;
        Coloured = false;
    }

    public DiscordDebug(LogLevel level) : this() => Level = level;

    public DiscordDebug(LogLevel level, bool coloured)
    {
        Level    = level;
        Coloured = coloured;
    }

    public bool Coloured { get; set; }

    [Obsolete("Use Coloured")]
    public bool Colored
    {
        get => Coloured;

        set => Coloured = value;
    }

    public LogLevel Level { get; set; }

    public void Trace(string message, params object[] args)
    {
        if (Level > LogLevel.Trace)
            return;

        if (Coloured)
            Console.ForegroundColor = ConsoleColor.Gray;

        string text = "TRACE: " + message;
        if (args.Length != 0)
        {
            Debug.Log(text + ": " +  args);

            return;
        }

        Debug.Log(text);
    }

    public void Info(string message, params object[] args)
    {
        if (Level > LogLevel.Info)
            return;

        string text = "INFO: " + message;
        if (args.Length != 0)
        {
            Debug.Log(text + ": " +  args);

            return;
        }

        Debug.Log(text);
    }

    public void Warning(string message, params object[] args)
    {
        if (Level > LogLevel.Warning)
            return;

        string text = "WARN: " + message;
        if (args.Length != 0)
        {
            Debug.LogWarning(text + ": " +  args);

            return;
        }

        Debug.LogWarning(text);
    }

    public void Error(string message, params object[] args)
    {
        if (Level > LogLevel.Error)
            return;

        string text = "ERR : " + message;
        if (args.Length != 0)
        {
            Debug.LogError(text + ": " + args);

            return;
        }

        Debug.LogError(text);
    }
}