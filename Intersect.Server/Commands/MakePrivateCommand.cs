using Intersect.Framework.Commands;
using Intersect.Framework.Commands.Parsing;
using Intersect.Server.Localization;

namespace Intersect.Server.Commands;

internal sealed class MakePrivateCommand : HelpableCommand
{
    public MakePrivateCommand() : base(
        Strings.Commands.MakePrivate,
        Strings.Commands.Arguments.Help
    ) { }

    protected override void HandleValue(
        ICommandContext commandContext,
        ParserResult result
    )
    {
        Console.WriteLine($@"    {Strings.Commands.madeprivate}");
        Options.AdminOnly = true;
        Options.SaveToDisk();
    }
}
