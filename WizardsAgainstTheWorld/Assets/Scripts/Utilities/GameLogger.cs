using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class GameLogger
{
    private static readonly List<string> _logBuffer = new();
    private static string _logFilePath;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Initialize()
    {
        _logFilePath = Path.Combine(Application.persistentDataPath, "gamelog.txt");
        Application.quitting += DumpLogsToFile;
    }

    public static void Log(object message) => LogInternal("LOG", message?.ToString(), Debug.Log);
    public static void Log(string message) => LogInternal("LOG", message, Debug.Log);
    public static void LogFormat(string format, params object[] args) => LogInternal("LOG", string.Format(format, args), Debug.Log);

    public static void LogWarning(object message) => LogInternal("WARN", message?.ToString(), Debug.LogWarning);
    public static void LogError(object message) => LogInternal("ERROR", message?.ToString(), Debug.LogError);

    public static void LogException(Exception exception)
    {
        if (exception == null) return;

        string logEntry = FormatLogEntry("EXCEPTION", exception.ToString());
        _logBuffer.Add(logEntry);
        Debug.LogException(exception);
    }

    private static void LogInternal(string level, string message, Action<object> unityLogMethod)
    {
        string logEntry = FormatLogEntry(level, message);
        _logBuffer.Add(logEntry);
        unityLogMethod.Invoke(message);
    }

    private static string FormatLogEntry(string level, string message)
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        return $"[{timestamp}] [{level}] {message}";
    }

    private static void DumpLogsToFile()
    {
        try
        {
            File.WriteAllLines(_logFilePath, _logBuffer);
            Debug.Log($"GameLogger: Logs dumped to {_logFilePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"GameLogger: Failed to write logs to file: {ex}");
        }
    }
}
