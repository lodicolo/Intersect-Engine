using Intersect.Framework.Runtime.Assets;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework.Graphics;

namespace Intersect.Runtime.MonoGame.Assets;

public sealed class MonoGameTexture : Asset, ITexture
{
    private readonly ILogger<MonoGameAudio>? _logger;

    private uint? _height;
    private bool? _mipMapEnabled;
    private uint? _width;
    private Texture2D? _texture;

    public MonoGameTexture(
        string name,
        uint? width = default,
        uint? height = default,
        bool? mipMapEnabled = default
    ) : base(name)
    {
        _height = height;
        _mipMapEnabled = mipMapEnabled;
        _width = width;
    }

    internal MonoGameTexture(
        string name,
        ITexture atlas,
        Point size
    ) : base(name)
    {
        Atlas = atlas;

        _height = (uint)size.Y;
        _width = (uint)size.X;
    }

    /// <inheritdoc />
    public override bool IsReady => Atlas?.IsReady ?? _texture != default;

    /// <inheritdoc />
    public ITexture? Atlas { get; }

    /// <inheritdoc />
    public bool IsAtlased => Atlas != default;

    /// <inheritdoc />
    public bool IsMipMapEnabled => _mipMapEnabled ?? _texture?.LevelCount > 1;

    /// <inheritdoc />
    public uint Height => _height ?? (uint?)(_texture?.Height) ??
        throw new InvalidOperationException(string.Format(AssetsResources.MonoGameTexture_TextureNotYetLoadedX, Name));

    /// <inheritdoc />
    public uint Width => _width ?? (uint?)(_texture?.Width) ??
        throw new InvalidOperationException(string.Format(AssetsResources.MonoGameTexture_TextureNotYetLoadedX, Name));

    /// <inheritdoc />
    public ITexture GetSubregion(System.Drawing.Point position, System.Drawing.Point size, bool rotated)
    {
        throw new NotImplementedException();
    }
}
