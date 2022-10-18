using System.Globalization;
using System.Reflection;
using Intersect.Framework.Services;
using Intersect.Server.Properties;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace Intersect.Server;

internal static partial class Program
{
    /// <summary>
    /// Prepare the file system, app domain, etc. for startup.
    /// </summary>
    /// <param name="args">The command line arguments passed into <see cref="Main(string[])"/>.</param>
    private static bool Bootstrap(string[] args)
    {
        try
        {
            // Standard default culture for Intersect is en-US
            CultureInfo.DefaultThreadCurrentCulture = new("en-US");

            // Create bootstrap logger
            Log.Logger = new LoggerConfiguration().Enrich.FromLogContext()
                .WriteTo.Console(theme: SystemConsoleTheme.Colored)
                .WriteTo.File($"bootstrap.{typeof(Program).Assembly.FullName ?? "info"}.log")
                .CreateBootstrapLogger();

            // Create default host builder to handle configurations
            var bootstrapHost = Host.CreateDefaultBuilder(args)
                .UseSerilog(Log.Logger)
                .UseExceptionHandler()
                .UseBootstrapper(
                    context =>
                    {
                        var executingAssembly = Assembly.GetExecutingAssembly();
                        return new EmbeddedResourceUnpackerService(
                            new(executingAssembly, "appsettings.json"),
                            new(executingAssembly, $"appsettings.{context.HostingEnvironment.EnvironmentName}.json")
                        );
                    }
                )
                .Build();

            bootstrapHost.Run();

            return true;
        }
        catch (Exception exception)
        {
            Log.Fatal(exception, Resources.Program_Bootstrap_ExceptionOccurredAbortingStartup);
            return false;
        }
    }
}
