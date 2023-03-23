using System.Diagnostics;
using Intersect.Framework.FileSystem.Overwriting;
using Intersect.Framework.Reflection;
using Microsoft.Extensions.Logging;

namespace Intersect.Framework.FileSystem;

public class FileOverwriter
{
    private readonly ILogger<FileOverwriter>? _logger;

    private IOverwriteCondition[]? _baseConditions;

    public FileOverwriter(ILoggerFactory? loggerFactory = default)
    {
        _logger = loggerFactory?.CreateLogger<FileOverwriter>();
    }

    private FileOverwriter(ILogger<FileOverwriter>? logger)
    {
        _logger = logger;
    }

    public OverwriteBehavior OverwriteBehavior { get; init; }

    public IOverwriteCondition[] OverwriteConditions
    {
        get => _baseConditions ??= Array.Empty<IOverwriteCondition>();
        init => _baseConditions = value;
    }

    public bool CanWriteTo(string targetPath, params IOverwriteCondition[] overwriteConditions)
    {
        var attributes = File.GetAttributes(targetPath);
        if (attributes.HasFlag(FileAttributes.Directory))
        {
            return false;
        }

        var targetInfo = new FileInfo(targetPath);

        if (targetInfo.IsReadOnly)
        {
            return false;
        }

        if (!targetInfo.Exists)
        {
            return true;
        }

        switch (OverwriteBehavior)
        {
            case OverwriteBehavior.DoNotOvewrite:
                return false;
            case OverwriteBehavior.Overwrite:
            case OverwriteBehavior.OverwriteWithBackup:
                break;
            default:
                throw new UnreachableException();
        }

        var conditionFailed = false;
        var baseConditionCount = OverwriteConditions.Length;
        var combinedConditions = OverwriteConditions.Concat(overwriteConditions).ToArray();
        for (var overwriteConditionIndex = 0;
             overwriteConditionIndex < combinedConditions.Length;
             overwriteConditionIndex++)
        {
            var overwriteCondition = combinedConditions[overwriteConditionIndex];
            if (overwriteCondition.IsMetFor(targetPath))
            {
                continue;
            }

            var overwriteConditionTypeName = overwriteCondition.GetType().GetQualifiedName();
            var id = overwriteCondition.Id ??
                     $"{nameof(IOverwriteCondition)}[{overwriteConditionIndex - baseConditionCount}/{overwriteConditionIndex}] ({overwriteConditionTypeName})";
            _logger?.LogTrace(FileSystemResources.OverwriteConditionNotMet, id, targetPath);
            conditionFailed = true;
        }

        if (conditionFailed)
        {
            return false;
        }

        if (OverwriteBehavior == OverwriteBehavior.Overwrite)
        {
            return true;
        }

        try
        {
            var lastModified = targetInfo.LastWriteTime;
            var now = DateTime.Now;

            const string dateFormat = "yyyy-MM-dd_HH-mm-ss";

            var extension = Path.GetExtension(targetPath);
            var baseName = Path.GetFileNameWithoutExtension(targetPath);
            var dateTimeStamps = $"{now.ToString(dateFormat)}-{lastModified.ToString(dateFormat)}";
            var backupName = $"{baseName}.backup.{dateTimeStamps}{extension}";
            var targetDir = targetInfo.DirectoryName ?? String.Empty;
            var backupPath = Path.Join(targetDir, backupName);
            File.Move(targetPath, backupPath);
            return true;
        }
        catch (Exception exception)
        {
            _logger?.LogWarning(exception, FileSystemResources.FailedToCreateBackup, targetPath);
            return false;
        }
    }

    public FileOverwriter AddOverwriteConditions(params IOverwriteCondition[] overwriteConditions) =>
        AddOverwriteConditions(overwriteConditions.AsEnumerable());

    public FileOverwriter AddOverwriteConditions(IEnumerable<IOverwriteCondition> overwriteConditions) => new(_logger)
    {
        OverwriteBehavior = OverwriteBehavior,
        OverwriteConditions = OverwriteConditions.Concat(overwriteConditions).ToArray()
    };

    public FileOverwriter WithOverwriteConditions(params IOverwriteCondition[] overwriteConditions) => new(_logger)
    {
        OverwriteBehavior = OverwriteBehavior,
        OverwriteConditions = overwriteConditions
    };

    public FileOverwriter WithOverwriteConditions(IEnumerable<IOverwriteCondition> overwriteConditions) =>
        WithOverwriteConditions(overwriteConditions.ToArray());
}