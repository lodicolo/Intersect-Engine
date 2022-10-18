using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace Intersect.Framework.Runtime.Assets;

/// <summary>
/// An abstract implementation of <see cref="IAssetLoader{TAsset}"/> that includes an internal logger.
/// </summary>
/// <typeparam name="TAsset"></typeparam>
/// <typeparam name="TLoggerContext"></typeparam>
public abstract class AssetLoader<TAsset, TLoggerContext> : IAssetLoader<TAsset> where TAsset : class, IAsset
{
    /// <summary>
    /// The logger instance to be used by this loader.
    /// </summary>
    protected readonly ILogger<TLoggerContext>? Logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AssetLoader{TAsset, TLoggerContext}"/> class.
    /// </summary>
    /// <param name="loggerFactory">
    /// The optional <see cref="LoggerFactory"/> to create a logger from to record informational messages.
    /// </param>
    protected AssetLoader(ILoggerFactory? loggerFactory)
    {
        Logger = loggerFactory?.CreateLogger<TLoggerContext>();
    }

    /// <inheritdoc />
    public abstract bool TryLoad(IAssetSource assetSource, string assetName, [NotNullWhen(true)] out TAsset? asset);
}

/// <inheritdoc />
public abstract class AssetLoader<TAsset> : AssetLoader<TAsset, AssetLoader<TAsset>> where TAsset : class, IAsset
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AssetLoader{TAsset}"/> class.
    /// </summary>
    /// <param name="loggerFactory">
    /// The optional <see cref="LoggerFactory"/> to create a logger from to record informational messages.
    /// </param>
    protected AssetLoader(ILoggerFactory? loggerFactory) : base(loggerFactory) { }
}
