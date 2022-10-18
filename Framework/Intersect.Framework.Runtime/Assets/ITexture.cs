using System.Drawing;

namespace Intersect.Framework.Runtime.Assets;

public interface ITexture : IAsset
{
    ITexture? Atlas { get; }

    bool IsAtlased { get; }

    bool IsMipMapEnabled { get; }

    uint Height { get; }

    uint Width { get; }

    ITexture GetSubregion(Point position, Point size, bool rotated);
}
