using Intersect.GameObjects;
using Intersect.Server.Database.PlayerData.Players;

using System;

namespace Intersect.Server.Database
{
    public interface IItem
    {
        #region Properties

        Bag Bag { get; set; }

        Guid? BagId { get; set; }

        ItemBase Descriptor { get; }

        double DropChance { get; set; }

        Guid ItemId { get; set; }

        string ItemName { get; }

        int Quantity { get; set; }

        int[] StatBuffs { get; set; }

        string StatBuffsJson { get; set; }

        #endregion Properties

        #region Methods

        IItem Clone();

        string Data();

        void Set(Item item);

        bool TryGetBag(out Bag bag);

        #endregion Methods
    }
}