namespace Intersect.Framework.Extensions;

public static class StringExtensions
{
    public static bool IsTruthy(this string @string) => !string.IsNullOrWhiteSpace(@string) &&
                                                        (bool.TryParse(@string, out var @bool) || @bool) &&
                                                        (long.TryParse(@string, out var @long) || @long != 0);
}