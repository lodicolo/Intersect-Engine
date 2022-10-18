using Newtonsoft.Json;

namespace Intersect.Localization.Common;

public class CommonGeneralNamespace : LocaleNamespace
{
    [JsonProperty(
        NullValueHandling = NullValueHandling.Ignore
    )]
    public readonly LocalizedString Cancel = @"Cancel";

    [JsonProperty(
        NullValueHandling = NullValueHandling.Ignore
    )]
    public readonly LocalizedString Close = @"Close";

    [JsonProperty(
        NullValueHandling = NullValueHandling.Ignore
    )]
    public readonly LocalizedString Exit = @"Exit";

    [JsonProperty(
        NullValueHandling = NullValueHandling.Ignore
    )]
    public readonly LocalizedString False = @"False";

    [JsonProperty(
        NullValueHandling = NullValueHandling.Ignore
    )]
    public readonly LocalizedString General = @"General";

    [JsonProperty(
        NullValueHandling = NullValueHandling.Ignore
    )]
    public readonly LocalizedString Id = @"Id";

    [JsonProperty(
        NullValueHandling = NullValueHandling.Ignore
    )]
    public readonly LocalizedString IdTooltipOfTheX = @"The ID of the {00:lower}.";

    [JsonProperty(
        NullValueHandling = NullValueHandling.Ignore
    )]
    public readonly LocalizedString Miscellaneous = @"Miscellaneous";

    [JsonProperty(
        NullValueHandling = NullValueHandling.Ignore
    )]
    public readonly LocalizedString MissingTranslation = @"Missing Translation";

    [JsonProperty(
        NullValueHandling = NullValueHandling.Ignore
    )]
    public readonly LocalizedString Name = @"Name";

    [JsonProperty(
        NullValueHandling = NullValueHandling.Ignore
    )]
    public readonly LocalizedString NameTooltipOfTheX = @"The name of the {00:lower}.";

    [JsonProperty(
        NullValueHandling = NullValueHandling.Ignore
    )]
    public readonly LocalizedString No = @"No";

    [JsonProperty(
        NullValueHandling = NullValueHandling.Ignore
    )]
    public readonly LocalizedString None = @"None";

    [JsonProperty(
        NullValueHandling = NullValueHandling.Ignore
    )]
    public readonly LocalizedString Ok = @"Ok";

    [JsonProperty(
        NullValueHandling = NullValueHandling.Ignore
    )]
    public readonly LocalizedString Open = @"Open";

    [JsonProperty(
        NullValueHandling = NullValueHandling.Ignore
    )]
    public readonly LocalizedString OpenX = @"Open {00}";

    [JsonProperty(
        NullValueHandling = NullValueHandling.Ignore
    )]
    public readonly LocalizedString Save = @"Save";

    [JsonProperty(
        NullValueHandling = NullValueHandling.Ignore
    )]
    public readonly LocalizedString SaveX = @"Save {00}";

    [JsonProperty(
        NullValueHandling = NullValueHandling.Ignore
    )]
    public readonly LocalizedString True = @"True";

    [JsonProperty(
        NullValueHandling = NullValueHandling.Ignore
    )]
    public readonly LocalizedString Yes = @"Yes";
}
