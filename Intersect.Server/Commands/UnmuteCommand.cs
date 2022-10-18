﻿using Intersect.Framework.Commands.Parsing;
using Intersect.Server.Database.PlayerData;
using Intersect.Server.Localization;

namespace Intersect.Server.Commands;

internal sealed class UnmuteCommand : TargetUserCommand
{
    public UnmuteCommand() : base(
        Strings.Commands.Unmute,
        Strings.Commands.Arguments.TargetUnmute
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

        Mute.Remove(target);
        Console.WriteLine($@"    {Strings.Account.UnmuteSuccess.ToString(target.Name)}");
    }
}
