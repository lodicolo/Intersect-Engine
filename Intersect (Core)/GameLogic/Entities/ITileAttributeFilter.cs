using Intersect.Framework.Core.Entities;
using Intersect.GameObjects.Maps;

namespace Intersect.GameLogic.Entities;

public interface ITileAttributeFilter
{
    bool Includes(IEntity inspectingEntity, MapAttribute tileAttribute);
}