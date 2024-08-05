namespace Intersect.Server.CustomChange.Utils;

public class SimpleIdGenerator
{
    private static readonly Random Random = new();
    private const string CHARACTERES= "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    public static string NewId => new(Enumerable.Repeat(CHARACTERES, 8)
                       .Select(s => s[Random.Next(s.Length)]).ToArray());
}
