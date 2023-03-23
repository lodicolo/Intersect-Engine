using System.Collections.Immutable;
using Intersect.Framework.Commands.Arguments;
using Intersect.Framework.Commands.Parsing.Errors;

namespace Intersect.Framework.Commands.Parsing;

public class ParserResult<TCommand> where TCommand : ICommand
{
    public ParserResult(
        TCommand command,
        ArgumentValuesMap parsed,
        IEnumerable<ParserError>? errors = default,
        IEnumerable<ICommandArgument>? missing = default,
        IEnumerable<ICommandArgument>? omitted = default
    )
    {
        Command = command;
        Parsed = parsed;
        Errors = (errors ?? Array.Empty<ParserError>()).ToList().AsReadOnly();
        Missing = (missing ?? Array.Empty<ICommandArgument>()).ToList().AsReadOnly();
        Omitted = (omitted ?? Array.Empty<ICommandArgument>()).ToList().AsReadOnly();
        Unhandled = Errors.OfType<UnhandledArgumentError>().ToList().AsReadOnly();
    }

    public ParserResult(
        TCommand command,
        ParserError error,
        IEnumerable<ICommandArgument>? missing = default,
        IEnumerable<ICommandArgument>? omitted = default
    ) : this(command, new(), new[] { error }, missing, omitted) { }

    public TCommand Command { get; }

    public ArgumentValuesMap Parsed { get; }

    public IReadOnlyList<UnhandledArgumentError> Unhandled { get; }

    public IReadOnlyList<ParserError> Errors { get; }

    public IReadOnlyList<ICommandArgument> Missing { get; }

    public IReadOnlyList<ICommandArgument> Omitted { get; }
}

public sealed class ParserResult : ParserResult<ICommand>
{
    public ParserResult(
        ICommand command,
        ArgumentValuesMap parsed,
        IEnumerable<ParserError> errors = null,
        IEnumerable<ICommandArgument> missing = null,
        IEnumerable<ICommandArgument> omitted = null
    ) : base(command, parsed, errors, missing, omitted) { }

    public ParserResult(
        ICommand command,
        ParserError error,
        IEnumerable<ICommandArgument> missing = null,
        IEnumerable<ICommandArgument> omitted = null
    ) : base(command, error, missing, omitted) { }
}
