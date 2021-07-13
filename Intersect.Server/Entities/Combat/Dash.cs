using Intersect.Enums;
using Intersect.Server.Framework.Entities;
using Intersect.Server.Framework.Entities.Combat;
using Intersect.Utilities;

namespace Intersect.Server.Entities.Combat
{
    public partial class Dash : IDash
    {
        #region Constructors

        public Dash(
            IEntity en,
            int range,
            byte direction,
            bool blockPass = false,
            bool activeResourcePass = false,
            bool deadResourcePass = false,
            bool zdimensionPass = false
        )
        {
            DistanceTraveled = 0;
            Direction = direction;
            Facing = (byte)en.Dir;

            CalculateRange(en, range, blockPass, activeResourcePass, deadResourcePass, zdimensionPass);
            if (Range <= 0)
            {
                return;
            } //Remove dash instance if no where to dash

            TransmittionTimer = Timing.Global.Milliseconds + (long)((float)Options.MaxDashSpeed / (float)Range);
            PacketSender.SendEntityDash(
                en, en.MapId, (byte)en.X, (byte)en.Y, (int)(Options.MaxDashSpeed * (Range / 10f)),
                Direction == Facing ? (sbyte)Direction : (sbyte)-1
            );

            en.MoveTimer = Timing.Global.Milliseconds + Options.MaxDashSpeed;
        }

        #endregion Constructors

        #region Properties

        public byte Direction { get; set; }

        public int DistanceTraveled { get; set; }

        public byte Facing { get; set; }

        public int Range { get; set; }

        public long TransmittionTimer { get; set; }

        #endregion Properties

        #region Methods

        public void CalculateRange(
            IEntity en,
            int range,
            bool blockPass = false,
            bool activeResourcePass = false,
            bool deadResourcePass = false,
            bool zdimensionPass = false
        )
        {
            var n = 0;
            en.MoveTimer = 0;
            Range = 0;
            for (var i = 1; i <= range; i++)
            {
                n = en.CanMove(Direction);
                if (n == -5) //Check for out of bounds
                {
                    return;
                } //Check for blocks

                if (n == -2 && blockPass == false)
                {
                    return;
                } //Check for ZDimensionTiles

                if (n == -3 && zdimensionPass == false)
                {
                    return;
                } //Check for active resources

                if (n == (int)EntityTypes.Resource && activeResourcePass == false)
                {
                    return;
                } //Check for dead resources

                if (n == (int)EntityTypes.Resource && deadResourcePass == false)
                {
                    return;
                } //Check for players and solid events

                if (n == (int)EntityTypes.Player || n == (int)EntityTypes.Event)
                {
                    return;
                }

                en.Move(Direction, null, true);
                en.Dir = Facing;

                Range = i;
            }
        }

        #endregion Methods
    }
}