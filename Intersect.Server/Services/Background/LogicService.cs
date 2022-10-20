using Intersect.Framework.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Intersect.Server.Services.Background;

public class LogicService : IntersectBackgroundService<LogicService, LogicServiceOptions, LogicServiceOptionsSetup>
{
    public LogicService(IOptions<LogicServiceOptions> options, ILogger<LogicService> logger) : base(options, logger) { }

    /// <inheritdoc />
    protected override Task ExecuteServiceAsync(CancellationToken cancellationToken)
    {
        return Task.Run(
            () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    Thread.Yield();
                }
            },
            cancellationToken
        );
    }
}
