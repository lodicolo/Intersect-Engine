using System.Reflection;
using Intersect.Framework.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace Intersect.Framework.Application;

public sealed partial class IntersectApplication : IHost
{
    private readonly IHost _host;

    internal IntersectApplication(IHost host)
    {
        _host = host;
    }

    public IServiceProvider Services => _host.Services;

    public void Dispose() => _host.Dispose();

    public async Task StartAsync(CancellationToken cancellationToken = new())
    {
        try
        {
            await _host.StartAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            try
            {
                var exceptionHandlerService = _host.Services.GetRequiredService<IExceptionHandlerService>();
                exceptionHandlerService.DispatchUnhandledException(exception, true);
            }
            catch (InvalidOperationException invalidOperationException)
            {
                var aggregateException = new AggregateException(invalidOperationException, exception);
                Log.Fatal(
                    aggregateException,
                    ApplicationResources.ApplicationUnrecoverableExceptionWhileHandlingException
                );
                Environment.Exit(3);
            }
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = new())
    {
        try
        {
            await _host.StopAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            try
            {
                var exceptionHandlerService = _host.Services.GetRequiredService<IExceptionHandlerService>();
                exceptionHandlerService.DispatchUnhandledException(exception, true);
            }
            catch (InvalidOperationException invalidOperationException)
            {
                var aggregateException = new AggregateException(invalidOperationException, exception);
                Log.Fatal(
                    aggregateException,
                    ApplicationResources.ApplicationUnrecoverableExceptionWhileHandlingException
                );
                Environment.Exit(3);
            }
        }
    }
}