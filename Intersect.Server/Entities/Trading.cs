using System;
using System.Collections.Generic;

using Intersect.Server.Database;
using Intersect.Server.Framework.Database;
using Intersect.Server.Framework.Entities;

namespace Intersect.Server.Entities
{

    public struct Trading : ITrading
    {

        private IPlayer mPlayer { get; }

        public bool Actively => Counterparty != null;

        public IPlayer Counterparty { get; set; }

        public bool Accepted { get; set; }

        public IItem[] Offer { get; set; }

        public IPlayer Requester { get; set; }

        public Dictionary<IPlayer, long> Requests { get; set; }

        public Trading(IPlayer player)
        {
            mPlayer = player;

            Accepted = false;
            Counterparty = null;
            Offer = new Item[Options.MaxInvItems];
            Requester = null;
            Requests = new Dictionary<IPlayer, long>();
        }

        public void Dispose()
        {
            Offer = Array.Empty<IItem>();
            Requester = null;
            Requests.Clear();
        }

    }

}
