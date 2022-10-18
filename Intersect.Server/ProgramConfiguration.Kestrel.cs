using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace Intersect.Server;

internal static partial class ProgramConfiguration
{
    private static partial bool IgnoreDefaultsForConfigureKestrel();

    internal static void ConfigureKestrel(WebHostBuilderContext context, KestrelServerOptions kestrelServerOptions)
    {
        if (IgnoreDefaultsForConfigureKestrel())
        {
            ConfigureKestrelForEnvironment(context, kestrelServerOptions);
            ConfigureKestrelCommon(context, kestrelServerOptions);
        }

        ConfigureKestrelForEnvironment(context, kestrelServerOptions);
        ConfigureKestrelCommon(context, kestrelServerOptions);
    }

    static partial void ConfigureKestrelForEnvironment(
        WebHostBuilderContext context,
        KestrelServerOptions kestrelServerOptions
    );

    static partial void ConfigureKestrelCommon(
        WebHostBuilderContext context,
        KestrelServerOptions kestrelServerOptions
    );
}
