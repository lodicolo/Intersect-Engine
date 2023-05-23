using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Intersect.Updater.Packing
{
    public sealed class PackedImage
    {
        public PackedImage(Rectangle bounds, Image<Rgba32> image)
        {
            Bounds = bounds;
            Image = image;
        }

        public Rectangle Bounds { get; }

        public Image<Rgba32> Image { get; }
    }
}