using Intersect.Framework.Services;
using Microsoft.Extensions.Options;

namespace Intersect.Server.Core;

internal sealed class BootstrapperOptions : ServiceOptions<BootstrapperService, BootstrapperOptions>
{
    public BootstrapperOptions() : base(true) { }
}

internal sealed class BootstrapperConfigureOptions : ServiceOptionsSetup<BootstrapperService, BootstrapperOptions>
{
    public BootstrapperConfigureOptions(IServiceProvider services) : base(services)
    {
    }
}

internal sealed class BootstrapperService
    : IntersectBackgroundService<BootstrapperService, BootstrapperOptions, BootstrapperConfigureOptions>
{
    private readonly IHostApplicationLifetime _hostApplicationLifetime;

    private readonly ProgramStartupOptions _programStartupOptions;

    public BootstrapperService(
        IOptions<BootstrapperOptions> options,
        ILogger<BootstrapperService> logger,
        IHostApplicationLifetime hostApplicationLifetime,
        ProgramStartupOptions programStartupOptions
    ) : base(options, logger)
    {
        _hostApplicationLifetime = hostApplicationLifetime;
        _programStartupOptions = programStartupOptions;
    }

    protected override Task ExecuteServiceAsync(CancellationToken cancellationToken) => Task.Run(
        () => { Bootstrapper.Start(_hostApplicationLifetime, cancellationToken, _programStartupOptions.Args); },
        cancellationToken
    );
}