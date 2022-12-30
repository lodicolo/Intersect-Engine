namespace Intersect.Backups
{
    public sealed class DatabaseBackupPolicy : BackupPolicy<DatabaseBackupPolicy>
    {
        public DatabaseBackupPolicy()
        {
            Frequency = BackupFrequency.DatabaseMigration;
        }

        public DatabaseBackupType BackupType { get; set; } = DatabaseBackupType.Sqlite;

        /// <inheritdoc />
        public override DatabaseBackupPolicy Clone()
        {
            var clone = base.Clone();
            clone.BackupType = BackupType;
            return clone;
        }
    }
}
