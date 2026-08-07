using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.CompilerServices;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Layout;

namespace CMP.Services.Log;

public class LoggerInstance
{
    private readonly ILog _log;

    internal LoggerInstance(ILog log)
    {
        _log = log;
    }

    private static void SetContextProperties(string file, int line)
    {
        GlobalContext.Properties["file"] = Path.GetFileName(file);
        GlobalContext.Properties["line"] = line;
    }

    public void Debug(string message, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        SetContextProperties(file, line);
        _log.Debug(message);
    }

    public void Info(string message, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        SetContextProperties(file, line);
        _log.Info(message);
    }

    public void Warn(string message, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        SetContextProperties(file, line);
        _log.Warn(message);
    }

    public void Error(string message, Exception? ex = null, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        SetContextProperties(file, line);
        _log.Error(ex == null ? message : $"{message}\nException: {ex}");
    }

    public void Fatal(string message, Exception? ex = null, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        SetContextProperties(file, line);
        _log.Fatal(ex == null ? message : $"{message}\nException: {ex}");
    }
}

public class Logger
{
    public static class Category
    {
        public const string General = "General";
        public const string Logging = "Logging";
        public const string RobotControl = "RobotControl";
    }

    private static readonly ConcurrentDictionary<string, LoggerInstance> Loggers = new();

    static Logger()
    {
        log4net.Util.LogLog.InternalDebugging = false;

        var fileAppender = CreateAppender(Category.General);
        BasicConfigurator.Configure(fileAppender);
    }

    private static RollingFileAppender CreateAppender(string name)
    {
        var fileAppender = new RollingFileAppender
        {
            Name = $"{name}Appender",
            LockingModel = new FileAppender.MinimalLock(),
            File = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Logs/{name}/"),
            AppendToFile = true,
            RollingStyle = RollingFileAppender.RollingMode.Date,
            DatePattern = "yyyy/MM/yyyy-MM-dd'.log'",
            StaticLogFileName = false,
            Layout = new PatternLayout("%date [%thread] (%property{file}:%property{line}) %-5level - %message%newline"),
            Threshold = log4net.Core.Level.All,
            ImmediateFlush = true
        };
            
        fileAppender.ActivateOptions();
        return fileAppender;
    }

    public static LoggerInstance For(string name)
    {
        return Loggers.GetOrAdd(name, n =>
        {
            var log = LogManager.GetLogger(n);
            
            if (log.Logger is log4net.Repository.Hierarchy.Logger logger)
            {
                if (logger.Appenders.Count == 0)
                {
                    logger.AddAppender(CreateAppender(n));
                    logger.Level = log4net.Core.Level.All;
                    logger.Additivity = false;
                }
            }
            
            return new LoggerInstance(log);
        });
    }

    private static LoggerInstance GeneralLogger => For(Category.General);

    public static void Debug(string message, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        GeneralLogger.Debug(message, file, line);
    }

    public static void Info(string message, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        GeneralLogger.Info(message, file, line);
    }

    public static void Warn(string message, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        GeneralLogger.Warn(message, file, line);
    }

    public static void Error(string message, Exception? ex = null, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        GeneralLogger.Error(message, ex, file, line);
    }

    public static void Fatal(string message, Exception? ex = null, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        GeneralLogger.Fatal(message, ex, file, line);
    }
}