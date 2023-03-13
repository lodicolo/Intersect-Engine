using System.Diagnostics;
using System.Security.Claims;
using Intersect.Server.Database.PlayerData;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Intersect.Server.Identity;

public sealed class IntersectUserManager : UserManager<User>
{
    public IntersectUserManager(
        IUserStore<User> store,
        IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<User> passwordHasher,
        IEnumerable<IUserValidator<User>> userValidators,
        IEnumerable<IPasswordValidator<User>> passwordValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        IServiceProvider services,
        ILogger<UserManager<User>> logger
    ) : base(
        store,
        optionsAccessor,
        passwordHasher,
        userValidators,
        passwordValidators,
        keyNormalizer,
        errors,
        services,
        logger
    )
    {
    }

    public override string GetUserName(ClaimsPrincipal principal)
    {
        var userName = base.GetUserName(principal);
        return userName;
    }

    public override string GetUserId(ClaimsPrincipal principal)
    {
        var userId = base.GetUserId(principal);
        return userId;
    }

    public override async Task<User> GetUserAsync(ClaimsPrincipal principal)
    {
        var user = await base.GetUserAsync(principal);
        return user;
    }

    public override async Task<string> GenerateConcurrencyStampAsync(User user)
    {
        var concurrencyStamp = await base.GenerateConcurrencyStampAsync(user);
        return concurrencyStamp;
    }

    public override async Task<IdentityResult> CreateAsync(User user)
    {
        var identityResult = await base.CreateAsync(user);
        return identityResult;
    }

    public override async Task<IdentityResult> UpdateAsync(User user)
    {
        var identityResult = await base.UpdateAsync(user);
        return identityResult;
    }

    public override async Task<IdentityResult> DeleteAsync(User user)
    {
        var identityResult = await base.DeleteAsync(user);
        return identityResult;
    }

    public override async Task<User> FindByIdAsync(string userId)
    {
        var user = await base.FindByIdAsync(userId);
        return user;
    }

    public override async Task<User> FindByNameAsync(string userName)
    {
        var user = await base.FindByNameAsync(userName);
        return user;
    }

    public override async Task<IdentityResult> CreateAsync(User user, string password)
    {
        var identityResult = await base.CreateAsync(user, password);
        return identityResult;
    }

    public override async Task<string> GetUserNameAsync(User user)
    {
        var userName = await base.GetUserNameAsync(user);
        return userName;
    }

    public override async Task<IdentityResult> SetUserNameAsync(User user, string userName)
    {
        var identityResult = await base.SetUserNameAsync(user, userName);
        return identityResult;
    }

    public override async Task<string> GetUserIdAsync(User user)
    {
        var userId = await base.GetUserIdAsync(user);
        return userId;
    }

    public override async Task<bool> CheckPasswordAsync(User user, string password)
    {
        var checkPassword = await base.CheckPasswordAsync(user, password);
        return checkPassword;
    }

    public override async Task<bool> HasPasswordAsync(User user)
    {
        var hasPassword = await base.HasPasswordAsync(user);
        return hasPassword;
    }

    public override async Task<IdentityResult> AddPasswordAsync(User user, string password)
    {
        var identityResult = await base.AddPasswordAsync(user, password);
        return identityResult;
    }

    public override async Task<IdentityResult> ChangePasswordAsync(User user, string currentPassword, string newPassword)
    {
        var identityResult = await base.ChangePasswordAsync(user, currentPassword, newPassword);
        return identityResult;
    }

    public override async Task<IdentityResult> RemovePasswordAsync(User user)
    {
        var identityResult = await base.RemovePasswordAsync(user);
        return identityResult;
    }

    protected override async Task<PasswordVerificationResult> VerifyPasswordAsync(
        IUserPasswordStore<User> store,
        User user,
        string password
    )
    {
        var passwordVerificationResult = await base.VerifyPasswordAsync(store, user, password);
        return passwordVerificationResult;
    }

    public override async Task<string> GetSecurityStampAsync(User user)
    {
        var securityStamp = await base.GetSecurityStampAsync(user);
        return securityStamp;
    }

    public override async Task<IdentityResult> UpdateSecurityStampAsync(User user)
    {
        var identityResult = await base.UpdateSecurityStampAsync(user);
        return identityResult;
    }

    public override async Task<string> GeneratePasswordResetTokenAsync(User user)
    {
        var passwordResetToken = await base.GeneratePasswordResetTokenAsync(user);
        return passwordResetToken;
    }

    public override async Task<IdentityResult> ResetPasswordAsync(User user, string token, string newPassword)
    {
        var identityResult = await base.ResetPasswordAsync(user, token, newPassword);
        return identityResult;
    }

    public override async Task<User> FindByLoginAsync(string loginProvider, string providerKey)
    {
        var user = await base.FindByLoginAsync(loginProvider, providerKey);
        return user;
    }

    public override async Task<IdentityResult> RemoveLoginAsync(User user, string loginProvider, string providerKey)
    {
        var identityResult = await base.RemoveLoginAsync(user, loginProvider, providerKey);
        return identityResult;
    }

    public override async Task<IdentityResult> AddLoginAsync(User user, UserLoginInfo login)
    {
        var identityResult = await base.AddLoginAsync(user, login);
        return identityResult;
    }

    public override Task<IList<UserLoginInfo>> GetLoginsAsync(User user)
    {
        Debugger.Break();
        return base.GetLoginsAsync(user);
    }

    public override Task<IdentityResult> AddClaimAsync(User user, Claim claim)
    {
        Debugger.Break();
        return base.AddClaimAsync(user, claim);
    }

    public override Task<IdentityResult> AddClaimsAsync(User user, IEnumerable<Claim> claims)
    {
        Debugger.Break();
        return base.AddClaimsAsync(user, claims);
    }

    public override Task<IdentityResult> ReplaceClaimAsync(User user, Claim claim, Claim newClaim)
    {
        Debugger.Break();
        return base.ReplaceClaimAsync(user, claim, newClaim);
    }

    public override Task<IdentityResult> RemoveClaimAsync(User user, Claim claim)
    {
        Debugger.Break();
        return base.RemoveClaimAsync(user, claim);
    }

    public override Task<IdentityResult> RemoveClaimsAsync(User user, IEnumerable<Claim> claims)
    {
        Debugger.Break();
        return base.RemoveClaimsAsync(user, claims);
    }

    public override Task<IList<Claim>> GetClaimsAsync(User user)
    {
        Debugger.Break();
        return base.GetClaimsAsync(user);
    }

    public override Task<IdentityResult> AddToRoleAsync(User user, string role)
    {
        Debugger.Break();
        return base.AddToRoleAsync(user, role);
    }

    public override Task<IdentityResult> AddToRolesAsync(User user, IEnumerable<string> roles)
    {
        Debugger.Break();
        return base.AddToRolesAsync(user, roles);
    }

    public override Task<IdentityResult> RemoveFromRoleAsync(User user, string role)
    {
        Debugger.Break();
        return base.RemoveFromRoleAsync(user, role);
    }

    public override Task<IdentityResult> RemoveFromRolesAsync(User user, IEnumerable<string> roles)
    {
        Debugger.Break();
        return base.RemoveFromRolesAsync(user, roles);
    }

    public override Task<IList<string>> GetRolesAsync(User user)
    {
        Debugger.Break();
        return base.GetRolesAsync(user);
    }

    public override Task<bool> IsInRoleAsync(User user, string role)
    {
        Debugger.Break();
        return base.IsInRoleAsync(user, role);
    }

    public override Task<string> GetEmailAsync(User user)
    {
        return base.GetEmailAsync(user);
    }

    public override Task<IdentityResult> SetEmailAsync(User user, string email)
    {
        Debugger.Break();
        return base.SetEmailAsync(user, email);
    }

    public override Task<User> FindByEmailAsync(string email)
    {
        Debugger.Break();
        return base.FindByEmailAsync(email);
    }

    public override Task UpdateNormalizedEmailAsync(User user)
    {
        return base.UpdateNormalizedEmailAsync(user);
    }

    public override Task<string> GenerateEmailConfirmationTokenAsync(User user)
    {
        Debugger.Break();
        return base.GenerateEmailConfirmationTokenAsync(user);
    }

    public override async Task<IdentityResult> ConfirmEmailAsync(User user, string token)
    {
        var identityResult = await base.ConfirmEmailAsync(user, token);
        return identityResult;
    }

    public override async Task<bool> IsEmailConfirmedAsync(User user)
    {
        var isEmailConfirmed = await base.IsEmailConfirmedAsync(user);
        return isEmailConfirmed;
    }

    public override Task<string> GenerateChangeEmailTokenAsync(User user, string newEmail)
    {
        Debugger.Break();
        return base.GenerateChangeEmailTokenAsync(user, newEmail);
    }

    public override Task<IdentityResult> ChangeEmailAsync(User user, string newEmail, string token)
    {
        Debugger.Break();
        return base.ChangeEmailAsync(user, newEmail, token);
    }

    public override async Task<string> GetPhoneNumberAsync(User user)
    {
        var phoneNumber = await base.GetPhoneNumberAsync(user);
        return phoneNumber;
    }

    public override Task<IdentityResult> SetPhoneNumberAsync(User user, string phoneNumber)
    {
        Debugger.Break();
        return base.SetPhoneNumberAsync(user, phoneNumber);
    }

    public override Task<IdentityResult> ChangePhoneNumberAsync(User user, string phoneNumber, string token)
    {
        Debugger.Break();
        return base.ChangePhoneNumberAsync(user, phoneNumber, token);
    }

    public override Task<bool> IsPhoneNumberConfirmedAsync(User user)
    {
        Debugger.Break();
        return base.IsPhoneNumberConfirmedAsync(user);
    }

    public override Task<string> GenerateChangePhoneNumberTokenAsync(User user, string phoneNumber)
    {
        Debugger.Break();
        return base.GenerateChangePhoneNumberTokenAsync(user, phoneNumber);
    }

    public override Task<bool> VerifyChangePhoneNumberTokenAsync(User user, string token, string phoneNumber)
    {
        Debugger.Break();
        return base.VerifyChangePhoneNumberTokenAsync(user, token, phoneNumber);
    }

    public override async Task<bool> VerifyUserTokenAsync(User user, string tokenProvider, string purpose, string token)
    {
        var verifiedUserToken = await base.VerifyUserTokenAsync(user, tokenProvider, purpose, token);
        return verifiedUserToken;
    }

    public override Task<string> GenerateUserTokenAsync(User user, string tokenProvider, string purpose)
    {
        Debugger.Break();
        return base.GenerateUserTokenAsync(user, tokenProvider, purpose);
    }

    public override void RegisterTokenProvider(string providerName, IUserTwoFactorTokenProvider<User> provider) =>
        base.RegisterTokenProvider(providerName, provider);

    public override Task<IList<string>> GetValidTwoFactorProvidersAsync(User user)
    {
        Debugger.Break();
        return base.GetValidTwoFactorProvidersAsync(user);
    }

    public override Task<bool> VerifyTwoFactorTokenAsync(User user, string tokenProvider, string token)
    {
        Debugger.Break();
        return base.VerifyTwoFactorTokenAsync(user, tokenProvider, token);
    }

    public override Task<string> GenerateTwoFactorTokenAsync(User user, string tokenProvider)
    {
        Debugger.Break();
        return base.GenerateTwoFactorTokenAsync(user, tokenProvider);
    }

    public override Task<bool> GetTwoFactorEnabledAsync(User user)
    {
        Debugger.Break();
        return base.GetTwoFactorEnabledAsync(user);
    }

    public override Task<IdentityResult> SetTwoFactorEnabledAsync(User user, bool enabled)
    {
        Debugger.Break();
        return base.SetTwoFactorEnabledAsync(user, enabled);
    }

    public override Task<bool> IsLockedOutAsync(User user)
    {
        Debugger.Break();
        return base.IsLockedOutAsync(user);
    }

    public override Task<IdentityResult> SetLockoutEnabledAsync(User user, bool enabled)
    {
        Debugger.Break();
        return base.SetLockoutEnabledAsync(user, enabled);
    }

    public override Task<bool> GetLockoutEnabledAsync(User user)
    {
        Debugger.Break();
        return base.GetLockoutEnabledAsync(user);
    }

    public override Task<DateTimeOffset?> GetLockoutEndDateAsync(User user)
    {
        Debugger.Break();
        return base.GetLockoutEndDateAsync(user);
    }

    public override Task<IdentityResult> SetLockoutEndDateAsync(User user, DateTimeOffset? lockoutEnd)
    {
        Debugger.Break();
        return base.SetLockoutEndDateAsync(user, lockoutEnd);
    }

    public override Task<IdentityResult> AccessFailedAsync(User user)
    {
        Debugger.Break();
        return base.AccessFailedAsync(user);
    }

    public override Task<IdentityResult> ResetAccessFailedCountAsync(User user)
    {
        Debugger.Break();
        return base.ResetAccessFailedCountAsync(user);
    }

    public override Task<int> GetAccessFailedCountAsync(User user)
    {
        Debugger.Break();
        return base.GetAccessFailedCountAsync(user);
    }

    public override Task<IList<User>> GetUsersForClaimAsync(Claim claim)
    {
        Debugger.Break();
        return base.GetUsersForClaimAsync(claim);
    }

    public override Task<IList<User>> GetUsersInRoleAsync(string roleName)
    {
        Debugger.Break();
        return base.GetUsersInRoleAsync(roleName);
    }

    public override Task<string> GetAuthenticationTokenAsync(User user, string loginProvider, string tokenName)
    {
        Debugger.Break();
        return base.GetAuthenticationTokenAsync(user, loginProvider, tokenName);
    }

    public override Task<IdentityResult> SetAuthenticationTokenAsync(
        User user,
        string loginProvider,
        string tokenName,
        string tokenValue
    )
    {
        Debugger.Break();
        return base.SetAuthenticationTokenAsync(user, loginProvider, tokenName, tokenValue);
    }

    public override Task<IdentityResult> RemoveAuthenticationTokenAsync(
        User user,
        string loginProvider,
        string tokenName
    )
    {
        Debugger.Break();
        return base.RemoveAuthenticationTokenAsync(user, loginProvider, tokenName);
    }

    public override Task<string> GetAuthenticatorKeyAsync(User user)
    {
        Debugger.Break();
        return base.GetAuthenticatorKeyAsync(user);
    }

    public override Task<IdentityResult> ResetAuthenticatorKeyAsync(User user)
    {
        Debugger.Break();
        return base.ResetAuthenticatorKeyAsync(user);
    }

    public override string GenerateNewAuthenticatorKey()
    {
        Debugger.Break();
        return base.GenerateNewAuthenticatorKey();
    }

    public override Task<IEnumerable<string>> GenerateNewTwoFactorRecoveryCodesAsync(User user, int number)
    {
        Debugger.Break();
        return base.GenerateNewTwoFactorRecoveryCodesAsync(user, number);
    }

    protected override string CreateTwoFactorRecoveryCode()
    {
        Debugger.Break();
        return base.CreateTwoFactorRecoveryCode();
    }

    public override Task<IdentityResult> RedeemTwoFactorRecoveryCodeAsync(User user, string code)
    {
        Debugger.Break();
        return base.RedeemTwoFactorRecoveryCodeAsync(user, code);
    }

    public override Task<int> CountRecoveryCodesAsync(User user)
    {
        Debugger.Break();
        return base.CountRecoveryCodesAsync(user);
    }

    protected override void Dispose(bool disposing) => base.Dispose(disposing);

    public override Task<byte[]> CreateSecurityTokenAsync(User user)
    {
        Debugger.Break();
        return base.CreateSecurityTokenAsync(user);
    }

    protected override Task<IdentityResult> UpdatePasswordHash(User user, string newPassword, bool validatePassword)
    {
        Debugger.Break();
        return base.UpdatePasswordHash(user, newPassword, validatePassword);
    }

    protected override async Task<IdentityResult> UpdateUserAsync(User user)
    {
        var identityResult = await base.UpdateUserAsync(user);
        return identityResult;
    }

    public override bool Equals(object obj)
    {
        Debugger.Break();
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        Debugger.Break();
        return base.GetHashCode();
    }

    public override string ToString()
    {
        Debugger.Break();
        return base.ToString();
    }
}