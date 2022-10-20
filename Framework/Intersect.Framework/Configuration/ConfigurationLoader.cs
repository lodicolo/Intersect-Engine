using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Intersect.Framework.Configuration;

public abstract class ConfigurationLoader
{
    private bool _loaded;

    internal ConfigurationLoader(
        ILogger logger,
        IHostEnvironment hostEnvironment,
        Options options,
        IConfiguration configuration,
        bool reloadOnChange
    )
    {
        Logger = logger;
        HostEnvironment = hostEnvironment;
        Options = options;
        Configuration = configuration;
        ReloadOnChange = reloadOnChange;
    }

    public IConfiguration Configuration { get; }

    protected IHostEnvironment HostEnvironment { get; }

    /// <summary>
    /// The <see cref="ILogger"/> instance for this loader.
    /// </summary>
    protected ILogger Logger { get; }

    protected internal Options Options { get; }

    public bool ReloadOnChange { get; }

    public Options Load()
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

public class ConfigurationLoader<TLoggerCategory, TOptions> : ConfigurationLoader
    where TOptions : Options<TOptions>, new()
{
    public ConfigurationLoader(
        ILogger<TLoggerCategory> logger,
        IHostEnvironment hostEnvironment,
        Options<TOptions> options,
        IConfiguration configuration,
        bool reloadOnChange
    ) : base(logger, hostEnvironment, options, configuration, reloadOnChange)
    {
        Logger = logger;
        Options = options;
    }

    /// <summary>
    /// The <see cref="ILogger{TCategoryName}"/> instance for this loader.
    /// </summary>
    public new ILogger<TLoggerCategory> Logger { get; }

    protected new Options<TOptions> Options { get; }

    private static void ConfigureBinderOptions(BinderOptions binderOptions)
    {
        binderOptions.BindNonPublicProperties = true;
        binderOptions.ErrorOnUnknownConfiguration = true;
    }

    /// <inheritdoc />
    public override bool Reload()
    {
        var reloadedOptions = Configuration.Get<TOptions>(ConfigureBinderOptions) ?? new TOptions();
        var changed = !reloadedOptions.Equals(Options);
        reloadedOptions.CopyTo(Options);
        return changed;
    }
}
