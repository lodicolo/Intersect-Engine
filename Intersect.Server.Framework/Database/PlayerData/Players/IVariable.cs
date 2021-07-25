using Intersect.GameObjects.Switches_and_Variables;
using Intersect.Server.Framework.Entities;
using System;

namespace Intersect.Server.Framework.Database.PlayerData.Players
{
    public interface IVariable : IPlayerOwned
    {
        Guid Id { get; }
        string Json { get; }
        VariableValue Value { get; set; }
        dynamic ValueData { get; }
        Guid VariableId { get; }
        string VariableName { get; }
    }
}