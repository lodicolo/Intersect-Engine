using Newtonsoft.Json;

namespace Intersect.Backups
{
    public class BackupRetentionPolicy : Cloneable<BackupRetentionPolicy>
    {
        /// <summary>
        ///     The number of additional retained copies beyond the latest (i.e. there will be 1 + RetainedCopies backups).
        ///     This applies to each segment specified by <see cref="Segmentation" /> (e.g. 1 per day, 3 per week, 2 per month).
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Populate)]
        public int RetainedCopies { get; set; }

        /// <summary>
        /// How backups are segmented into archive files.
        /// </summary>
        public BackupSegmentation Segmentation { get; set; }

        /// <inheritdoc />
        public override BackupRetentionPolicy Clone()
        {
            return new BackupRetentionPolicy { RetainedCopies = RetainedCopies, Segmentation = Segmentation };
        }
    }
}
