using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Intersect.Framework.Services;

public static class ExceptionHandlerServiceHostBuilderExtensions
{
    public static IHostBuilder UseExceptionHandler(this IHostBuilder hostBuilder) =>
        hostBuilder.ConfigureServices((_, services) => services.AddHostedService<ExceptionHandlerService>());
}
