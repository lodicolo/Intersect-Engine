using Intersect.Server.Framework.Database;
using System;
using System.Collections.Generic;

namespace Intersect.Server.Framework.Entities
{
    public interface IBankInterface
    {
        bool CanStoreItem(IItem item);
        bool CanTakeItem(Guid itemId, int quantity);
        void Dispose();
        int FindItemQuantity(Guid itemId);
        int FindItemSlot(Guid itemId, int quantity = 1);
        List<int> FindItemSlots(Guid itemId, int quantity = 1);
        int FindOpenSlot();
        List<int> FindOpenSlots();
        void SendBankUpdate(int slot, bool sendToAll = true);
        void SendCloseBank();
        void SendOpenBank();
        void SwapBankItems(int item1, int item2);
        bool TryDepositItem(IItem item, bool sendUpdate = true);
        bool TryDepositItem(int slot, int amount, bool sendUpdate = true);
        void WithdrawItem(int slot, int amount);
    }
}