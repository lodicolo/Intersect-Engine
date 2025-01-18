namespace Intersect.GameObjects.Annotations;

[AttributeUsage(AttributeTargets.Property)]
public class EditorRangeAttribute<TValue> : EditorAttribute
    where TValue : notnull
{
    public EditorRangeAttribute() { }

    public EditorRangeAttribute(TValue max) : this(default, max)
    {
    }

    public EditorRangeAttribute(TValue min, TValue max)
    {
        Min = min;
        Max = max;
    }

    public TValue? Min { get; init; }

    public TValue? Max { get; init; }
}