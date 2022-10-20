using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Intersect.Framework.Configuration;

[Serializable]
public abstract class Options : IEquatable<Options>
{
    /// <summary>
    /// Provides a configuration source where options will be loaded from on start.
    /// The default is <see langword="null"/>.
    /// </summary>
    public ConfigurationLoader? ConfigurationLoader { get; internal set; }

    /// <summary>
    /// Copies the configuration values from this instance of <see cref="Options"/> to another.
    /// </summary>
    /// <param name="other">The instance to copy configuration values to.</param>
    /// <exception cref="InvalidOperationException">Thrown if the other options is not an exact type match.</exception>
    public virtual void CopyTo(Options other)
    {
        if (GetType() != other.GetType())
        {
            throw new InvalidOperationException(
                string.Format(
                    ConfigurationStrings.CopyTo_TypeMismatch,
                    other.GetType().FullName,
                    GetType().FullName
                )
            );
        }
    }

    /// <inheritdoc />
    public virtual bool Equals(Options? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return GetType() == other.GetType();
    }

    /// <inheritdoc />
    public sealed override bool Equals(object? obj) => obj is Options other && Equals(other);

    /// <inheritdoc />
    // ReSharper disable once NonReadonlyMemberInGetHashCode
    public override int GetHashCode() => GetType().GetHashCode();

    public virtual void Validate() { }
}

[Serializable]
public abstract class Options<TOptions> : Options, IEquatable<Options<TOptions>>
    where TOptions : Options<TOptions>, new()
{
    public IServiceProvider? Services { get; internal set; }

    public ConfigurationLoader Configure<TLoggerCategory>(IConfiguration configuration, bool reloadOnChange)
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
        var logger = Services.GetRequiredService<ILogger<TLoggerCategory>>();

        var configurationLoader = new ConfigurationLoader<TLoggerCategory, TOptions>(
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
    public override void CopyTo(Options other)
    {
        if (other is not Options<TOptions> otherTOptions)
        {
            throw new InvalidOperationException(
                string.Format(
                    ConfigurationStrings.CopyTo_TypeMismatch,
                    other.GetType().FullName,
                    typeof(TOptions).FullName
                )
            );
        }

        base.CopyTo(other);

        CopyTo(otherTOptions);
    }

    /// <summary>
    /// Copies the configuration values from this instance of <see cref="Options{TOptions}"/> to another.
    /// </summary>
    /// <param name="other">The instance to copy configuration values to.</param>
    /// <exception cref="InvalidOperationException">Thrown if the other options is not an exact type match.</exception>
    public virtual void CopyTo(Options<TOptions> other) { }

    /// <inheritdoc />
    public virtual bool Equals(Options<TOptions>? other) => base.Equals(other);

}
