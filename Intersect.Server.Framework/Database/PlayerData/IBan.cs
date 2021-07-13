using System;

namespace Intersect.Server.Framework.Database.PlayerData
{
    public interface IBan
    {
        #region Properties

        string Banner { get; }

        DateTime EndTime { get; set; }

        Guid Id { get; }

        bool IsExpired { get; }

        string Reason { get; }

        DateTime StartTime { get; }

        IUser User { get; }

        Guid UserId { get; }

        #endregion Properties
    }
}