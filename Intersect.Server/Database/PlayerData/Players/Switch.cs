﻿using System;
using System.ComponentModel.DataAnnotations.Schema;

using Intersect.Server.Framework.Database.PlayerData.Players;
using Intersect.Server.Framework.Entities;
using Newtonsoft.Json;

// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace Intersect.Server.Database.PlayerData.Players
{

    public class Switch : IPlayerOwned
    {

        public Switch()
        {
        }

        public Switch(Guid id)
        {
            SwitchId = id;
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; private set; }

        public Guid SwitchId { get; private set; }

        public bool Value { get; set; }

        public Guid PlayerId { get; private set; }

        [JsonIgnore]
        public virtual IPlayer Player { get; private set; }

    }

}
