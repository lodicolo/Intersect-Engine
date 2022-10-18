namespace Intersect.Framework.Runtime.Assets.Sources;

public sealed class FileSystemAssetSource : IAssetSource
{
    private readonly HashSet<string> _availableAssetNames = new();
    private readonly Dictionary<string, string> _availableAssetFileNames = new();
    private readonly string _rootPath;

    public FileSystemAssetSource(string? rootPath = default)
    {
        _rootPath = Path.Combine(Environment.CurrentDirectory, _rootPath ?? ".");
    }

    public IEnumerable<string> AvailableAssets => _availableAssetNames;

    private void IndexAssets()
    {
        var directoryInfo = new DirectoryInfo(_rootPath);
        if (!directoryInfo.Exists)
        {
            return;
        }

        IndexAssets(directoryInfo);
    }

    private void IndexAssets(DirectoryInfo directoryInfo)
    {
        var subdirectoryInfos = directoryInfo.GetDirectories();
        foreach (var subdirectoryInfo in subdirectoryInfos)
        {
            IndexAssets(subdirectoryInfo);
        }

        var fileInfos = directoryInfo.GetFiles();
        foreach (var fileInfo in fileInfos)
        {
            _availableAssetNames.Add(Path.GetRelativePath(_rootPath, fileInfo.FullName));
        }
    }

    public bool TryOpenRead(string qualifiedAssetName, out Stream? stream)
    {
        stream = default;

        if (!_availableAssetNames.Contains(qualifiedAssetName))
        {
            return false;
        }

        var resolvedAssetPath = Path.Join(_rootPath, qualifiedAssetName);

        var fileInfo = new FileInfo(resolvedAssetPath);
        if (!fileInfo.Exists)
        {
            return false;
        }

        stream = fileInfo.OpenRead();
        return true;
    }

    public bool TryOpenWrite(string qualifiedAssetName, out Stream? stream)
    {
        stream = default;

        if (!_availableAssetNames.Contains(qualifiedAssetName))
        {
            return false;
        }

        var resolvedAssetPath = Path.Join(_rootPath, qualifiedAssetName);

        var fileInfo = new FileInfo(resolvedAssetPath);
        if (fileInfo.IsReadOnly)
        {
            return false;
        }

        stream = fileInfo.OpenRead();
        return true;
    }

    public bool TryResolve(string assetName, out string? qualifiedAssetName)
    {
        var normalizedAssetName = assetName.Replace(
            Path.AltDirectorySeparatorChar,
            Path.DirectorySeparatorChar
        );

        if (_availableAssetNames.Contains(normalizedAssetName))
        {
            qualifiedAssetName = normalizedAssetName;
            return true;
        }

        // DANGEROUSLY initialize to default, because there's no point in
        // performing that string manipulation until it's actually useful,
        // and once it is we do not want to have to re-check it for null,
        // and we do not want to recompute this.
        string assetFileName = default!;
        foreach (var availableAssetName in _availableAssetNames)
        {
            if (availableAssetName.Length < normalizedAssetName.Length)
            {
                // We know that the assetName has to be shorter because
                //   it's not the same length or it would have been
                //   equal, so if it's not we can skip.
                continue;
            }

            if (!availableAssetName.EndsWith(normalizedAssetName))
            {
                // If the assetName isn't at the end, it can't be
                //   a match. assetName *must* match the filename
                //   exactly, the rest of the path is optional.
                continue;
            }

            // Ok, we really do need that filename, compute it now if we have not already
            assetFileName ??= Path.GetFileName(normalizedAssetName);

            // Get the plain filename without any paths
            if (!_availableAssetFileNames.TryGetValue(availableAssetName, out var availableAssetFileName))
            {
                availableAssetFileName = Path.GetFileName(availableAssetName);
                _availableAssetFileNames[availableAssetName] = availableAssetFileName;
            }

            if (!availableAssetFileName.Equals(assetFileName, StringComparison.Ordinal))
            {
                // If the actual filenames do not match, we can safely skip.
                // We're testing the difference here between these two cases:
                // - sprites/12.png
                // - sprites/2.png
                // Both end in "2.png" which could be the asset name we are looking for.
                continue;
            }

            // Here's a trick. How do you verify that the available asset matches
            //   if you are searching for "sprites/2.png" and the available assets
            //   include "entities/sprites/2.png" and "npc-sprites/2.png"?
            // Split the directories of assetName and avaialbleAssetName and check
            //   that the latter sequence ends with the former?
            // Wrong!
            // We know 2 things right now:
            // 1. assetName is DEFINITELY shorter than availableAssetName
            // 2. we will only consider it a match if it matches directory segments match,
            //    and this will only hold true if the character immediately preceding it
            //    in the availableAssetName is a directory separator! So just check for that.
            if (availableAssetFileName[^(normalizedAssetName.Length + 1)] != Path.DirectorySeparatorChar)
            {
                // If it's not, skip
                continue;
            }

            // If it matches, return this
            qualifiedAssetName = availableAssetFileName;
            return true;
        }

        qualifiedAssetName = default;
        return false;
    }
}
