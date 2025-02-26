using Intersect.Framework.Core.Entities;

namespace Intersect.GameLogic.Entities;

public interface IMapEntitiesProvider
{
    IEntity[] GetEntitiesOnTile(Guid mapId, int tileX, int tileY, int tileZ, IEntityFilter? entityFilter = null);
}