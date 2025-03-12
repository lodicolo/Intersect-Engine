using Intersect.Core;
using Intersect.Framework.IO;

namespace Intersect.Editor.Core;

public sealed class DummyStartupOptions : ICommandLineOptions
{
    public string WorkingDirectory => Environment.CurrentDirectory;
    public IEnumerable<string> PluginDirectories => [];
}