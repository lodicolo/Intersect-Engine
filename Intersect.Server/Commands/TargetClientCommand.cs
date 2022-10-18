using Intersect.Framework.Commands.Arguments;
using Intersect.Localization;
using Intersect.Server.General;
using Intersect.Server.Networking;

namespace Intersect.Server.Commands;

internal abstract class TargetClientCommand : TargettedCommand<Client>
{
    protected TargetClientCommand(
        LocaleCommand command,
        LocaleArgument argument,
        params ICommandArgument[] arguments
    ) : base(
        command,
        argument,
        arguments
    ) { }

    protected override Client FindTarget(string targetName)
    {
        if (string.IsNullOrWhiteSpace(targetName))
        {
            return null;
        }

        return Globals.Clients.Find(
            client =>
            {
                var playerName = client?.Entity?.Name;

                return string.Equals(
                    playerName,
                    targetName,
                    StringComparison.OrdinalIgnoreCase
                );
            }
        );
    }
}
