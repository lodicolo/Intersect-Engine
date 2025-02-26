using Intersect.Framework.Core.Entities;
using Intersect.GameObjects.Maps;

namespace Intersect.GameLogic.Entities;

public interface IEntityFilter
{
    bool Includes(MapBase mapDescriptor, IEntity entity);
}