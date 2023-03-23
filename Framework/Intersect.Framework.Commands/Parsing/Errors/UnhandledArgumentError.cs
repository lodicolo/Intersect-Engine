using Intersect.Localization;

namespace Intersect.Framework.Commands.Parsing.Errors;

public class UnhandledArgumentError : ParserError
{
    protected UnhandledArgumentError(
        string commandName,
        string argumentName,
        string message
    ) : base(message)
    {
        CommandName = commandName;
        ArgumentName = argumentName;
    }

    protected UnhandledArgumentError(
        string commandName,
        int position,
        string value,
        string message
    ) : base(message)
    {
        CommandName = commandName;
        Position = position;
        Value = value;
    }

    public string CommandName { get; }

    public string? ArgumentName { get; }

    public int Position { get; }

    public string? Value { get; }

    public bool Positional => string.IsNullOrWhiteSpace(ArgumentName);

    public static UnhandledArgumentError Create(
        string commandName,
        string argumentName,
        LocalizedString message
    ) => new(commandName, argumentName, message.ToString(argumentName, commandName));

    public static UnhandledArgumentError Create(
        string commandName,
        int position,
        string argumentValue,
        LocalizedString message
    ) => new(commandName, position, argumentValue, message.ToString(argumentValue, position, commandName));
}
