using Amib.Threading;
using Intersect.Framework.Services;
using Intersect.Server.Services.Background;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Intersect.Server.Services;

public class LogicService : IntersectBackgroundService<LogicService, LogicServiceOptions, LogicServiceOptionsSetup>
{
    private readonly SmartThreadPool _threadPool;

    public LogicService(IOptions<LogicServiceOptions> options, ILogger<LogicService> logger) : base(options, logger)
    {
        _threadPool = new(new STPStartInfo
        {
            EnableLocalPerformanceCounters = true,
            IdleTimeout = Options.IdleTimeout,
            MaxWorkerThreads = Options.MaximumWorkerThreads,
            MinWorkerThreads = Options.MinimumWorkerThreads
        });
    }

    /// <inheritdoc />
    protected override Task ExecuteServiceAsync(CancellationToken cancellationToken)
    {
        return Task.Run(
            () =>
            {
                _threadPool.Start();

                while (!cancellationToken.IsCancellationRequested)
                {
                    Thread.Yield();
                }

                _threadPool.Shutdown();
            },
            cancellationToken
        );
    }

    /// <inheritdoc />
    protected override void HandleConfigurationChange()
    {
        base.HandleConfigurationChange();

        _threadPool.STPStartInfo.IdleTimeout = Options.IdleTimeout;
        _threadPool.STPStartInfo.MaxWorkerThreads = Options.MaximumWorkerThreads;
        _threadPool.STPStartInfo.MinWorkerThreads = Options.MinimumWorkerThreads;
    }
}
