using System.Diagnostics;
using Intersect.Framework.Hosting;
using Intersect.Framework.Logging;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Intersect.Framework.Application;

public sealed partial class IntersectApplicationBuilder
{
    private readonly List<Action<IHostBuilder>> _applicationBuilderConfigurationSteps = new(2);

    private readonly List<Action<HostBuilderContext, LoggerConfiguration>> _applicationGlobalLoggerConfigurationSteps =
        new(2);

    private readonly List<Action<HostBuilderContext, LoggerConfiguration>>
        _applicationLoggerConfigurationSteps = new(2);

    public IntersectApplicationBuilder ConfigureApplicationBuilder(Action<IHostBuilder> configureBuilderAction)
    {
        _applicationBuilderConfigurationSteps.Add(configureBuilderAction);
        return this;
    }

    public IntersectApplicationBuilder ConfigureApplicationGlobalLogger(
        Action<HostBuilderContext, LoggerConfiguration> configureLoggerAction
    )
    {
        _applicationGlobalLoggerConfigurationSteps.Add(configureLoggerAction);
        return this;
    }

    public IntersectApplicationBuilder ConfigureApplicationLogger(
        Action<HostBuilderContext, LoggerConfiguration> configureLoggerAction
    )
    {
        _applicationLoggerConfigurationSteps.Add(configureLoggerAction);
        return this;
    }

    private IHost BuildApplication()
    {
        try
        {
            var hostBuilder = Host.CreateDefaultBuilder(_args);

            // Configure application-global logger
            hostBuilder.ConfigureLogging(
                (hostBuilderContext, _) =>
                {
                    var globalLoggerConfiguration = new LoggerConfiguration().ReadFrom
                        .Configuration(hostBuilderContext.Configuration, "Logging:Common")
                        .ReadFrom.Configuration(hostBuilderContext.Configuration, "Logging:ApplicationGlobal")
                        .Apply(hostBuilderContext, _applicationGlobalLoggerConfigurationSteps);

                    // Create global logger
                    Log.Logger = globalLoggerConfiguration.CreateBootstrapLogger();
                }
            );

            var applicationHost = hostBuilder
                .ConfigureServices((_, services) => services.RemoveAll(typeof(ILoggerFactory)))
                .UseSerilog(
                    (hostBuilderContext, loggerConfiguration) =>
                    {
                        loggerConfiguration.ReadFrom.Configuration(hostBuilderContext.Configuration, "Logging:Common")
                            .ReadFrom.Configuration(hostBuilderContext.Configuration, "Logging:Application")
                            .Apply(hostBuilderContext, _applicationLoggerConfigurationSteps);
                    }
                )
                .Apply(_applicationBuilderConfigurationSteps)
                .Build();

            return applicationHost;
        }
        catch (Exception exception)
        {
            Log.Fatal(exception, ApplicationResources.HostBuilderExceptionDuringConfigurationAbortingStartup);
            Environment.Exit(2);

            throw new UnreachableException(ApplicationResources.ApplicationShouldHaveExited);
        }
    }
}