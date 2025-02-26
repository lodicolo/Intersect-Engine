using Intersect.Framework.Core.Entities;

namespace Intersect.Server.Framework.Entities;

public interface IServerEntity : IEntity
{
    Guid MapInstanceId { get; }
}
