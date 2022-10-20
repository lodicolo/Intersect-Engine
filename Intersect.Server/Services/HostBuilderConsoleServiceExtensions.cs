using Intersect.Framework.Services;
using Microsoft.Extensions.Hosting;

namespace Intersect.Server.Services;

/// <summary>
/// Extensions to configure <see cref="IHostBuilder"/> with <see cref="ConsoleService"/>.
/// </summary>
public static class HostBuilderConsoleServiceExtensions
{
    public static IHostBuilder UseConsoleService(this IHostBuilder hostBuilder) => hostBuilder
        .UseIntersectBackgroundService<ConsoleService, ConsoleServiceOptions, ConsoleServiceOptionsSetup>();

    public static IHostBuilder UseConsoleService(
        this IHostBuilder hostBuilder,
        Action<HostBuilderContext, ConsoleServiceOptions> configureOptions
    ) => hostBuilder.UseIntersectBackgroundService<ConsoleService, ConsoleServiceOptions, ConsoleServiceOptionsSetup>(
        configureOptions
    );
}
