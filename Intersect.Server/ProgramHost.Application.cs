using Intersect.Framework.Services;
using Intersect.Server.Core;
using BootstrapperService = Intersect.Server.Core.BootstrapperService;

namespace Intersect.Server;

internal record ProgramStartupOptions
{
    public string[] Args { get; init; }
}

internal partial class ProgramHost
{
    private void ConfigureApplicationBuilder(IHostBuilder appBuilder) => appBuilder
        .ConfigureServices((_, services) => services.AddSingleton(new ProgramStartupOptions { Args = _args, }))
        .ConfigureIntersectBackgroundServiceDefaults<BootstrapperService, BootstrapperOptions,
            BootstrapperConfigureOptions>()
        .ConfigureWebHostDefaults(ConfigureWebHostDefaults);
}