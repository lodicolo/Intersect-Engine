using Intersect.Framework.Runtime.Assets;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework.Audio;
using NVorbis;

namespace Intersect.Runtime.MonoGame.Assets;

internal class MonoGameAudio : Asset, IAudio
{
    private readonly ILogger<MonoGameAudio>? _logger;
    private readonly SoundEffect? _soundEffect;
    private readonly VorbisReader? _vorbisReader;

    private MonoGameAudio(string name, ILoggerFactory? loggerFactory) : base(name) =>
        _logger = loggerFactory?.CreateLogger<MonoGameAudio>();

    internal MonoGameAudio(string name, ILoggerFactory? loggerFactory, SoundEffect soundEffect) : this(
        name,
        loggerFactory
    ) => _soundEffect = soundEffect;

    internal MonoGameAudio(string name, ILoggerFactory? loggerFactory, VorbisReader vorbisReader) : this(
        name,
        loggerFactory
    ) => _vorbisReader = vorbisReader;

    public override bool IsReady => _soundEffect != default || _vorbisReader != default;

    public string Title => _soundEffect?.Name ?? Name;

    public TimeSpan Duration =>
        _soundEffect?.Duration ?? _vorbisReader?.TotalTime ??
        throw new InvalidOperationException(string.Format(AssetsResources.MonoGameAudio_AudioTrackNotYetLoaded, Name));
}
