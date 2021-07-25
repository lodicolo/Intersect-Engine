using Intersect.Server.Framework.Database;
using System;
using System.Collections.Generic;

namespace Intersect.Server.Framework.Entities
{
    public interface ITrading : IDisposable
    {
        bool Accepted { get; set; }
        bool Actively { get; }
        IPlayer Counterparty { get; set; }
        IItem[] Offer { get; set; }
        IPlayer Requester { get; set; }
        Dictionary<IPlayer, long> Requests { get; set; }
    }
}