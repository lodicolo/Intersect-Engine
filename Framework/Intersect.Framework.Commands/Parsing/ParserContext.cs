using System.Collections.Immutable;
using Intersect.Framework.Commands.Arguments;
using Intersect.Framework.Commands.Parsing.Errors;

namespace Intersect.Framework.Commands.Parsing;

public struct ParserContext
{
    public ICommand Command { get; set; }

    public IReadOnlyList<string> Tokens { get; set; }

    public IReadOnlyList<ParserError> Errors { get; set; }

    public IReadOnlyDictionary<ICommandArgument, ArgumentValues>? Parsed { get; set; }
}
