using Intersect.Framework.Commands;
using Intersect.Framework.Commands.Parsing;
using Intersect.Server.Localization;
using Intersect.Server.Networking.Helpers;

namespace Intersect.Server.Commands;

internal sealed class NetDebugCommand : HelpableCommand
{
    public NetDebugCommand() : base(
        Strings.Commands.NetDebug,
        Strings.Commands.Arguments.Help
    ) { }

    protected override void HandleValue(
        ICommandContext commandContext,
        ParserResult result
    ) => NetDebug.GenerateDebugFile();
}
