namespace Intersect.GameLogic.Entities;

public readonly partial struct DashLogicOptions
{
    public readonly float Range;

    public readonly bool IgnoreBlocks;

    public readonly bool IgnoreActiveResources;

    public readonly bool IgnoreDeadResources;

    public readonly bool IgnoreZDimension;
}