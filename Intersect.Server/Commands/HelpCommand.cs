using Intersect.Framework.Commands;
using Intersect.Framework.Commands.Parsing;
using Intersect.Server.Localization;

namespace Intersect.Server.Commands;

internal sealed class HelpCommand : HelpableCommand
{
    public HelpCommand(ParserSettings parserSettings) : base(
        Strings.Commands.Help,
        Strings.Commands.Arguments.Help
    ) => ParserSettings = parserSettings;

    private ParserSettings ParserSettings { get; }

    protected override void HandleValue(
        ICommandContext commandContext,
        ParserResult result
    )
    {
        Console.WriteLine(@"    " + Strings.Commandoutput.helpheader);

        Strings.Commands.CommandList.ForEach(
            command => { Console.WriteLine($@"    {command?.Name,-20} - {command?.Help}"); }
        );

        var helpArgument = Help.HasShortName ? ParserSettings.PrefixShort + Help.ShortName.ToString()
            : ParserSettings.PrefixLong + Help.Name;

        Console.WriteLine($@"    {Strings.Commandoutput.helpfooter.ToString(helpArgument)}");
    }
}
