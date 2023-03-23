namespace Intersect.Framework.Services;

public interface IExceptionHandlerService
{
    event EventHandler<UnhandledExceptionEventArgs>? UnhandledException;

    event EventHandler<UnobservedTaskExceptionEventArgs>? UnobservedTaskException;

    void DispatchUnhandledException(Exception exception, bool isTerminating);
}
