using Intersect.Framework.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Intersect.Server.Services;

public static class HostBuilderIntersectServerExtensions
{
    public static IHostBuilder ConfigureIntersectBackgroundServiceDefaults<TService, TOptions, TConfigureOptions>(
        this IHostBuilder hostBuilder
    )
        where TOptions : ServiceOptions<TService, TOptions>, new()
        where TService : IntersectBackgroundService<TService, TOptions, TConfigureOptions>
        where TConfigureOptions : class, IConfigureOptions<TOptions>
    {
        return hostBuilder.UseIntersectBackgroundService<TService, TOptions, TConfigureOptions>(
            (hostBuilderContext, options) =>
            {
                var serviceName = typeof(TService).Name.Replace("Service", string.Empty);
                var configuration = hostBuilderContext.Configuration.GetSection($"Intersect:{serviceName}");
                options.Configure(configuration, reloadOnChange: true);
            }
        );
    }

    public static IHostBuilder ConfigureIntersectServerDefaults(this IHostBuilder hostBuilder)
    {
        return hostBuilder
            .ConfigureIntersectBackgroundServiceDefaults<ConsoleService, ConsoleServiceOptions,
                ConsoleServiceOptionsSetup>()
            .ConfigureIntersectBackgroundServiceDefaults<LogicService, LogicServiceOptions, LogicServiceOptionsSetup>();
    }
}
