using Intersect.Framework.FileSystem;
using Intersect.Framework.FileSystem.Overwriting;
using Intersect.Framework.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Intersect.Framework.Services.EmbeddedResourceUnpacker;

public sealed class EmbeddedResourceUnpackerService : IBootstrapTask
{
    private readonly IReadOnlyCollection<EmbeddedResourceUnpackingRequest> _resourceUnpackingRequests;

    public EmbeddedResourceUnpackerService(params EmbeddedResourceUnpackingRequest[] resourceUnpackingRequests)
    {
        _resourceUnpackingRequests = resourceUnpackingRequests;
    }

    public Task ExecuteAsync(
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken
    )
    {
        var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
        var logger = loggerFactory?.CreateLogger<EmbeddedResourceUnpackerService>();

        var appSettingsSourceDir = configuration.GetValue<string>("INTERSECT_BINARY_DIR");
        logger?.LogDebug("INTERSECT_BINARY_DIR={INTERSECT_BINARY_DIR}", appSettingsSourceDir);

        if (appSettingsSourceDir?.Contains('\\') ?? false)
        {
            appSettingsSourceDir = appSettingsSourceDir.Replace('\\', Path.DirectorySeparatorChar);
            logger?.LogDebug("Patched INTERSECT_BINARY_DIR to {INTERSECT_BINARY_DIR}", appSettingsSourceDir);
        }

        logger?.LogDebug(
            "Sourcing appsettings.json/appsettings.*.json from {appSettingsSourceDir}",
            appSettingsSourceDir
        );

        var overwriteBehaviorRaw =
            configuration.GetValue<string>("INTERSECT_EMBEDDEDRESOURCEUNPACKER_OVERWRITEBEHAVIOR");
        logger?.LogDebug(
            "INTERSECT_EMBEDDEDRESOURCEUNPACKER_OVERWRITEBEHAVIOR={INTERSECT_EMBEDDEDRESOURCEUNPACKER_OVERWRITEBEHAVIOR}",
            overwriteBehaviorRaw
        );

        var overwriteBehavior =
            !string.IsNullOrWhiteSpace(overwriteBehaviorRaw) &&
            Enum.TryParse<OverwriteBehavior>(overwriteBehaviorRaw, true, out var parsedBehavior) ? parsedBehavior
                : OverwriteBehavior.DoNotOvewrite;
        logger?.LogDebug(
            "Parsed INTERSECT_EMBEDDEDRESOURCEUNPACKER_OVERWRITEBEHAVIOR to {overwriteBehavior}",
            overwriteBehavior
        );

        var fileOverwriter = new FileOverwriter(loggerFactory) { OverwriteBehavior = overwriteBehavior, };

        return Task.WhenAll(
            _resourceUnpackingRequests.Select(
                resourceUnpackingRequest =>
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return Task.FromCanceled(cancellationToken);
                    }

                    var resourceName = resourceUnpackingRequest.ResourceName;
                    var overwriteConditions = string.IsNullOrWhiteSpace(appSettingsSourceDir)
                        ? Array.Empty<IOverwriteCondition>() : new[]
                        {
                            new IfIsNewer(Path.Join(appSettingsSourceDir, resourceName))
                        };

                    logger?.LogInformation(
                        "Unpacking {resourceName} from {assemblyShortName} ({assemblyFullName}).",
                        resourceUnpackingRequest.Assembly.GetName().Name,
                        resourceUnpackingRequest.Assembly.FullName
                    );
                    return resourceUnpackingRequest.Assembly.UnpackEmbeddedFileAsync(
                        cancellationToken,
                        resourceName,
                        fileOverwriter.AddOverwriteConditions(overwriteConditions)
                    );
                }
            )
        );
    }
}