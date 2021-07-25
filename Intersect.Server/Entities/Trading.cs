using System;
using System.Collections.Generic;

using Intersect.Server.Database;
using Intersect.Server.Framework.Database;
using Intersect.Server.Framework.Entities;

namespace Intersect.Server.Entities
{

    public struct Trading : ITrading
    {

        private readonly IPlayer mPlayer;

        public bool Actively => Counterparty != null;

        public IPlayer Counterparty;

        public bool Accepted;

        public IItem[] Offer;

        public IPlayer Requester;

        public Dictionary<IPlayer, long> Requests;

        public Trading(IPlayer player)
        {
            mPlayer = player;

            Accepted = false;
            Counterparty = null;
            Offer = new Item[Options.MaxInvItems];
            Requester = null;
            Requests = new Dictionary<IPlayer, long>();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Offer = Array.Empty<IItem>();
            Requester = null;
            Requests.Clear();
        }
        
        /// <inheritdoc />
        bool ITrading.Accepted
        {
            get => Accepted;
            set => Accepted = value;
        }

        /// <inheritdoc />
        IPlayer ITrading.Counterparty
        {
            get => Counterparty;
            set => Counterparty = value;
        }

        /// <inheritdoc />
        IItem[] ITrading.Offer
        {
            get => Offer;
            set => Offer = value;
        }

        /// <inheritdoc />
        IPlayer ITrading.Requester
        {
            get => Requester;
            set => Requester = value;
        }

        /// <inheritdoc />
        Dictionary<IPlayer, long> ITrading.Requests
        {
            get => Requests;
            set => Requests = value;
        }
    }

}
