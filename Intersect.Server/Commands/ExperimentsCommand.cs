using Intersect.Core.ExperimentalFeatures;
using Intersect.Framework.Commands.Arguments;
using Intersect.Framework.Commands.Parsing;
using Intersect.Server.Core.ExperimentalFeatures;
using Intersect.Server.Localization;

namespace Intersect.Server.Commands;

internal class ExperimentsCommand : TargettedCommand<IExperimentalFlag>
{
    public ExperimentsCommand() : base(
        Strings.Commands.Experiments,
        Strings.Commands.Arguments.TargetExperimentalFeature,
        new VariableArgument<bool>(
            Strings.Commands.Arguments.EnablementBoolean,
            positional: true
        )
    ) { }

    private VariableArgument<bool> Enablement => FindArgumentOrThrow<VariableArgument<bool>>();

    protected override IExperimentalFlag FindTarget(string targetName)
    {
        if (Guid.TryParse(
                targetName,
                out var flagId
            ) && Experiments.Instance.TryGet(
                flagId,
                out var flag
            ))
        {
            return flag;
        }

        if (!string.IsNullOrWhiteSpace(targetName) && Experiments.Instance.TryGet(
                targetName,
                out flag
            ))
        {
            return flag;
        }

        Console.WriteLine($@"    {Strings.Commands.ExperimentalFlagNotFound.ToString(targetName)}");

        return default;
    }

    protected override void HandleTarget(
        ParserResult result,
        IExperimentalFlag target
    )
    {
        if (target == default(IExperimentalFlag))
        {
            return;
        }

        if (result.TryFind(
                Enablement,
                out var enablement,
                allowImplicit: false
            ))
        {
            if (!Experiments.Instance.TrySet(
                    target,
                    enablement
                ))
            {
                throw new(@"Unknown error occurred.");
            }
        }
        else
        {
            enablement = target.Enabled;
        }

        var statusString = enablement ? Strings.General.EnabledLowerCase : Strings.General.DisabledLowerCase;
        var enabledString = Strings.Commandoutput.ExperimentalFeatureEnablement.ToString(
            target.Name,
            statusString
        );

        Console.WriteLine($@"    {enabledString}");
    }
}
