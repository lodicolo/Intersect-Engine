using Newtonsoft.Json;

namespace Intersect.Updater.Packing
{
    public sealed class PackedTextureInfo
    {
        [JsonProperty("filename")] public string Filename { get; set; }

        [JsonProperty("spriteSourceSize")] public PackedTextureBounds SpriteSourceSize { get; set; }

        [JsonProperty("sourceSize")] public PackedTextureBounds SourceSize { get; set; }

        [JsonProperty("frame")] public PackedTextureBounds Frame { get; set; }

        [JsonProperty("rotated")] public bool Rotated { get; set; }
    }
}