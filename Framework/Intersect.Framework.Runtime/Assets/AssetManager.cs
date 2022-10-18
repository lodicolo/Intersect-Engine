using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace Intersect.Framework.Runtime.Assets;

/// <summary>
/// Generic implementation of <see cref="IAssetManager"/>.
/// </summary>
public sealed class AssetManager : IAssetManager
{
    private readonly Dictionary<string, IAsset> _assets = new();
    private readonly HashSet<IAssetLoader> _loaders = new();
    private readonly Dictionary<Type, HashSet<IAssetLoader>> _loadersByType = new();
    private readonly ILogger? _logger;
    private readonly HashSet<IAssetSource> _sources = new();

    /// <summary>
    /// Creates an instance of this <see cref="AssetManager"/>.
    /// </summary>
    /// <param name="loggerFactory">The optional <see cref="ILoggerFactory"/> to use to create a logger for messages.</param>
    public AssetManager(ILoggerFactory? loggerFactory)
    {
        _logger = loggerFactory?.CreateLogger(typeof(AssetManager));
    }

    /// <inheritdoc />
    public IReadOnlyCollection<IAssetLoader> Loaders => _loaders;

    /// <inheritdoc />
    public IReadOnlyCollection<IAssetSource> Sources => _sources;

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">Thrown if the asset loader is listed in the type-specific loaders,
    /// but is not known to the manager (perhaps an error occurred while removing it previously).</exception>
    public IAssetManager AddLoader<TAsset>(IAssetLoader<TAsset> assetLoader) where TAsset : class, IAsset
    {
        var type = typeof(TAsset);

        if (!_loaders.Add(assetLoader))
        {
            // Loader already registered
            return this;
        }

        var loadersForType = (_loadersByType[type] ??= new());
        if (!loadersForType.Add(assetLoader))
        {
            throw new InvalidOperationException(
                AssetManagerResources.AddLoader_AddingAnAssetLoaderThatWasNotCompletelyRemoved
            );
        }

        return this;
    }

    /// <inheritdoc />
    public IAssetManager AddSource(IAssetSource assetSource)
    {
        _sources.Add(assetSource);
        return this;
    }

    /// <inheritdoc />
    public bool TryGet<TAsset>(
        string assetName,
        [NotNullWhen(true)] out TAsset? asset
    ) where TAsset : class, IAsset
    {
        if (!_loadersByType.TryGetValue(typeof(TAsset), out var loadersForType))
        {
            // No loader to even handle this type
            _logger?.LogWarning(
                AssetManagerResources.TryGet_LogWarning_NoLoaderForType,
                typeof(TAsset).FullName,
                assetName
            );
            asset = default;
            return false;
        }

        if (_assets.TryGetValue(assetName, out var genericAsset))
        {
            asset = genericAsset as TAsset;
            return asset != default;
        }

        foreach (var source in _sources)
        {
            if (!source.TryResolve(assetName, out var qualifiedAssetName))
            {
                continue;
            }

            if (_assets.TryGetValue(qualifiedAssetName, out genericAsset))
            {
                asset = genericAsset as TAsset;
                return asset != default;
            }

            if (!loadersForType.Any(loader => loader.TryLoad(source, qualifiedAssetName, out genericAsset)))
            {
                continue;
            }

            asset = genericAsset as TAsset;
            if (asset == default)
            {
                _logger?.LogError(
                    AssetManagerResources.TryGet_LogError_ConflictingAssetOfDifferentType,
                    assetName,
                    typeof(TAsset).FullName,
                    genericAsset?.GetType().FullName,
                    qualifiedAssetName
                );

                // The loader returned an invalid asset (of a different type)
                return false;
            }

            _assets[assetName] = asset;
            _assets[qualifiedAssetName] = asset;
            return true;
        }

        _logger?.LogWarning(
            AssetManagerResources.TryGet_LogError_ConflictingAssetOfDifferentType,
            assetName,
            typeof(TAsset).FullName
        );

        asset = default;
        return false;
    }

    /// <inheritdoc />
    public bool TryLoad<TAsset>(string assetName) where TAsset : class, IAsset =>
        TryGet<TAsset>(assetName, out _);

    /// <inheritdoc />
    public bool TryRemoveLoader<TAsset>(IAssetLoader<TAsset> assetLoader) where TAsset : class, IAsset
    {
        if (!_loaders.Remove(assetLoader))
        {
            return false;
        }

        var type = typeof(TAsset);
        return _loadersByType.TryGetValue(type, out var loadersForType) &&
               loadersForType.Remove(assetLoader);
    }

    /// <inheritdoc />
    /// <remarks>Will also return false if it was not registered.</remarks>
    public bool TryRemoveSource(IAssetSource assetSource) => _sources.Remove(assetSource);
}
