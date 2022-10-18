using Intersect.Framework.Commands;
using Microsoft.Extensions.Hosting;

namespace Intersect.Server.Services.Background;

internal class ConsoleServiceCommandContext : ICommandContext
{
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly IServiceProvider _serviceProvider;

    private bool _shutdownRequested;

    internal ConsoleServiceCommandContext(
        IHostApplicationLifetime applicationLifetime,
        IServiceProvider serviceProvider
    )
    {
        _applicationLifetime = applicationLifetime;
        _serviceProvider = serviceProvider;
    }

    public bool IsShutdownRequested => _shutdownRequested;

    public IServiceProvider ServiceProvider => _serviceProvider;

    public void RequestShutdown()
    {
        _shutdownRequested = true;
        _applicationLifetime.StopApplication();
    }
}
