using Intersect.Localization;

namespace Intersect.Framework.Commands.Arguments;

public class VariableArgument<TValue> : CommandArgument<TValue>
{
    public VariableArgument(
        LocaleArgument localization,
        bool required = false,
        bool positional = false,
        bool allowsMultiple = false,
        TValue defaultValue = default
    ) : base(localization, required, positional, allowsMultiple, defaultValue) { }

    public VariableArgument(
        LocaleArgument localization,
        ArgumentRequiredPredicate requiredPredicate,
        bool positional = false,
        bool allowsMultiple = false,
        TValue defaultValue = default
    ) : base(localization, requiredPredicate, positional, allowsMultiple, defaultValue) { }
}
