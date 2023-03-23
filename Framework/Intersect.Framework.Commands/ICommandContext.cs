namespace Intersect.Framework.Commands;

public interface ICommandContext
{
    bool IsShutdownRequested { get; }

    IServiceProvider ServiceProvider { get; }

    void RequestShutdown();
}
