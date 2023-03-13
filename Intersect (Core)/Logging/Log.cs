using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

using Intersect.Logging.Formatting;
using Intersect.Logging.Output;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace Intersect.Logging
{
    public static partial class Log
    {
        public static Serilog.Core.Logger Logger { get; } = new LoggerConfiguration().ConfigureIntersect(new()
        {
            Console = new()
            {
                Enabled = true,
                Theme = SystemConsoleTheme.Literate
            },
            Debug = new(),
            File = new()
        }).CreateLogger();
        
        internal static readonly DateTime Initial = DateTime.Now;

        private static string ExecutableName =>
            Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName);

        public static string SuggestFilename(
            DateTime? time = null,
            string prefix = null,
            string extensionPrefix = null
        ) =>
            $"{prefix?.Trim() ?? ""}{ExecutableName}-{time ?? Initial:yyyy_MM_dd-HH_mm_ss_fff}{(string.IsNullOrWhiteSpace(extensionPrefix) ? "" : "." + extensionPrefix)}.log";

        #region Global

        static Log()
        {
            var outputs = ImmutableList.Create<ILogOutput>(
                              new FileOutput(), new FileOutput($"errors-{ExecutableName}.log", LogLevel.Error),
                              new ConciseConsoleOutput(Debugger.IsAttached ? LogLevel.All : LogLevel.Error)
                          ) ??
                          throw new InvalidOperationException();

            Pretty = new Logger(
                new LogConfiguration
                {
                    Formatters = ImmutableList.Create(new PrettyFormatter()) ?? throw new InvalidOperationException(),
                    LogLevel = LogConfiguration.Default.LogLevel,
                    Outputs = outputs
                }
            );

            Default = new Logger(
                new LogConfiguration
                {
                    Formatters = ImmutableList.Create(new DefaultFormatter()) ?? throw new InvalidOperationException(),
                    LogLevel = LogConfiguration.Default.LogLevel,
                    Outputs = outputs
                }
            );
        }

        public static Logger Pretty { get; internal set; }

        public static Logger Default { get; internal set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Fatal(string message)
        {
            Logger.Fatal(message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Fatal(string format, params object[] args)
        {
            Logger.Fatal(format, args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Fatal(Exception exception, string message = null)
        {
            Logger.Fatal(exception, message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(string message)
        {
            Logger.Error(message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(string format, params object[] args)
        {
            Logger.Error(format, args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(Exception exception, string message = null)
        {
            Logger.Error(exception, message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(string message)
        {
            Logger.Warning(message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(string format, params object[] args)
        {
            Logger.Warning(format, args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(Exception exception, string message = null)
        {
            Logger.Warning(exception, message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Information(string message)
        {
            Logger.Information(message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Information(string format, params object[] args)
        {
            Logger.Information(format, args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Information(Exception exception, string message = null)
        {
            Logger.Information(exception, message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Debug(string message)
        {
            Logger.Debug(message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Debug(string format, params object[] args)
        {
            Logger.Debug(format, args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Debug(Exception exception, string message = null)
        {
            Logger.Debug(exception, message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Verbose(string message)
        {
            Logger.Verbose(message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Verbose(string format, params object[] args)
        {
            Logger.Verbose(format, args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Verbose(Exception exception, string message = null)
        {
            Logger.Verbose(exception, message);
        }

        #endregion
    }
}
