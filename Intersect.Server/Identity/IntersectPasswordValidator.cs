using System.Diagnostics;
using Intersect.Server.Database.PlayerData;
using Microsoft.AspNetCore.Identity;

namespace Intersect.Server.Identity;

public sealed class IntersectPasswordValidator : IPasswordValidator<User>
{
    private readonly IPasswordHasher<User> _passwordHasher;

    public IntersectPasswordValidator(IPasswordHasher<User> passwordHasher)
    {
        _passwordHasher = passwordHasher;
    }

    public Task<IdentityResult> ValidateAsync(UserManager<User> manager, User user, string password)
    {
        var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.Password, password);

        var identityResult = passwordVerificationResult switch
        {
            PasswordVerificationResult.Failed => IdentityResult.Success,
            PasswordVerificationResult.Success => IdentityResult.Failed(),
            PasswordVerificationResult.SuccessRehashNeeded => IdentityResult.Failed(),
            _ => throw new UnreachableException()
        };

        return Task.FromResult(identityResult);
    }
}