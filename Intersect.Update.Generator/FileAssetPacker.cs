using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Intersect.Compression;
using Intersect.IO.Files;
using Serilog;

namespace Intersect.Update.Generator
{
    public sealed class FileAssetPacker : IAssetPacker
    {
        private readonly bool _debug;
        private readonly string _extensionFilter;
        private readonly string _indexFileName;
        private readonly ILogger _logger;
        private readonly string _packFileNameTemplate;
        private readonly uint _packSize;
        private readonly string _rootSourceDirectory;
        private readonly string _sourceDirectory;
        private readonly string _targetDirectory;

        public FileAssetPacker(
            ILogger logger,
            bool debug,
            string extensionFilter,
            string indexFileName,
            string packFileNameTemplate,
            uint packSize,
            string rootSourceDirectory,
            string sourceDirectory,
            string targetDirectory
        )
        {
            _logger = logger?.ForContext<FileAssetPacker>();

            _debug = debug;
            _extensionFilter = string.IsNullOrWhiteSpace(extensionFilter) ? default : extensionFilter.Trim();

            if (string.IsNullOrWhiteSpace(indexFileName))
            {
                throw new ArgumentNullException(nameof(indexFileName));
            }

            _indexFileName = indexFileName;

            if (string.IsNullOrWhiteSpace(packFileNameTemplate))
            {
                throw new ArgumentNullException(nameof(packFileNameTemplate));
            }

            if (!packFileNameTemplate.Contains("{0}"))
            {
                throw new ArgumentException(
                    "Pack file name template must contain '{0}'.",
                    nameof(packFileNameTemplate)
                );
            }

            _packFileNameTemplate = packFileNameTemplate;

            _packSize = packSize;

            if (string.IsNullOrWhiteSpace(rootSourceDirectory))
            {
                throw new ArgumentNullException(nameof(rootSourceDirectory));
            }

            if (!Directory.Exists(rootSourceDirectory))
            {
                throw new ArgumentException("Root source directory does not exist.", nameof(rootSourceDirectory));
            }

            _rootSourceDirectory = rootSourceDirectory;

            if (string.IsNullOrWhiteSpace(sourceDirectory))
            {
                throw new ArgumentNullException(nameof(sourceDirectory));
            }

            if (!Directory.Exists(sourceDirectory))
            {
                throw new ArgumentException("Source directory does not exist.", nameof(sourceDirectory));
            }

            _sourceDirectory = sourceDirectory;

            if (string.IsNullOrWhiteSpace(targetDirectory))
            {
                throw new ArgumentNullException(nameof(targetDirectory));
            }

            _targetDirectory = targetDirectory;
        }

        public Task<List<string>> Run()
        {
            return Task.Run(
                () => AssetPacker.PackageAssets(
                        _sourceDirectory,
                        _extensionFilter,
                        _targetDirectory,
                        _indexFileName,
                        _packFileNameTemplate,
                        _packSize,
                        _debug
                    )
                    .Select(assetPath => FileSystemHelper.RelativePath(_rootSourceDirectory, assetPath))
                    .ToList()
            );
        }
    }
}