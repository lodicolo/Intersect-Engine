using System.Diagnostics.CodeAnalysis;
using Intersect.Framework.Runtime.Assets;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework.Audio;
using NVorbis;

namespace Intersect.Runtime.MonoGame.Assets.Loaders;

/// <summary>
/// A concrete implementation of <see cref="IAssetLoader{TAsset}"/> to load audio assets for MonoGame.
/// </summary>
internal sealed class MonoGameAudioLoader : AssetLoader<IAudio, MonoGameAudioLoader>
{
    private readonly ILoggerFactory? _loggerFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="MonoGameAudioLoader"/> class.
    /// </summary>
    /// <param name="loggerFactory">
    /// The optional <see cref="LoggerFactory"/> to create a logger from to record informational messages.
    /// </param>
    public MonoGameAudioLoader(ILoggerFactory? loggerFactory) : base(loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    /// <inheritdoc />
    public override bool TryLoad(IAssetSource assetSource, string assetName, [NotNullWhen(true)] out IAudio? asset)
    {
        asset = default;

        if (!assetSource.TryOpenRead(assetName, out var readStream))
        {
            Logger?.LogDebug("Failed to open read stream for asset \"{AssetName}\".", assetName);
            return false;
        }

        try
        {
            var extension = Path.GetExtension(assetName).ToLowerInvariant();
            var name = Path.GetFileNameWithoutExtension(assetName);
            switch (extension)
            {
                case ".wav":
                    asset = new MonoGameAudio(name, _loggerFactory, SoundEffect.FromStream(readStream));
                    return true;

                default:
                    asset = new MonoGameAudio(name, _loggerFactory, new VorbisReader(readStream, true));
                    return true;
            }
        }
        catch (Exception exception)
        {
            Logger?.LogDebug(exception, "Failed to load audio asset \"{AssetName}\".", assetName);
            return false;
        }
    }
}
