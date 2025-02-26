using Intersect.Enums;
using Intersect.Framework.Core.Entities;
using Intersect.GameObjects.Maps;

namespace Intersect.GameLogic.Entities;

public interface IEntityMapLogicProvider
{
    bool IsBlocked(
        IEntity entity,
        Guid mapId,
        int x,
        int y,
        int z,
        IEntityFilter? entityFilter,
        ITileAttributeFilter? tileAttributeFilter,
        out MovementBlockerType blockerType,
        out EntityType blockingEntityType,
        out IEntity? blockingEntity
    );

    bool IsBlocked(
        IEntity entity,
        MapBase mapDescriptor,
        int x,
        int y,
        int z,
        IEntityFilter? entityFilter,
        ITileAttributeFilter? tileAttributeFilter,
        out MovementBlockerType blockerType,
        out EntityType blockingEntityType,
        out IEntity? blockingEntity
    );
}