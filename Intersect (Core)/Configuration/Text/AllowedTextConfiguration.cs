using System;

using Intersect.Utilities;
using Intersect.Utilities.Text.FormatRules;

using Newtonsoft.Json;

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Intersect.Configuration.Text
{
    public class AllowedTextConfiguration
    {
        public static AllowedTextConfiguration Default => new AllowedTextConfiguration
        {
            Pattern = "^[A-Za-z0-9]{2,24}$"
        };

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public LengthConstraint Length { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Pattern { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public RegexOptions PatternOptions { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IReadOnlyList<TextFormatRange> Ranges { get; set; }

        public TextFormatRuleMatcher AsMatcher()
        {
            var rules = new List<ITextFormatRule>();

            if (Length != default)
            {
                if (!Length.IsValid())
                {
                    throw new Exception(
                        $"{nameof(Length)} is invalid, {nameof(Length.Minimum)} > {nameof(Length.Maximum)}."
                    );
                }

                rules.Add(new TextFormatLengthRule(Length));
            }

            if (!string.IsNullOrEmpty(Pattern))
            {
                rules.Add(new TextFormatPatternRule(Pattern, PatternOptions));
            }

            if (Ranges != null && Ranges.Count > 0)
            {
                rules.Add(new TextFormatRangeRule(Ranges));
            }

            var matcher = new TextFormatRuleMatcher(rules);

            if (!matcher.IsValid)
            {
                throw new Exception("No valid rules, cannot create usable matcher.");
            }

            return matcher;
        }
    }
}
