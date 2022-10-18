using Intersect.Framework.Commands.Parsing;
using Intersect.Server.Database.PlayerData;
using Intersect.Server.Localization;

namespace Intersect.Server.Commands;

internal sealed class UnbanCommand : TargetUserCommand
{
    public UnbanCommand() : base(
        Strings.Commands.Unban,
        Strings.Commands.Arguments.TargetUnban
    ) { }

    protected override void HandleTarget(
        ParserResult result,
        User target
    )
    {
        if (target == null)
        {
            Console.WriteLine($@"    {Strings.Account.notfound.ToString(result.Find(Target))}");

            return;
        }

        Ban.Remove(target);
        Console.WriteLine($@"    {Strings.Account.UnbanSuccess.ToString(target.Name)}");
    }
}
