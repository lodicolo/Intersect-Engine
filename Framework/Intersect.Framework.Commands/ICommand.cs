using System.Collections.Immutable;
using Intersect.Framework.Commands.Arguments;
using Intersect.Framework.Commands.Parsing;
using Microsoft.Extensions.Hosting;

namespace Intersect.Framework.Commands;

public interface ICommand
{
    ImmutableList<ICommandArgument> Arguments { get; }

    ImmutableList<ICommandArgument> UnsortedArguments { get; }

    ImmutableList<ICommandArgument> NamedArguments { get; }

    ImmutableList<ICommandArgument> PositionalArguments { get; }

    string Name { get; }

    string Description { get; }

    ICommandArgument FindArgument(char shortName);

    ICommandArgument FindArgument(string name);

    void Handle(
        ICommandContext commandContext,
        ParserResult result
    );

    string FormatUsage(
        ParserSettings parserSettings,
        ParserContext parserContext,
        bool formatPrint = false
    );
}
