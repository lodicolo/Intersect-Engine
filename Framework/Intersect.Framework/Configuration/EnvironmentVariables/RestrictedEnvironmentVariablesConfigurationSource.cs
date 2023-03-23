using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;

namespace Intersect.Framework.Configuration.EnvironmentVariables;

public class RestrictedEnvironmentVariablesConfigurationSource : EnvironmentVariablesConfigurationSource
{
    public string[]? IgnoredPrefixes { get; init; }

    public IConfigurationProvider Build(IConfigurationBuilder builder) =>
        new RestrictedEnvironmentVariablesConfigurationProvider(Prefix, IgnoredPrefixes);
}