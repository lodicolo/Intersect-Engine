using Intersect.Framework.Commands.Parsing.Tokenization;

namespace Intersect.Framework.Commands.Parsing;

public sealed class ParserSettings
{
    public static readonly CommandParsingNamespace DefaultLocalization = new();

    public static readonly string DefaultPrefixLong = @"--";

    public static readonly string DefaultPrefixShort = @"-";

    public ParserSettings(
        string? prefixShort = default,
        string? prefixLong = default,
        CommandParsingNamespace? localization = default,
        TokenizerSettings? tokenizerSettings = default
    )
    {
        PrefixShort = ValidatePrefix(prefixShort ?? DefaultPrefixShort);
        PrefixLong = ValidatePrefix(prefixLong ?? DefaultPrefixLong);
        Localization = localization ?? DefaultLocalization;
        TokenizerSettings = tokenizerSettings ?? TokenizerSettings.Default;
    }

    public static ParserSettings Default => new(DefaultPrefixShort, DefaultPrefixLong, DefaultLocalization);

    public string PrefixShort { get; }

    public string PrefixLong { get; }

    public CommandParsingNamespace Localization { get; }

    public TokenizerSettings TokenizerSettings { get; }

    public static string ValidatePrefix(string prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
        {
            throw new ArgumentException(@"Prefix cannot be null, empty, or whitespace.");
        }

        if (prefix.Contains('='))
        {
            throw new ArgumentException(@"Prefixes cannot contain '='.");
        }

        if (prefix.Contains(' ') || prefix.Contains('\n') || prefix.Contains('\r') || prefix.Contains('\t'))
        {
            throw new ArgumentException(@"Prefixes cannot contain whitespace.");
        }

        if (prefix.Contains('\0'))
        {
            throw new ArgumentException(@"Prefixes cannot contain the null character.");
        }

        return prefix;
    }
}
