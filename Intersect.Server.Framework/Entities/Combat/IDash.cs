namespace Intersect.Server.Framework.Entities.Combat
{
    public partial interface IDash
    {
        #region Properties

        byte Direction { get; set; }

        int DistanceTraveled { get; set; }

        byte Facing { get; set; }

        int Range { get; set; }

        long TransmittionTimer { get; set; }

        #endregion Properties

        #region Methods

        void CalculateRange(IEntity en, int range, bool blockPass = false, bool activeResourcePass = false, bool deadResourcePass = false, bool zdimensionPass = false);

        #endregion Methods
    }
}