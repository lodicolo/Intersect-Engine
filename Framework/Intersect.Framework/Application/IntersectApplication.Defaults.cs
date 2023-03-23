using System.Reflection;
using Intersect.Framework.Configuration.EnvironmentVariables;
using Intersect.Framework.Services;
using Intersect.Framework.Services.EmbeddedResourceUnpacker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace Intersect.Framework.Application;

public sealed partial class IntersectApplication
{
    public static IntersectApplicationBuilder CreateDefaultBuilder(
        string[]? args = default,
        Assembly? embeddedAppSettingsAssembly = default
    )
    {
        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        var assemblyName = assembly.GetName();
        var loggerBaseName = assemblyName.CultureName;

        if (string.IsNullOrWhiteSpace(loggerBaseName))
        {
            loggerBaseName = assemblyName.Name;
        }

        if (string.IsNullOrWhiteSpace(loggerBaseName))
        {
            loggerBaseName = nameof(IntersectApplicationBuilder);
        }

        loggerBaseName = loggerBaseName.Replace(' ', '_')
            .Replace(Path.PathSeparator, '_')
            .Replace(Path.DirectorySeparatorChar, '_')
            .Replace(Path.AltDirectorySeparatorChar, '_');

        var intersectApplicationBuilder = new IntersectApplicationBuilder(args).ConfigureBootstrapLogger(
                (_, bootstrapLoggerConfiguration) =>
                {
                    bootstrapLoggerConfiguration.Enrich.FromLogContext()
                        .WriteTo.Console(theme: SystemConsoleTheme.Colored)
                        .WriteTo.File($"logs/{loggerBaseName}.bootstrap.{DateTime.UtcNow:yyyyMMdd-HHmmss}.log");
                }
            )
            .ConfigureBootstrapBuilder(
                bootstrapHostBuilder =>
                {
                    bootstrapHostBuilder.ConfigureHostConfiguration(
                        configurationBuilder => configurationBuilder.AddEnvironmentVariables("INTERSECT_BOOTSTRAP_")
                    );
                    bootstrapHostBuilder
                        .UseExceptionHandler()
                        .UseBootstrapper(
                            (context, _) =>
                            {
                                var resourceUnpackingRequests = embeddedAppSettingsAssembly == default
                                    ? Array.Empty<EmbeddedResourceUnpackingRequest>()
                                    : new EmbeddedResourceUnpackingRequest[]
                                    {
                                        new(embeddedAppSettingsAssembly, "appsettings.json"),
                                        new(
                                            embeddedAppSettingsAssembly,
                                            $"appsettings.{context.HostingEnvironment.EnvironmentName}.json"
                                        )
                                    };
                                return new EmbeddedResourceUnpackerService(resourceUnpackingRequests);
                            }
                        );
                }
            )
            .ConfigureApplicationGlobalLogger(
                (_, loggerConfiguration) => loggerConfiguration.Enrich.FromLogContext()
                    .WriteTo.Console(theme: SystemConsoleTheme.Colored)
                    .WriteTo.File(
                        $"logs/{loggerBaseName}.global.log",
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true
                    )
            )
            .ConfigureApplicationLogger(
                (_, loggerConfiguration) => loggerConfiguration.Enrich.FromLogContext()
                    .WriteTo.Console(theme: SystemConsoleTheme.Colored)
                    .WriteTo.File(
                        $"logs/{loggerBaseName}.log",
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true
                    )
            )
            .ConfigureApplicationBuilder(
                applicationHostBuilder =>
                {
                    applicationHostBuilder.ConfigureHostConfiguration(
                        configurationBuilder =>
                            configurationBuilder.AddEnvironmentVariables("INTERSECT_", "INTERSECT_BOOTSTRAP_")
                    );
                    applicationHostBuilder.UseExceptionHandler();
                }
            );

        return intersectApplicationBuilder;
    }
}