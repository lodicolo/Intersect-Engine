using Intersect.GameObjects;

namespace Intersect.Framework.Core.Entities;

public interface IResourceEntity : IEntity
{
    ResourceBase? Descriptor { get; }

    bool IsDepleted { get; }
}