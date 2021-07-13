using Intersect.GameObjects;
using Intersect.Server.Framework.Database.PlayerData.Players;
using System;

namespace Intersect.Server.Framework.Database
{
    public interface IItem
    {
        #region Properties

        IBag Bag { get; set; }

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

        void Set(IItem item);

        bool TryGetBag(out IBag bag);

        #endregion Methods
    }
}