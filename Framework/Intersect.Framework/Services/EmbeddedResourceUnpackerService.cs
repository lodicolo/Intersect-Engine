using System.Reflection;
using Intersect.Framework.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Intersect.Framework.Services;

public interface IBootstrapTask
{
    Task ExecuteAsync(CancellationToken cancellationToken);
}

public record BootstrapServiceOptions(IReadOnlyCollection<IBootstrapTask> Tasks)
{
    public IReadOnlyCollection<IBootstrapTask> Tasks { get; } = Tasks;
}

public interface IBootstrapperService : IHostedService { }

public sealed class BootstrapperService : IBootstrapperService
{
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly BootstrapServiceOptions _options;

    private CancellationTokenSource? _cancellationTokenSource;

    public BootstrapperService(IHostApplicationLifetime applicationLifetime, BootstrapServiceOptions options)
    {
        _applicationLifetime = applicationLifetime;
        _options = options;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        return Task.WhenAll(_options.Tasks.Select(task => task.ExecuteAsync(_cancellationTokenSource.Token)))
            .ContinueWith(
                _ =>
                {
                    _applicationLifetime.StopApplication();
                    return Task.CompletedTask;
                },
                _cancellationTokenSource.Token
            );
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource?.Cancel();
        return Task.CompletedTask;
    }
}

public static class BootstrapperServiceHostBuilderExtensions
{
    public static IHostBuilder UseBootstrapper(
        this IHostBuilder hostBuilder,
        params Func<HostBuilderContext, IBootstrapTask>[] taskFactories
    ) => hostBuilder.ConfigureServices(
        (context, services) =>
        {
            services.AddSingleton(
                new BootstrapServiceOptions(taskFactories.Select(taskFactory => taskFactory(context)).ToArray())
            );
            services.AddHostedService<BootstrapperService>();
        }
    );
}

public sealed record EmbeddedResourceUnpackingRequest(Assembly Assembly, string ResourceName)
{
    public Assembly Assembly { get; } = Assembly;

    public string ResourceName { get; } = ResourceName;
}

public sealed class EmbeddedResourceUnpackerService : IBootstrapTask
{
    private readonly IReadOnlyCollection<EmbeddedResourceUnpackingRequest> _resourceUnpackingRequests;

    public EmbeddedResourceUnpackerService(params EmbeddedResourceUnpackingRequest[] resourceUnpackingRequests)
    {
        _resourceUnpackingRequests = resourceUnpackingRequests;
    }

    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        return Task.WhenAll(
            _resourceUnpackingRequests.Select(
                resourceUnpackingRequest => resourceUnpackingRequest.Assembly.UnpackEmbeddedFileAsync(
                    cancellationToken,
                    resourceUnpackingRequest.ResourceName
                )
            )
        );
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
