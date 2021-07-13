using Intersect.GameObjects;
using Intersect.GameObjects.Events;
using Intersect.GameObjects.Maps;
using Intersect.Models;
using Intersect.Network.Packets.Server;
using Intersect.Server.Framework.Database;
using Intersect.Server.Framework.Entities;
using Intersect.Server.Framework.Entities.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Intersect.Server.Framework.Maps
{
    public interface IMapInstance : IDatabaseObject
    {
        ConcurrentDictionary<Guid, IMapItem> AllMapItems { get; }
        ConcurrentDictionary<Guid, IProjectile> MapProjectiles { get; }
        ConcurrentDictionary<Guid, ResourceSpawn> ResourceSpawns { get; set; }
        Guid[] SurroundingMapIds { get; set; }
        IMapInstance[] SurroundingMaps { get; set; }
        ConcurrentDictionary<Guid, IMapItem>[] TileItems { get; }

        void AddBatchedActionMessage(ActionMsgPacket packet);
        void AddBatchedAnimation(PlayAnimationPacket packet);
        void AddBatchedMovement(IEntity en, bool correction, IPlayer forPlayer);
        void AddEntity(IEntity en);
        void ClearConnections(int side = -1);
        void ClearEntityTargetsOf(IEntity en);
        void Delete();
        void DespawnEverything();
        void DespawnItems();
        void DespawnItemsOf(ItemBase itemBase);
        void DespawnNpcsOf(NpcBase npcBase);
        void DespawnProjectiles();
        void DespawnProjectilesOf(ProjectileBase projectileBase);
        void DespawnResourcesOf(ResourceBase resourceBase);
        void DespawnTraps();
        void DestroyOrphanedLayers();
        bool FindEvent(EventBase baseEvent, IEventPageInstance globalClone);
        IMapItem FindItem(Guid uniqueId);
        ICollection<IMapItem> FindItemsAt(int tileIndex);
        Dictionary<IMapInstance, List<int>> FindSurroundingTiles(Point location, int distance);
        BytePoint[] GetCachedBlocks(bool isPlayer);
        IEntity[] GetCachedEntities();
        List<IEntity> GetEntities(bool includeSurroundingMaps = false);
        IEvent GetGlobalEventInstance(EventBase baseEvent);
        ConcurrentDictionary<Guid, IEntity> GetLocalEntities();
        object GetMapLock();
        ICollection<IPlayer> GetPlayersOnMap();
        Guid[] GetSurroundingMapIds(bool includingSelf = false);
        IMapInstance[] GetSurroundingMaps(bool includingSelf = false);
        bool HasPlayersOnMap();
        void Initialize();
        void Load(string json, bool keepCreationTime = true);
        void Load(string json, int keepRevision = -1);
        void PlayerEnteredMap(IPlayer player);
        void RemoveEntity(IEntity en);
        void RemoveItem(IMapItem item, bool respawn = true);
        void RemoveProjectile(IProjectile en);
        void RemoveTrap(IMapTrapInstance trap);
        void RespawnEverything();
        void SendMapEntitiesTo(IPlayer player);
        void SpawnItem(int x, int y, IItem item, int amount);
        void SpawnItem(int x, int y, IItem item, int amount, Guid owner, bool sendUpdate = true);
        void SpawnMapProjectile(IEntity owner, ProjectileBase projectile, SpellBase parentSpell, ItemBase parentItem, Guid mapId, byte x, byte y, byte z, byte direction, IEntity target);
        IEntity SpawnNpc(byte tileX, byte tileY, byte dir, Guid npcId, bool despawnable = false);
        void SpawnTrap(IEntity owner, SpellBase parentSpell, byte x, byte y, byte z);
        bool TileBlocked(int x, int y);
        void Update(long timeMs);
        void UpdateProjectiles(long timeMs);
    }
}