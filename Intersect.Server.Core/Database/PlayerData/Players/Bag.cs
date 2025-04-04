using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Intersect.Core;
using Intersect.Framework.Core.GameObjects.Items;
using Intersect.GameObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace Intersect.Server.Database.PlayerData.Players;


public partial class Bag
{

    public Bag()
    {
    }

    public Bag(int slots)
    {
        SlotCount = slots;
        ValidateSlots();
        Save(create: true);
    }

    [JsonIgnore, NotMapped]
    public bool IsEmpty => Slots?.All(slot => slot?.ItemId == default || ItemDescriptor.Get(slot.ItemId) == default) ?? true;

    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; private set; } = Guid.NewGuid();

    public int SlotCount { get; private set; }

    public virtual List<BagSlot> Slots { get; set; } = new List<BagSlot>();

    public void ValidateSlots(bool checkItemExistence = true)
    {
        if (Slots == null)
        {
            Slots = new List<BagSlot>(SlotCount);
        }

        var slots = Slots
            .Where(bagSlot => bagSlot != null)
            .OrderBy(bagSlot => bagSlot.Slot)
            .Select(
                bagSlot =>
                {
                    if (checkItemExistence && (bagSlot.ItemId == Guid.Empty || bagSlot.Descriptor == null))
                    {
                        bagSlot.Set(new Item());
                    }

                    return bagSlot;
                }
            )
            .ToList();

        for (var slotIndex = 0; slotIndex < SlotCount; ++slotIndex)
        {
            if (slotIndex < slots.Count)
            {
                var slot = slots[slotIndex];
                if (slot != null)
                {
                    if (slot.Slot != slotIndex)
                    {
                        slots.Insert(slotIndex, new BagSlot(slotIndex));
                    }
                }
                else
                {
                    slots[slotIndex] = new BagSlot(slotIndex);
                }
            }
            else
            {
                slots.Add(new BagSlot(slotIndex));
            }
        }

        Slots = slots;
    }

    public void Save (bool create = false)
    {
        try
        {
            using (var context = DbInterface.CreatePlayerContext(readOnly: false))
            {
                if (create)
                {
                    context.Bags.Add(this);
                }
                else
                {
                    context.Bags.Update(this);
                }
                context.ChangeTracker.DetectChanges();
                context.SaveChanges();
            }
        }
        catch (Exception exception)
        {
            ApplicationContext.Context.Value?.Logger.LogError(exception, "Failed to save bag {BagId}", Id);
        }
    }

    /// <summary>
    /// Finds all bag slots matching the desired item and quantity.
    /// </summary>
    /// <param name="itemId">The item Id to look for.</param>
    /// <param name="quantity">The quantity of the item to look for.</param>
    /// <returns>A list of <see cref="InventorySlot"/> containing the requested item.</returns>
    public List<BagSlot> FindBagItemSlots(Guid itemId, int quantity = 1)
    {
        var slots = new List<BagSlot>();

        for (var i = 0; i < SlotCount; i++)
        {
            var item = Slots[i];
            if (item?.ItemId != itemId)
            {
                continue;
            }

            if (item.Quantity >= quantity)
            {
                slots.Add(item);
            }
        }

        return slots;
    }

    /// <summary>
    /// Retrieves a list of open bag slots for this bag.
    /// </summary>
    /// <returns>A list of <see cref="BagSlot"/></returns>
    public List<BagSlot> FindOpenBagSlots()
    {
        var slots = new List<BagSlot>();

        for (var i = 0; i < SlotCount; i++)
        {
            var inventorySlot = Slots[i];

            if (inventorySlot != null && inventorySlot.ItemId == Guid.Empty)
            {
                slots.Add(inventorySlot);
            }
        }
        return slots;
    }

    /// <summary>
    /// Finds the index of a given <see cref="BagSlot"/> within this bag's Slots.
    /// </summary>
    /// <param name="slot">The <see cref="BagSlot"/>to search for</param>
    /// <returns>The index if found, otherwise -1</returns>
    public int FindSlotIndex(BagSlot slot)
    {
        return Slots.FindIndex(sl => sl.Id == slot.Id);
    }

    public static bool TryGetBag(Guid bagId, [NotNullWhen(true)] out Bag? bag)
    {
        try
        {
            using var context = DbInterface.CreatePlayerContext();
            bag = context.Bags.Where(bag => bag.Id == bagId).Include(bag => bag.Slots).SingleOrDefault();
            if (bag == null)
            {
                return false;
            }

            bag.Slots = bag.Slots.OrderBy(p => p.Slot).ToList();
            bag.ValidateSlots();

            // Remove any items from this bag that have been removed from the game
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var bagSlot in bag.Slots)
            {
                if (ItemDescriptor.TryGet(bagSlot.ItemId, out _))
                {
                    continue;
                }

                bagSlot.Set(Item.None);
            }

            return true;

        }
        catch (Exception exception)
        {
            ApplicationContext.Context.Value?.Logger.LogError(exception, "Failed to get bag {BagId}", bagId);
            bag = null;
            return false;
        }
    }
}
