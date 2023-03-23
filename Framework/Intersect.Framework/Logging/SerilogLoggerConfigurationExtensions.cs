using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Intersect.Framework.Logging;

public static class SerilogLoggerConfigurationExtensions
{
    public static LoggerConfiguration Apply(
        this LoggerConfiguration loggerConfiguration,
        HostBuilderContext hostBuilderContext,
        IEnumerable<Action<HostBuilderContext, LoggerConfiguration>> loggerConfigurationSteps
    )
    {
        foreach (var step in loggerConfigurationSteps)
        {
            Debug.Assert(step != default, "step != default");
            step(hostBuilderContext, loggerConfiguration);
        }

        return loggerConfiguration;
    }

    public static LoggerConfiguration Apply(
        this LoggerConfiguration loggerConfiguration,
        HostBuilderContext hostBuilderContext,
        IServiceProvider serviceProvider,
        IEnumerable<Action<HostBuilderContext, IServiceProvider, LoggerConfiguration>> loggerConfigurationSteps
    )
    {
        foreach (var step in loggerConfigurationSteps)
        {
            Debug.Assert(step != default, "step != default");
            step(hostBuilderContext, serviceProvider, loggerConfiguration);
        }

        return loggerConfiguration;
    }
}