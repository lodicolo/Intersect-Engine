using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Intersect.Core;
using Intersect.Enums;
using Intersect.Framework.Core;
using Intersect.Framework.Core.GameObjects.Events;
using Intersect.Framework.Core.GameObjects.Variables;
using Intersect.Framework.Reflection;
using Intersect.Security;
using Intersect.Server.Collections.Indexing;
using Intersect.Server.Collections.Sorting;
using Intersect.Server.Core;
using Intersect.Server.Database.Logging.Entities;
using Intersect.Server.Database.PlayerData.Api;
using Intersect.Server.Database.PlayerData.Players;
using Intersect.Server.Database.PlayerData.Security;
using Intersect.Server.Entities;
using Intersect.Server.General;
using Intersect.Server.Localization;
using Intersect.Server.Networking;
using Intersect.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VariableValue = Intersect.Framework.Core.GameObjects.Variables.VariableValue;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Intersect.Server.Database.PlayerData;

[ApiVisibility(ApiVisibility.Restricted | ApiVisibility.Private)]
public partial class User
{
    private long _lastSave;
    private readonly object _lastSaveLock = new();

    private static readonly ConcurrentDictionary<Guid, User> OnlineUsers = new();

    [JsonIgnore][NotMapped] private readonly object mSavingLock = new();

    /// <summary>
    ///     Variables that have been updated for this account which need to be saved to the db
    /// </summary>
    [JsonIgnore]
    public ConcurrentDictionary<Guid, UserVariableDescriptor> UpdatedVariables = new();

    public static int OnlineCount => OnlineUsers.Count;

    public static List<User> OnlineList => OnlineUsers.Values.ToList();

    [Column(Order = 0)]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
    public Guid Id { get; private set; } = Guid.NewGuid();

    [Column(Order = 1)] public string Name { get; set; }

    [JsonIgnore] public string Salt { get; set; }

    [JsonIgnore] public string Password { get; set; }

    [Column(Order = 2)] public string Email { get; set; }

    [Column("Power")]
    [JsonIgnore]
    public string PowerJson
    {
        get => JsonConvert.SerializeObject(Power);
        set => Power = JsonConvert.DeserializeObject<UserRights>(value);
    }

    [NotMapped] public UserRights Power { get; set; }

    [JsonIgnore, NotMapped] public ImmutableList<string> Roles => Power.Roles;

    [JsonIgnore] public virtual List<Player> Players { get; set; } = new();

    [JsonIgnore] public virtual List<RefreshToken> RefreshTokens { get; set; } = new();

    public string PasswordResetCode { get; set; }

    [JsonIgnore] public DateTime? PasswordResetTime { get; set; }

    public DateTime? RegistrationDate { get; set; } = DateTime.UtcNow;

    private ulong mLoadedPlaytime { get; set; }

    public ulong PlayTimeSeconds
    {
        get =>
            mLoadedPlaytime +
            (ulong)(LoginTime != null ? DateTime.UtcNow - (DateTime)LoginTime : TimeSpan.Zero).TotalSeconds;

        set => mLoadedPlaytime = value;
    }

    /// <summary>
    ///     User Variable Values
    /// </summary>
    [JsonIgnore]
    public virtual List<UserVariable> Variables { get; set; } = new();

    [NotMapped] public DateTime? LoginTime { get; set; }

    public string LastIp { get; set; }

    public static bool TryFindOnline(LookupKey lookupKey, [NotNullWhen(true)] out User? user)
    {
        if (lookupKey.IsId)
        {
            return OnlineUsers.TryGetValue(lookupKey.Id, out user);
        }

        if (lookupKey.IsName)
        {
            user = OnlineUsers.Values.FirstOrDefault(
                onlineUser => string.Equals(lookupKey.Name, onlineUser.Name, StringComparison.OrdinalIgnoreCase)
            );
            return user != null;
        }

        user = null;
        return false;
    }

    public static User FindOnline(Guid id) => OnlineUsers.ContainsKey(id) ? OnlineUsers[id] : null;

    public static User FindOnline(string username) =>
        OnlineUsers.Values.FirstOrDefault(s => s.Name.ToLower().Trim() == username.ToLower().Trim());

    public static User FindOnlineFromEmail(string email) =>
        OnlineUsers.Values.FirstOrDefault(s => s.Email.ToLower().Trim() == email.ToLower().Trim());

    public static void Login(User user, string ip)
    {
        if (!OnlineUsers.ContainsKey(user.Id))
        {
            OnlineUsers.TryAdd(user.Id, user);
        }

        user.LastIp = ip;
    }

    public void TryLogout(bool softLogout = false)
    {
        //If we still have a character online (probably being held up in combat) then don't logout yet.
        foreach (var chr in Players)
        {
            if (Player.FindOnline(chr.Id) != null)
            {
                return;
            }
        }

        if (!softLogout && OnlineUsers.ContainsKey(Id))
        {
            OnlineUsers.TryRemove(Id, out var removed);
        }
    }

    public static string GenerateSalt(ushort sizeInBits = 256)
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(sizeInBits >> 3));
    }

    public static string SaltPasswordHash(string passwordHash, string salt)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(passwordHash.ToUpperInvariant() + salt)));
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

    public bool TryChangePassword(string oldPassword, string newPassword) =>
        IsPasswordValid(oldPassword) && TrySetPassword(newPassword);

    public bool TrySetPassword(string passwordHash)
    {
        var salt = GenerateSalt();
        SaltPasswordHash(passwordHash, salt);

        Salt = salt;
        Password = SaltPasswordHash(passwordHash, salt);

        if (Save() != UserSaveResult.DatabaseFailure)
        {
            return true;
        }

        var client = Client.LookupByConnectionId.Values.FirstOrDefault(c => c.User.Id == Id);
        client?.LogAndDisconnect(default, nameof(TrySetPassword));
        return false;
    }

    public bool TryAddCharacter(Player? newCharacter)
    {
        if (newCharacter == default)
        {
            return false;
        }

        //No passing in custom contexts here.. they may already have this user in the change tracker and things just get weird.
        //The cost of making a new context is almost nil.
        try
        {
            lock (mSavingLock)
            {
                using var context = DbInterface.CreatePlayerContext(false);
                context.Users.Update(this);

                Players.Add(newCharacter);

                _ = Player.Validate(newCharacter);

                context.ChangeTracker.DetectChanges();

                context.StopTrackingUsersExcept(this);

                //If we have a new character, intersect already generated the id.. which means the change tracker is gonna see them as modified and not added.. we need to manually set their state
                context.Entry(newCharacter).State = EntityState.Added;

                return context.SaveChanges() > -1;
            }
        }
        catch (Exception ex)
        {
            ApplicationContext.Context.Value?.Logger.LogError(ex, $"Failed to save user while adding character: {Name}");
            ServerContext.DispatchUnhandledException(
                new Exception("Failed to save user, shutting down to prevent rollbacks!")
            );
            return false;
        }
    }

    public bool TryDeleteCharacter(Player deleteCharacter)
    {
        //No passing in custom contexts here.. they may already have this user in the change tracker and things just get weird.
        //The cost of making a new context is almost nil.
        try
        {
            lock (mSavingLock)
            {
                using var context = DbInterface.CreatePlayerContext(false);

                context.Users.Update(this);

                Players.Remove(deleteCharacter);

                context.ChangeTracker.DetectChanges();

                context.StopTrackingUsersExcept(this);

                context.Entry(deleteCharacter).State = EntityState.Deleted;

                return context.SaveChanges() > -1;
            }
        }
        catch (Exception ex)
        {
            ApplicationContext.Context.Value?.Logger.LogError(ex, "Failed to save user while deleting character: " + Name);
            return false;
        }
    }

    public bool TryDelete()
    {
        //No passing in custom contexts here.. they may already have this user in the change tracker and things just get weird.
        //The cost of making a new context is almost nil.
        try
        {
            lock (mSavingLock)
            {
                using var context = DbInterface.CreatePlayerContext(false);

                context.Users.Remove(this);

                context.ChangeTracker.DetectChanges();

                context.StopTrackingUsersExcept(this);

                context.Entry(this).State = EntityState.Deleted;

                return context.SaveChanges() > -1;
            }
        }
        catch (Exception ex)
        {
            ApplicationContext.Context.Value?.Logger.LogError(ex, "Failed to delete user: " + Name);
            return false;
        }
    }

    public async Task<UserSaveResult> SaveAsync(
        bool force = false,
        PlayerContext? playerContext = default,
        CancellationToken cancellationToken = default
    ) => Save(playerContext, force);

    public void SaveWithDebounce(long debounceMs = 5000)
    {
        lock (_lastSaveLock)
        {
            if (_lastSave < debounceMs + Timing.Global.MillisecondsUtc)
            {
                ApplicationContext.Context.Value?.Logger.LogDebug("Skipping save due to debounce");
                return;
            }
        }

        if (Save() != UserSaveResult.DatabaseFailure)
        {
            return;
        }

        var client = Client.LookupByConnectionId.Values.FirstOrDefault(c => c.User.Id == Id);
        client?.LogAndDisconnect(default, nameof(SaveWithDebounce));
    }

    public UserSaveResult Save(bool force) => Save(force: force, create: false);

    public UserSaveResult Save(bool force = false, bool create = false) => Save(default, force, create);

#if DIAGNOSTIC
    private int _saveCounter = 0;
#endif

    public UserSaveResult Save(PlayerContext? playerContext, bool force = false, bool create = false)
    {
        lock (_lastSaveLock)
        {
            _lastSave = Timing.Global.MillisecondsUtc;
        }

#if DIAGNOSTIC
        var currentExecutionId = _saveCounter++;
#endif

        //No passing in custom contexts here.. they may already have this user in the change tracker and things just get weird.
        //The cost of making a new context is almost nil.
        var lockTaken = false;
        PlayerContext? createdContext = default;
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

            if (!lockTaken)
            {
#if DIAGNOSTIC
                ApplicationContext.Context.Value?.Logger.LogDebug($"Failed to take lock {Environment.StackTrace}");
#endif
                return UserSaveResult.SkippedCouldNotTakeLock;
            }

#if DIAGNOSTIC
            ApplicationContext.Context.Value?.Logger.LogDebug($"DBOP-A Save({playerContext}, {force}, {create}) #{currentExecutionId} {Name} ({Id})");
#endif

            if (playerContext == null)
            {
                createdContext = DbInterface.CreatePlayerContext(false);
                playerContext = createdContext;
            }

            if (create)
            {
                playerContext.Users.Add(this);
            }
            else
            {
                // playerContext.Attach(this);
                try
                {
                    playerContext.Users.Update(this);
                }
                catch (InvalidOperationException invalidOperationException)
                {
                    // ReSharper disable once ConstantConditionalAccessQualifier
                    // ReSharper disable once ConstantNullCoalescingCondition
                    if (invalidOperationException.Message?.Contains("Collection was modified") ?? false)
                    {
                        try
                        {
                            playerContext.Users.Update(this);
                            ApplicationContext.Context.Value?.Logger.LogWarning(invalidOperationException, $"Successfully recovered from {nameof(InvalidOperationException)}");
                        }
                        catch (Exception exception)
                        {
                            throw new AggregateException(
                                $"Failed to recover from {nameof(InvalidOperationException)}",
                                invalidOperationException,
                                exception
                            );
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            playerContext.ChangeTracker.DetectChanges();

            playerContext.StopTrackingUsersExcept(this);

            if (UserBan != null)
            {
                playerContext.Entry(UserBan).State = EntityState.Detached;
            }

            if (UserMute != null)
            {
                playerContext.Entry(UserMute).State = EntityState.Detached;
            }

            if (playerContext.SaveChanges() > -1)
            {
                return UserSaveResult.Completed;
            }

#if DIAGNOSTIC
            ApplicationContext.Context.Value?.Logger.LogDebug($"DBOP-B Save({playerContext}, {force}, {create}) #{currentExecutionId} {Name} ({Id})");
#endif

            var client = Client.LookupByConnectionId.Values.FirstOrDefault(c => c.User.Id == Id);
            client?.LogAndDisconnect(default, "User.Save");

            return UserSaveResult.DatabaseFailure;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var concurrencyErrors = new StringBuilder();
            foreach (var entry in ex.Entries)
            {
                var type = entry.GetType().FullName;
                concurrencyErrors.AppendLine($"Entry Type [{type} / {entry.State}]");
                concurrencyErrors.AppendLine("--------------------");
                concurrencyErrors.AppendLine($"Type: {entry.Entity.GetFullishName()}");

                var proposedValues = entry.CurrentValues;
                var databaseValues = entry.GetDatabaseValues();

                var propertyNameColumnSize = proposedValues.Properties.Max(property => property.Name.Length);

                foreach (var property in proposedValues.Properties)
                {
                    concurrencyErrors.AppendLine(
                        $"\t{property.Name:propertyNameColumnSize} (Token: {property.IsConcurrencyToken}): Proposed: {proposedValues[property]}  Original Value: {entry.OriginalValues[property]}  Database Value: {(databaseValues != null ? databaseValues[property] : "null")}"
                    );
                }

                concurrencyErrors.AppendLine("");
                concurrencyErrors.AppendLine("");
            }

            var suffix = string.Empty;
#if DIAGNOSTIC
            suffix = $"#{currentExecutionId}";
#endif
            ApplicationContext.Context.Value?.Logger.LogError(ex, $"Jackpot! Concurrency Bug For {Name} in {(createdContext == default ? "Existing" : "Created")} Context {suffix}");
            ApplicationContext.Context.Value?.Logger.LogError(concurrencyErrors.ToString());

#if DIAGNOSTIC
            ApplicationContext.Context.Value?.Logger.LogDebug($"DBOP-C Save({playerContext}, {force}, {create}) #{currentExecutionId} {Name} ({Id})");
#endif

            var client = Client.LookupByConnectionId.Values.FirstOrDefault(c => c.User.Id == Id);
            client?.LogAndDisconnect(default, "User.Save");

            if (Options.Instance.PlayerDatabase.KillServerOnConcurrencyException)
            {
                ServerContext.DispatchUnhandledException(
                    new Exception("Failed to save user, shutting down to prevent rollbacks!")
                );
            }

            return UserSaveResult.DatabaseFailure;
        }
        catch (Exception ex)
        {
            ApplicationContext.Context.Value?.Logger.LogError(ex, "Failed to save user: " + Name);

#if DIAGNOSTIC
            ApplicationContext.Context.Value?.Logger.LogDebug($"DBOP-C Save({playerContext}, {force}, {create}) #{currentExecutionId} {Name} ({Id})");
#endif

            var client = Client.LookupByConnectionId.Values.FirstOrDefault(c => c.User.Id == Id);
            client?.LogAndDisconnect(default, "User.Save");

            if (Options.Instance.PlayerDatabase.KillServerOnConcurrencyException)
            {
                ServerContext.DispatchUnhandledException(
                    new Exception("Failed to save user, shutting down to prevent rollbacks!")
                );
            }

            return UserSaveResult.DatabaseFailure;
        }
        finally
        {
            if (lockTaken)
            {
                createdContext?.Dispose();
                Monitor.Exit(mSavingLock);
            }
        }
    }

    [return: NotNullIfNotNull(nameof(user))]
    public static User? PostLoad(User? user, PlayerContext? playerContext = default)
    {
        if (user == default)
        {
            return user;
        }

        if (playerContext == default)
        {
            using var context = DbInterface.CreatePlayerContext();
            if (context == default)
            {
                throw new InvalidOperationException();
            }

            // ReSharper disable once TailRecursiveCall
            return PostLoad(user, context);
        }

        var entityEntry = playerContext.Users.Attach(user);
        entityEntry.Collection(u => u.Variables).Load();

        return user;
    }

    public static bool TryFind(LookupKey lookupKey, [NotNullWhen(true)] out User? user)
    {
        using var playerContext = DbInterface.CreatePlayerContext();
        return TryFind(lookupKey, playerContext, out user);
    }

    public static bool TryFetch(LookupKey lookupKey, [NotNullWhen(true)] out User? user) => TryFetch(lookupKey, out user, out _);

    public static bool TryFetch(LookupKey lookupKey, [NotNullWhen(true)] out User? user, out Client? client)
    {
        if (lookupKey.IsInvalid)
        {
            user = default;
            client = default;
            return false;
        }

        if (lookupKey.IsId)
        {
            client = Client.Instances.Find(queryClient => lookupKey.Id == queryClient?.User?.Id);
            user = client?.User ?? FindById(lookupKey.Id);
            return user != default;
        }

        client = Client.Instances.Find(queryClient => Entity.CompareName(lookupKey.Name, queryClient?.User?.Name));
        user = client?.User ?? Find(lookupKey.Name);
        return user != default;
    }

    public bool TryLoadAvatarName(
        [NotNullWhen(true)] out Player? playerWithAvatar,
        [NotNullWhen(true)] out string? avatarName,
        out bool isFace
    )
    {
        foreach (var player in Players)
        {
            if (!player.TryLoadAvatarName(out avatarName, out isFace) || string.IsNullOrWhiteSpace(avatarName))
            {
                continue;
            }

            playerWithAvatar = player;
            return true;
        }

        avatarName = null;
        isFace = false;
        playerWithAvatar = null;
        return false;
    }

    public static bool TryAuthenticate(string username, string password, [NotNullWhen(true)] out User? user)
    {
        user = FindOnline(username);
        if (user != default)
        {
            var hashedPassword = SaltPasswordHash(password, user.Salt);
            if (string.Equals(user.Password, hashedPassword, StringComparison.Ordinal))
            {
                return true;
            }

            user = default;
            return false;
        }

        try
        {
            using var context = DbInterface.CreatePlayerContext();
            var salt = GetUserSalt(username);
            if (!string.IsNullOrWhiteSpace(salt))
            {
                var pass = SaltPasswordHash(password, salt);
                user = QueryUserByNameAndPasswordShallow(context, username, pass);
            }
        }
        catch (Exception exception)
        {
            ApplicationContext.Context.Value?.Logger.LogError(exception, "Failed to authenticate {Username}", username);
        }

        return user != default;
    }

    public static bool TryLogin(
        string username,
        string passwordClientHash,
        [NotNullWhen(true)] out User? user,
        out LoginFailureReason failureReason
    )
    {
        user = FindOnline(username);
        failureReason = default;

        if (user != null)
        {
            var hashedPassword = SaltPasswordHash(passwordClientHash, user.Salt);
            if (!string.Equals(user.Password, hashedPassword, StringComparison.Ordinal))
            {
                ApplicationContext.Context.Value?.Logger.LogDebug($"Login to {username} failed due invalid credentials");
                user = default;
                failureReason = new LoginFailureReason(LoginFailureType.InvalidCredentials);
                return false;
            }

            var result = user.Save();
            if (result != UserSaveResult.Completed)
            {
                ApplicationContext.Context.Value?.Logger.LogError($"Login to {username} failed due to pre-logged in User save failure: {result}");
                user = default;
                failureReason = new LoginFailureReason(LoginFailureType.ServerError);
                return false;
            }

            user = PostLoad(user);
            if (user != default)
            {
                return true;
            }

            ApplicationContext.Context.Value?.Logger.LogError($"Login to {username} failed due to {nameof(PostLoad)}() returning null.");
            user = default;
            failureReason = new LoginFailureReason(LoginFailureType.ServerError);
            return false;
        }

        try
        {
            using var context = DbInterface.CreatePlayerContext();
            var salt = GetUserSalt(username);
            if (string.IsNullOrWhiteSpace(salt))
            {
                if (UserExists(username))
                {
                    ApplicationContext.Context.Value?.Logger.LogError($"Login to {username} failed because the salt is empty.");
                    failureReason = new LoginFailureReason(LoginFailureType.ServerError);
                }
                else
                {
                    failureReason = new LoginFailureReason(LoginFailureType.InvalidCredentials);
                }

                user = default;
                return false;
            }

            var saltedPasswordHash = SaltPasswordHash(passwordClientHash, salt);
            var queriedUser = QueryUserByNameAndPasswordShallow(context, username, saltedPasswordHash);
            user = PostLoad(queriedUser, context);

            if (user == default)
            {
                failureReason = new LoginFailureReason(LoginFailureType.InvalidCredentials);
                return false;
            }

            failureReason = default;
            return true;
        }
        catch (Exception exception)
        {
            ApplicationContext.Context.Value?.Logger.LogError(exception, $"Login to {username} failed due to an exception");
            user = default;
            failureReason = new LoginFailureReason(LoginFailureType.ServerError);
            return false;
        }
    }

    public static bool TryFind(LookupKey lookupKey, PlayerContext playerContext, [NotNullWhen(true)] out User? user)
    {
        if (lookupKey.IsId)
        {
            return TryFindById(lookupKey.Id, playerContext, out user);
        }

        if (lookupKey.IsName)
        {
            return TryFindByName(lookupKey.Name, playerContext, out user);
        }

        throw new InvalidOperationException($"Lookup key has neither an id nor a name: '{lookupKey}'");
    }

    public static bool TryFindByName(string username, [NotNullWhen(true)] out User? user)
    {
        using var playerContext = DbInterface.CreatePlayerContext();
        return TryFindByName(username, playerContext, out user);
    }

    public static bool TryFindByName(string username, PlayerContext playerContext, [NotNullWhen(true)] out User? user)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            user = default;
            return false;
        }

        user = FindOnline(username);

        if (user != null)
        {
            return true;
        }

        try
        {
            using var context = DbInterface.CreatePlayerContext();
            var queriedUser = QueryUserByNameShallow(context, username);
            if (queriedUser != default)
            {
                user = queriedUser;
                return true;
            }
        }
        catch (Exception exception)
        {
            ApplicationContext.Context.Value?.Logger.LogError(exception, $"Failed to find user by name '{username}'");
        }

        return false;
    }

    public static bool TryFindById(Guid userId, [NotNullWhen(true)] out User? user)
    {
        using var playerContext = DbInterface.CreatePlayerContext();
        return TryFindById(userId, playerContext, out user);
    }

    public static bool TryFindById(Guid userId, PlayerContext playerContext, [NotNullWhen(true)] out User? user)
    {
        if (userId == default)
        {
            user = default;
            return false;
        }

        user = FindOnline(userId);

        if (user != null)
        {
            return true;
        }

        try
        {
            user = QueryUserByIdShallow(playerContext, userId);
        }
        catch (Exception exception)
        {
            ApplicationContext.Context.Value?.Logger.LogError(exception, $"Failed to find user by id '{userId}'");
        }

        return user != default;
    }

    public static User? FindById(Guid userId) => TryFindById(userId, out var user) ? user : default;

    public static User Find(string username)
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
            using var context = DbInterface.CreatePlayerContext();
            var queriedUser = QueryUserByNameShallow(context, username);
            return queriedUser;
        }
        catch (Exception exception)
        {
            ApplicationContext.Context.Value?.Logger.LogError(exception, "Failed to find {Username}", username);
            return null;
        }
    }

    public static User FindFromNameOrEmail(string nameOrEmail)
    {
        using var playerContext = DbInterface.CreatePlayerContext();
        return FindByNameOrEmail(nameOrEmail, playerContext);
    }

    public static User FindByNameOrEmail(string nameOrEmail, PlayerContext playerContext)
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
            var queriedUser = QueryUserByNameOrEmailShallow(playerContext, nameOrEmail);
            return queriedUser;
        }
        catch (Exception exception)
        {
            ApplicationContext.Context.Value?.Logger.LogError(
                exception,
                "Failed to load user by name or email '{UsernameOrEmail}'",
                nameOrEmail
            );
            return null;
        }
    }

    public static User FindByEmail(string email)
    {
        using var context = DbInterface.CreatePlayerContext();
        return FindByEmail(email);
    }

    public static User FindByEmail(string email, PlayerContext playerContext)
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
            return QueryUserByEmailShallow(playerContext, email);
        }
        catch (Exception exception)
        {
            ApplicationContext.Context.Value?.Logger.LogError(
                exception,
                "Failed to load user for email address '{Email}'",
                email
            );
            return null;
        }
    }

    public static string? GetUserSalt(string username) => TryGetSalt(username, out var salt) ? salt : null;

    public static bool TryGetSalt(string username, [NotNullWhen(true)] out string? salt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username, nameof(username));

        if (TryFindOnline(username, out var user))
        {
            salt = user.Salt;
            if (!string.IsNullOrWhiteSpace(salt))
            {
                return true;
            }
        }

        try
        {
            using var context = DbInterface.CreatePlayerContext();
            salt = QuerySaltByName(context, username);
            return !string.IsNullOrWhiteSpace(salt);
        }
        catch (Exception exception)
        {
            ApplicationContext.Context.Value?.Logger.LogError(
                exception,
                "Error getting salt for '{Username}'",
                username
            );
            salt = null;
            return false;
        }
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
        catch (Exception exception)
        {
            ApplicationContext.Context.Value?.Logger.LogError(
                exception,
                "Error checking if user known by '{NameOrEmail}' exists",
                nameOrEmail
            );
            return false;
        }
    }

    /// <summary>
    /// </summary>
    public void Update()
    {
        // ReSharper disable once InvertIf
        if (UpdatedVariables.Count > 0)
        {
            if (Save() == UserSaveResult.DatabaseFailure)
            {
                var client = Client.LookupByConnectionId.Values.FirstOrDefault(c => c.User.Id == Id);
                client?.LogAndDisconnect(default, "User.Save");
            }

            UpdatedVariables.Clear();
        }
    }

    /// <summary>
    ///     Returns a variable object given a user variable id
    /// </summary>
    /// <param name="id">Variable id</param>
    /// <param name="createIfNull">Creates this variable for the user if it hasn't been set yet</param>
    /// <returns></returns>
    public Variable GetVariable(Guid id, bool createIfNull = false)
    {
        foreach (var v in Variables)
        {
            if (v.VariableId == id)
            {
                return v;
            }
        }

        if (createIfNull)
        {
            return CreateVariable(id);
        }

        return null;
    }

    /// <summary>
    ///     Creates a variable for this user with a given id if it doesn't already exist
    /// </summary>
    /// <param name="id">Variablke id</param>
    /// <returns></returns>
    private Variable CreateVariable(Guid id)
    {
        if (UserVariableDescriptor.Get(id) == null)
        {
            return null;
        }

        var variable = new UserVariable(id);
        Variables.Add(variable);

        return variable;
    }

    /// <summary>
    ///     Gets the value of a account variable given a variable id
    /// </summary>
    /// <param name="id">Variable id</param>
    /// <returns></returns>
    public VariableValue GetVariableValue(Guid id)
    {
        var v = GetVariable(id, true);

        if (v == null)
        {
            return new VariableValue();
        }

        return v.Value;
    }

    /// <summary>
    ///     Starts all common events with a specified trigger for any character online of this account
    /// </summary>
    /// <param name="trigger">The common event trigger to run</param>
    /// <param name="command">The command which started this common event</param>
    /// <param name="param">Common event parameter</param>
    public void StartCommonEventsWithTriggerForAll(CommonEventTrigger trigger, string command, string param)
    {
        foreach (var plyr in Players)
        {
            if (Player.FindOnline(plyr.Id) != null)
            {
                plyr.StartCommonEventsWithTrigger(trigger, command, param);
            }
        }
    }

    public static bool TryRegister(
        RegistrationActor actor,
        string username,
        string email,
        string password,
        [NotNullWhen(false)] out string error,
        [NotNullWhen(true)] out User user
    )
    {
        error = default;
        user = default;

        if (Options.Instance.BlockClientRegistrations)
        {
            error = Strings.Account.RegistrationsBlocked;
            return false;
        }

        if (!FieldChecking.IsValidUsername(username, Strings.Regex.Username))
        {
            error = Strings.Account.InvalidName;
            return false;
        }

        if (!FieldChecking.IsWellformedEmailAddress(email, Strings.Regex.Email))
        {
            error = Strings.Account.InvalidEmail;
            return false;
        }

        if (Ban.IsBanned(actor.IpAddress, out var message))
        {
            error = message;
            return false;
        }

        if (UserExists(username))
        {
            error = Strings.Account.AccountAlreadyExists;
            return false;
        }

        if (UserExists(email))
        {
            error = Strings.Account.EmailExists;
            return false;
        }

        UserActivityHistory.LogActivity(
            Guid.Empty,
            Guid.Empty,
            actor.IpAddress.ToString(),
            actor.PeerType,
            UserActivityHistory.UserAction.Create,
            string.Empty
        );

        if (DbInterface.TryRegister(username, email, password, out user))
        {
            return true;
        }

        error = Strings.Account.UnknownErrorWhileSaving;
        return false;
    }

    public sealed record RegistrationActor(IPAddress IpAddress, UserActivityHistory.PeerType PeerType);

    #region Instance Variables

    [NotMapped]
    [ApiVisibility(ApiVisibility.Restricted | ApiVisibility.Private)]
    public bool IsBanned => Ban != null;

    [NotMapped]
    [ApiVisibility(ApiVisibility.Restricted | ApiVisibility.Private)]
    public bool IsMuted => Mute != null;

    [ApiVisibility(ApiVisibility.Restricted)]
    public Ban Ban
    {
        get => UserBan ?? IpBan;
        set => UserBan = value;
    }

    [ApiVisibility(ApiVisibility.Restricted)]
    public Mute Mute
    {
        get => UserMute ?? IpMute;
        set => UserMute = value;
    }

    [NotMapped] public Ban IpBan { get; set; }

    [NotMapped] public Mute IpMute { get; set; }

    [ApiVisibility(ApiVisibility.Restricted)]
    [NotMapped]
    public Ban UserBan { get; set; }

    [ApiVisibility(ApiVisibility.Restricted)]
    [NotMapped]
    public Mute UserMute { get; set; }

    #endregion

    #region Listing

    public static int Count()
    {
        using (var context = DbInterface.CreatePlayerContext())
        {
            return context.Users.Count();
        }
    }

    public static IList<User> List(
        string query,
        string sortBy,
        SortDirection sortDirection,
        int skip,
        int take,
        out int total
    )
    {
        try
        {
            using (var context = DbInterface.CreatePlayerContext())
            {
                foreach (var user in Player.OnlinePlayers.Select(p => p.User))
                {
                    if (user != default)
                    {
                        context.Entry(user).State = EntityState.Unchanged;
                    }
                }

                var compiledQuery = string.IsNullOrWhiteSpace(query)
                    ? context.Users.Include(p => p.Ban).Include(p => p.Mute) : context.Users.Where(
                        u => EF.Functions.Like(u.Name, $"%{query}%") || EF.Functions.Like(u.Email, $"%{query}%")
                    );

                total = compiledQuery.Count();

                switch (sortBy?.ToLower() ?? "")
                {
                    case "email":
                        compiledQuery = sortDirection == SortDirection.Ascending
                            ? compiledQuery.OrderBy(u => u.Email.ToUpper())
                            : compiledQuery.OrderByDescending(u => u.Email.ToUpper());
                        break;
                    case "registrationdate":
                        compiledQuery = sortDirection == SortDirection.Ascending
                            ? compiledQuery.OrderBy(u => u.RegistrationDate)
                            : compiledQuery.OrderByDescending(u => u.RegistrationDate);
                        break;
                    case "playtime":
                        compiledQuery = sortDirection == SortDirection.Ascending
                            ? compiledQuery.OrderBy(u => u.PlayTimeSeconds)
                            : compiledQuery.OrderByDescending(u => u.PlayTimeSeconds);
                        break;
                    case "name":
                    default:
                        compiledQuery = sortDirection == SortDirection.Ascending
                            ? compiledQuery.OrderBy(u => u.Name.ToUpper())
                            : compiledQuery.OrderByDescending(u => u.Name.ToUpper());
                        break;
                }

                var users = compiledQuery.Skip(skip).Take(take).AsTracking().ToList();
                return users;
            }
        }
        catch (Exception exception)
        {
            ApplicationContext.Context.Value?.Logger.LogError(exception, "Error listing users");
            total = 0;
            return null;
        }
    }

    #endregion

    #region Compiled Queries

    private static readonly Func<PlayerContext, string, User> QueryUserByNameShallow = EF.CompileQuery(
            // ReSharper disable once SpecifyStringComparison
            (PlayerContext context, string username) => context.Users.Where(u => u.Name == username)
                .Include(p => p.Ban)
                .Include(p => p.Mute)
                .Include(p => p.Players)
                .AsSplitQuery()
                .FirstOrDefault()
        ) ??
        throw new InvalidOperationException();

    private static readonly Func<PlayerContext, string, User> QueryUserByNameOrEmailShallow = EF.CompileQuery(
            // ReSharper disable once SpecifyStringComparison
            (PlayerContext context, string usernameOrEmail) => context.Users
                .Where(u => u.Name == usernameOrEmail || u.Email == usernameOrEmail)
                .Include(p => p.Ban)
                .Include(p => p.Mute)
                .Include(p => p.Players)
                .AsSplitQuery()
                .FirstOrDefault()
        ) ??
        throw new InvalidOperationException();

    private static readonly Func<PlayerContext, string, string, User> QueryUserByNameAndPasswordShallow =
        EF.CompileQuery(
            // ReSharper disable once SpecifyStringComparison
            (PlayerContext context, string username, string password) => context.Users
                .Where(u => u.Name == username && u.Password == password)
                .Include(p => p.Ban)
                .Include(p => p.Mute)
                .Include(p => p.Players)
                .AsSplitQuery()
                .FirstOrDefault()
        ) ??
        throw new InvalidOperationException();

    private static readonly Func<PlayerContext, Guid, User> QueryUserByIdShallow = EF.CompileQuery(
            (PlayerContext context, Guid id) => context.Users.Where(u => u.Id == id)
                .Include(p => p.Ban)
                .Include(p => p.Mute)
                .Include(p => p.Players)
                .AsSplitQuery()
                .FirstOrDefault()
        ) ??
        throw new InvalidOperationException();

    private static readonly Func<PlayerContext, string, bool> AnyUserByNameOrEmail = EF.CompileQuery(
        // ReSharper disable once SpecifyStringComparison
        (PlayerContext context, string nameOrEmail) =>
            context.Users.Where(u => u.Name == nameOrEmail || u.Email == nameOrEmail).Any()
    );

    private static readonly Func<PlayerContext, string, string> QuerySaltByName = EF.CompileQuery(
        // ReSharper disable once SpecifyStringComparison
        (PlayerContext context, string userName) =>
            context.Users.Where(u => u.Name == userName).Select(u => u.Salt).FirstOrDefault()
    );

    private static readonly Func<PlayerContext, string, User> QueryUserByEmailShallow = EF.CompileQuery(
            // ReSharper disable once SpecifyStringComparison
            (PlayerContext context, string email) => context.Users.Where(u => u.Email == email)
                .Include(p => p.Ban)
                .Include(p => p.Mute)
                .Include(p => p.Players)
                .AsSplitQuery()
                .FirstOrDefault()
    ) ??
    throw new InvalidOperationException();

    #endregion
}
