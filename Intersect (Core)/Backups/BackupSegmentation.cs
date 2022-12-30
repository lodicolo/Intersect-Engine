using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Intersect.Backups
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BackupSegmentation
    {
        Daily,

        Weekly,

        Biweekly,

        Monthly,

        Bimonthly,

        Quarterly
    }
}
