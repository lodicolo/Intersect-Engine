using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Intersect.Backups
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DatabaseBackupType
    {
        Sqlite,

        None = int.MaxValue
    }
}
