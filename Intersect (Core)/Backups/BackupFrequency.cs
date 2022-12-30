using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Intersect.Backups
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BackupFrequency
    {
        DatabaseMigration,

        Startup,

        Nightly,

        Never = int.MaxValue
    }
}
