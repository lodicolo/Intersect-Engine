using System.Diagnostics;
using Intersect.Framework.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Options = Intersect.Framework.Configuration.Options;

namespace Intersect.Framework.Services;

[Serializable]
public abstract class ServiceOptions : Options, IEquatable<ServiceOptions>
{
    protected ServiceOptions() { }

    protected ServiceOptions(bool enabled = false)
    {
        Enabled = enabled;
    }

    /// <summary>
    /// Provides a configuration source where service options will be loaded from on start.
    /// The default is <see langword="null"/>.
    /// </summary>
    public new ServiceConfigurationLoader? ConfigurationLoader { get; internal set; }

    /// <summary>
    /// If the service is enabled or not.
    /// The default is <see langword="false"/>.
    /// </summary>
    public bool Enabled { get; protected internal set; }

    public override void CopyTo(Options other)
    {
        base.CopyTo(other);

        CopyTo((ServiceOptions)other);
    }

    public virtual void CopyTo(ServiceOptions other)
    {
        other.Enabled = Enabled;
    }

    /// <inheritdoc />
    public virtual bool Equals(ServiceOptions? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Enabled == other.Enabled;
    }

    /// <inheritdoc />
    // ReSharper disable once NonReadonlyMemberInGetHashCode
    public override int GetHashCode() => HashCode.Combine(Enabled);
}

[Serializable]
public abstract class ServiceOptions<TService, TOptions> : ServiceOptions, IEquatable<TOptions>
    where TOptions : ServiceOptions<TService, TOptions>, new()
{
    protected ServiceOptions() { }

    protected ServiceOptions(bool enabled = false) : base(enabled) { }

    public IServiceProvider? Services { get; internal set; }

    public ServiceConfigurationLoader Configure(IConfiguration configuration, bool reloadOnChange)
    {
        if (Services is null)
        {
            throw new InvalidOperationException(
                string.Format(
                    ConfigurationStrings.ServicesNotSet,
                    nameof(Services),
                    nameof(IConfigureOptions<TOptions>)
                )
            );
        }

        var hostEnvironment = Services.GetRequiredService<IHostEnvironment>();
        var logger = Services.GetRequiredService<ILogger<TService>>();

        var configurationLoader = new ServiceConfigurationLoader<TService, TOptions>(
            logger,
            hostEnvironment,
            this,
            configuration,
            reloadOnChange
        );
        ConfigurationLoader = configurationLoader;
        return configurationLoader;
    }

    public override void CopyTo(ServiceOptions other)
    {
        base.CopyTo(other);

        CopyTo((ServiceOptions<TService, TOptions>)other);
    }

    public virtual void CopyTo(ServiceOptions<TService, TOptions> other) { }

    /// <inheritdoc />
    public virtual bool Equals(TOptions? other) => base.Equals(other);
}
