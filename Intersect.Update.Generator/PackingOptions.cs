namespace Intersect.Update.Generator
{
    public sealed class PackingOptions
    {
        /// <summary>
        ///     The maximum size of a single music track pack file in MiB (default 8 MiB), 0 to disable packing music tracks.
        /// </summary>
        public uint MusicTrackPackSize { get; set; } = 8;

        /// <summary>
        ///     The maximum size of a single sound effect pack file in MiB (default 8 MiB), 0 to disable packing sound effects.
        /// </summary>
        public uint SoundEffectPackSize { get; set; } = 8;

        /// <summary>
        ///     The maximum dimension in pixels (default 2048px) of a square texture atlas, 0 to disable packing textures, must be
        ///     a power of 2 and at least 256.
        /// </summary>
        public uint TextureAtlasSize { get; set; } = 2048;
    }
}