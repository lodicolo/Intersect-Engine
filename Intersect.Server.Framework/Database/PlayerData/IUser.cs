﻿using Intersect.Server.Framework.Database.PlayerData.Security;
using Intersect.Server.Framework.Entities;

using System;
using System.Collections.Generic;

namespace Intersect.Server.Framework.Database.PlayerData
{
    public interface IUser
    {
        #region Properties

        string Email { get; set; }
        Guid Id { get; }
        DateTime? LoginTime { get; set; }
        string Name { get; set; }
        List<IPlayer> Players { get; }
        IBan Ban { get; set; }
        IBan IpBan { get; set; }
        IMute IpMute { get; set; }
        bool IsBanned { get; }
        bool IsMuted { get; }
        IMute Mute { get; set; }
        ulong PlayTimeSeconds { get; set; }
        IUserRights Power { get; set; }
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

        bool TryLogout();

        bool TryChangePassword(string oldPassword, string newPassword);

        bool TrySetPassword(string passwordHash);

        bool IsPasswordValid(string passwordHash);

        #endregion Methods
    }
}