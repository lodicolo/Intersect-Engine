namespace Intersect.GameLogic.Entities;

public sealed partial class DataProviders
{
    public IMapProvider MapProvider { get; set; } = new LookupMapProvider();

    public required IMapEntitiesProvider MapEntitiesProvider { get; set; }
}