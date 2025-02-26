using Intersect.Config;
using Intersect.Enums;
using Intersect.Framework;
using Intersect.Framework.Core.Entities;
using Intersect.GameObjects.Maps;

namespace Intersect.GameLogic.Entities;

public class StockDashLogicOptionsFilter : IEntityFilter, ITileAttributeFilter
{
    private readonly DashLogicOptions _dashLogicOptions;
    private readonly PassabilityOptions _passabilityOptions;

    public StockDashLogicOptionsFilter(DashLogicOptions dashLogicOptions, PassabilityOptions passabilityOptions)
    {
        _dashLogicOptions = dashLogicOptions;
        _passabilityOptions = passabilityOptions;
    }

    public bool Includes(MapBase mapDescriptor, IEntity entity)
    {
        switch (entity.Type)
        {
            case EntityType.GlobalEntity:
                return true;
            case EntityType.Player:
                return !_passabilityOptions.IsPassable(mapDescriptor.ZoneType);
            case EntityType.Resource:
                if (entity is not IResourceEntity resourceEntity)
                {
                    return true;
                }

                return !(resourceEntity.IsDepleted
                    ? _dashLogicOptions.IgnoreDeadResources
                    : _dashLogicOptions.IgnoreActiveResources);

            case EntityType.Projectile:
                return true;
            case EntityType.Event:
                return true;
            case EntityType.NPC:
                return true;
            case EntityType.None:
                return false;
            default:
                throw Exceptions.UnreachableInvalidEnum(entity.Type);
        }
    }

    public bool Includes(IEntity inspectingEntity, MapAttribute tileAttribute)
    {
        switch (tileAttribute.Type)
        {
            case MapAttributeType.Blocked:
                return !_dashLogicOptions.IgnoreBlocks;

            case MapAttributeType.NpcAvoid:
                return inspectingEntity.Type == EntityType.NPC && !_dashLogicOptions.IgnoreBlocks;

            case MapAttributeType.ZDimension:
                return !_dashLogicOptions.IgnoreZDimension;
        }

        return true;
    }
}