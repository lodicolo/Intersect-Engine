using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Intersect.IO.Files;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Intersect.Update.Generator
{
    public sealed class TextureAssetPacker : IAssetPacker
    {
        private readonly bool _debug;
        private readonly ILogger _logger;
        private readonly string _packAssetFileNameTemplate;
        private readonly string _packMetadataFileNameTemplate;
        private readonly uint _packSize;
        private readonly string _rootSourceDirectory;
        private readonly string _sourceDirectory;
        private readonly string _targetDirectory;

        public TextureAssetPacker(
            ILogger logger,
            bool debug,
            string packAssetFileNameTemplate,
            string packMetadataFileNameTemplate,
            uint packSize,
            string rootSourceDirectory,
            string sourceDirectory,
            string targetDirectory
        )
        {
            _debug = debug;
            _logger = logger?.ForContext<TextureAssetPacker>();
            _packAssetFileNameTemplate = packAssetFileNameTemplate;
            _packMetadataFileNameTemplate = packMetadataFileNameTemplate;
            _packSize = packSize;
            _rootSourceDirectory = rootSourceDirectory;
            _sourceDirectory = sourceDirectory;
            _targetDirectory = targetDirectory;
        }

        public async Task<List<string>> Run()
        {
            _logger?.Information("Finding textures...");
            var textureFilePaths = Directory.GetFiles(_sourceDirectory, "*.png", SearchOption.AllDirectories).ToList();

            _logger?.Information("Found {TextureCount} textures to pack...", textureFilePaths.Count);

            var packedAssetPaths = new List<string>();

            var atlases = new List<TextureAtlas>();
            while (textureFilePaths.Count > 0)
            {
                var textureFilePath = textureFilePaths.Last();
                var textureName = FileSystemHelper.RelativePath(_sourceDirectory, textureFilePath);

                textureFilePaths.Remove(textureFilePath);

                packedAssetPaths.Add(FileSystemHelper.RelativePath(_rootSourceDirectory, textureFilePath));

                Image<Rgba32> image = null;
                if (atlases.Any(atlas => atlas.TryInsert(textureFilePath, textureName, out image)))
                {
                    continue;
                }

                if (image == null)
                {
                    image = Image.Load<Rgba32>(textureFilePath);
                }

                var atlasWidth = (uint)Math.Max(_packSize, image.Width);
                var atlasHeight = (uint)Math.Max(_packSize, image.Height);
                var allowRotations = atlasWidth == _packSize && atlasHeight == _packSize;

                if (!allowRotations)
                {
                    _logger?.Information(
                        "Found oversize texture, creating solo atlas for '{OversizedTextureFilePath}'",
                        textureFilePath.Replace(_sourceDirectory, string.Empty)
                    );
                }

                var newAtlas = new TextureAtlas(_targetDirectory, atlasWidth, atlasHeight, allowRotations);
                if (!newAtlas.TryInsert(textureFilePath, textureName, image))
                {
                    throw new InvalidOperationException("Failed to pack image in new atlas, this should never happen.");
                }

                atlases.Add(newAtlas);
            }

            _logger?.Information("Processed textures into {AtlasCount} atlases.", atlases.Count);

            _logger?.Information("Exporting atlases...");
            var atlasIndex = 0;
            var atlasExportTasks = atlases.Select(
                    atlas =>
                    {
                        _logger?.Debug("Exporting atlas {AtlasId}...", atlasIndex);
                        return atlas.Export(
                            _targetDirectory,
                            _packAssetFileNameTemplate,
                            _packMetadataFileNameTemplate,
                            atlasIndex++,
                            _debug
                        );
                    }
                )
                .ToList();

            await Task.WhenAll(atlasExportTasks).ConfigureAwait(false);

            _logger?.Information("Done exporting atlases.");

            return packedAssetPaths;
        }
    }
}