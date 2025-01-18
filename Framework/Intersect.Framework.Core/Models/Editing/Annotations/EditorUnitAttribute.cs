namespace Intersect.GameObjects.Annotations;

[AttributeUsage(AttributeTargets.Property)]
public class EditorUnitAttribute(UnitHint unitHint) : EditorFormattedAttribute("FormatTimeMilliseconds")
{
    public UnitHint Unit { get; init; } = unitHint;
}