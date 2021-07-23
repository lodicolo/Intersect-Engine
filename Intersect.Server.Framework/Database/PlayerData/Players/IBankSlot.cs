﻿using Intersect.Server.Framework.Entities;
using System;

namespace Intersect.Server.Framework.Database.PlayerData.Players
{
    public interface IBankSlot : IItem, ISlot, IPlayerOwned
    {
        Guid Id { get; }
        IPlayer Player { get; }
        Guid PlayerId { get; }
        int Slot { get; }
    }
}