using System.Collections.Generic;
using Newtonsoft.Json;

namespace Intersect.Updater.Packing
{
    public sealed class PackedAtlas
    {
        [JsonProperty("meta")] public PackedAtlasMetadata Metadata { get; set; }

        [JsonProperty("frames")] public List<PackedTextureInfo> Frames { get; set; }
    }
}