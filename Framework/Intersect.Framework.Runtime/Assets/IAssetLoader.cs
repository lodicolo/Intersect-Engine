using System.Diagnostics.CodeAnalysis;

namespace Intersect.Framework.Runtime.Assets;

public interface IAssetLoader
{
    Type Type { get; }

    bool TryLoad(IAssetSource assetSource, string assetName, [NotNullWhen(true)] out IAsset? asset);
}

public interface IAssetLoader<TAsset> : IAssetLoader where TAsset : class, IAsset
{
    Type IAssetLoader.Type => typeof(TAsset);

    bool IAssetLoader.TryLoad(IAssetSource assetSource, string assetName, [NotNullWhen(true)] out IAsset? asset)
    {
        if (TryLoad(assetSource, assetName, out TAsset? typedAsset))
        {
            asset = typedAsset;
            return true;
        }

        asset = default;
        return false;
    }

    bool TryLoad(IAssetSource assetSource, string assetName, [NotNullWhen(true)] out TAsset? asset);
}
