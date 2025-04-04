﻿using Intersect.Server.Core.CommandParsing;
using Intersect.Server.Core.CommandParsing.Arguments;
using Intersect.Server.Localization;
using Intersect.Server.Networking;

namespace Intersect.Server.Core.Commands
{

    internal sealed partial class AnnouncementCommand : ServerCommand
    {

        public AnnouncementCommand() : base(
            Strings.Commands.Announcement,
            new VariableArgument<string>(Strings.Commands.Arguments.AnnouncementMessage, RequiredIfNotHelp, true)
        )
        {
        }

        private VariableArgument<string> Message => FindArgumentOrThrow<VariableArgument<string>>();

        protected override void HandleValue(ServerContext context, ParserResult result)
        {
            PacketSender.SendGlobalMsg(result.Find(Message));

            if (Options.Instance.Chat.ShowAnnouncementBanners)
            {
                PacketSender.SendGameAnnouncement(result.Find(Message), Options.Instance.Chat.AnnouncementDisplayDuration);
            }
        }

    }

}
