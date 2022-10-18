﻿using Intersect.Framework.Services;

namespace Intersect.Server.Services.Background;

/// <summary>
/// Configuration initializer for <see cref="ConsoleServiceOptions"/>.
/// </summary>
public sealed class ConsoleServiceOptionsSetup : ServiceOptionsSetup<ConsoleService, ConsoleServiceOptions>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleServiceOptionsSetup"/> class.
    /// </summary>
    /// <param name="services">The <see cref="IServiceProvider"/> of the application.</param>
    public ConsoleServiceOptionsSetup(IServiceProvider services) : base(services)
    {
    }
}
