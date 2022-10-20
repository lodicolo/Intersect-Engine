using Microsoft.Extensions.Options;

namespace Intersect.Framework.Services;

/// <summary>
/// Generic configuration initializer for <see cref="ServiceOptions{TService, TOptions}"/>.
/// </summary>
/// <typeparam name="TService">The type of service <see cref="TOptions"/> belongs to.</typeparam>
/// <typeparam name="TOptions">The type of <see cref="ServiceOptions"/> this class configures.</typeparam>
public abstract class ServiceOptionsSetup<TService, TOptions> : IConfigureOptions<TOptions>
    where TOptions : ServiceOptions<TService, TOptions>, new()
{
    private readonly IServiceProvider _services;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceOptionsSetup{TService,TOptions}"/> class.
    /// </summary>
    /// <param name="services">The <see cref="IServiceProvider"/> of the application.</param>
    protected ServiceOptionsSetup(IServiceProvider services)
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
