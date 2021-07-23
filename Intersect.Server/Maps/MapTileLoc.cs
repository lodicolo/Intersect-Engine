using Intersect.Server.Framework.Maps;
using System;

namespace Intersect.Server.Maps
{
    public struct MapTileLoc : IMapTileLoc
    {
        public Guid MapId { get; }
        public int X { get; }
        public int Y { get; }

    public MapTileLoc(Guid id, int x, int y)
        {
            MapId = id;
            X = x;
            Y = y;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return MapId.GetHashCode() + (Y * Options.MapHeight) + X;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is MapTileLoc loc)
            {
                if (X == loc.X && Y == loc.Y && MapId == loc.MapId)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
