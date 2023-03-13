using System.Diagnostics;
using Intersect.Server.Database.PlayerData;
using Microsoft.AspNetCore.Identity;

namespace Intersect.Server.Identity;

public sealed class IntersectUserStore
    : IUserStore<User>, IUserEmailStore<User>, IUserPasswordStore<User>, IUserPhoneNumberStore<User>
{
    private readonly PlayerContext _playerContext;

    public IntersectUserStore(PlayerContext playerContext)
    {
        _playerContext = playerContext;
    }

    public Task SetEmailAsync(User user, string email, CancellationToken cancellationToken)
    {
        user.Email = email;
        return user.SaveAsync(playerContext: _playerContext, cancellationToken: cancellationToken);
    }

    public Task<string> GetEmailAsync(User user, CancellationToken cancellationToken) => Task.FromResult(user.Email);

    public Task<bool> GetEmailConfirmedAsync(User user, CancellationToken cancellationToken) =>
        // TODO: Email confirmation
        Task.FromResult(true);

    public Task SetEmailConfirmedAsync(User user, bool confirmed, CancellationToken cancellationToken) =>
        // TODO: Email confirmation
        Task.CompletedTask;

    public Task<User> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        var user = User.FindByEmail(normalizedEmail, _playerContext);
        return Task.FromResult(user);
    }

    public Task<string> GetNormalizedEmailAsync(User user, CancellationToken cancellationToken) =>
        Task.FromResult(user.Email);

    public Task SetNormalizedEmailAsync(User user, string normalizedEmail, CancellationToken cancellationToken) =>
        SetEmailAsync(user, normalizedEmail, cancellationToken);

    public Task SetPasswordHashAsync(User user, string passwordHash, CancellationToken cancellationToken)
    {
        user.Password = passwordHash;
        return user.SaveAsync(playerContext: _playerContext, cancellationToken: cancellationToken);
    }

    public Task<string> GetPasswordHashAsync(User user, CancellationToken cancellationToken) =>
        Task.FromResult(user.Password);

    public Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken) => Task.FromResult(true);

    public Task SetPhoneNumberAsync(User user, string phoneNumber, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task<string> GetPhoneNumberAsync(User user, CancellationToken cancellationToken) =>
        Task.FromResult(string.Empty);

    public Task<bool> GetPhoneNumberConfirmedAsync(User user, CancellationToken cancellationToken) =>
        Task.FromResult(true);

    public Task SetPhoneNumberConfirmedAsync(User user, bool confirmed, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public void Dispose() => _playerContext.Dispose();

    public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken) =>
        Task.FromResult(user.Id.ToString());

    public Task<string> GetUserNameAsync(User user, CancellationToken cancellationToken) => Task.FromResult(user.Name);

    public Task SetUserNameAsync(User user, string userName, CancellationToken cancellationToken)
    {
        user.Name = userName;
        return user.SaveAsync(playerContext: _playerContext, cancellationToken: cancellationToken);
    }

    public Task<string> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken) =>
        Task.FromResult(user.Name);

    public Task SetNormalizedUserNameAsync(User user, string normalizedName, CancellationToken cancellationToken)
    {
        user.Name = normalizedName;
        return user.SaveAsync(playerContext: _playerContext, cancellationToken: cancellationToken);
    }

    public Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
    {
        Debugger.Break();
        throw new NotImplementedException();
    }

    public Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
    {
        Debugger.Break();
        throw new NotImplementedException();
    }

    public Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
    {
        Debugger.Break();
        throw new NotImplementedException();
    }

    public Task<User> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        var id = Guid.Parse(userId);
        var user = User.FindById(id);
        return Task.FromResult(user);
    }

    public Task<User> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        var user = User.FindByNameOrEmail(normalizedUserName, _playerContext);
        return Task.FromResult(user);
    }
}