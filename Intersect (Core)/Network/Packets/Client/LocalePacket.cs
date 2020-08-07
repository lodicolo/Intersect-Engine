using Ceras;

using System.Globalization;

namespace Intersect.Network.Packets.Client
{
    public class LocalePacket : CerasPacket
    {
        [Exclude]
        public CultureInfo Locale { get; set; }

        public int LocaleId
        {
            get => Locale.LCID;
            set => Locale = new CultureInfo(value);
        }
    }
}
