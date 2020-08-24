namespace Intersect.Utilities.Text.FormatRules
{
    public interface ITextFormatRule
    {
        bool IsValid { get; }

        bool Matches(string text);
    }
}
