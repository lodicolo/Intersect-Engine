using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Intersect.Framework.Services;

/// <summary>
/// Extensions to configure <see cref="IHostBuilder"/> with services for Intersect applications.
/// </summary>
public static class HostBuilderIntersectExtensions
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
                var configuration = hostBuilderContext.Configuration.GetSection($"Intersect:Services:{serviceName}");
                options.Configure(configuration, reloadOnChange: true);
            }
        );
    }
}
