using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Hashing;
using System.Linq;
using System.Threading.Tasks;
using Intersect.Updater;
using Newtonsoft.Json;
using Serilog;

namespace Intersect.Update.Generator
{
    public sealed partial class UpdateGenerator
    {
        private readonly ILogger _logger;
        private readonly UpdateOptions _updateOptions;

        public UpdateGenerator(ILogger logger, UpdateOptions updateOptions)
        {
            _logger = logger?.ForContext<UpdateGenerator>();
            _updateOptions = updateOptions;
        }

        public async Task<int> Run()
        {
            _logger?.Debug(
                "Running intersect-updategen with Debug={Debug}, Music Pack Size={MusicPackSize}, Sound Pack Size={SoundPackSize}, Texture Atlas Size={TextureAtlasSize}, Source={Source}, Target={Target}",
                _updateOptions.Debug,
                _updateOptions.PackingOptions.MusicTrackPackSize,
                _updateOptions.PackingOptions.SoundEffectPackSize,
                _updateOptions.PackingOptions.TextureAtlasSize,
                _updateOptions.SourceDirectory,
                _updateOptions.TargetDirectory
            );

            var targetUpdatePath = Path.Combine(_updateOptions.TargetDirectory, "update.json");
            Updater.Update existingUpdate = null;
            try
            {
                if (File.Exists(targetUpdatePath))
                {
                    var existingUpdateJson = File.ReadAllText(targetUpdatePath);
                    existingUpdate = JsonConvert.DeserializeObject<Updater.Update>(existingUpdateJson);
                    _logger?.Information("Found existing update.json, generating an update with only changed files...");
                }
            }
#pragma warning disable CA1031
            catch (Exception exception)
#pragma warning restore CA1031
            {
                _logger?.Error(
                    exception,
                    "Failed to parse existing update.json at {UpdateJsonAbsolutePath}",
                    targetUpdatePath
                );
                return 1;
            }

            List<string> packedAssetPaths;
            try
            {
                packedAssetPaths = await PackAssets().ConfigureAwait(false);
            }
#pragma warning disable CA1031
            catch (Exception exception)
#pragma warning restore CA1031
            {
                _logger?.Error(exception, "Failed to pack assets");
                return 1;
            }

            try
            {
                await GenerateUpdate(existingUpdate, packedAssetPaths).ConfigureAwait(false);
            }
#pragma warning disable CA1031
            catch (Exception exception)
#pragma warning restore CA1031
            {
                _logger?.Error(
                    exception,
                    "Failed to generate the update based on the existing update {ExistingUpdate}",
                    JsonConvert.SerializeObject(existingUpdate, Formatting.Indented)
                );
                return 1;
            }

            return 0;
        }

        private delegate Task<List<string>> TaskFactory();
    }
}