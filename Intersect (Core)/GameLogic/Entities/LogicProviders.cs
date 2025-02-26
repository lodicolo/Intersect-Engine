namespace Intersect.GameLogic.Entities;

public sealed partial class LogicProviders
{
    public required IEntityMapLogicProvider EntityMapLogicProvider { get; set; }

    public required IDashLogicProvider DashLogicProvider { get; set; }
}