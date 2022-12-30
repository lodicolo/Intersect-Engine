using Newtonsoft.Json;

namespace Intersect.Backups
{
    public abstract partial class BackupPolicy<TBackupPolicy> : Cloneable<TBackupPolicy>
        where TBackupPolicy : BackupPolicy<TBackupPolicy>, new()
    {
        public BackupFrequency Frequency { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public BackupRetentionPolicy Retention { get; set; } = new BackupRetentionPolicy
        {
            RetainedCopies = 0, Segmentation = BackupSegmentation.Weekly
        };

        public override TBackupPolicy Clone()
        {
            return new TBackupPolicy { Frequency = Frequency, Retention = Retention };
        }
    }
}
