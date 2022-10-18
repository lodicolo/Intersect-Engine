using Intersect.Framework.Commands;
using Intersect.Framework.Commands.Arguments;
using Intersect.Framework.Commands.Parsing;
using Intersect.Server.Localization;
using Intersect.Server.Networking;

namespace Intersect.Server.Commands;

internal sealed class AnnouncementCommand : HelpableCommand
{
    public AnnouncementCommand() : base(
        Strings.Commands.Announcement,
        Strings.Commands.Arguments.Help,
        new VariableArgument<string>(
            Strings.Commands.Arguments.AnnouncementMessage,
            RequiredIfNotHelp,
            true
        )
    ) { }

    private VariableArgument<string> Message => FindArgumentOrThrow<VariableArgument<string>>();

    protected override void HandleValue(
        ICommandContext commandContext,
        ParserResult result
    )
    {
        PacketSender.SendGlobalMsg(result.Find(Message));

        if (Options.Chat.ShowAnnouncementBanners)
        {
            PacketSender.SendGameAnnouncement(
                result.Find(Message),
                Options.Chat.AnnouncementDisplayDuration
            );
        }
    }
}
