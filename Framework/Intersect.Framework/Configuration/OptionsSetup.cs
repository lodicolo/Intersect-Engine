using Microsoft.Extensions.Options;

namespace Intersect.Framework.Configuration;

/// <summary>
/// Generic configuration initializer for <see cref="Options{TOptions}"/>.
/// </summary>
/// <typeparam name="TOptions">The type of <see cref="Options"/> this class configures.</typeparam>
public abstract class OptionsSetup<TOptions> : IConfigureOptions<TOptions> where TOptions : Options<TOptions>, new()
{
    private readonly IServiceProvider _services;

    /// <summary>
    /// Initializes a new instance of the <see cref="OptionsSetup{TOptions}"/> class.
    /// </summary>
    /// <param name="services">The <see cref="IServiceProvider"/> of the application.</param>
    protected OptionsSetup(IServiceProvider services)
    {
        _services = services;
    }

    /// <summary>
    /// Configures an <see cref="TOptions"/> instance.
    /// </summary>
    /// <param name="options">The <see cref="TOptions"/> instance to configure.</param>
    public virtual void Configure(TOptions options)
    {
        options.Services = _services;
    }
}
