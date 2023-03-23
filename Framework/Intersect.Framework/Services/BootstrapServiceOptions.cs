namespace Intersect.Framework.Services;

public record BootstrapServiceOptions(IReadOnlyCollection<IBootstrapTask> Tasks)
{
    public IReadOnlyCollection<IBootstrapTask> Tasks { get; } = Tasks;
}