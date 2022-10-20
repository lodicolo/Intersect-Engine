using Intersect.Framework.Services;

namespace Intersect.Server.Services;

/// <summary>
/// Configuration initializer for <see cref="LogicServiceOptions"/>.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global Instantiated via generic host
public sealed class LogicServiceOptionsSetup : ServiceOptionsSetup<LogicService, LogicServiceOptions>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleServiceOptionsSetup"/> class.
    /// </summary>
    /// <param name="services">The <see cref="IServiceProvider"/> of the application.</param>
    public LogicServiceOptionsSetup(IServiceProvider services) : base(services)
    {
    }
}
