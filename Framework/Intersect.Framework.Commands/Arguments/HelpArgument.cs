using Intersect.Localization;

namespace Intersect.Framework.Commands.Arguments;

public class HelpArgument : CommandArgument<bool>
{
    public HelpArgument(LocaleArgument helpArgument) : base(helpArgument) { }

    public override bool IsFlag => true;
}
