using Intersect.Framework.Commands;
using Intersect.Framework.Commands.Arguments;
using Intersect.Framework.Commands.Parsing;
using Intersect.Server.Localization;

namespace Intersect.Server.Commands;

internal sealed class MetricsCommand : HelpableCommand
{
    public MetricsCommand() : base(
        Strings.Commands.Metrics,
        Strings.Commands.Arguments.Help,
        new EnumArgument<string>(
            Strings.Commands.Arguments.MetricsOperation,
            RequiredIfNotHelp,
            true,
            Strings.Commands.Arguments.MetricsDisable.ToString(),
            Strings.Commands.Arguments.MetricsEnable.ToString()
        )
    ) { }

    private EnumArgument<string> Operation => FindArgumentOrThrow<EnumArgument<string>>();

    protected override void HandleValue(
        ICommandContext commandContext,
        ParserResult result
    )
    {
        var operation = result.Find(Operation);
        if (operation == Strings.Commands.Arguments.MetricsDisable)
        {
            Options.Instance.Metrics.Enable = false;
            Metrics.MetricsRoot.Instance.Disable();
            Options.SaveToDisk();
        }
        else if (operation == Strings.Commands.Arguments.MetricsEnable)
        {
            if (Metrics.MetricsRoot.Instance == null)
            {
                Metrics.MetricsRoot.Init();
            }

            Options.Instance.Metrics.Enable = true;
            Options.SaveToDisk();
        }

        Console.WriteLine(
            Options.Instance.Metrics.Enable ? Strings.Commandoutput.metricsenabled.ToString()
                : Strings.Commandoutput.metricsdisabled.ToString()
        );
    }
}
