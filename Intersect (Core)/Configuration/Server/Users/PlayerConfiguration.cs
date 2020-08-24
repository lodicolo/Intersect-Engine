using System;

using Intersect.Configuration.Text;

using Newtonsoft.Json;

namespace Intersect.Configuration.Server.Users
{
    public class PlayerConfiguration
    {
        public AllowedTextConfiguration Name { get; set; }

        public int ItemDropChance { get; set; } = 0;

        [Obsolete("Use MaxBankSlots.", true)]
        public int MaxBank
        {
            get => MaxBankSlots;
            set => MaxBankSlots = value;
        }

        public int MaxBankSlots { get; set; } = 100;

        public int MaxCharacters { get; set; } = 1;

        public int MaxInventory { get; set; } = 35;

        public int MaxLevel { get; set; } = 100;

        public int MaxSpells { get; set; } = 35;

        public int MaxStat { get; set; } = 255;

        public int RequestTimeout { get; set; } = 300000;

        public int TradeRange { get; set; } = 6;
    }
}
