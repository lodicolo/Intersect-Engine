using Intersect.Framework.Commands.Parsing;
using Intersect.Server.Database.PlayerData;
using Intersect.Server.Localization;

namespace Intersect.Server.Commands;

internal class ApiRolesCommand : TargetUserCommand
{
    public ApiRolesCommand() : base(
        Strings.Commands.ApiRoles,
        Strings.Commands.Arguments.TargetApi
    ) { }

    protected override void HandleTarget(
        ParserResult result,
        User target
    )
    {
        if (target == null)
        {
            Console.WriteLine($@"    {Strings.Account.notfound}");

            return;
        }

        if (target.Power == null)
        {
            throw new ArgumentNullException(nameof(target.Power));
        }

        Console.WriteLine(Strings.Commandoutput.apiroles.ToString(target.Name));
        Console.WriteLine("users.query: " + target.Power.ApiRoles.UserQuery);
        Console.WriteLine("users.manage: " + target.Power.ApiRoles.UserManage);
    }
}
