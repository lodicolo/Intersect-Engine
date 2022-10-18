using Newtonsoft.Json;

namespace Intersect.Localization;

[Serializable]
public class LocaleCommand : LocaleDescribableToken
{
    [JsonProperty(
        nameof(Help),
        NullValueHandling = NullValueHandling.Ignore
    )]
    private LocalizedString? _help;

    public LocaleCommand() { }

    public LocaleCommand(
        string name,
        string? description = default,
        string? help = default
    ) : base(
        name,
        description
    )
    {
        _help = help?.Trim();
    }

    [JsonIgnore]
    public LocalizedString Help
    {
        get => _help ?? string.Empty;
        set => _help = value;
    }
}
