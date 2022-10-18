using System.Diagnostics.CodeAnalysis;

namespace Intersect.Framework.Runtime.Assets;

public interface IAssetSource
{
    IEnumerable<string> AvailableAssets { get; }

    bool TryOpenRead(string qualifiedAssetName, [NotNullWhen(true)] out Stream? assetReadStream);

    bool TryOpenWrite(string qualifiedAssetName, [NotNullWhen(true)] out Stream? assetWriteStream);

    bool TryResolve(string assetName, [NotNullWhen(true)] out string? qualifiedAssetName);
}
