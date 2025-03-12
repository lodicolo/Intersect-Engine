using System.Security;

namespace Intersect.VersionManager;

public sealed record RepositoryOptions
{
    public string Owner { get; init; } = "AscensionGameDev";

    public string Repository { get; init; } = "Intersect-Engine";
}

public sealed record GitHubOptions
{
    public string? AccessToken { get; init; }

    public RepositoryOptions Repository { get; init; } = new();
}