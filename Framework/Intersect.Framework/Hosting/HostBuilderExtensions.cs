using System.Diagnostics;
using Microsoft.Extensions.Hosting;

namespace Intersect.Framework.Hosting;

public static class HostBuilderExtensions
{
    public static IHostBuilder Apply(
        this IHostBuilder hostBuilder,
        IEnumerable<Action<IHostBuilder>> hostBuilderConfigurationSteps
    )
    {
        foreach (var step in hostBuilderConfigurationSteps)
        {
            Debug.Assert(step != default, "step != default");
            step(hostBuilder);
        }

        return hostBuilder;
    }
}