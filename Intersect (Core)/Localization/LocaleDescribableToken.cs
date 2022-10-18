using Newtonsoft.Json;

namespace Intersect.Localization;

[Serializable]
public class LocaleDescribableToken : LocaleToken
{
    [JsonProperty(
        nameof(Description),
        NullValueHandling = NullValueHandling.Ignore
    )]
    private LocalizedString? _description;

    public LocaleDescribableToken() { }

    public LocaleDescribableToken(
        string name,
        string? description = null
    ) : base(
        name
    )
    {
        _description = description?.Trim();
    }

    [JsonIgnore]
    public virtual LocalizedString Description
    {
        get => _description ?? string.Empty;
        set => _description ??= value;
    }
}
