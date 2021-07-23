using Intersect.Enums;
using Intersect.GameObjects;
using Intersect.GameObjects.Events;
using Intersect.GameObjects.Switches_and_Variables;
using Intersect.Network;
using Intersect.Network.Packets.Server;
using Intersect.Server.Framework.Database;
using Intersect.Server.Framework.Database.PlayerData;
using Intersect.Server.Framework.Database.PlayerData.Players;
using Intersect.Server.Framework.Database.PlayerData.Security;
using Intersect.Server.Framework.Entities.Events;
using Intersect.Server.Framework.Maps;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Intersect.Server.Framework.Entities
{
    public interface IPlayer : IEntity
    {
        #region Properties

        List<IBankSlot> Bank { get; set; }

        Dictionary<Guid, string> CachedFriends { get; set; }

        Guid ClassId { get; set; }

        string ClassName { get; }

        long ClientAttackTimer { get; set; }

        long ClientMoveTimer { get; set; }

        int CommonAutorunEvents { get; }

        DateTime? CreationDate { get; set; }

        IGuild DbGuild { get; set; }

        int[] Equipment { get; set; }

        string EquipmentJson { get; set; }

        long Exp { get; set; }

        long ExperienceToNextLevel { get; }

        List<IPlayer> Party { get; set; }

        IPlayer PartyRequester { get; set; }

        Dictionary<IPlayer, long> PartyRequests { get; set; }

        List<IFriend> Friends { get; set; }

        IPlayer FriendRequester { get; set; }

        Dictionary<IPlayer, long> FriendRequests { get; set; }

        ITrading Trading { get; set; }

        Gender Gender { get; set; }

        IGuild Guild { get; set; }

        Tuple<IPlayer, IGuild> GuildInvite { get; set; }

        DateTime GuildJoinDate { get; set; }

        int GuildRank { get; set; }

        List<IHotbarSlot> Hotbar { get; set; }

        bool InBank { get; }

        bool IsValidPlayer { get; }

        string ItemCooldownsJson { get; set; }

        DateTime? LastOnline { get; set; }

        DateTime? LoginTime { get; set; }

        int MapAutorunEvents { get; }

        int[] MaxVitals { get; }

        bool Online { get; }

        TimeSpan OnlineTime { get; }

        ulong PlayTimeSeconds { get; set; }

        IUserRights Power { get; }

        List<IQuest> Quests { get; set; }

        long SaveTimer { get; set; }

        int StatPoints { get; set; }

        IUser User { get; }

        Guid UserId { get; }

        List<IVariable> Variables { get; set; }

        IBag InBag { get; set; }

        ShopBase InShop { get; set; }

        bool GuildBank { get; set; }
        IBankInterface BankInterface { get; set; }

        Networking.IClient Client { get; set; }

        Guid LastMapEntered { get; set; }

        ConcurrentDictionary<Guid, long> ItemCooldowns { get; set; }

        ConcurrentDictionary<IEventPageInstance, IEvent> GlobalPageInstanceLookup { get; set; }

        List<INpc> SpawnedNpcs { get; set; }

        ConcurrentDictionary<Guid, IEvent> EventLookup { get; set; }

        ConcurrentDictionary<IMapTileLoc, IEvent> EventTileLookup { get; set; }

        ConcurrentDictionary<Guid, IEvent> EventBaseIdLookup { get; set; }

        Guid CraftingTableId { get; set; }

        Guid CraftId { get; set; }

        long CraftTimer { get; set; }

        List<Guid> QuestOffers { get; set; }

        IPlayer ChatTarget { get; set; }

        long LastChatTime { get; set; }

        bool InGame { get; set; }

        #endregion Properties

        #region Methods

        void AcceptQuest(Guid questId);

        void AddFriend(IPlayer friend);

        void AddParty(IPlayer target);

        void BuyItem(int slot, int amount);

        int CalculateAttackTime();

        bool CanAttack(IEntity entity, SpellBase spell);

        void CancelQuest(Guid questId);

        void CancelTrade();

        bool CanGiveItem(Guid itemId, int quantity);

        bool CanGiveItem(IItem item);

        int CanMove(int moveDir);

        bool CanSpellCast(SpellBase spell, IEntity target, bool checkVitalReqs);

        bool CanStartQuest(QuestBase quest);

        bool CanTakeItem(Guid itemId, int quantity);

        bool CanTakeItem(IItem item);

        void CastSpell(Guid spellId, int spellSlot = -1);

        bool CheckCrafting(Guid id);

        void CloseBag();

        void CloseBank();

        void CloseCraftingTable();

        void CloseShop();

        void CompleteLogout();

        void CompleteQuest(Guid questId, bool skipCompletionEvent);

        void CompleteQuestTask(Guid questId, Guid taskId);

        int CountItems(Guid itemId, bool inInventory = true, bool inBank = false);

        void CraftItem(Guid id);

        void DeclineQuest(Guid questId);

        void Die(bool dropItems = true, IEntity killer = null);

        void Dispose();

        void DropItemFrom(int slotIndex, int amount);

        EntityPacket EntityPacket(EntityPacket packet = null, IPlayer forPlayer = null);

        void EquipItem(ItemBase itemBase, int slot = -1);

        void EquipmentProcessItemLoss(int slot);

        void EquipmentProcessItemSwap(int item1, int item2);

        IEventPageInstance EventAt(Guid mapId, int x, int y, int z);

        IEvent EventExists(IMapTileLoc loc);

        IEvent FindGlobalEventInstance(IEventPageInstance en);

        int FindInventoryItemQuantity(Guid itemId);

        IInventorySlot FindInventoryItemSlot(Guid itemId, int quantity = 1);

        List<IInventorySlot> FindInventoryItemSlots(Guid itemId, int quantity = 1);

        IInventorySlot FindOpenInventorySlot();

        List<IInventorySlot> FindOpenInventorySlots();

        IQuest FindQuest(Guid questId);

        int FindSpell(Guid spellId);

        void FixVitals();

        void ForgetSpell(int spellSlot);

        void FriendRequest(IPlayer fromPlayer);

        IBag GetBag();

        decimal GetCooldownReduction();

        EntityTypes GetEntityType();

        int GetEquipmentVitalRegen(Vitals vital);

        int GetExpMultiplier();

        Guid GetFriendId(string name);

        Tuple<int, int> GetItemStatBuffs(Stats statType);

        decimal GetLifeSteal();

        double GetLuck();

        int GetMaxVital(int vital);

        int GetMaxVital(Vitals vital);

        bool GetSwitchValue(Guid id);

        double GetTenacity();

        IVariable GetVariable(Guid id, bool createIfNull = false);

        VariableValue GetVariableValue(Guid id);

        int GetWeaponDamage();

        void GiveExperience(long amount);

        void HandleEventCollision(IEvent evt, int pageNum);

        bool HasBag(IBag bag);

        bool HasFriend(string name);

        void HotbarChange(int index, int type, int slot);

        void HotbarSwap(int index, int swapIndex);

        bool InParty(IPlayer member);

        void InviteToParty(IPlayer fromPlayer);

        void InviteToTrade(IPlayer fromPlayer);

        bool IsAllyOf(IEntity otherEntity);

        bool IsAllyOf(IPlayer otherPlayer);

        bool IsBusy();

        void KickParty(Guid target);

        void KilledEntity(IEntity entity);

        bool KnowsSpell(Guid spellId);

        void LeaveParty();

        void LevelUp(bool resetExperience = true, int levels = 1);

        void LoadFriends();

        void LoadGuild();

        void Move(int moveDir, IPlayer forPlayer, bool dontUpdate = false, bool correction = false);

        void NotifySwarm(IEntity attacker);

        void OfferItem(int slot, int amount);

        void OfferQuest(QuestBase quest);

        bool OpenBag(IItem bagItem, ItemBase itemDescriptor);

        bool OpenBank(bool guild = false);

        bool OpenCraftingTable(CraftingTableBase table);

        bool OpenShop(ShopBase shop);

        void PictureClosed(Guid eventId);

        void ProcessRegen();

        bool QuestCompleted(Guid questId);

        bool QuestInProgress(Guid questId, QuestProgressState progress, Guid taskId);

        void RecalculateStatsAndPoints();

        void RemoveEvent(Guid id, bool sendLeave = true);

        void RespondToEvent(Guid eventId, int responseId);

        void RespondToEventInput(Guid eventId, int newValue, string newValueString, bool canceled = false);

        void RetrieveBagItem(int slot, int amount, int invSlot);

        void ReturnTradeItems();

        void RevokeItem(int slot, int amount);

        void SellItem(int slot, int amount);

        void SendEvents();

        void SendPacket(IPacket packet, TransmissionMode mode = TransmissionMode.All);

        void SetLevel(int level, bool resetExperience = false);

        void SetOnline();

        void SetSwitchValue(Guid id, bool value);

        void SetVariableValue(Guid id, long value);

        void SetVariableValue(Guid id, string value);

        bool StartCommonEvent(EventBase baseEvent, CommonEventTrigger trigger = CommonEventTrigger.None, string command = "", string param = "");

        void StartCommonEventsWithTrigger(CommonEventTrigger trigger, string command = "", string param = "");

        void StartQuest(QuestBase quest);

        void StartTrade(IPlayer target);

        void StoreBagItem(int slot, int amount, int bagSlot);

        void SwapBagItems(int item1, int item2);

        void SwapItems(int fromSlotIndex, int toSlotIndex);

        void SwapSpells(int spell1, int spell2);

        void TakeExperience(long amount);

        void TryActivateEvent(Guid eventId);

        void TryAttack(IEntity target);

        void TryAttack(IEntity target, ProjectileBase projectile, SpellBase parentSpell, ItemBase parentItem, byte projectileDir);

        void TryBumpEvent(Guid mapId, Guid eventId);

        bool TryChangeName(string newName);

        bool TryDropItemFrom(int slotIndex, int amount);

        bool TryForgetSpell(ISpell spell, bool sendUpdate = true);

        bool TryGetItemAt(int slotIndex, out IItem item);

        bool TryGetSlot(int slotIndex, out IInventorySlot slot, bool createSlotIfNull = false);

        bool TryGiveItem(Guid itemId, int quantity);

        bool TryGiveItem(Guid itemId, int quantity, ItemHandling handler);

        bool TryGiveItem(Guid itemId, int quantity, ItemHandling handler, bool bankOverflow = false, bool sendUpdate = true);

        bool TryGiveItem(IItem item);

        bool TryGiveItem(IItem item, ItemHandling handler = ItemHandling.Normal, bool bankOverflow = false, bool sendUpdate = true);

        bool TryGiveItem(IItem item, ItemHandling handler);

        void TryLogout(bool force = false);

        bool TryTakeItem(Guid itemId, int amount, ItemHandling handler = ItemHandling.Normal, bool sendUpdate = true);

        bool TryTakeItem(IInventorySlot slot, int amount, ItemHandling handler = ItemHandling.Normal, bool sendUpdate = true);

        bool TryTeachSpell(ISpell spell, bool sendUpdate = true);

        void UnequipInvalidItems();

        void UnequipItem(Guid itemId, bool sendUpdate = true);

        void UnequipItem(int slot, bool sendUpdate = true);

        void Update(long timeMs);

        void UpdateCooldown(ItemBase item);

        void UpdateCooldown(SpellBase spell);

        void UpdateGlobalCooldown();

        void UpdateQuestKillTasks(IEntity en);

        void UpgradeStat(int statIndex);

        void UseItem(int slot, IEntity target = null);

        void UseSpell(int spellSlot, IEntity target);

        bool ValidateLists();

        void Warp(Guid newMapId, float newX, float newY, bool adminWarp = false);

        void Warp(Guid newMapId, float newX, float newY, byte newDir, bool adminWarp = false, byte zOverride = 0, bool mapSave = false);

        void WarpToSpawn(bool sendWarp = false);

        #endregion Methods
    }
}