using Microsoft.Extensions.Configuration;

namespace Intersect.Framework.Configuration.EnvironmentVariables;

public static class RestrictedEnvironmentVariablesExtensions
{
    public static IConfigurationBuilder AddEnvironmentVariables(
        this IConfigurationBuilder configurationBuilder,
        string? prefix,
        params string[] ignoredPrefixes
    )
    {
        return configurationBuilder.Add(
            new RestrictedEnvironmentVariablesConfigurationSource
            {
                Prefix = prefix,
                IgnoredPrefixes = ignoredPrefixes
            }
        );
    }
}