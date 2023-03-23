using System.Collections;
using System.Collections.Immutable;

namespace Intersect.Framework.Commands.Arguments;

public sealed class ArgumentValues : IEnumerable<object>
{
    private readonly IList<object> mValues;

    public ArgumentValues(
        string argumentName,
        params object[] values
    ) : this(argumentName, values.AsEnumerable()) { }

    public ArgumentValues(
        string argumentName,
        bool isImplicit,
        params object[] values
    ) : this(argumentName, values.AsEnumerable(), isImplicit) { }

    public ArgumentValues(
        string argumentName,
        IEnumerable<object> values = null,
        bool isImplicit = false
    )
    {
        ArgumentName = argumentName;
        mValues = new List<object>(values ?? Array.Empty<object>());
        IsImplicit = isImplicit;
    }

    public string ArgumentName { get; }

    public object Value => mValues.FirstOrDefault();

    public IList<object> Values => mValues.ToImmutableList() ?? throw new InvalidOperationException();

    public bool IsEmpty => mValues.Count < 1;

    public bool IsImplicit { get; }

    public IEnumerator<object> GetEnumerator() => mValues.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public TValue ToTypedValue<TValue>(int index = 0)
    {
        if (mValues.ElementAtOrDefault(index) is TValue typedValue)
        {
            return typedValue;
        }

        return default;
    }

    public IList<TValue> ToTypedValues<TValue>() =>
        mValues.Select(value => value is TValue typedValue ? typedValue : default).ToImmutableList();
}
