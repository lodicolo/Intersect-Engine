using Intersect.Enums;
using Intersect.Network.Packets.Server;
using Intersect.Server.Database.PlayerData.Players;
using Intersect.Server.Framework.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Intersect.Server.Framework.Database.PlayerData.Players
{
    public interface IGuild
    {
        List<IGuildBankSlot> Bank { get; set; }
        int BankSlotsCount { get; set; }
        DateTime FoundingDate { get; }
        Guid Id { get; }
        long LastUpdateTime { get; set; }
        object Lock { get; }
        ConcurrentDictionary<Guid, GuildMember> Members { get; }
        string Name { get; }

        void AddMember(IPlayer player, int rank, IPlayer initiator = null);
        void BankSlotUpdated(int slot);
        void ExpandBankSlots(int count);
        List<IPlayer> FindOnlineMembers();
        int GetPlayerRank(Guid id);
        int GetPlayerRank(IPlayer player);
        bool IsMember(Guid id);
        bool IsMember(IPlayer player);
        void RemoveMember(IPlayer player, IPlayer initiator = null, GuildActivityType action = GuildActivityType.Left);
        bool Rename(string name, IPlayer initiator = null);
        void Save();
        void SetPlayerRank(Guid id, int rank, IPlayer initiator = null);
        void SetPlayerRank(IPlayer player, int rank, IPlayer initiator = null);
        bool TransferOwnership(IPlayer newOwner, IPlayer initiator = null);
        void UpdateMemberList();
    }
}