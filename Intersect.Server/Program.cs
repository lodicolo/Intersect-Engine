using System.Data.Common;
using System.Diagnostics;
using Intersect.Config;
using Intersect.Framework.Services;
using Intersect.Server.Database;
using Intersect.Server.Properties;
using Intersect.Server.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySqlConnector;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace Intersect.Server;

internal static partial class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        // Prepare environment for startup
        if (!Bootstrap(args))
        {
            Environment.Exit(1);
        }

        IHost? host = default;
        try
        {
            host = Host.CreateDefaultBuilder(args)
                .UseSerilog(
                    (hostBuilderContext, _, loggerConfiguration) => loggerConfiguration.ReadFrom
                        .Configuration(hostBuilderContext.Configuration)
                        .Enrich.FromLogContext()
                        .WriteTo.Console(theme: SystemConsoleTheme.Colored)
                        .WriteTo.File(
                            $"{typeof(Program).Assembly.FullName ?? "info"}.log",
                            rollingInterval: RollingInterval.Day,
                            rollOnFileSizeLimit: true
                        )
                )
                .UseExceptionHandler()
                .ConfigureIntersectServerDefaults()
                .ConfigureServices(
                    (_, services) =>
                    {
                        services.AddHealthChecks();
                    }
                )
                .ConfigureWebHostDefaults(
                    webBuilder =>
                    {
                        webBuilder.ConfigureKestrel(ProgramConfiguration.ConfigureKestrel);
                        webBuilder.Configure(ProgramConfiguration.ConfigureAppBuilder);
                    }
                )
                .Build();
        }
        catch (Exception exception)
        {
            Log.Fatal(exception, Resources.Program_HostBuilder_ExceptionDuringConfigurationAbortingStartup);
            Environment.Exit(2);
        }

        // If the host were null we already would have exited
        Debug.Assert(host != default);

        try
        {
            host.Run();
        }
        catch (Exception exception)
        {
            try
            {
                var exceptionHandlerService = host.Services.GetRequiredService<IExceptionHandlerService>();
                exceptionHandlerService.DispatchUnhandledException(exception: exception, isTerminating: true);
            }
            catch (InvalidOperationException invalidOperationException)
            {
                var aggregateException = new AggregateException(invalidOperationException, exception);
                Log.Fatal(aggregateException, "Unhandled exception occurred before the exception handler service was attached to the host, aborting.");
                Environment.Exit(3);
            }
        }
    }

    /// <summary>
    /// Host builder method for Entity Framework Design Tools to use when generating migrations.
    /// </summary>
    public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
        .ConfigureServices(
            (hostContext, services) =>
            {
                var rawDatabaseType = hostContext.Configuration.GetValue<string>("DatabaseType") ??
                                      DatabaseType.Sqlite.ToString();
                if (!Enum.TryParse(rawDatabaseType, out DatabaseType databaseType))
                {
                    throw new InvalidOperationException($"Invalid database type: {rawDatabaseType}");
                }

                var connectionString = hostContext.Configuration.GetValue<string>("ConnectionString");
                DbConnectionStringBuilder connectionStringBuilder = databaseType switch
                {
                    DatabaseType.MySql => new MySqlConnectionStringBuilder(connectionString),
                    DatabaseType.Sqlite => new SqliteConnectionStringBuilder(connectionString),
                    _ => throw new IndexOutOfRangeException($"Unsupported database type: {databaseType}")
                };

                DatabaseContextOptions databaseContextOptions = new()
                {
                    ConnectionStringBuilder = connectionStringBuilder, DatabaseType = databaseType
                };

                services.AddSingleton(databaseContextOptions);
            }
        );
}
