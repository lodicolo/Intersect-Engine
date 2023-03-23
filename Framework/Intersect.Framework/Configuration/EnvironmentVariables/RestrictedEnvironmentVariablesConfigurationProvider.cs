using Microsoft.Extensions.Configuration.EnvironmentVariables;

namespace Intersect.Framework.Configuration.EnvironmentVariables;

public class RestrictedEnvironmentVariablesConfigurationProvider : EnvironmentVariablesConfigurationProvider
{
    /// <summary>
    ///     Initializes a new instance with the specified prefix.
    /// </summary>
    /// <param name="prefix">A prefix used to filter the environment variables.</param>
    /// <param name="ignoredPrefixes">A list of "sub"-prefixes to filter out.</param>
    public RestrictedEnvironmentVariablesConfigurationProvider(string? prefix, string[]? ignoredPrefixes) : base(prefix)
    {
        _ignoredPrefixes = ignoredPrefixes ?? Array.Empty<string>();
    }

    private string[] _ignoredPrefixes { get; }

    public override void Load()
    {
        base.Load();

        if (_ignoredPrefixes.Length < 1)
        {
            return;
        }

        foreach (var (key, _) in Data)
        {
            if (_ignoredPrefixes.Any(key.StartsWith))
            {
                Data.Remove(key);
            }
        }
    }
}