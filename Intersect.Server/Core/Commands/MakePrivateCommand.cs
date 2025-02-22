﻿using Intersect.Server.Core.CommandParsing;
using Intersect.Server.Localization;

namespace Intersect.Server.Core.Commands
{

    internal sealed partial class MakePrivateCommand : ServerCommand
    {

        public MakePrivateCommand() : base(Strings.Commands.MakePrivate)
        {
        }

        protected override void HandleValue(ServerContext context, ParserResult result)
        {
            Console.WriteLine($@"    {Strings.Commands.madeprivate}");
            Options.Instance.AdminOnly = true;
            Options.SaveToDisk();
        }

    }

}
