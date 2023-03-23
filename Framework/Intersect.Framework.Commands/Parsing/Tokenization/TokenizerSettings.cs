using System.Collections.Immutable;

namespace Intersect.Framework.Commands.Parsing.Tokenization;

public sealed class TokenizerSettings
{
    public TokenizerSettings(
        bool allowQuotedStrings = true,
        ImmutableArray<char>? quotationMarks = null,
        char delimeter = ' '
    )
    {
        AllowQuotedStrings = allowQuotedStrings;
        QuotationMarks = quotationMarks ?? "\"".ToImmutableArray();
        Delimeter = delimeter;
    }

    public static TokenizerSettings Default => new();

    public bool AllowQuotedStrings { get; }

    public ImmutableArray<char> QuotationMarks { get; }

    public char Delimeter { get; }
}
