﻿using Intersect.Core;
using Intersect.Server.Core.CommandParsing;
using Intersect.Server.Core.CommandParsing.Arguments;
using Intersect.Server.General;
using Intersect.Server.Localization;

namespace Intersect.Server.Core.Commands
{

    internal sealed partial class CpsCommand : ServerCommand
    {

        public CpsCommand() : base(
            Strings.Commands.Cps,
            new EnumArgument<string>(
                Strings.Commands.Arguments.CpsOperation, RequiredIfNotHelp, true,
                Strings.Commands.Arguments.CpsLock.ToString(), Strings.Commands.Arguments.CpsStatus.ToString(),
                Strings.Commands.Arguments.CpsUnlock.ToString()
            )
        )
        {
        }

        private EnumArgument<string> Operation => FindArgumentOrThrow<EnumArgument<string>>();

        protected override void HandleValue(ServerContext context, ParserResult result)
        {
            var operation = result.Find(Operation);
            if (operation == Strings.Commands.Arguments.CpsLock)
            {
                Options.Instance.Processing.CpsLock = true;
                Options.SaveToDisk();
            }
            else if (operation == Strings.Commands.Arguments.CpsUnlock)
            {
                Options.Instance.Processing.CpsLock = false;
                Options.SaveToDisk();
            }

            //else if (operation == Strings.Commands.Arguments.CpsStatus)
            //{
            //    Console.WriteLine(Globals.CpsLock
            //        ? Strings.Commandoutput.cpslocked
            //        : Strings.Commandoutput.cpsunlocked);
            //}
            var cyclesPerSecond = ApplicationContext.GetCurrentContext<IServerContext>().LogicService.CyclesPerSecond;
            // TODO: Rethink what messages we want to display here. Confirmation of the change is ideal. To reuse code we effectively don't need to really handle status.
            Console.WriteLine(Options.Instance.Processing.CpsLock ? (Strings.Commandoutput.CpsLocked.ToString() + " (" + cyclesPerSecond + ")") : Strings.Commandoutput.CpsUnlocked.ToString() + " (" + cyclesPerSecond + ")");
        }

    }

}
