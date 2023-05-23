using Newtonsoft.Json;

namespace Intersect.Updater.Packing
{
    public sealed class PackedAtlasMetadata
    {
        [JsonProperty("image")] public string Name { get; set; }

        [JsonProperty("size")] public PackedTextureBounds Size { get; set; }
    }
}