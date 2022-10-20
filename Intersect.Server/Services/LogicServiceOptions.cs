using System.ComponentModel.DataAnnotations;
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

    /// <summary>
    /// The time in milliseconds that a thread can be idle before being shut down.
    /// </summary>
    public int IdleTimeout { get; set; } = 30000;

    /// <summary>
    /// The maximum number of worker threads that will be used to process game logic.
    /// </summary>
    public int MaximumWorkerThreads { get; set; } = 2;

    /// <summary>
    /// The minimum number of worker threads that will be used to process game logic.
    /// </summary>
    public int MinimumWorkerThreads { get; set; } = 2;

    /// <inheritdoc />
    public override bool Equals(LogicServiceOptions? other) => base.Equals(other) && IdleTimeout == other.IdleTimeout &&
                                                               MaximumWorkerThreads == other.MaximumWorkerThreads &&
                                                               MinimumWorkerThreads == other.MinimumWorkerThreads;

    /// <inheritdoc />
    public override int GetHashCode() => base.GetHashCode();

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();

        if (IdleTimeout < -1)
        {
            throw new ValidationException(
                string.Format(LogicServiceStrings.IdleTimeoutMustBeGreaterThanOrEqualToNegative1, nameof(IdleTimeout))
            );
        }

        if (MinimumWorkerThreads < 0)
        {
            throw new ValidationException(
                string.Format(LogicServiceStrings.MinimumWorkerThreadsCannotBeNegative, nameof(MinimumWorkerThreads))
            );
        }

        if (MaximumWorkerThreads < 1)
        {
            throw new ValidationException(
                string.Format(LogicServiceStrings.MaximumWorkerThreadsMustBeAtLeast1, nameof(MaximumWorkerThreads))
            );
        }

        if (MaximumWorkerThreads < MinimumWorkerThreads)
        {
            throw new ValidationException(
                string.Format(
                    LogicServiceStrings.MaximumWorkerThreadsCannotBeLessThanMinimumWorkerThreads,
                    nameof(MaximumWorkerThreads),
                    nameof(MinimumWorkerThreads)
                )
            );
        }
    }
}
