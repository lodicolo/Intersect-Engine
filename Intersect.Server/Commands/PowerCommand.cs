using Intersect.Enums;
using Intersect.Framework.Commands.Arguments;
using Intersect.Framework.Commands.Parsing;
using Intersect.Server.Database;
using Intersect.Server.Database.PlayerData.Security;
using Intersect.Server.Localization;
using Intersect.Server.Networking;

namespace Intersect.Server.Commands;

internal sealed class PowerCommand : TargetClientCommand
{
    public PowerCommand() : base(
        Strings.Commands.Power,
        Strings.Commands.Arguments.TargetPower,
        new VariableArgument<Access>(
            Strings.Commands.Arguments.Power,
            RequiredIfNotHelp,
            true
        )
    ) { }

    private VariableArgument<Access> Power => FindArgumentOrThrow<VariableArgument<Access>>();

    protected override void HandleTarget(
        ParserResult result,
        Client target
    )
    {
        if (target?.Entity == null)
        {
            Console.WriteLine($@"    {Strings.Player.offline}");

            return;
        }

        if (target.Name == null)
        {
            Console.WriteLine($@"    {Strings.Account.notfound}");

            return;
        }

        var power = result.Find(Power);
        DbInterface.SetPlayerPower(
            target.Name,
            power.AsUserRights()
        );
        PacketSender.SendEntityDataToProximity(target.Entity);
        PacketSender.SendGlobalMsg(
            power != Access.None ? Strings.Player.admin.ToString(target.Entity.Name)
                : Strings.Player.deadmin.ToString(target.Entity.Name)
        );

        Console.WriteLine($@"    {Strings.Commandoutput.powerchanged.ToString(target.Entity.Name)}");
    }
}
