using System;
using Intersect.Server.Database;
using Intersect.Server.Database.PlayerData.Players;
using Intersect.Server.Framework.Database;
using Intersect.Server.Framework.Database.PlayerData.Players;
using Intersect.Server.Framework.Maps;
using Newtonsoft.Json;

namespace Intersect.Server.Maps
{

    public partial class MapItem : Item, IMapItem
    {

        [JsonIgnore] public int AttributeSpawnX { get; set; } = -1;

        [JsonIgnore] public int AttributeSpawnY { get; set; } = -1;

        [JsonIgnore] public long DespawnTime { get; set; }

        public int X { get; private set; }

        public int Y { get; private set; }

        [JsonIgnore] public int TileIndex => Y * Options.MapWidth + X;

        /// <summary>
        /// The Unique Id of this particular MapItemInstance so we can refer to it elsewhere.
        /// </summary>
        public Guid UniqueId { get; private set; }

        public Guid Owner { get; set; }

        [JsonIgnore] public long OwnershipTime { get; set; }

        // We need this mostly for the client-side.. They can't keep track of our timer after all!
        public bool VisibleToAll { get; set; } = true;

        public MapItem(Guid itemId, int quantity, int x, int y) : base(itemId, quantity, null, null)
        {
            UniqueId = Guid.NewGuid();
            X = x;
            Y = y;
        }

        public MapItem(Guid itemId, int quantity, int x, int y, Guid? bagId, Bag bag) : base(itemId, quantity, bagId, bag)
        {
            UniqueId = Guid.NewGuid();
            X = x;
            Y = y;
        }

        public string Data()
        {
            return JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// Sets up the Stat Buffs on this map item from a supplied item.
        /// </summary>
        /// <param name="item">The item to take the Stat Buffs from and apply them to this MapItem.</param>
        public void SetupStatBuffs(IItem item)
        {
            if (StatBuffs != null && item.StatBuffs != null)
            {
                for (var i = 0; i < StatBuffs.Length; ++i)
                {
                    StatBuffs[i] = item.StatBuffs.Length > i ? item.StatBuffs[i] : 0;
                }
            }
        }

    }

}
