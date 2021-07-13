using System;

namespace Intersect.Server.Database.PlayerData
{
    public interface IMute
    {
        #region Properties

        DateTime EndTime { get; set; }

        Guid Id { get; }

        bool IsExpired { get; }

        bool IsIp { get; }

        string Muter { get; }

        string Reason { get; }

        DateTime StartTime { get; }

        User User { get; }

        Guid UserId { get; }

        #endregion Properties
    }
}