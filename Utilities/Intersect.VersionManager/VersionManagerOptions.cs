using CommandLine;
using Intersect.Framework.IO;

namespace Intersect.VersionManager;

public partial class VersionManagerOptions : ICommandLineOptions
{
    [Option("working-directory", Default = null, Required = false)]
    public string? WorkingDirectory { get; init; }

    IEnumerable<string>? ICommandLineOptions.PluginDirectories { get; } = [];
}