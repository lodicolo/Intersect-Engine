using Intersect.Framework.FileSystem.Overwriting;

namespace Intersect.Framework.FileSystem;

public class IfIsNewer : IOverwriteCondition
{
    private FileInfo? _sourceInfo;

    public IfIsNewer(string sourcePath)
    {
        SourcePath = sourcePath;
    }

    protected FileInfo SourceInfo => _sourceInfo ??= new(SourcePath);
    
    protected string SourcePath { get; }

    public bool IsMetFor(string targetPath)
    {
        var sourceInfo = SourceInfo;
        var targetInfo = new FileInfo(targetPath);
        var conditionMet = sourceInfo.LastWriteTimeUtc > targetInfo.LastWriteTimeUtc;
        return conditionMet;
    }
}