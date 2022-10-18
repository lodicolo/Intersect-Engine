using System.Diagnostics.CodeAnalysis;

namespace Intersect.Framework.Runtime.Assets;

/// <summary>
/// Declaration of the interface for managing <see cref="IAsset"/>s.
/// </summary>
public interface IAssetManager
{
    /// <summary>
    /// The loaders known to this <see cref="IAssetManager"/>.
    /// </summary>
    IReadOnlyCollection<IAssetLoader> Loaders { get; }

    /// <summary>
    /// The sources known to this <see cref="IAssetManager"/>.
    /// </summary>
    IReadOnlyCollection<IAssetSource> Sources { get; }

    /// <summary>
    /// Add an asset loader for a given asset type to this <see cref="IAssetManager"/>.
    /// </summary>
    /// <param name="assetLoader">The loader to add.</param>
    /// <typeparam name="TAsset">The type of asset this loader is for.</typeparam>
    /// <returns>The current <see cref="IAssetManager"/> for chaining.</returns>
    IAssetManager AddLoader<TAsset>(IAssetLoader<TAsset> assetLoader) where TAsset : class, IAsset;

    /// <summary>
    /// Add an asset source to this <see cref="IAssetManager"/>.
    /// </summary>
    /// <param name="assetSource">The source to add.</param>
    /// <returns>The current <see cref="IAssetManager"/> for chaining.</returns>
    IAssetManager AddSource(IAssetSource assetSource);

    /// <summary>
    /// Attempts to get the asset with the specified name when it is needed.
    /// </summary>
    /// <param name="assetName">The name of the asset to get.</param>
    /// <param name="asset">The asset.</param>
    /// <typeparam name="TAsset">The type of asset to get.</typeparam>
    /// <returns>If the asset was returned.</returns>
    /// <remarks>
    /// This is asynchronous and the asset may not be ready; check <see cref="IAsset.IsReady"/>
    /// before trying to use it. Built-in systems should already automatically check for this.
    /// </remarks>
    bool TryGet<TAsset>(string assetName, [NotNullWhen(true)] out TAsset? asset) where TAsset : class, IAsset;

    /// <summary>
    /// Attempts to load the asset with the specified name before it is needed.
    /// </summary>
    /// <param name="assetName">The name of the asset to load.</param>
    /// <typeparam name="TAsset">The type of asset to load for the asset name.</typeparam>
    /// <returns>If the asset is being loaded.</returns>
    /// <remarks>
    /// Loading is asynchronous; returning true only means the asset has been found and loading has begun.
    /// </remarks>
    bool TryLoad<TAsset>(string assetName) where TAsset : class, IAsset;

    /// <summary>
    /// Attempt to remove the given loader from this <see cref="AssetManager"/>.
    /// </summary>
    /// <param name="assetLoader">The loader to remove.</param>
    /// <returns>If the given loader was successfully removed.</returns>
    public bool TryRemoveLoader<TAsset>(IAssetLoader<TAsset> assetLoader) where TAsset : class, IAsset;

    /// <summary>
    /// Attempt to remove the given source from this <see cref="AssetManager"/>.
    /// </summary>
    /// <param name="assetSource">The source to remove.</param>
    /// <returns>If the given source was successfully removed.</returns>
    bool TryRemoveSource(IAssetSource assetSource);
}
