namespace Intersect.Framework.Runtime.Assets;

public interface IAudio : IAsset
{
    string Title { get; }

    TimeSpan Duration { get; }
}
