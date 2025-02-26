using System.Numerics;
using Intersect.Framework.Core.Entities;

namespace Intersect.GameLogic.Entities;

public class StockDashLogicProvider : IDashLogicProvider
{
    private readonly IEntityMapLogicProvider _entityMapLogicProvider;

    public StockDashLogicProvider(IEntityMapLogicProvider entityMapLogicProvider)
    {
        _entityMapLogicProvider = entityMapLogicProvider;
    }

    public Vector3 CalculateRange(IEntity entity, Vector3 directionNormal, DashLogicOptions options)
    {
        var direction = directionNormal * options.Range;

        var startPosition = entity.Position;
        var startTileX = (int)Math.Floor(startPosition.X);
        var startTileY = (int)Math.Floor(startPosition.Y);
        var startTileZ = (int)Math.Floor(startPosition.Z);

        var endPosition = startPosition + direction;
        var endTileX = (int)Math.Floor(endPosition.X);
        var endTileY = (int)Math.Floor(endPosition.Y);
        var endTileZ = (int)Math.Floor(endPosition.Z);

        if (startTileX == endTileX && startTileY == endTileY && startTileZ == endTileZ)
        {
            return direction;
        }

        var directionX = Math.Sign(direction.X);
        var directionY = Math.Sign(direction.Y);
        var directionZ = Math.Sign(direction.Z);

        for (var x = startTileX; x != endTileX; x += directionX)
        {
            if (x == startTileX)
            {
                continue;
            }

            for (var y = startTileY; y != endTileY; y += directionY)
            {
                for (var z = startTileZ; z != endTileZ; z += directionZ)
                {

                }
            }
        }

        return default;


        // for (var i = 1; i <= range; i++)
        // {
        //     if (!en.CanMoveInDirection(Direction, out var blockerType, out var blockingEntityType))
        //     {
        //         switch (blockerType)
        //         {
        //             case MovementBlockerType.OutOfBounds:
        //                 return;
        //
        //             case MovementBlockerType.MapAttribute:
        //                 if (!blockPass)
        //                 {
        //                     return;
        //                 }
        //
        //                 break;
        //
        //             case MovementBlockerType.ZDimension:
        //                 if (!zDimensionPass)
        //                 {
        //                     return;
        //                 }
        //
        //                 break;
        //
        //             case MovementBlockerType.Entity:
        //                 switch (blockingEntityType)
        //                 {
        //                     case EntityType.Resource:
        //                         if (activeResourcePass || deadResourcePass)
        //                         {
        //                             break;
        //                         }
        //
        //                         return;
        //
        //                     case EntityType.Event:
        //                     case EntityType.Player:
        //                         return;
        //
        //                     case EntityType.GlobalEntity:
        //                     case EntityType.Projectile:
        //                         break;
        //
        //                     default:
        //                         throw new NotImplementedException($"{blockingEntityType} not implemented.");
        //                 }
        //
        //                 break;
        //
        //             case MovementBlockerType.NotBlocked:
        //             case MovementBlockerType.Slide:
        //                 break;
        //
        //             default:
        //                 throw new NotImplementedException($"{blockerType} not implemented.");
        //         }
        //     }
        //
        //     en.Move(Direction, null, true);
        //     en.Dir = Facing;
        //
        //     Range = i;
        // }
    }
}