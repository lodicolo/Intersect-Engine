using Intersect.Localization;
using Newtonsoft.Json;

namespace Intersect.Framework.Commands.Parsing;

public sealed class CommandParserFormattingNamespace : LocaleNamespace
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public readonly LocalizedString Optional = @"[{00}]";

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public readonly LocalizedString Type = @":{00}";

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public readonly LocalizedString Usage = @"Usage: {00}";
}
