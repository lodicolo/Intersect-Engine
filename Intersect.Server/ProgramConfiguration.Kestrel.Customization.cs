using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace Intersect.Server;

internal static partial class ProgramConfiguration
{
    private static partial bool IgnoreDefaultsForConfigureKestrel() => false;

    static partial void ConfigureKestrelForEnvironment(
        WebHostBuilderContext context,
        KestrelServerOptions kestrelServerOptions
    ) { }

    static partial void ConfigureKestrelCommon(
        WebHostBuilderContext context,
        KestrelServerOptions kestrelServerOptions
    ) { }
}
