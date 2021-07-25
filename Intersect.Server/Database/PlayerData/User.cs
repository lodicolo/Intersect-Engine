using Intersect.Logging;
using Intersect.Security;
using Intersect.Server.Core;
using Intersect.Server.Database.PlayerData.Api;
using Intersect.Server.Database.PlayerData.Security;
using Intersect.Server.Entities;
using Intersect.Server.Framework.Database.PlayerData;
using Intersect.Server.Framework.Database.PlayerData.Security;
using Intersect.Server.Framework.Entities;
using Intersect.Server.Framework.Networking;
using Intersect.Server.General;
using Intersect.Server.Web.RestApi.Payloads;

using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Intersect.Server.Database.PlayerData
{
    [ApiVisibility(ApiVisibility.Restricted | ApiVisibility.Private)]
    public class User : IUser
    {
        #region Fields

        private static readonly ConcurrentDictionary<Guid, User> OnlineUsers = new ConcurrentDictionary<Guid, User>();

        [JsonIgnore]
        [NotMapped]
        private object mSavingLock = new object();

        #endregion Fields

        #region Properties

        public static int OnlineCount => OnlineUsers.Count;

        public static List<User> OnlineList => OnlineUsers.Values.ToList();

        [Column(Order = 2)]
        public string Email { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(Order = 0)]
        public Guid Id { get; private set; }

        public string LastIp { get; set; }

        [NotMapped]
        public DateTime? LoginTime { get; set; }

        [Column(Order = 1)]
        public string Name { get; set; }

        [JsonIgnore]
        public string Password { get; set; }

        public string PasswordResetCode { get; set; }

        [JsonIgnore]
        public DateTime? PasswordResetTime { get; set; }

        [JsonIgnore]
        public virtual List<Player> Players { get; set; } = new List<Player>();

        [JsonIgnore, NotMapped]
        List<IPlayer> IUser.Players => Players.Cast<IPlayer>().ToList();

        public ulong PlayTimeSeconds
        {
            get
            {
                return mLoadedPlaytime + (ulong)(LoginTime != null ? (DateTime.UtcNow - (DateTime)LoginTime) : TimeSpan.Zero).TotalSeconds;
            }

            set
            {
                mLoadedPlaytime = value;
            }
        }

        [NotMapped]
        public IUserRights Power { get; set; }

        [Column("Power")]
        [JsonIgnore]
        public string PowerJson
        {
            get => JsonConvert.SerializeObject(Power);
            set => Power = JsonConvert.DeserializeObject<UserRights>(value);
        }

        [JsonIgnore]
        public virtual List<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

        public DateTime? RegistrationDate { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public string Salt { get; set; }

        private ulong mLoadedPlaytime { get; set; } = 0;

        #endregion Properties

        #region Methods

        public static Tuple<IClient, IUser> Fetch(Guid userId)
        {
            var client = Globals.Clients.Find(queryClient => userId == queryClient?.User?.Id);

            return new Tuple<IClient, IUser>(client, client?.User ?? Find(userId));
        }

        public static Tuple<IClient, IUser> Fetch(string userName)
        {
            var client = Globals.Clients.Find(queryClient => Entity.CompareName(userName, queryClient?.User?.Name));

            return new Tuple<IClient, IUser>(client, client?.User ?? Find(userName));
        }

        public static IUser Find(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return null;
            }

            var user = FindOnline(userId);

            if (user != null)
            {
                return user;
            }

            try
            {
                using (var context = DbInterface.CreatePlayerContext())
                {
                    return PostLoad(QueryUserById(context, userId));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return null;
            }
        }

        public static IUser Find(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            var user = FindOnline(username);

            if (user != null)
            {
                return user;
            }

            try
            {
                using (var context = DbInterface.CreatePlayerContext())
                {
                    return PostLoad(QueryUserByName(context, username));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return null;
            }
        }

        public static IUser FindFromEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            var user = FindOnlineFromEmail(email);

            if (user != null)
            {
                return user;
            }

            try
            {
                using (var context = DbInterface.CreatePlayerContext())
                {
                    return PostLoad(QueryUserByEmail(context, email));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return null;
            }
        }

        public static User FindFromNameOrEmail(string nameOrEmail)
        {
            if (string.IsNullOrWhiteSpace(nameOrEmail))
            {
                return null;
            }

            var user = FindOnlineFromEmail(nameOrEmail);
            if (user != null)
            {
                return user;
            }

            user = FindOnline(nameOrEmail);
            if (user != null)
            {
                return user;
            }

            try
            {
                using (var context = DbInterface.CreatePlayerContext())
                {
                    return PostLoad(QueryUserByName(context, nameOrEmail));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return null;
            }
        }

        public static User FindOnline(Guid id)
        {
            return OnlineUsers.ContainsKey(id) ? OnlineUsers[id] : null;
        }

        public static User FindOnline(string username)
        {
            return OnlineUsers.Values.FirstOrDefault(s => s.Name.ToLower().Trim() == username.ToLower().Trim());
        }

        public static User FindOnlineFromEmail(string email)
        {
            return OnlineUsers.Values.FirstOrDefault(s => s.Email.ToLower().Trim() == email.ToLower().Trim());
        }

        public static string GetUserSalt(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return null;
            }

            var user = FindOnline(userName);
            if (user != null)
            {
                return user.Salt;
            }

            try
            {
                using (var context = DbInterface.CreatePlayerContext())
                {
                    return SaltByName(context, userName);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return null;
            }
        }

        public static void Login(User user, string ip)
        {
            if (!OnlineUsers.ContainsKey(user.Id))
                OnlineUsers.TryAdd(user.Id, user);

            user.LastIp = ip;
        }

        public static User PostLoad(User user)
        {
            if (user != null)
            {
                foreach (var player in user.Players)
                {
                    Player.Load(player);
                }
            }
            return user;
        }

        public static string SaltPasswordHash(string passwordHash, string salt)
        {
            using (var sha = new SHA256Managed())
            {
                return BitConverter
                    .ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(passwordHash.ToUpperInvariant() + salt)))
                    .Replace("-", "");
            }
        }

        public static IUser TryLogin(string username, string ptPassword)
        {
            var user = FindOnline(username);
            try
            {
                using (var context = DbInterface.CreatePlayerContext())
                {
                    if (user != null)
                    {
                        var pass = SaltPasswordHash(ptPassword, user.Salt);
                        if (pass == user.Password)
                        {
                            return PostLoad(user);
                        }
                    }
                    else
                    {
                        var salt = GetUserSalt(username);
                        if (!string.IsNullOrWhiteSpace(salt))
                        {
                            var pass = SaltPasswordHash(ptPassword, salt);
                            return PostLoad(QueryUserByNameAndPassword(context, username, pass));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return null;
            }
            return null;
        }

        public static bool UserExists(string nameOrEmail)
        {
            if (string.IsNullOrWhiteSpace(nameOrEmail))
            {
                return false;
            }

            var user = FindOnlineFromEmail(nameOrEmail);
            if (user != null)
            {
                return true;
            }

            user = FindOnline(nameOrEmail);
            if (user != null)
            {
                return true;
            }

            try
            {
                using (var context = DbInterface.CreatePlayerContext())
                {
                    return AnyUserByNameOrEmail(context, nameOrEmail);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return false;
            }
        }

        public void AddCharacter(IPlayer player)
        {
            if (!(player is Player newCharacter))
            {
                return;
            }

            //No passing in custom contexts here.. they may already have this user in the change tracker and things just get weird.
            //The cost of making a new context is almost nil.
            try
            {
                lock (mSavingLock)
                {
                    using (var context = DbInterface.CreatePlayerContext(readOnly: false))
                    {
                        context.Users.Update(this);

                        Players.Add(newCharacter);

                        Player.Load(newCharacter);

                        context.ChangeTracker.DetectChanges();

                        context.StopTrackingUsersExcept(this);

                        //If we have a new character, intersect already generated the id.. which means the change tracker is gonna see them as modified and not added.. we need to manually set their state
                        context.Entry(newCharacter).State = EntityState.Added;

                        context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to save user while adding character: " + Name);
                ServerContext.DispatchUnhandledException(new Exception("Failed to save user, shutting down to prevent rollbacks!"), true);
            }
        }

        public void Delete()
        {
            //No passing in custom contexts here.. they may already have this user in the change tracker and things just get weird.
            //The cost of making a new context is almost nil.
            try
            {
                lock (mSavingLock)
                {
                    using (var context = DbInterface.CreatePlayerContext(readOnly: false))
                    {
                        context.Users.Remove(this);

                        context.ChangeTracker.DetectChanges();

                        context.StopTrackingUsersExcept(this);

                        context.Entry(this).State = EntityState.Deleted;

                        context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to delete user: " + Name);
                ServerContext.DispatchUnhandledException(new Exception("Failed to delete user, shutting down to prevent rollbacks!"), true);
            }
        }

        public void DeleteCharacter(IPlayer player)
        {
            if (!(player is Player deleteCharacter))
            {
                return;
            }

            //No passing in custom contexts here.. they may already have this user in the change tracker and things just get weird.
            //The cost of making a new context is almost nil.
            try
            {
                lock (mSavingLock)
                {
                    using (var context = DbInterface.CreatePlayerContext(readOnly: false))
                    {
                        context.Users.Update(this);

                        context.ChangeTracker.DetectChanges();

                        context.StopTrackingUsersExcept(this);

                        context.Entry(deleteCharacter).State = EntityState.Deleted;

                        Players.Remove(deleteCharacter);

                        context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to save user while deleting character: " + Name);
                ServerContext.DispatchUnhandledException(new Exception("Failed to save user, shutting down to prevent rollbacks!"), true);
            }
        }

        public bool IsPasswordValid(string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash) || string.IsNullOrWhiteSpace(Salt))
            {
                return false;
            }

            var saltedPasswordHash = SaltPasswordHash(passwordHash, Salt);

            return string.Equals(Password, saltedPasswordHash, StringComparison.Ordinal);
        }

        public void Save(bool force = false)
        {
            //No passing in custom contexts here.. they may already have this user in the change tracker and things just get weird.
            //The cost of making a new context is almost nil.
            var lockTaken = false;
            PlayerContext context = null;
            try
            {
                if (force)
                {
                    Monitor.Enter(mSavingLock);
                    lockTaken = true;
                }
                else
                {
                    Monitor.TryEnter(mSavingLock, 0, ref lockTaken);
                }

                if (lockTaken)
                {
                    context = DbInterface.CreatePlayerContext(readOnly: false);

                    context.Users.Update(this);

                    context.ChangeTracker.DetectChanges();

                    context.StopTrackingUsersExcept(this);

                    if (UserBan != null)
                    {
                        context.Entry(UserBan).State = EntityState.Detached;
                    }

                    if (UserMute != null)
                    {
                        context.Entry(UserMute).State = EntityState.Detached;
                    }

                    context.SaveChanges();
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var concurrencyErrors = new StringBuilder();
                foreach (var entry in ex.Entries)
                {
                    var type = entry.GetType().FullName.ToString();
                    concurrencyErrors.AppendLine($"Entry Type [{type}]");
                    concurrencyErrors.AppendLine("--------------------");

                    var proposedValues = entry.CurrentValues;
                    var databaseValues = entry.GetDatabaseValues();

                    foreach (var property in proposedValues.Properties)
                    {
                        concurrencyErrors.AppendLine($"{property.Name} (Token: {property.IsConcurrencyToken}): Proposed: {proposedValues[property]}  Original Value: {entry.OriginalValues[property]}  Database Value: {(databaseValues != null ? databaseValues[property] : "null")}");
                    }

                    concurrencyErrors.AppendLine("");
                    concurrencyErrors.AppendLine("");
                }
                Log.Error(ex, "Jackpot! Concurrency Bug For " + Name);
                Log.Error(concurrencyErrors.ToString());
                ServerContext.DispatchUnhandledException(new Exception("Failed to save user, shutting down to prevent rollbacks!"), true);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to save user: " + Name);
                ServerContext.DispatchUnhandledException(new Exception("Failed to save user, shutting down to prevent rollbacks!"), true);
            }
            finally
            {
                if (lockTaken)
                {
                    context?.Dispose();
                    Monitor.Exit(mSavingLock);
                }
            }
        }

        public bool TryChangePassword(string oldPassword, string newPassword)
        {
            return IsPasswordValid(oldPassword) && TrySetPassword(newPassword);
        }

        public bool TryLogout()
        {
            //If we still have a character online (probably being held up in combat) then don't logout yet.
            if (Players.Any(chr => Player.FindOnline(chr.Id) != default))
            {
                return false;
            }

            return OnlineUsers.ContainsKey(Id) && OnlineUsers.TryRemove(Id, out _);
        }

        public bool TrySetPassword(string passwordHash)
        {
            using (var sha = new SHA256Managed())
            {
                using (var rng = new RNGCryptoServiceProvider())
                {
                    /* Generate a Salt */
                    var saltBuffer = new byte[20];
                    rng.GetBytes(saltBuffer);
                    var salt = BitConverter
                        .ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(Convert.ToBase64String(saltBuffer))))
                        .Replace("-", "");

                    Salt = salt;
                    Password = SaltPasswordHash(passwordHash, salt);

                    Save();

                    return true;
                }
            }
        }

        #endregion Methods

        #region Instance Variables

        [ApiVisibility(ApiVisibility.Restricted)]
        [NotMapped]
        IBan IUser.Ban
        {
            get => Ban;
            set => Ban = value as Ban;
        }

        public Ban Ban
        {
            get => UserBan as Ban ?? IpBan as Ban;
            set => UserBan = value;
        }

        [NotMapped]
        public IBan IpBan { get; set; }

        [NotMapped]
        public IMute IpMute { get; set; }

        [NotMapped, ApiVisibility(ApiVisibility.Restricted | ApiVisibility.Private)]
        public bool IsBanned => Ban != null;

        [NotMapped, ApiVisibility(ApiVisibility.Restricted | ApiVisibility.Private)]
        public bool IsMuted => Mute != null;

        [ApiVisibility(ApiVisibility.Restricted)]
        [NotMapped]
        IMute IUser.Mute
        {
            get => Mute;
            set => Mute = value as Mute;
        }

        public Mute Mute
        {
            get => UserMute as Mute ?? IpMute as Mute;
            set => UserMute = value;
        }

        [ApiVisibility(ApiVisibility.Restricted), NotMapped]
        public IBan UserBan { get; set; }

        [ApiVisibility(ApiVisibility.Restricted), NotMapped]
        public IMute UserMute { get; set; }

        #endregion Instance Variables

        #region Listing

        public static int Count()
        {
            try
            {
                using (var context = DbInterface.CreatePlayerContext())
                {
                    return context.Users.Count();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static IList<User> List(string query, string sortBy, SortDirection sortDirection, int skip, int take, out int total)
        {
            try
            {
                using (var context = DbInterface.CreatePlayerContext())
                {
                    var compiledQuery = string.IsNullOrWhiteSpace(query) ? context.Users.Include(p => p.Ban).Include(p => p.Mute) : context.Users.Where(u => EF.Functions.Like(u.Name, $"%{query}%") || EF.Functions.Like(u.Email, $"%{query}%"));

                    total = compiledQuery.Count();

                    switch (sortBy?.ToLower() ?? "")
                    {
                        case "email":
                            compiledQuery = sortDirection == SortDirection.Ascending ? compiledQuery.OrderBy(u => u.Email.ToUpper()) : compiledQuery.OrderByDescending(u => u.Email.ToUpper());
                            break;

                        case "registrationdate":
                            compiledQuery = sortDirection == SortDirection.Ascending ? compiledQuery.OrderBy(u => u.RegistrationDate) : compiledQuery.OrderByDescending(u => u.RegistrationDate);
                            break;

                        case "playtime":
                            compiledQuery = sortDirection == SortDirection.Ascending ? compiledQuery.OrderBy(u => u.PlayTimeSeconds) : compiledQuery.OrderByDescending(u => u.PlayTimeSeconds);
                            break;

                        case "name":
                        default:
                            compiledQuery = sortDirection == SortDirection.Ascending ? compiledQuery.OrderBy(u => u.Name.ToUpper()) : compiledQuery.OrderByDescending(u => u.Name.ToUpper());
                            break;
                    }

                    return compiledQuery.Skip(skip).Take(take).ToList();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                total = 0;
                return null;
            }
        }

        #endregion Listing

        #region Compiled Queries

        private static readonly Func<PlayerContext, string, bool> AnyUserByNameOrEmail =
            EF.CompileQuery(
                // ReSharper disable once SpecifyStringComparison
                (PlayerContext context, string nameOrEmail) => context.Users.Where(u => u.Name == nameOrEmail || u.Email == nameOrEmail).Any());

        private static readonly Func<PlayerContext, string, User> QueryUserByEmail =
            EF.CompileQuery(
                // ReSharper disable once SpecifyStringComparison
                (PlayerContext context, string email) => context.Users.Where(u => u.Email == email)
                    .Include(p => p.Ban)
                    .Include(p => p.Mute)
                    .Include(p => p.Players)
                    .ThenInclude(c => c.Bank)
                    .Include(p => p.Players)
                    .ThenInclude(c => c.Hotbar)
                    .Include(p => p.Players)
                    .ThenInclude(c => c.Quests)
                    .Include(p => p.Players)
                    .ThenInclude(c => c.Variables)
                    .Include(p => p.Players)
                    .ThenInclude(c => c.Items)
                    .Include(p => p.Players)
                    .ThenInclude(c => c.Spells)
                    .Include(p => p.Players)
                    .ThenInclude(c => c.Bank)
                    .FirstOrDefault()
                ) ?? throw new InvalidOperationException();

        private static readonly Func<PlayerContext, Guid, User> QueryUserById =
            EF.CompileQuery(
                (PlayerContext context, Guid id) => context.Users.Where(u => u.Id == id)
                    .Include(p => p.Ban)
                    .Include(p => p.Mute)
                    .Include(p => p.Players)
                    .ThenInclude(c => c.Bank)
                    .Include(p => p.Players)
                    .ThenInclude(c => c.Hotbar)
                    .Include(p => p.Players)
                    .Include(p => p.Players)
                    .ThenInclude(c => c.Quests)
                    .Include(p => p.Players)
                    .ThenInclude(c => c.Variables)
                    .Include(p => p.Players)
                    .ThenInclude(c => c.Items)
                    .Include(p => p.Players)
                    .ThenInclude(c => c.Spells)
                    .Include(p => p.Players)
                    .ThenInclude(c => c.Bank)
                    .FirstOrDefault()
            ) ??
            throw new InvalidOperationException();

        private static readonly Func<PlayerContext, string, User> QueryUserByName =
                                    EF.CompileQuery(
                // ReSharper disable once SpecifyStringComparison
                (PlayerContext context, string username) => context.Users.Where(u => u.Name == username)
                    .Include(p => p.Ban)
                    .Include(p => p.Mute)
                    .Include(p => p.Players)
                    .ThenInclude(c => c.Bank)
                    .Include(p => p.Players)
                    .ThenInclude(c => c.Hotbar)
                    .Include(p => p.Players)
                    .Include(p => p.Players)
                    .ThenInclude(c => c.Quests)
                    .Include(p => p.Players)
                    .ThenInclude(c => c.Variables)
                    .Include(p => p.Players)
                    .ThenInclude(c => c.Items)
                    .Include(p => p.Players)
                    .ThenInclude(c => c.Spells)
                    .Include(p => p.Players)
                    .ThenInclude(c => c.Bank)
                    .FirstOrDefault()
            ) ??
            throw new InvalidOperationException();

        private static readonly Func<PlayerContext, string, string, User> QueryUserByNameAndPassword =
            EF.CompileQuery(
                // ReSharper disable once SpecifyStringComparison
                (PlayerContext context, string username, string password) => context.Users.Where(u => u.Name == username && u.Password == password)
                    .Include(p => p.Ban)
                    .Include(p => p.Mute)
                    .Include(p => p.Players)
                    .ThenInclude(c => c.Bank)
                    .Include(p => p.Players)
                    .ThenInclude(c => c.Hotbar)
                    .Include(p => p.Players)
                    .Include(p => p.Players)
                    .ThenInclude(c => c.Quests)
                    .Include(p => p.Players)
                    .ThenInclude(c => c.Variables)
                    .Include(p => p.Players)
                    .ThenInclude(c => c.Items)
                    .Include(p => p.Players)
                    .ThenInclude(c => c.Spells)
                    .Include(p => p.Players)
                    .ThenInclude(c => c.Bank)
                    .FirstOrDefault()
            ) ??
            throw new InvalidOperationException();

        private static readonly Func<PlayerContext, string, User> QueryUserByNameOrEmail =
            EF.CompileQuery(
                // ReSharper disable once SpecifyStringComparison
                (PlayerContext context, string nameOrEmail) => context.Users.Where(u => u.Name == nameOrEmail || u.Email == nameOrEmail)
                    .Include(p => p.Ban)
                    .Include(p => p.Mute)
                    .Include(p => p.Players)
                    .ThenInclude(c => c.Bank)
                    .Include(p => p.Players)
                    .ThenInclude(c => c.Hotbar)
                    .Include(p => p.Players)
                    .ThenInclude(c => c.Quests)
                    .Include(p => p.Players)
                    .ThenInclude(c => c.Variables)
                    .Include(p => p.Players)
                    .ThenInclude(c => c.Items)
                    .Include(p => p.Players)
                    .ThenInclude(c => c.Spells)
                    .Include(p => p.Players)
                    .ThenInclude(c => c.Bank)
                    .FirstOrDefault()
                ) ?? throw new InvalidOperationException();

        private static readonly Func<PlayerContext, string, string> SaltByName =
                EF.CompileQuery(
                    // ReSharper disable once SpecifyStringComparison
                    (PlayerContext context, string userName) => context.Users.Where(u => u.Name == userName).Select(u => u.Salt).FirstOrDefault());

        #endregion Compiled Queries
    }
}