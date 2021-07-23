using Intersect.Server.Framework.Entities;
using System;

namespace Intersect.Server.Framework.Database.PlayerData.Players
{
    public interface IQuest : IPlayerOwned
    {
        bool Completed { get; set; }
        Guid Id { get; }
        IPlayer Player { get; }
        Guid PlayerId { get; }
        Guid QuestId { get; }
        Guid TaskId { get; set; }
        int TaskProgress { get; set; }

        string Data();
    }
}