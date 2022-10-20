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

    /// <summary>
    /// The name of the service. This is the type name with <c>"Service"</c> removed.
    /// </summary>
    protected readonly string ServiceName = typeof(TService).Name.Replace("Service", string.Empty);

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

    private CancellationTokenSource CreateCancellationTokenSource(CancellationToken stoppingToken)
    {
        return CancellationTokenSource.CreateLinkedTokenSource(_stopCancellationTokenSource.Token, stoppingToken);
    }

    /// <inheritdoc />
    public sealed override Task StartAsync(CancellationToken cancellationToken) =>
        base.StartAsync(cancellationToken: cancellationToken);

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
                    string.Format(ServicesResources.BackgroundService_ExecuteAsync_ServiceAlreadyStarted, ServiceName)
                );
            }

            Options.ConfigurationLoader?.Load();

            var executionState = new ExecutionState(
                Service: this,
                ExecutionTask: default,
                ExecutionCancellationTokenSource: CreateCancellationTokenSource(stoppingToken),
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

        bool enabled;
        IChangeToken? reloadToken = default;
        var executionTask = executionState.ExecutionTask;
        var executionCancellationTokenSource = executionState.ExecutionCancellationTokenSource;

        try
        {
            if (_stopping != 0)
            {
                throw new InvalidOperationException(
                    ServicesResources.BackgroundService_ExecuteAsync_ServiceShutdownAlreadyBegun
                );
            }

            if (Options.ConfigurationLoader?.ReloadOnChange == true)
            {
                reloadToken = Options.ConfigurationLoader.Configuration.GetReloadToken();
            }

            var changed = !initial && (Options.ConfigurationLoader?.Reload() ?? false);

            if (!changed && !initial)
            {
                executionTask = executionState.ExecutionTask;
                return;
            }

            ValidateOptions();

            enabled = Options.ConfigurationLoader?.Options.Enabled ?? false;

            switch (enabled)
            {
                case true when executionState.ExecutionTask == default:
                    Logger.LogInformation(
                        changed ? ServicesResources.BackgroundService_StartingServiceDueToConfigurationChange
                            : ServicesResources.BackgroundService_StartingService,
                        ServiceName
                    );

                    executionCancellationTokenSource = CreateCancellationTokenSource(executionState.StoppingToken);

                    executionTask = Task.Run(
                        async () =>
                        {
                            await ExecuteServiceAsync(executionCancellationTokenSource.Token);
                            Logger.LogInformation(
                                (_stopCancellationTokenSource.IsCancellationRequested ||
                                 executionState.StoppingToken.IsCancellationRequested)
                                    ? ServicesResources.BackgroundService_StoppingService
                                    : ServicesResources.BackgroundService_StoppingServiceDueToConfigurationChange,
                                ServiceName
                            );
                        },
                        executionCancellationTokenSource.Token
                    );
                    break;

                case false when executionState.ExecutionTask != default:
                    executionState.ExecutionCancellationTokenSource.Cancel();
                    executionTask = default;
                    break;

                case true:
                    Logger.LogInformation(
                        ServicesResources.BackgroundService_ReconfiguringServiceDueToConfigurationChange,
                        ServiceName
                    );
                    HandleConfigurationChange();
                    break;

                case false:
                    break;
            }
        }
        finally
        {
            // need to pass an execution context so we can cancel
            _configurationChangedRegistration = reloadToken?.RegisterChangeCallback(
                OnConfigurationChanged,
                new ExecutionState(
                    Service: this,
                    ExecutionTask: executionTask,
                    ExecutionCancellationTokenSource: executionCancellationTokenSource,
                    StoppingToken: executionState.StoppingToken
                )
            );

            _executionSemaphore.Release();
        }
    }

    protected abstract Task ExecuteServiceAsync(CancellationToken cancellationToken);

    protected virtual void ValidateOptions() { }

    protected virtual void HandleConfigurationChange() { }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        base.Dispose();
        return new(Task.CompletedTask);
    }

    private record ExecutionState(
        IntersectBackgroundService<TService, TOptions, TConfigureOptions> Service,
        Task? ExecutionTask,
        CancellationTokenSource ExecutionCancellationTokenSource,
        CancellationToken StoppingToken
    )
    {
        public CancellationTokenSource ExecutionCancellationTokenSource { get; } = ExecutionCancellationTokenSource;

        public CancellationReason CancellationReason { get; internal set; } = CancellationReason.Default;

        public Task? ExecutionTask { get; } = ExecutionTask;

        public IntersectBackgroundService<TService, TOptions, TConfigureOptions> Service { get; } = Service;

        public CancellationToken StoppingToken { get; } = StoppingToken;
    }

    private enum CancellationReason
    {
        Default,

        ConfigurationChange
    }
}
