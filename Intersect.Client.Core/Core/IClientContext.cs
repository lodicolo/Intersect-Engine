using Intersect.Core;
using Intersect.Features;

namespace Intersect.Client.Core;

/// <summary>
/// Declares the API surface of client contexts.
/// </summary>
internal interface IClientContext : IApplicationContext<ClientCommandLineOptions>
{
    /// <summary>
    /// The platform-specific runner that initializes the actual user-visible client.
    /// </summary>
    IPlatformRunner PlatformRunner { get; }

    /// <summary>
    /// The capabilities of the current client instance.
    /// </summary>
    HashSet<ClientCapabilities> Capabilities { get; }
}
