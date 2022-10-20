using Intersect.Framework.Services;

namespace Intersect.Server.Services.Background;

/// <summary>
///
/// </summary>
[Serializable]
public sealed class LogicServiceOptions : ServiceOptions<LogicService, LogicServiceOptions>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LogicServiceOptions"/> class.
    /// </summary>
    public LogicServiceOptions() : base(true) { }

    public int IdleTimeout { get; set; } = 20000;

    /// <summary>
    /// The maximum number of worker threads that will be used to process game logic.
    /// </summary>
    public int MaximumWorkerThreads { get; set; } = 2;

    /// <summary>
    /// The minimum number of worker threads that will be used to process game logic.
    /// </summary>
    public int MinimumWorkerThreads { get; set; } = 2;
}
