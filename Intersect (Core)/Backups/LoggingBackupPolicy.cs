namespace Intersect.Backups
{
    public sealed partial class LoggingBackupPolicy : BackupPolicy<LoggingBackupPolicy>
    {
        public LoggingBackupPolicy()
        {
            Frequency = BackupFrequency.Nightly;
            Retention = new BackupRetentionPolicy { RetainedCopies = 31, Segmentation = BackupSegmentation.Monthly };
        }
    }
}
