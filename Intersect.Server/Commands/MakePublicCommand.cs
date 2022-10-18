using Intersect.Framework.Commands;
using Intersect.Framework.Commands.Parsing;
using Intersect.Server.Localization;

namespace Intersect.Server.Commands;

internal sealed class MakePublicCommand : HelpableCommand
{
    public MakePublicCommand() : base(
        Strings.Commands.MakePublic,
        Strings.Commands.Arguments.Help
    ) { }

    protected override void HandleValue(
        ICommandContext commandContext,
        ParserResult result
    )
    {
        Console.WriteLine($@"    {Strings.Commands.madepublic}");
        Options.AdminOnly = false;
        Options.SaveToDisk();
    }
}
