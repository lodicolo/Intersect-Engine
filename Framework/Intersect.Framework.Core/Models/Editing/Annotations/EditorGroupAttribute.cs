namespace Intersect.GameObjects.Annotations;

[AttributeUsage(AttributeTargets.Property)]
public class EditorGroupAttribute(string? label = null) : EditorAttribute
{
    public string? Label { get; init; } = label;
}