using Intersect.Framework.Commands.Parsing;

namespace Intersect.Framework.Commands.Arguments;

public interface ICommandArgument
{
    char ShortName { get; }

    string Name { get; }

    string Description { get; }

    Type ValueType { get; }

    object ValueTypeDefault { get; }

    object DefaultValue { get; }

    bool HasShortName { get; }

    bool AllowsMultiple { get; }

    bool IsCollection { get; }

    bool IsFlag { get; }

    bool IsRequirementConditional { get; }

    bool IsRequiredByDefault { get; }

    bool IsPositional { get; }

    string Delimeter { get; }

    bool IsRequired(ParserContext parserContext);

    TValue DefaultValueAsType<TValue>();

    bool IsValueAllowed(object value);
}
