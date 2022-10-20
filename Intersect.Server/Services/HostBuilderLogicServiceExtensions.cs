using Intersect.Framework.Services;
using Microsoft.Extensions.Hosting;

namespace Intersect.Server.Services;

/// <summary>
/// Extensions to configure <see cref="IHostBuilder"/> with <see cref="LogicService"/>.
/// </summary>
public static class HostBuilderLogicServiceExtensions
{
    public static IHostBuilder UseLogicService(this IHostBuilder hostBuilder) => hostBuilder
        .UseIntersectBackgroundService<LogicService, LogicServiceOptions, LogicServiceOptionsSetup>();

    public static IHostBuilder UseLogicService(
        this IHostBuilder hostBuilder,
        Action<HostBuilderContext, LogicServiceOptions> configureOptions
    ) => hostBuilder.UseIntersectBackgroundService<LogicService, LogicServiceOptions, LogicServiceOptionsSetup>(
        configureOptions
    );
}
