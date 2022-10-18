using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace Intersect.Server;

internal static partial class ProgramConfiguration
{
    private static partial bool IgnoreDefaultsForConfigureAppBuilder() => false;

    static partial void ConfigureAppBuilderForEnvironment(
        WebHostBuilderContext context,
        IApplicationBuilder app
    ) { }

    static partial void ConfigureAppBuilderCommon(WebHostBuilderContext context, IApplicationBuilder app) { }
}
