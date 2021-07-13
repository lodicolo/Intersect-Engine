namespace Intersect.Server.Framework.Entities.Combat
{
    public partial interface IStat
    {
        #region Properties

        int BaseStat { get; set; }

        #endregion Properties

        #region Methods

        void AddBuff(IBuff buff);

        void Reset();

        bool Update(long time);

        int Value();

        #endregion Methods
    }
}