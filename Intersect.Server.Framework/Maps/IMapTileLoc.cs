using System;

namespace Intersect.Server.Framework.Maps
{
    public interface IMapTileLoc
    {
        Guid MapId { get; }
        int X { get; }
        int Y { get; }

        bool Equals(object obj);
        int GetHashCode();
    }
}