using Intersect.Framework.Commands.Parsing;
using Intersect.Localization;

namespace Intersect.Framework.Commands.Arguments;

public delegate bool ArgumentRequiredPredicate(ParserContext parserContext);

public abstract class CommandArgument<TValue> : ICommandArgument<TValue>
{
    private readonly ArgumentRequiredPredicate? _requiredPredicate;

    protected CommandArgument(
        LocaleArgument localization,
        bool required = false,
        bool positional = false,
        bool allowsMultiple = false,
        TValue? defaultValue = default
    )
    {
        Localization = localization;
        IsPositional = positional;
        IsRequiredByDefault = required;
        AllowsMultiple = allowsMultiple;
        DefaultValue = defaultValue!;
    }

    protected CommandArgument(
        LocaleArgument localization,
        ArgumentRequiredPredicate requiredPredicate,
        bool positional = false,
        bool allowsMultiple = false,
        TValue defaultValue = default!
    )
    {
        Localization = localization;
        IsPositional = positional;
        IsRequiredByDefault = true;
        AllowsMultiple = allowsMultiple;
        DefaultValue = defaultValue;

        _requiredPredicate = requiredPredicate;
    }

    public LocaleArgument Localization { get; }

    public char ShortName => Localization.ShortName;

    public string Name => Localization.Name;

    public string Description => Localization.Description;

    public Type ValueType => typeof(TValue);

    public TValue DefaultValue { get; }

    public bool HasShortName => ShortName != '\0';

    public virtual bool IsFlag => false;

    public virtual bool AllowsMultiple { get; }

    public virtual bool IsCollection => false;

    public string? Delimeter { get; protected set; }

    public bool IsRequirementConditional => _requiredPredicate != null;

    public bool IsRequiredByDefault { get; }

    public bool IsRequired(ParserContext parserContext) =>
        _requiredPredicate?.Invoke(parserContext) ?? IsRequiredByDefault;

    public bool IsPositional { get; }

    public TDefaultValue? DefaultValueAsType<TDefaultValue>() =>
        DefaultValue == null ? default : (TDefaultValue)(this as ICommandArgument).DefaultValue!;

    public virtual bool IsValueAllowed(object value) => true;

    public TValue? DefaultValueAsType() => DefaultValueAsType<TValue>();
}
