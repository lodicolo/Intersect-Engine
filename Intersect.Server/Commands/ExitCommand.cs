using Intersect.Framework.Commands;
using Intersect.Framework.Commands.Parsing;
using Intersect.Server.Localization;

namespace Intersect.Server.Commands;

internal sealed class ExitCommand : HelpableCommand
{
    public ExitCommand() : base(
        Strings.Commands.Exit,
        Strings.Commands.Arguments.Help
    ) { }

    protected override void HandleValue(
        ICommandContext commandContext,
        ParserResult result
    ) => commandContext.RequestShutdown();
}
