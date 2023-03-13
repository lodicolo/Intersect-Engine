using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.SystemConsole.Themes;

namespace Intersect.Logging;

public abstract class SinkConfiguration
{
    public const string DefaultOutputTemplate = "[{Timestamp:HH:mm:ss}][{Level:u3}] {Message:lj}{NewLine}{Exception}";

    private LoggingLevelSwitch _loggingLevelSwitch;

    protected SinkConfiguration(bool enabledByDefault = true)
    {
        Enabled = enabledByDefault;
    }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string CultureOverride { get; set; }

    public bool Enabled { get; set; }

    [JsonIgnore] public ITextFormatter Formatter { get; set; }

    public LogEventLevel InitialMinimumLevel { get; set; } =
#if DEBUG
        LogEventLevel.Debug;
#else
            LogEventLevel.Information;
#endif

    [JsonIgnore]
    public LoggingLevelSwitch LoggingLevelSwitch
    {
        get => _loggingLevelSwitch ?? (_loggingLevelSwitch = new LoggingLevelSwitch(InitialMinimumLevel));
        set => _loggingLevelSwitch = value;
    }

    [DefaultValue(DefaultOutputTemplate)] public string OutputTemplate { get; set; } = DefaultOutputTemplate;
}

public sealed class ConsoleSinkConfiguration : SinkConfiguration
{
    [JsonIgnore] public object SyncRoot { get; set; }

    public ConsoleTheme Theme { get; set; }
}

public sealed class DebugSinkConfiguration : SinkConfiguration
{
    public DebugSinkConfiguration() :
#if DEBUG
        base(true)
#else
        base(false)
#endif
    {
    }
}

public sealed class FileSinkConfiguration : SinkConfiguration
{
    public bool Buffered { get; set; } = true;

    public string ExtensionPrefix { get; set; }

    public int FileSizeLimitBytes { get; set; } = 100 * 1024 * 1024;

    public TimeSpan? FlushToDiskInterval { get; set; } = default;

    public string Prefix { get; set; }

    public int RetainedFileCountLimit { get; set; } = 50;

    public TimeSpan RetainedFileTimeLimit { get; set; } = TimeSpan.FromDays(30);

    public RollingInterval RollingInterval { get; set; } = RollingInterval.Day;

    public bool RollOnFileSizeLimit { get; set; } = true;
}

public sealed class IntersectSerilogConfiguration
{
    public ConsoleSinkConfiguration Console { get; set; }

    public DebugSinkConfiguration Debug { get; set; }

    public FileSinkConfiguration File { get; set; }
}

public static class LoggerConfigurationExtensions
{
    public static LoggerConfiguration ConfigureIntersect(
        this LoggerConfiguration loggerConfiguration,
        IntersectSerilogConfiguration intersectSerilogConfiguration = default
    )
    {
        return loggerConfiguration.ConfigureConsoleSink(intersectSerilogConfiguration.Console)
            .ConfigureDebugSink(intersectSerilogConfiguration.Debug)
            .ConfigureFileSink(intersectSerilogConfiguration.File);
    }

    private static LoggerConfiguration ConfigureConsoleSink(
        this LoggerConfiguration loggerConfiguration, ConsoleSinkConfiguration consoleSinkConfiguration
    )
    {
        if (consoleSinkConfiguration is not { Enabled: true })
        {
            return loggerConfiguration;
        }

        if (consoleSinkConfiguration.Formatter != default)
        {
            return loggerConfiguration.WriteTo.Console(consoleSinkConfiguration.Formatter,
                levelSwitch: consoleSinkConfiguration.LoggingLevelSwitch, syncRoot: consoleSinkConfiguration.SyncRoot);
        }

        IFormatProvider formatProvider = CultureInfo.InvariantCulture;
        if (consoleSinkConfiguration.CultureOverride != default)
        {
            formatProvider = new CultureInfo(consoleSinkConfiguration.CultureOverride);
        }

        return loggerConfiguration.WriteTo.Console(formatProvider: formatProvider,
            levelSwitch: consoleSinkConfiguration.LoggingLevelSwitch,
            outputTemplate: consoleSinkConfiguration.OutputTemplate, syncRoot: consoleSinkConfiguration.SyncRoot,
            theme: consoleSinkConfiguration.Theme);
    }

    private static LoggerConfiguration ConfigureDebugSink(
        this LoggerConfiguration loggerConfiguration, DebugSinkConfiguration debugSinkConfiguration
    )
    {
        if (debugSinkConfiguration is not { Enabled: true })
        {
            return loggerConfiguration;
        }

        if (debugSinkConfiguration.Formatter != default)
        {
            return loggerConfiguration.WriteTo.Debug(levelSwitch: debugSinkConfiguration.LoggingLevelSwitch,
                outputTemplate: debugSinkConfiguration.OutputTemplate);
        }

        IFormatProvider formatProvider = CultureInfo.InvariantCulture;
        if (debugSinkConfiguration.CultureOverride != default)
        {
            formatProvider = new CultureInfo(debugSinkConfiguration.CultureOverride);
        }

        return loggerConfiguration.WriteTo.Debug(formatProvider: formatProvider,
            levelSwitch: debugSinkConfiguration.LoggingLevelSwitch,
            outputTemplate: debugSinkConfiguration.OutputTemplate);
    }

    private static LoggerConfiguration ConfigureFileSink(
        this LoggerConfiguration loggerConfiguration, FileSinkConfiguration fileSinkConfiguration
    )
    {
        if (fileSinkConfiguration is not { Enabled: true })
        {
            return loggerConfiguration;
        }

        var fileNameBuilder = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(fileSinkConfiguration.Prefix))
        {
            fileNameBuilder.Append(fileSinkConfiguration.Prefix).Append('-');
        }

        fileNameBuilder.Append(Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName));

        if (!string.IsNullOrWhiteSpace(fileSinkConfiguration.ExtensionPrefix))
        {
            fileNameBuilder.Append('.').Append(fileSinkConfiguration.ExtensionPrefix);
        }

        fileNameBuilder.Append(".log");

        if (fileSinkConfiguration.Formatter != default)
        {
            return loggerConfiguration.WriteTo.File(buffered: fileSinkConfiguration.Buffered, encoding: Encoding.UTF8,
                fileSizeLimitBytes: fileSinkConfiguration.FileSizeLimitBytes,
                formatter: fileSinkConfiguration.Formatter,
                flushToDiskInterval: fileSinkConfiguration.FlushToDiskInterval,
                levelSwitch: fileSinkConfiguration.LoggingLevelSwitch, path: fileNameBuilder.ToString(),
                retainedFileCountLimit: fileSinkConfiguration.RetainedFileCountLimit,
                retainedFileTimeLimit: fileSinkConfiguration.RetainedFileTimeLimit,
                rollingInterval: fileSinkConfiguration.RollingInterval,
                rollOnFileSizeLimit: fileSinkConfiguration.RollOnFileSizeLimit);
        }

        IFormatProvider formatProvider = CultureInfo.InvariantCulture;
        if (fileSinkConfiguration.CultureOverride != default)
        {
            formatProvider = new CultureInfo(fileSinkConfiguration.CultureOverride);
        }

        return loggerConfiguration.WriteTo.File(buffered: fileSinkConfiguration.Buffered, encoding: Encoding.UTF8,
            fileSizeLimitBytes: fileSinkConfiguration.FileSizeLimitBytes, formatProvider: formatProvider,
            flushToDiskInterval: fileSinkConfiguration.FlushToDiskInterval,
            levelSwitch: fileSinkConfiguration.LoggingLevelSwitch, outputTemplate: fileSinkConfiguration.OutputTemplate,
            path: fileNameBuilder.ToString(), retainedFileCountLimit: fileSinkConfiguration.RetainedFileCountLimit,
            retainedFileTimeLimit: fileSinkConfiguration.RetainedFileTimeLimit,
            rollingInterval: fileSinkConfiguration.RollingInterval,
            rollOnFileSizeLimit: fileSinkConfiguration.RollOnFileSizeLimit);
    }
}