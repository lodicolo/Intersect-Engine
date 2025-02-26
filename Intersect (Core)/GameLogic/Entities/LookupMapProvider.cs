using Intersect.GameObjects;
using Intersect.GameObjects.Maps;

namespace Intersect.GameLogic.Entities;

public class LookupMapProvider : IMapProvider
{
    public bool TryGet(Guid mapId, out MapBase mapDescriptor)
    {
        return MapBase.TryGet(mapId, out mapDescriptor);
    }
}