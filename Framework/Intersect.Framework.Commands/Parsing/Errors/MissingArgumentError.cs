using Intersect.Localization;

namespace Intersect.Framework.Commands.Parsing.Errors;

public class MissingArgumentError : ParserError
{
    protected MissingArgumentError(
        string argumentName,
        string message
    ) : base(message)
    {
        ArgumentName = argumentName;
    }

    public string ArgumentName { get; }

    public static MissingArgumentError Create(
        string commandName,
        string argumentName,
        LocalizedString message
    ) => new(argumentName, message.ToString(argumentName, commandName));

    public static MissingArgumentError Create(
        string commandName,
        string argumentName,
        LocalizedString message,
        string prefix
    ) => new(argumentName, message.ToString(prefix, argumentName, commandName));
}
