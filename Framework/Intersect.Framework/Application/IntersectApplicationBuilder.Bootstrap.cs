using System.Globalization;
using Intersect.Framework.Hosting;
using Intersect.Framework.Logging;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Intersect.Framework.Application;

public sealed partial class IntersectApplicationBuilder
{
    private readonly List<Action<IHostBuilder>> _bootstrapBuilderConfigurationSteps = new(2);

    private readonly List<Action<HostBuilderContext, LoggerConfiguration>> _bootstrapLoggerConfigurationSteps = new(2);

    public IntersectApplicationBuilder ConfigureBootstrapBuilder(Action<IHostBuilder> configureBuilderAction)
    {
        _bootstrapBuilderConfigurationSteps.Add(configureBuilderAction);
        return this;
    }

    public IntersectApplicationBuilder ConfigureBootstrapLogger(
        Action<HostBuilderContext, LoggerConfiguration> configureLoggerAction
    )
    {
        _bootstrapLoggerConfigurationSteps.Add(configureLoggerAction);
        return this;
    }

    private void Bootstrap()
    {
        try
        {
            // Standard default culture for Intersect is en-US
            CultureInfo.DefaultThreadCurrentCulture = new("en-US");

            var hostBuilder = Host.CreateDefaultBuilder(_args);

            // Configure bootstrapper logger
            hostBuilder.ConfigureLogging(
                (hostBuilderContext, _) =>
                {
                    var bootstrapLoggerConfiguration = new LoggerConfiguration().ReadFrom
                        .Configuration(hostBuilderContext.Configuration, "Logging:Common")
                        .ReadFrom.Configuration(hostBuilderContext.Configuration, "Logging:Bootstrapper")
                        .Apply(hostBuilderContext, _bootstrapLoggerConfigurationSteps);

                    // Create bootstrap logger
                    Log.Logger = bootstrapLoggerConfiguration.CreateBootstrapLogger();
                }
            );


            // Create default host builder to handle configurations
            var bootstrapHost = hostBuilder
                .ConfigureServices((_, services) => services.RemoveAll(typeof(ILoggerFactory)))
                .UseSerilog(Log.Logger)
                .Apply(_bootstrapBuilderConfigurationSteps)
                .Build();

            bootstrapHost.Run();
        }
        catch (Exception exception)
        {
            Log.Fatal(exception, ApplicationResources.BootstrapExceptionOccurredAbortingStartup);
            Environment.Exit(1);
        }
    }
}