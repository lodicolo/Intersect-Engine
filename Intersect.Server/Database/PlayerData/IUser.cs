using Intersect.Server.Database.PlayerData.Security;
using Intersect.Server.Framework.Entities;

using System;
using System.Collections.Generic;

namespace Intersect.Server.Database.PlayerData
{
    public interface IUser
    {
        #region Properties

        IBan Ban { get; set; }

        Guid Id { get; }

        IBan IpBan { get; set; }

        IMute IpMute { get; set; }

        bool IsBanned { get; }

        bool IsMuted { get; }

        DateTime? LoginTime { get; set; }

        IMute Mute { get; set; }

        List<IPlayer> Players { get; set; }

        ulong PlayTimeSeconds { get; set; }

        UserRights Power { get; set; }

        string PowerJson { get; set; }

        DateTime? RegistrationDate { get; set; }

        IBan UserBan { get; set; }

        IMute UserMute { get; set; }

        #endregion Properties

        #region Methods

        void AddCharacter(IPlayer newCharacter);

        void Delete();

        void DeleteCharacter(IPlayer deleteCharacter);

        void Save(bool force = false);

        void TryLogout();

        #endregion Methods
    }
}