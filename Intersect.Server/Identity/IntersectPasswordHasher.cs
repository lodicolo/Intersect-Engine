using System.Security.Cryptography;
using System.Text;
using Intersect.Server.Database.PlayerData;
using Microsoft.AspNetCore.Identity;

namespace Intersect.Server.Identity;

public sealed class IntersectPasswordHasher : IPasswordHasher<User>
{
    public string HashPassword(User user, string password)
    {
        var hashData = SHA256.HashData(Encoding.UTF8.GetBytes(password.ToUpperInvariant() + user.Salt));
        return BitConverter.ToString(hashData).Replace("-", string.Empty);
    }

    public PasswordVerificationResult VerifyHashedPassword(User user, string hashedPassword, string providedPassword)
    {
        var hashedProvidedPassword = HashPassword(user, providedPassword);
        return string.Equals(hashedPassword, hashedProvidedPassword, StringComparison.Ordinal)
            ? PasswordVerificationResult.Success : PasswordVerificationResult.Failed;
    }
}