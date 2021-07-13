using System;
using System.Collections.Generic;
using System.Linq;
using Intersect.Enums;
using Intersect.GameObjects.Events.Commands;
using Intersect.Server.Framework.Entities.Events;

namespace Intersect.Server.Entities.Events
{

    public partial class CommandInstance : ICommandInstance
    {
        public Guid[]
            BranchIds
        { get; set; } = null; //Potential Branches for Commands that require responses such as ShowingOptions or Offering a Quest

        public EventCommand Command { get; set; }

        private int commandIndex;

        public List<EventCommand> CommandList { get; set; }

        public Guid CommandListId { get; set; }

        public GameObjects.Events.EventPage Page { get; set; }

        public EventResponse WaitingForResponse { get; set; } = EventResponse.None;

        public Guid WaitingForRoute { get; set; }

        public Guid WaitingForRouteMap { get; set; }

        public EventCommand WaitingOnCommand { get; set; } = null;

        public CommandInstance(GameObjects.Events.EventPage page, int listIndex = 0)
        {
            Page = page;
            CommandList = page.CommandLists.Values.First();
            CommandIndex = listIndex;
        }

        public CommandInstance(GameObjects.Events.EventPage page, List<EventCommand> commandList, int listIndex = 0)
        {
            Page = page;
            CommandList = commandList;
            CommandIndex = listIndex;
        }

        public CommandInstance(GameObjects.Events.EventPage page, Guid commandListId, int listIndex = 0)
        {
            Page = page;
            CommandList = page.CommandLists[commandListId];
            CommandIndex = listIndex;
        }

        public int CommandIndex
        {
            get => commandIndex;
            set
            {
                commandIndex = value;
                Command = commandIndex >= 0 && commandIndex < CommandList.Count ? CommandList[commandIndex] : null;
            }
        }

    }

}
