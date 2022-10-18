using Intersect.Extensions;
using Intersect.Framework.Commands;
using Intersect.Framework.Commands.Arguments;
using Intersect.Framework.Commands.Parsing;
using Intersect.Localization;
using Intersect.Server.Localization;

namespace Intersect.Server.Commands;

internal abstract class TargettedCommand<TTarget> : HelpableCommand
{
    protected TargettedCommand(
        LocaleCommand command,
        LocaleArgument argument,
        params ICommandArgument[] arguments
    ) : base(
        command,
        Strings.Commands.Arguments.Help,
        arguments.Prepend(
            new VariableArgument<string>(
                argument,
                RequiredIfNotHelp,
                true
            )
        )
    ) { }

    protected VariableArgument<string> Target => FindArgumentOrThrow<VariableArgument<string>>();

    protected override void HandleValue(
        ICommandContext commandContext,
        ParserResult result
    )
    {
        var target = FindTarget(result.Find(Target));
        HandleTarget(
            result,
            target
        );
    }

    protected abstract TTarget FindTarget(string targetName);

    protected abstract void HandleTarget(
        ParserResult result,
        TTarget target
    );
}
