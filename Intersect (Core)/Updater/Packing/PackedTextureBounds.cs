using Newtonsoft.Json;

namespace Intersect.Updater.Packing
{
    public sealed class PackedTextureBounds
    {
        [JsonProperty("x", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int X { get; set; }

        [JsonProperty("y", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Y { get; set; }

        [JsonProperty("w")] public int Width { get; set; }
        [JsonProperty("h")] public int Height { get; set; }
    }
}