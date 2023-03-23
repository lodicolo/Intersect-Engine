using Intersect.Framework.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Intersect.Framework.Services;

/// <summary>
/// Generic typeless configuration loader for services.
/// </summary>
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

    /// <summary>
    /// The <see cref="ServiceOptions"/> this loader is bound to.
    /// </summary>
    protected internal new ServiceOptions Options { get; }
}

/// <summary>
/// Generic typed configuration loader for services.
/// </summary>
/// <typeparam name="TService">The type of service the options are for.</typeparam>
/// <typeparam name="TOptions">The type of options this loader is for.</typeparam>
public class ServiceConfigurationLoader<TService, TOptions> : ServiceConfigurationLoader
    where TOptions : ServiceOptions<TService, TOptions>, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceConfigurationLoader{TService,TOptions}"/> class.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="hostEnvironment"></param>
    /// <param name="serviceOptions"></param>
    /// <param name="configuration"></param>
    /// <param name="reloadOnChange"></param>
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

    /// <summary>
    /// The <see cref="ILogger{TCategoryName}"/> instance for this loader.
    /// </summary>
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
