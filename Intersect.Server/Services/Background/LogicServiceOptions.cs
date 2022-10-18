using Intersect.Framework.Services;

namespace Intersect.Server.Services.Background;

[Serializable]
public sealed class LogicServiceOptions : ServiceOptions<LogicService, LogicServiceOptions>
{
    public LogicServiceOptions() : base(true) { }

    public int IdleTimeout { get; set; } = 20000;

    public int MaximumWorkerThreads { get; set; } = 2;

    public int MinimumWorkerThreads { get; set; } = 2;
}
