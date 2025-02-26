using System.Numerics;
using Intersect.Framework.Core.Entities;

namespace Intersect.GameLogic.Entities;

public interface IDashLogicProvider
{
    Vector3 CalculateRange(IEntity entity, Vector3 directionNormal, DashLogicOptions options);
}