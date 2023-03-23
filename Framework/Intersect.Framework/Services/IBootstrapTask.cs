using Microsoft.Extensions.Configuration;

namespace Intersect.Framework.Services;

public interface IBootstrapTask
{
    // ReSharper disable once UnusedParameter.Global
    Task ExecuteAsync(
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken
    );
}