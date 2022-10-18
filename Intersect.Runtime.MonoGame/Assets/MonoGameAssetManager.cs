using System.Diagnostics.CodeAnalysis;
using Intersect.Framework.Runtime.Assets;
using Intersect.Runtime.MonoGame.Assets.Loaders;
using Microsoft.Extensions.Logging;

namespace Intersect.Runtime.MonoGame.Assets;

/// <summary>
/// Implementation of <see cref="IAssetManager"/> specific to MonoGame, uses <see cref="AssetManager"/> internally.
/// </summary>
public class MonoGameAssetManager : IAssetManager
{
    private readonly AssetManager _assetManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="MonoGameAssetManager"/> class.
    /// </summary>
    /// <param name="loggerFactory">
    /// The optional <see cref="LoggerFactory"/> to create a logger from to record informational messages.
    /// </param>
    public MonoGameAssetManager(ILoggerFactory? loggerFactory)
    {
        _assetManager = new(loggerFactory);
        _assetManager.AddLoader(new MonoGameAudioLoader(loggerFactory));
    }

    IReadOnlyCollection<IAssetLoader> IAssetManager.Loaders => _assetManager.Loaders;

    IReadOnlyCollection<IAssetSource> IAssetManager.Sources => _assetManager.Sources;

    IAssetManager IAssetManager.AddLoader<TAsset>(IAssetLoader<TAsset> assetLoader) =>
        _assetManager.AddLoader(assetLoader);

    IAssetManager IAssetManager.AddSource(IAssetSource assetSource) => _assetManager.AddSource(assetSource);

    /// <inheritdoc />
    public bool TryGet<TAsset>(
        string assetName,
        [NotNullWhen(true)] out TAsset? asset
    ) where TAsset : class, IAsset => _assetManager.TryGet<TAsset>(assetName, out asset);

    /// <inheritdoc />
    public bool TryLoad<TAsset>(string assetName) where TAsset : class, IAsset =>
        _assetManager.TryLoad<TAsset>(assetName);

    bool IAssetManager.TryRemoveLoader<TAsset>(IAssetLoader<TAsset> assetLoader) =>
        _assetManager.TryRemoveLoader(assetLoader);

    bool IAssetManager.TryRemoveSource(IAssetSource assetSource) => _assetManager.TryRemoveSource(assetSource);
}
