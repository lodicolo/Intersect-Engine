using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Intersect.Framework.Services;

[Serializable]
public abstract class ServiceOptions : IEquatable<ServiceOptions>
{
    protected ServiceOptions() { }

    protected ServiceOptions(bool enabled = false)
    {
        Enabled = enabled;
    }

    /// <summary>
    /// If the service is enabled or not.
    /// The default is <see langword="false"/>.
    /// </summary>
    public bool Enabled { get; protected internal set; }

    public virtual void CopyTo(ServiceOptions other)
    {
        other.Enabled = Enabled;
    }

    /// <inheritdoc />
    public bool Equals(ServiceOptions? other)
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
    public override bool Equals(object? obj) => obj is ServiceOptions other && Equals(other);

    /// <inheritdoc />
    // ReSharper disable once NonReadonlyMemberInGetHashCode
    public override int GetHashCode() => HashCode.Combine(Enabled);
}

[Serializable]
public abstract class ServiceOptions<TService, TOptions> : ServiceOptions
    where TOptions : ServiceOptions<TService, TOptions>
{
    protected ServiceOptions() { }

    protected ServiceOptions(bool enabled = false) : base(enabled) { }

    /// <summary>
    /// Provides a configuration source where endpoints will be loaded from on start.
    /// The default is <see langword="null"/>.
    /// </summary>
    public ServiceConfigurationLoader? ConfigurationLoader { get; internal set; }

    public IServiceProvider? Services { get; internal set; }

    public ServiceConfigurationLoader Configure(IConfiguration configuration, bool reloadOnChange)
    {
        if (Services is null)
        {
            throw new InvalidOperationException(
                string.Format(
                    ServicesResources.ServiceOptions_Configure_OptionsServicesNotSet,
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
        if (other is not TOptions otherTOptions)
        {
            throw new InvalidOperationException(
                string.Format(
                    ServicesResources.ServiceOptions_CopyTo_TypeMismatch,
                    other.GetType().FullName,
                    typeof(TOptions).FullName
                )
            );
        }

        base.CopyTo(other);

        CopyTo(otherTOptions);
    }

    public virtual void CopyTo(TOptions other) { }
}
