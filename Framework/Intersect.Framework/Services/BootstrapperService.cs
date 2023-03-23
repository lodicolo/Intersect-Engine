using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Intersect.Framework.Services;

public sealed class BootstrapperService : IBootstrapperService
{
    private readonly IHostApplicationLifetime _applicationLifetime;

    private readonly IConfiguration _configuration;

    private readonly BootstrapServiceOptions _options;

    private readonly IServiceProvider _serviceProvider;

    private CancellationTokenSource? _cancellationTokenSource;

    public BootstrapperService(
        IHostApplicationLifetime applicationLifetime,
        BootstrapServiceOptions options,
        IConfiguration configuration,
        IServiceProvider serviceProvider
    )
    {
        _applicationLifetime = applicationLifetime;
        _configuration = configuration;
        _options = options;
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        await Task.WhenAll(
            _options.Tasks.Select(
                task => task.ExecuteAsync(_configuration, _serviceProvider, _cancellationTokenSource.Token)
            )
        );

        if (_cancellationTokenSource.IsCancellationRequested)
        {
            return;
        }

        _applicationLifetime.StopApplication();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource?.Cancel();
        return Task.CompletedTask;
    }
}