using Intersect.Framework.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Options = Intersect.Framework.Configuration.Options;

namespace Intersect.Framework.Services;

/// <summary>
/// The representation of generic options for services.
/// </summary>
[Serializable]
public abstract class ServiceOptions : Options, IEquatable<ServiceOptions>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceOptions"/> class.
    /// </summary>
    protected ServiceOptions() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceOptions"/> class.
    /// </summary>
    /// <param name="enabled">If the <see cref="Enabled"/> is true by default.</param>
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

    /// <inheritdoc />
    public override void CopyTo(Options other)
    {
        base.CopyTo(other);

        CopyTo((ServiceOptions)other);
    }

    /// <summary>
    /// Copies the configuration values of this instance to the provided instance.
    /// </summary>
    /// <param name="other">The instance to copy configuration values to.</param>
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

/// <summary>
/// The generic-typed representation of generic options for services.
/// </summary>
/// <typeparam name="TService">The type of service this is meant for.</typeparam>
/// <typeparam name="TOptions">The concrete type of options.</typeparam>
[Serializable]
public abstract class ServiceOptions<TService, TOptions> : ServiceOptions, IEquatable<TOptions>
    where TOptions : ServiceOptions<TService, TOptions>, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceOptions{TService,TOptions}"/> class.
    /// </summary>
    protected ServiceOptions() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceOptions{TService,TOptions}"/> class.
    /// </summary>
    /// <param name="enabled">If the <see cref="ServiceOptions.Enabled"/> is true by default.</param>
    protected ServiceOptions(bool enabled = false) : base(enabled) { }

    /// <summary>
    /// The <see cref="IServiceProvider"/> of the application host.
    /// </summary>
    public IServiceProvider? Services { get; internal set; }

    /// <summary>
    /// Configure the loader for this <see cref="ServiceOptions{TService,TOptions}"/> instance and returns it.
    /// </summary>
    /// <param name="configuration">The <see cref="IConfiguration"/> instance the loader will use.</param>
    /// <param name="reloadOnChange">If the configuration loader should listen for changes to the configuration.</param>
    /// <returns>The configured <see cref="ServiceConfigurationLoader"/> instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <see cref="Services"/> is null when this method is called. This is usually performed by <see cref="ServiceOptionsSetup{TService,TOptions}"/>.
    /// </exception>
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

    /// <inheritdoc />
    public override void CopyTo(ServiceOptions other)
    {
        base.CopyTo(other);

        CopyTo((ServiceOptions<TService, TOptions>)other);
    }

    /// <summary>
    /// Copies the configuration values from this instance of <see cref="ServiceOptions{TService,TOptions}"/> to another.
    /// </summary>
    /// <param name="other">The instance to copy configuration values to.</param>
    public virtual void CopyTo(ServiceOptions<TService, TOptions> other) { }

    /// <inheritdoc />
    public virtual bool Equals(TOptions? other) => base.Equals(other);
}
