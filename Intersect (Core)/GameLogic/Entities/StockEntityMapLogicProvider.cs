using Intersect.Enums;
using Intersect.Framework.Core.Entities;
using Intersect.GameObjects.Maps;

namespace Intersect.GameLogic.Entities;

public class StockEntityMapLogicProvider : IEntityMapLogicProvider
{
    private readonly IMapProvider _mapProvider;
    private readonly IMapEntitiesProvider _mapEntitiesProvider;
    private readonly int _mapWidth;
    private readonly int _mapHeight;

    public StockEntityMapLogicProvider(IMapProvider mapProvider, IMapEntitiesProvider mapEntitiesProvider)
    {
        _mapProvider = mapProvider;
        _mapEntitiesProvider = mapEntitiesProvider;

        var mapOptions = Options.Instance.Map;
        _mapWidth = mapOptions.MapWidth;
        _mapHeight = mapOptions.MapHeight;
    }

    public bool IsBlocked(
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
    )
    {
        if (_mapProvider.TryGet(mapId, out var mapDescriptor))
        {
            return IsBlocked(
                entity: entity,
                mapDescriptor: mapDescriptor,
                x: x,
                y: y,
                z: z,
                entityFilter: entityFilter,
                tileAttributeFilter: tileAttributeFilter,
                blockerType: out blockerType,
                blockingEntityType: out blockingEntityType,
                blockingEntity: out blockingEntity
            );
        }

        blockerType = MovementBlockerType.MapDoesNotExist;
        blockingEntityType = EntityType.None;
        blockingEntity = null;
        return true;
    }

    public bool IsBlocked(
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
    )
    {
        if (x < 0 || y < 0 || x >= _mapWidth || y >= _mapHeight)
        {
            blockerType = MovementBlockerType.OutOfBounds;
            blockingEntityType = EntityType.None;
            blockingEntity = null;
            return true;
        }

        if (mapDescriptor.Attributes[x, y] is { } tileAttribute)
        {
            if (tileAttributeFilter?.Includes(inspectingEntity: entity, tileAttribute: tileAttribute) != false)
            {
                if (entity.IsBlockedBy(tileAttribute))
                {
                    blockerType = tileAttribute.Type switch
                    {
                        MapAttributeType.Slide => MovementBlockerType.Slide,
                        MapAttributeType.ZDimension => MovementBlockerType.ZDimension,
                        _ => MovementBlockerType.MapAttribute,
                    };
                    blockingEntityType = EntityType.None;
                    blockingEntity = null;
                    return true;
                }
            }
        }

        var entitiesOnTile = _mapEntitiesProvider.GetEntitiesOnTile(
            mapId: mapDescriptor.Id,
            tileX: x,
            tileY: y,
            tileZ: z,
            entityFilter: entityFilter
        );

        foreach (var entityOnTile in entitiesOnTile)
        {
            if (!entity.IsBlockedBy(entityOnTile))
            {
                continue;
            }

            blockerType = MovementBlockerType.Entity;
            blockingEntityType = entity.Type;
            blockingEntity = entityOnTile;
            return true;
        }

        blockerType = MovementBlockerType.NotBlocked;
        blockingEntityType = EntityType.None;
        blockingEntity = null;
        return false;
    }
}