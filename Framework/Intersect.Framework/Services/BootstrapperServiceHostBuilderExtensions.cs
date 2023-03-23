using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Intersect.Framework.Services;

public static class BootstrapperServiceHostBuilderExtensions
{
    public static IHostBuilder UseBootstrapper(
        this IHostBuilder hostBuilder,
        params Func<HostBuilderContext, IServiceCollection, IBootstrapTask>[] taskFactories
    ) => hostBuilder.ConfigureServices(
        (context, services) =>
        {
            services.AddSingleton(
                new BootstrapServiceOptions(
                    taskFactories.Select(taskFactory => taskFactory(context, services)).ToArray()
                )
            );
            services.AddHostedService<BootstrapperService>();
        }
    );
}