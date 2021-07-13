using Intersect.Enums;
using Intersect.GameObjects.Events;
using Intersect.GameObjects.Events.Commands;
using System;
using System.Collections.Generic;

namespace Intersect.Server.Framework.Entities.Events
{
    public interface ICommandInstance
    {
        Guid[] BranchIds { get; set; }
        EventCommand Command { get; set; }
        int CommandIndex { get; set; }
        List<EventCommand> CommandList { get; set; }
        Guid CommandListId { get; set; }
        EventPage Page { get; set; }
        EventResponse WaitingForResponse { get; set; }
        Guid WaitingForRoute { get; set; }
        Guid WaitingForRouteMap { get; set; }
        EventCommand WaitingOnCommand { get; set; }
    }
}