using Intersect.Localization;
using Newtonsoft.Json;

namespace Intersect.Framework.Commands.Parsing;

public sealed class CommandParsingNamespace : LocaleNamespace
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public readonly CommandParserErrorsNamespace Errors = new();

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public readonly CommandParserFormattingNamespace Formatting = new();

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public readonly LocaleCommand HelpCommand = new()
    {
        Name = @"help",
        Description = @"help",
        Help = @"displays list of available commands"
    };

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public readonly LocaleDictionary<string, LocalizedString> TypeNames = new(
        new Dictionary<string, LocalizedString>
        {
            { nameof(Boolean), "bool" },
            { nameof(Byte), "byte" },
            { nameof(SByte), "sbyte" },
            { nameof(Int16), "short" },
            { nameof(UInt16), "ushort" },
            { nameof(Int32), "int" },
            { nameof(UInt32), "uint" },
            { nameof(Int64), "long" },
            { nameof(UInt64), "ulong" },
            { nameof(Single), "float" },
            { nameof(Double), "double" },
            { nameof(Decimal), "decimal" },
            { nameof(Object), "object" },
            { nameof(Char), "char" },
            { nameof(String), "string" }
        });

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public readonly LocalizedString TypeUnknown = @"unknown";

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public readonly LocalizedString ValueFalse = @"false";

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public readonly LocalizedString ValueTrue = @"true";
}
