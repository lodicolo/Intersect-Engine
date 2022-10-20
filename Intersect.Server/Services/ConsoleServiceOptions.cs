﻿using Intersect.Framework.Services;

namespace Intersect.Server.Services;

[Serializable]
public sealed class ConsoleServiceOptions : ServiceOptions<ConsoleService, ConsoleServiceOptions>
{
    /// Initializes a new instance of the <see cref="ConsoleServiceOptions"/> class.
    public ConsoleServiceOptions() : base(true) { }
}