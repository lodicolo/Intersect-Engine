using Intersect.Framework.Commands.Parsing;
using Intersect.Server.Localization;
using Intersect.Server.Networking;

namespace Intersect.Server.Commands;

internal sealed class KickCommand : TargetClientCommand
{
    public KickCommand() : base(
        Strings.Commands.Kick,
        Strings.Commands.Arguments.TargetKick
    ) { }

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

        var name = target.Entity.Name;
        target.Disconnect();
        PacketSender.SendGlobalMsg(Strings.Player.serverkicked.ToString(name));
        Console.WriteLine($@"    {Strings.Player.serverkicked.ToString(name)}");
    }
}
