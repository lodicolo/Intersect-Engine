using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Intersect.Updater.Packing
{
    public static class PackedImageExtensions
    {
        public static void Deconstruct(this PackedImage packedImage, out Rectangle bounds, out Image<Rgba32> image)
        {
            bounds = packedImage.Bounds;
            image = packedImage.Image;
        }
    }
}