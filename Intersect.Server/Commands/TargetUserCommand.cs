using Intersect.Framework.Commands.Arguments;
using Intersect.Localization;
using Intersect.Server.Database.PlayerData;

namespace Intersect.Server.Commands;

internal abstract class TargetUserCommand : TargettedCommand<User>
{
    protected TargetUserCommand(
        LocaleCommand command,
        LocaleArgument argument,
        params ICommandArgument[] arguments
    ) : base(
        command,
        argument,
        arguments
    ) { }

    protected override User FindTarget(string targetName) =>
        string.IsNullOrWhiteSpace(targetName) ? null : User.Find(targetName);
}
