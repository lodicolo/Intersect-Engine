using Intersect.Framework.Commands.Arguments;
using Intersect.Framework.Commands.Parsing;
using Intersect.Localization;
using Microsoft.Extensions.Hosting;

namespace Intersect.Framework.Commands;

public interface IHelpableCommand : ICommand
{
    HelpArgument? Help { get; }
}

public abstract class HelpableCommand : Command, IHelpableCommand
{
    protected HelpableCommand(
        LocaleCommand localization,
        LocaleArgument localizedHelpArgument,
        params ICommandArgument[] arguments
    ) : base(localization, arguments.Prepend(new HelpArgument(localizedHelpArgument)).ToArray()) { }

    public HelpArgument Help => FindArgumentOrThrow<HelpArgument>();

    public static bool RequiredIfNotHelp(ParserContext context)
    {
        if (context.Command is not HelpableCommand command)
        {
            return false;
        }

        if (context.Parsed == default)
        {
            return false;
        }

        return !(context.Parsed.TryGetValue(command.Help, out var value) && (!value?.IsImplicit ?? true));
    }

    protected override void HandleSuccess(
        ICommandContext commandContext,
        ParserResult result
    )
    {
        if (result.Find(Help))
        {
            return;
        }

        HandleValue(commandContext, result);
    }

    protected abstract void HandleValue(
        ICommandContext commandContext,
        ParserResult result
    );
}
