using Intersect.Framework.Eventing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Intersect.Framework.Services;

public sealed class ExceptionHandlerService : IExceptionHandlerService, IHostedService
{
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly ILogger<ExceptionHandlerService> _logger;
    private readonly IServiceProvider _serviceProvider;

    /// <inheritdoc />
    public event EventHandler<UnhandledExceptionEventArgs>? UnhandledException;

    /// <inheritdoc />
    public event EventHandler<UnobservedTaskExceptionEventArgs>? UnobservedTaskException;

    public ExceptionHandlerService(
        IHostApplicationLifetime applicationLifetime,
        ILogger<ExceptionHandlerService> logger,
        IServiceProvider serviceProvider
    )
    {
        _applicationLifetime = applicationLifetime;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public void DispatchUnhandledException(Exception exception, bool isTerminating) => DispatchUnhandledException(
        sender: default,
        exception: exception,
        isTerminating: isTerminating
    );

    public void DispatchUnhandledException(object? sender, Exception exception, bool isTerminating) =>
        HandleUnhandledTaskException(sender: sender, args: new(exception: exception, isTerminating: isTerminating));

    private void HandleUnhandledTaskException(object? sender, UnhandledExceptionEventArgs args)
    {
        UnhandledException?.Invoke(new AggregateSender<IExceptionHandlerService>(this, sender), args);

        if (args.IsTerminating)
        {
            _applicationLifetime.StopApplication();
        }
    }

    private void HandleUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs args)
    {
        UnobservedTaskException?.Invoke(new AggregateSender<IExceptionHandlerService>(this, sender), args);
        _applicationLifetime.StopApplication();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        AppDomain.CurrentDomain.UnhandledException += HandleUnhandledTaskException;
        TaskScheduler.UnobservedTaskException += HandleUnobservedTaskException;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        AppDomain.CurrentDomain.UnhandledException -= HandleUnhandledTaskException;
        TaskScheduler.UnobservedTaskException -= HandleUnobservedTaskException;
        return Task.CompletedTask;
    }
}
