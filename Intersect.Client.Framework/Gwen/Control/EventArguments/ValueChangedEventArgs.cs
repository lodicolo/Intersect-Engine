namespace Intersect.Client.Framework.Gwen.Control.EventArguments;

public class ValueChangedEventArgs<TValue> : EventArgs
{
    public ValueChangedEventArgs() { }

    public required TValue Value { get; init; }

    public TValue OldValue { get; init; }

    public void Deconstruct(out TValue value, out TValue oldValue)
    {
        value = Value;
        oldValue = OldValue;
    }
}