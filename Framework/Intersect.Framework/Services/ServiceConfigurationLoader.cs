using Intersect.Framework.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Intersect.Framework.Services;

public abstract class ServiceConfigurationLoader : ConfigurationLoader
{
    internal ServiceConfigurationLoader(
        ILogger logger,
        IHostEnvironment hostEnvironment,
        ServiceOptions serviceOptions,
        IConfiguration configuration,
        bool reloadOnChange
    ) : base(logger, hostEnvironment, serviceOptions, configuration, reloadOnChange)
    {
        Options = serviceOptions;
    }

    protected internal new ServiceOptions Options { get; }
}

public class ServiceConfigurationLoader<TService, TOptions> : ServiceConfigurationLoader
    where TOptions : ServiceOptions<TService, TOptions>, new()
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

    private static void ConfigureBinderOptions(BinderOptions binderOptions)
    {
        binderOptions.BindNonPublicProperties = true;
        binderOptions.ErrorOnUnknownConfiguration = true;
    }

    public override bool Reload()
    {
        var reloadedOptions = Configuration.Get<TOptions>(ConfigureBinderOptions) ?? new TOptions();
        var changed = !reloadedOptions.Equals(Options);
        reloadedOptions.CopyTo((Options)Options);
        return changed;
    }
}
