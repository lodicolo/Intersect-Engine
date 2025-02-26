using System.Diagnostics.CodeAnalysis;
using Intersect.GameObjects.Maps;

namespace Intersect.GameLogic.Entities;

public interface IMapProvider
{
    bool TryGet(Guid mapId, [NotNullWhen(true)] out MapBase? mapDescriptor);
}