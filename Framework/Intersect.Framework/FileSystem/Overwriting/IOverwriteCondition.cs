namespace Intersect.Framework.FileSystem.Overwriting;

public interface IOverwriteCondition
{
    string? Id => default;
    
    bool IsMetFor(string targetPath);
}