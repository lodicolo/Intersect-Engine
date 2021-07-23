using Intersect.GameObjects.Events;
using Intersect.Server.Framework.Maps;
using System;
using System.Collections.Generic;

namespace Intersect.Server.Framework.Entities.Events
{
    public interface IEvent : IEntity
    {
        EventBase BaseEvent { get; set; }
        Stack<ICommandInstance> CallStack { get; set; }
        bool Global { get; set; }
        IEventPageInstance[] GlobalPageInstance { get; set; }
        bool HoldingPlayer { get; set; }
        Guid Id { get; set; }
        Guid MapId { get; set; }
        IMapInstance MapInstance { get; set; }
        int PageIndex { get; set; }
        IEventPageInstance PageInstance { get; set; }
        IPlayer Player { get; set; }
        bool PlayerHasDied { get; set; }
        bool[] SelfSwitch { get; set; }
        int SpawnX { get; set; }
        int SpawnY { get; set; }
        long WaitTimer { get; set; }
        int X { get; set; }
        int Y { get; set; }

        string FormatParameters(IPlayer player);
        string GetParam(IPlayer player, string key);
        Dictionary<string, string> GetParams(IPlayer player);
        void SetParam(string key, string value);
        void Update(long timeMs, IMapInstance map);
    }
}