using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using Intersect.Config;
using Intersect.Server.Core;
using Intersect.Server.Database;
using Microsoft.Data.Sqlite;
using MySqlConnector;

namespace Intersect.Server;

public sealed partial class Program
{
    private const string LdLibraryPath = "LD_LIBRARY_PATH";

    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            var programHost = new ProgramHost(args);
            programHost.Run();
        }
        catch (Exception exception)
        {
            ServerContext.DispatchUnhandledException(exception, true);
        }
    }

    /// <summary>
    ///     Host builder method for Entity Framework Design Tools to use when generating migrations.
    /// </summary>
    public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
        .ConfigureServices(
            (hostContext, services) =>
            {
                Debugger.Launch();
                var rawDatabaseType = hostContext.Configuration.GetValue<string>("DatabaseType") ??
                                      DatabaseOptions.DatabaseType.SQLite.ToString();
                if (!Enum.TryParse(rawDatabaseType, out DatabaseOptions.DatabaseType databaseType))
                {
                    throw new InvalidOperationException($"Invalid database type: {rawDatabaseType}");
                }

                var connectionString = hostContext.Configuration.GetValue<string>("ConnectionString");
                DbConnectionStringBuilder connectionStringBuilder = databaseType switch
                {
                    DatabaseOptions.DatabaseType.MySQL => new MySqlConnectionStringBuilder(connectionString),
                    DatabaseOptions.DatabaseType.SQLite => new SqliteConnectionStringBuilder(connectionString),
                    _ => throw new IndexOutOfRangeException($"Unsupported database type: {databaseType}")
                };

                DatabaseContextOptions databaseContextOptions = new()
                {
                    ConnectionStringBuilder = connectionStringBuilder,
                    DatabaseType = databaseType
                };

                services.AddSingleton(databaseContextOptions);
            }
        );
}