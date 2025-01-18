using Intersect.Framework.Core.Content;

namespace Intersect.GameObjects.Annotations;

[AttributeUsage(AttributeTargets.Property)]
public class EditorTextureAttribute : EditorAttribute
{
    public TextureType Type { get; set; }
}