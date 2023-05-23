using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Hashing;
using System.Linq;
using System.Threading.Tasks;
using Intersect.Updater;
using Newtonsoft.Json;

namespace Intersect.Update.Generator
{
    public sealed partial class UpdateGenerator
    {
        private async Task GenerateUpdate(Updater.Update existingUpdate, List<string> packedAssetPaths)
        {
            var directories = new List<string>();

            directories.AddRange(
                Directory.GetDirectories(_updateOptions.SourceDirectory, "*", SearchOption.TopDirectoryOnly)
            );

            directories.AddRange(
                Directory.GetDirectories(_updateOptions.TargetDirectory, "*", SearchOption.TopDirectoryOnly)
            );

            var filteredDirectories = directories.Where(
                    directoryPath => !EXCLUDED_DIRECTORY_NAMES.Contains(Path.GetFileName(directoryPath))
                )
                .ToArray();

            var files = new HashSet<string>(
                filteredDirectories.SelectMany(
                        directoryPath => Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories)
                            .Where(filePath => !EXCLUDED_DIRECTORY_NAMES.Contains(Path.GetExtension(filePath)))
                    )
                    .Where(
                        filePath =>
                        {
                            var extension = Path.GetExtension(filePath);
                            return !EXCLUDED_EXTENSIONS.Contains(extension);
                        }
                    )
            );

            var newUpdate = new Updater.Update();

            var algorithm = new Crc64();

            if (existingUpdate != null)
            {
                foreach (var file in existingUpdate.Files)
                {
                    var sourcePath = Path.Combine(_updateOptions.SourceDirectory, file.Path);
                    var sourceInfo = new FileInfo(sourcePath);

                    var targetPath = Path.Combine(_updateOptions.TargetDirectory, file.Path);
                    var targetInfo = new FileInfo(targetPath);

                    if (files.Contains(sourcePath) && !sourceInfo.Exists)
                    {
                        newUpdate.Files.Add(file);
                        files.Remove(sourcePath);
                        continue;
                    }

                    if (files.Contains(targetPath) && !targetInfo.Exists)
                    {
                        newUpdate.Files.Add(file);
                        files.Remove(targetPath);
                        continue;
                    }

                    var newFile = sourceInfo.Exists
                        ? new UpdateFile(_updateOptions.SourceDirectory, sourceInfo, default, algorithm)
                        : new UpdateFile(_updateOptions.TargetDirectory, targetInfo, default, algorithm);

                    if (string.Equals(newFile.Hash, file.Hash, StringComparison.Ordinal))
                    {
                        newUpdate.Files.Add(file);
                        if (targetInfo.Exists)
                        {
                            targetInfo.Delete();
                        }
                    }
                    else
                    {
                        sourceInfo.CopyTo(targetInfo.FullName, true);
                        newUpdate.Files.Add(newFile);
                    }
                }
            }

            newUpdate.Files.AddRange(
                files.Select(
                    newFilePath =>
                    {
                        var targetInfo = new FileInfo(newFilePath);
                        var baseDirectory =
                            newFilePath.StartsWith(_updateOptions.SourceDirectory, StringComparison.Ordinal)
                                ? _updateOptions.SourceDirectory
                                : _updateOptions.TargetDirectory;
                        var newFile = new UpdateFile(baseDirectory, targetInfo, default, algorithm);
                        return newFile;
                    }
                )
            );

            foreach (var file in newUpdate.Files)
            {
                var basename = Path.GetFileNameWithoutExtension(file.Path);

                file.ClientIgnore = EXCLUDED_FILE_NAMES_CLIENT.Contains(file.Path) ||
                                    string.Equals(
                                        _updateOptions.EditorExecutableName,
                                        basename,
                                        StringComparison.Ordinal
                                    ) ||
                                    packedAssetPaths.Contains(file.Path);
                file.EditorIgnore = EXCLUDED_FILE_NAMES_EDITOR.Contains(file.Path) ||
                                    string.Equals(
                                        _updateOptions.ClientExecutableName,
                                        basename,
                                        StringComparison.Ordinal
                                    );
            }

            var updateJson = JsonConvert.SerializeObject(newUpdate);
            var updateFilePath = Path.Combine(_updateOptions.TargetDirectory, "update.json");
            File.WriteAllText(updateFilePath, updateJson);
        }

        // @formatter:wrap_array_initializer_style chop_always
        private static readonly string[] EXCLUDED_DIRECTORY_NAMES =
        {
            "logs",
            "screenshots"
        };

        private static readonly string[] EXCLUDED_EXTENSIONS =
        {
            ".asset.png",
            ".index.json",
            ".meta.json"
        };

        private static readonly string[] EXCLUDED_FILE_NAMES_CLIENT =
        {
            "resources/editor_strings.json"
        };

        private static readonly string[] EXCLUDED_FILE_NAMES_EDITOR =
        {
            "resources/client_strings.json"
        };

        // @formatter:wrap_array_initializer_style restore
    }
}