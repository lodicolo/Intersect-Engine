using Intersect.Framework.Commands.Arguments;
using Intersect.Framework.Commands.Parsing;
using Intersect.Server.Database.PlayerData;
using Intersect.Server.Localization;

namespace Intersect.Server.Commands;

internal class ApiCommand : TargetUserCommand
{
    public ApiCommand() : base(
        Strings.Commands.Api,
        Strings.Commands.Arguments.TargetApi,
        new VariableArgument<bool>(
            Strings.Commands.Arguments.AccessBoolean,
            RequiredIfNotHelp,
            true
        )
    ) { }

    private VariableArgument<bool> Access => FindArgumentOrThrow<VariableArgument<bool>>();

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

        var access = result.Find(Access);
        target.Power.Api = access;
        if (!access)
        {
            target.Power.ApiRoles = new();
        }

        target.Save();

        Console.WriteLine(
            access ? $@"    {Strings.Commandoutput.apigranted.ToString(target.Name)}"
                : $@"    {Strings.Commandoutput.apirevoked.ToString(target.Name)}"
        );
    }
}
