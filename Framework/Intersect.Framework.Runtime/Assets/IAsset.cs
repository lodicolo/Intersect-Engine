namespace Intersect.Framework.Runtime.Assets;

public interface IAsset
{
    string Name { get; }

    bool IsReady { get; }
}

public abstract class Asset : IAsset
{
    protected Asset(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public string Name { get; }

    public abstract bool IsReady { get; }
}
