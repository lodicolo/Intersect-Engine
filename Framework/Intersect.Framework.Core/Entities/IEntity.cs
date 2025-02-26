using System.Numerics;
using Intersect.Enums;
using Intersect.GameObjects.Maps;

namespace Intersect.Framework.Core.Entities;

public interface IEntity : IDisposable
{
    Guid Id { get; }

    EntityType Type { get; }

    string Name { get; set; }

    Vector3 Position { get; set; }

    int TileX { get; }

    int TileY { get; }

    int TileZ { get; }

    bool IsBlockedBy(MapAttribute mapAttribute);

    bool IsBlockedBy(IEntity entity);
}