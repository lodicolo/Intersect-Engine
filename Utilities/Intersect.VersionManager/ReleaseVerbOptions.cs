using CommandLine;

namespace Intersect.VersionManager;

public abstract partial class ReleaseVerbOptions : VersionManagerOptions
{
    public const string TagLatest = "latest";
    public const string PlatformAll = "all";

    private readonly string _tag = TagLatest;

    [Option(longName: "platforms", shortName: 'p')]
    public IReadOnlyList<string> Platforms { get; init; } = [PlatformAll];

    [Value(0, Required = true)]
    public string Tag
    {
        get => _tag;
        init => _tag = string.IsNullOrWhiteSpace(value) ? TagLatest : value.Trim().ToLowerInvariant();
    }
}