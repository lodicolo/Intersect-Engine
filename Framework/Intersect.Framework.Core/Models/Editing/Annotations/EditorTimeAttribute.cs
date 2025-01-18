namespace Intersect.GameObjects.Annotations;

[AttributeUsage(AttributeTargets.Property)]
public class EditorTimeAttribute : EditorFormattedAttribute
{
    public EditorTimeAttribute() : base("FormatTimeMilliseconds")
    {
    }
}