using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Intersect.Framework.Services;

public abstract class ServiceConfigurationLoader
{
    private bool _loaded;

    internal ServiceConfigurationLoader(
        ILogger logger,
        IHostEnvironment hostEnvironment,
        ServiceOptions serviceOptions,
        IConfiguration configuration,
        bool reloadOnChange
    )
    {
        Logger = logger;
        HostEnvironment = hostEnvironment;
        Options = serviceOptions;
        Configuration = configuration;
        ReloadOnChange = reloadOnChange;
    }

    public IConfiguration Configuration { get; }

    protected IHostEnvironment HostEnvironment { get; }

    protected ILogger Logger { get; }

    protected internal ServiceOptions Options { get; }

    public bool ReloadOnChange { get; }

    public ServiceOptions Load()
    {
        if (_loaded)
        {
            return Options;
        }

        _loaded = true;

        Reload();

        return Options;
    }

    public abstract bool Reload();
}

public class ServiceConfigurationLoader<TService, TOptions> : ServiceConfigurationLoader
    where TOptions : ServiceOptions<TService, TOptions>
{
    public ServiceConfigurationLoader(
        ILogger<TService> logger,
        IHostEnvironment hostEnvironment,
        ServiceOptions<TService, TOptions> serviceOptions,
        IConfiguration configuration,
        bool reloadOnChange
    ) : base(logger, hostEnvironment, serviceOptions, configuration, reloadOnChange)
    {
        Logger = logger;
        Options = serviceOptions;
    }

    protected new ILogger<TService> Logger { get; }

    protected new ServiceOptions<TService, TOptions> Options { get; }

    public override bool Reload()
    {
        var reloadedOptions = Configuration.Get<TOptions>();
        var changed = reloadedOptions.Equals(Options);
        reloadedOptions.CopyTo(Options);
        return changed;
    }
}
