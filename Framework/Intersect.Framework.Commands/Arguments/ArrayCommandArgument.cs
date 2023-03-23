using Intersect.Localization;

namespace Intersect.Framework.Commands.Arguments;

public abstract class ArrayCommandArgument<TValue> : CommandArgument<TValue>
{
    protected ArrayCommandArgument(
        LocaleArgument localization,
        int count,
        string delimeter = null
    ) : base(localization)
    {
        if (count < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        Count = count;
        Delimeter = delimeter;
    }

    public int Count { get; }

    public override bool AllowsMultiple => Count > 1;

    public override bool IsCollection => true;
}
