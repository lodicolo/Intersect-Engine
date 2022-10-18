using Intersect.Localization;

namespace Intersect.Framework.Commands.Parsing.Errors;

public class MissingCommandError : ParserError
{
    private MissingCommandError(
        string commandName,
        string message
    ) : base(message) => CommandName = commandName;

    public string CommandName { get; }

    public static MissingCommandError Create(
        string commandName,
        LocalizedString message,
        LocaleCommand helpCommand
    ) => new(
        commandName,
        message.ToString(
            commandName,
            helpCommand.Name
        )
    );
}
