namespace Intersect.Framework.Runtime.Assets;

public interface IFontAsset : IAsset
{
    string Family { get; }

    ReadOnlySpan<int> SupportedSizes { get; }
}
