using System;
using System.Collections.Generic;

namespace Intersect.Server.Framework.Database.PlayerData.Players
{
    public interface IBag
    {
        Guid Id { get; }
        bool IsEmpty { get; }
        int SlotCount { get; }
        List<IBagSlot> Slots { get; set; }

        void Save();
        void ValidateSlots(bool checkItemExistence = true);
    }
}