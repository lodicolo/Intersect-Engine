using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Intersect.Update.Generator
{
    public sealed partial class UpdateGenerator
    {
        private async Task<List<string>> PackAssets()
        {
            var packingTasks = new List<TaskFactory>();

            var packsDirectory = Path.Combine(_updateOptions.TargetDirectory, "resources", "packs");

            if (_updateOptions.PackingOptions.MusicTrackPackSize > 0)
            {
                var filePacker = new FileAssetPacker(
                    _logger,
                    _updateOptions.Debug,
                    "*.ogg",
                    "music.index",
                    "music{0}.asset",
                    _updateOptions.PackingOptions.MusicTrackPackSize,
                    _updateOptions.SourceDirectory,
                    Path.Combine(_updateOptions.SourceDirectory, "resources", "music"),
                    packsDirectory
                );

                packingTasks.Add(filePacker.Run);
            }

            if (_updateOptions.PackingOptions.SoundEffectPackSize > 0)
            {
                var filePacker = new FileAssetPacker(
                    _logger,
                    _updateOptions.Debug,
                    "*.wav",
                    "sound.index",
                    "sound{0}.asset",
                    _updateOptions.PackingOptions.SoundEffectPackSize,
                    _updateOptions.SourceDirectory,
                    Path.Combine(_updateOptions.SourceDirectory, "resources", "sounds"),
                    packsDirectory
                );

                packingTasks.Add(filePacker.Run);
            }

            if (_updateOptions.PackingOptions.TextureAtlasSize > 0)
            {
                var texturePacker = new TextureAssetPacker(
                    _logger,
                    _updateOptions.Debug,
                    "graphics{0}.asset",
                    "graphics{0}.meta",
                    _updateOptions.PackingOptions.TextureAtlasSize,
                    _updateOptions.SourceDirectory,
                    Path.Combine(_updateOptions.SourceDirectory, "resources"),
                    packsDirectory
                );

                packingTasks.Add(texturePacker.Run);
            }

            if (packingTasks.Count < 1)
            {
                Console.WriteLine("No packing tasks queued, skipping...");
                return new List<string>();
            }

            Console.WriteLine("Packing assets...");

            Directory.CreateDirectory(packsDirectory);

            var packedAssetPaths = await Task.WhenAll(packingTasks.Select(packingTask => packingTask())).ConfigureAwait(false);
            Console.WriteLine("Finished packing assets.");
            return packedAssetPaths.SelectMany(taskPaths => taskPaths).ToList();

        }
    }
}