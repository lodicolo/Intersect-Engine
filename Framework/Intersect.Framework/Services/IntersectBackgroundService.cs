using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Intersect.Framework.Services;

public abstract class
    IntersectBackgroundService<TService, TOptions, TConfigureOptions> : BackgroundService, IAsyncDisposable
    where TService : IntersectBackgroundService<TService, TOptions, TConfigureOptions>
    where TOptions : ServiceOptions<TService, TOptions>, new()
    where TConfigureOptions : class, IConfigureOptions<TOptions>
{
    /// <summary>
    /// The <see cref="ILogger{TCategoryName}"/> for this service.
    /// </summary>
    protected readonly ILogger<TService> Logger;

    /// <summary>
    /// The <see cref="TOptions"/> for this service.
    /// </summary>
    protected readonly TOptions Options;

    private readonly SemaphoreSlim _executionSemaphore;

    private readonly CancellationTokenSource _stopCancellationTokenSource;

    private readonly TaskCompletionSource<bool> _stopTaskCompletionSource;

    private IDisposable? _configurationChangedRegistration;

    private int _stopping;

    protected IntersectBackgroundService(IOptions<TOptions> options, ILogger<TService> logger)
    {
        Logger = logger;
        Options = options.Value ?? new TOptions();
        _executionSemaphore = new(initialCount: 1);
        _stopCancellationTokenSource = new();
        _stopTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    /// <inheritdoc />
    public sealed override Task StartAsync(CancellationToken cancellationToken) => base.StartAsync(cancellationToken);

    /// <inheritdoc />
    public sealed override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (Interlocked.Exchange(ref _stopping, 1) != 0)
        {
            await _stopTaskCompletionSource.Task.ConfigureAwait(false);
            return;
        }

        _stopCancellationTokenSource.Cancel();

        await _executionSemaphore.WaitAsync(default(CancellationToken)).ConfigureAwait(false);

        try
        {
            await base.StopAsync(cancellationToken);
        }
        finally
        {
            _configurationChangedRegistration?.Dispose();
            _stopCancellationTokenSource.Dispose();
            _executionSemaphore.Release();
        }

        _stopTaskCompletionSource.TrySetResult(true);
    }

    /// <inheritdoc />
    protected sealed override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            if (ExecuteTask != default)
            {
                throw new InvalidOperationException(
                    string.Format(
                        ServicesResources.IntersectBackgroundService_ExecuteAsync_TServiceAlreadyStarted,
                        typeof(TService).Name
                    )
                );
            }

            Options.ConfigurationLoader?.Load();

            var executionState = new ExecutionState(
                Service: this,
                IsRunning: false,
                ExecutionCancellationTokenSource: CancellationTokenSource.CreateLinkedTokenSource(stoppingToken),
                StoppingToken: stoppingToken
            );
            await TryBeginExecutionAsync(executionState, true).ConfigureAwait(false);
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    private static void OnConfigurationChanged(object state)
    {
        var executionState = (ExecutionState)state;
        _ = executionState.Service.TryBeginExecutionAsync(executionState, false);
    }

    private async Task TryBeginExecutionAsync(ExecutionState executionState, bool initial)
    {
        await _executionSemaphore.WaitAsync(executionState.StoppingToken).ConfigureAwait(false);

        try
        {
            if (_stopping != 0)
            {
                throw new InvalidOperationException(
                    ServicesResources.IntersectBackgroundService_ExecuteAsync_ServiceShutdownAlreadyBegun
                );
            }

            IChangeToken? reloadToken = default;

            if (Options.ConfigurationLoader?.ReloadOnChange == true)
            {
                reloadToken = Options.ConfigurationLoader.Configuration.GetReloadToken();
            }

            var changed = !initial && (Options.ConfigurationLoader?.Reload() ?? false);
            var enabled = Options.ConfigurationLoader?.Options.Enabled ?? false;

            var executionCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                _stopCancellationTokenSource.Token,
                executionState.StoppingToken
            );

            if (enabled)
            {
                await ExecuteService(executionCancellationTokenSource.Token);
            }
            else if (executionState.IsRunning)
            {
                executionState.ExecutionCancellationTokenSource.Cancel();
            }

            // need to pass an execution context so we can cancel
            _configurationChangedRegistration = reloadToken?.RegisterChangeCallback(
                OnConfigurationChanged,
                new ExecutionState(
                    Service: this,
                    IsRunning: enabled,
                    ExecutionCancellationTokenSource: executionCancellationTokenSource,
                    StoppingToken: executionState.StoppingToken
                )
            );
        }
        finally
        {
            _executionSemaphore.Release();
        }
    }

    protected abstract Task ExecuteService(CancellationToken cancellationToken);

    protected virtual void ValidateOptions() { }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        base.Dispose();
        return new(Task.CompletedTask);
    }

    private record ExecutionState(
        IntersectBackgroundService<TService, TOptions, TConfigureOptions> Service,
        bool IsRunning,
        CancellationTokenSource ExecutionCancellationTokenSource,
        CancellationToken StoppingToken
    )
    {
        public CancellationTokenSource ExecutionCancellationTokenSource { get; } = ExecutionCancellationTokenSource;

        public bool IsRunning { get; } = IsRunning;

        public IntersectBackgroundService<TService, TOptions, TConfigureOptions> Service { get; } = Service;

        public CancellationToken StoppingToken { get; } = StoppingToken;
    }
}
