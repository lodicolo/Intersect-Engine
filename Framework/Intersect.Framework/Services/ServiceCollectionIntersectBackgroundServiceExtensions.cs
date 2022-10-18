using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Intersect.Framework.Services;

public static class ServiceCollectionIntersectBackgroundServiceExtensions
{
    public static IHostBuilder UseIntersectBackgroundService<TService, TOptions, TConfigureOptions>(
        this IHostBuilder hostBuilder
    )
        where TOptions : ServiceOptions<TService, TOptions>, new()
        where TService : IntersectBackgroundService<TService, TOptions, TConfigureOptions>
        where TConfigureOptions : class, IConfigureOptions<TOptions>
    {
        return hostBuilder.ConfigureServices(
            services =>
            {
                services.AddTransient<IConfigureOptions<TOptions>, TConfigureOptions>();
                services.AddSingleton<TService>();
            }
        );
    }

    public static IHostBuilder UseIntersectBackgroundService<TService, TOptions, TConfigureOptions>(
        this IHostBuilder hostBuilder,
        Action<HostBuilderContext, TOptions> configureOptions
    )
        where TOptions : ServiceOptions<TService, TOptions>, new()
        where TService : IntersectBackgroundService<TService, TOptions, TConfigureOptions>
        where TConfigureOptions : class, IConfigureOptions<TOptions>
    {
        return hostBuilder.UseIntersectBackgroundService<TService, TOptions, TConfigureOptions>()
            .ConfigureIntersectBackgroundService<TService, TOptions, TConfigureOptions>(configureOptions);
    }

    public static IHostBuilder ConfigureIntersectBackgroundService<TService, TOptions, TConfigureOptions>(
        this IHostBuilder hostBuilder,
        Action<HostBuilderContext, TOptions> configureOptions
    )
        where TOptions : ServiceOptions<TService, TOptions>, new()
        where TService : IntersectBackgroundService<TService, TOptions, TConfigureOptions>
        where TConfigureOptions : class, IConfigureOptions<TOptions>
    {
        return hostBuilder.ConfigureServices(
            (context, services) => services.Configure<TOptions>(options => configureOptions(context, options))
        );
    }
}
