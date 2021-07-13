using Intersect.Server.Framework.Database;
using System;

namespace Intersect.Server.Framework.Maps
{
    public interface IMapItem
    {
        int TileIndex { get; }
        Guid UniqueId { get; }
        int X { get; }
        int Y { get; }

        string Data();
        void SetupStatBuffs(IItem item);
    }
}