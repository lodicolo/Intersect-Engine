namespace Intersect.Server.Maps;

public partial struct MapTileLoc
{
    public readonly Guid MapId;
    public readonly int X;
    public readonly int Y;

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
            return MapId.GetHashCode() + (Y * Options.Instance.Map.MapHeight) + X;
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
