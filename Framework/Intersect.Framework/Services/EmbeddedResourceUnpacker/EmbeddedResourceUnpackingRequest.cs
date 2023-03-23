using System.Reflection;
using Intersect.Framework.FileSystem;
using Intersect.Framework.FileSystem.Overwriting;

namespace Intersect.Framework.Services.EmbeddedResourceUnpacker;

public sealed record EmbeddedResourceUnpackingRequest(
    Assembly Assembly,
    string ResourceName,
    OverwriteBehavior OverwriteBehavior = OverwriteBehavior.DoNotOvewrite,
    params IOverwriteCondition[] OverwriteConditions
)
{
    public Assembly Assembly { get; } = Assembly;

    public OverwriteBehavior OverwriteBehavior { get; } = OverwriteBehavior;

    public IOverwriteCondition[] OverwriteConditions { get; } = OverwriteConditions;

    public string ResourceName { get; } = ResourceName;
}