using System.Collections.Immutable;
using System.Globalization;
using Newtonsoft.Json;

namespace Intersect.Framework.Core.Config;

public class LocalizationOptions
{
    public static readonly ImmutableArray<CultureInfo> DefaultSupportedCultures =
    [
        CultureInfo.GetCultureInfoByIetfLanguageTag("en-US"),
        CultureInfo.GetCultureInfoByIetfLanguageTag("es"),
        // CultureInfo.GetCultureInfoByIetfLanguageTag("it"),
        // CultureInfo.GetCultureInfoByIetfLanguageTag("pt-BR"),
    ];

    private HashSet<CultureInfo> _supportedCultures = DefaultSupportedCultures.ToHashSet();

    [JsonProperty(nameof(SupportedCultures))]
    private ImmutableSortedSet<string> SupportedCulturesIetfTags
    {
        get => _supportedCultures.Select(c => c.IetfLanguageTag).ToImmutableSortedSet();
        set => _supportedCultures = value.Select(CultureInfo.GetCultureInfoByIetfLanguageTag).ToHashSet();
    }

    [JsonIgnore] public IReadOnlyList<CultureInfo> SupportedCultures
    {
        get => _supportedCultures.ToList().AsReadOnly();
        set => _supportedCultures = value.ToHashSet();
    }
}