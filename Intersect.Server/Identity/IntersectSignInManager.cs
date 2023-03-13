using System.Diagnostics;
using System.Security.Claims;
using Intersect.Server.Database.PlayerData;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Intersect.Server.Identity;

public sealed class IntersectSignInManager : SignInManager<User>
{
    public IntersectSignInManager(
        UserManager<User> userManager,
        IHttpContextAccessor contextAccessor,
        IUserClaimsPrincipalFactory<User> claimsFactory,
        IOptions<IdentityOptions> optionsAccessor,
        ILogger<SignInManager<User>> logger,
        IAuthenticationSchemeProvider schemes,
        IUserConfirmation<User> confirmation
    ) : base(
        userManager,
        contextAccessor,
        claimsFactory,
        optionsAccessor,
        logger,
        schemes,
        confirmation
    )
    {
    }

    public override ILogger Logger { get; set; }

    public override async Task<ClaimsPrincipal> CreateUserPrincipalAsync(User user)
    {
        var claimsPrincipal = await base.CreateUserPrincipalAsync(user);
        return claimsPrincipal;
    }

    public override bool IsSignedIn(ClaimsPrincipal principal)
    {
        var isSignedIn = base.IsSignedIn(principal);
        return isSignedIn;
    }

    public override async Task<bool> CanSignInAsync(User user)
    {
        var canSignIn = await base.CanSignInAsync(user);
        return canSignIn;
    }

    public override Task RefreshSignInAsync(User user)
    {
        Debugger.Break();
        return base.RefreshSignInAsync(user);
    }

    public override Task SignInAsync(User user, bool isPersistent, string authenticationMethod = null)
    {
        Debugger.Break();
        return base.SignInAsync(user, isPersistent, authenticationMethod);
    }

    public override Task SignInAsync(
        User user,
        AuthenticationProperties authenticationProperties,
        string authenticationMethod = null
    )
    {
        Debugger.Break();
        return base.SignInAsync(user, authenticationProperties, authenticationMethod);
    }

    public override Task SignInWithClaimsAsync(User user, bool isPersistent, IEnumerable<Claim> additionalClaims)
    {
        return base.SignInWithClaimsAsync(user, isPersistent, additionalClaims);
    }

    public override Task SignInWithClaimsAsync(
        User user,
        AuthenticationProperties authenticationProperties,
        IEnumerable<Claim> additionalClaims
    )
    {
        return base.SignInWithClaimsAsync(user, authenticationProperties, additionalClaims);
    }

    public override Task SignOutAsync()
    {
        Debugger.Break();
        return base.SignOutAsync();
    }

    public override Task<User> ValidateSecurityStampAsync(ClaimsPrincipal principal)
    {
        Debugger.Break();
        return base.ValidateSecurityStampAsync(principal);
    }

    public override Task<User> ValidateTwoFactorSecurityStampAsync(ClaimsPrincipal principal)
    {
        Debugger.Break();
        return base.ValidateTwoFactorSecurityStampAsync(principal);
    }

    public override Task<bool> ValidateSecurityStampAsync(User user, string securityStamp)
    {
        Debugger.Break();
        return base.ValidateSecurityStampAsync(user, securityStamp);
    }

    public override Task<SignInResult> PasswordSignInAsync(
        User user,
        string password,
        bool isPersistent,
        bool lockoutOnFailure
    )
    {
        return base.PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure);
    }

    public override Task<SignInResult> PasswordSignInAsync(
        string userName,
        string password,
        bool isPersistent,
        bool lockoutOnFailure
    )
    {
        return base.PasswordSignInAsync(userName, password, isPersistent, lockoutOnFailure);
    }

    public override Task<SignInResult> CheckPasswordSignInAsync(User user, string password, bool lockoutOnFailure)
    {
        return base.CheckPasswordSignInAsync(user, password, lockoutOnFailure);
    }

    public override Task<bool> IsTwoFactorClientRememberedAsync(User user)
    {
        Debugger.Break();
        return base.IsTwoFactorClientRememberedAsync(user);
    }

    public override Task RememberTwoFactorClientAsync(User user)
    {
        Debugger.Break();
        return base.RememberTwoFactorClientAsync(user);
    }

    public override Task ForgetTwoFactorClientAsync()
    {
        Debugger.Break();
        return base.ForgetTwoFactorClientAsync();
    }

    public override async Task<SignInResult> TwoFactorRecoveryCodeSignInAsync(string recoveryCode)
    {
        var signInResult = await base.TwoFactorRecoveryCodeSignInAsync(recoveryCode);
        return signInResult;
    }

    public override async Task<SignInResult> TwoFactorAuthenticatorSignInAsync(
        string code,
        bool isPersistent,
        bool rememberClient
    )
    {
        var signInResult = await base.TwoFactorAuthenticatorSignInAsync(code, isPersistent, rememberClient);
        return signInResult;
    }

    public override async Task<SignInResult> TwoFactorSignInAsync(
        string provider,
        string code,
        bool isPersistent,
        bool rememberClient
    )
    {
        var signInResult = await base.TwoFactorSignInAsync(provider, code, isPersistent, rememberClient);
        return signInResult;
    }

    public override async Task<User> GetTwoFactorAuthenticationUserAsync()
    {
        var user = await base.GetTwoFactorAuthenticationUserAsync();
        return user;
    }

    public override async Task<SignInResult> ExternalLoginSignInAsync(
        string loginProvider,
        string providerKey,
        bool isPersistent
    )
    {
        var signInResult = await base.ExternalLoginSignInAsync(loginProvider, providerKey, isPersistent);
        return signInResult;
    }

    public override async Task<SignInResult> ExternalLoginSignInAsync(
        string loginProvider,
        string providerKey,
        bool isPersistent,
        bool bypassTwoFactor
    )
    {
        var signInResult = await base.ExternalLoginSignInAsync(
            loginProvider,
            providerKey,
            isPersistent,
            bypassTwoFactor
        );
        return signInResult;
    }

    public override async Task<IEnumerable<AuthenticationScheme>> GetExternalAuthenticationSchemesAsync()
    {
        var externalAuthenticationSchemes = (await base.GetExternalAuthenticationSchemesAsync()).ToArray();
        return externalAuthenticationSchemes;
    }

    public override async Task<ExternalLoginInfo> GetExternalLoginInfoAsync(string expectedXsrf = null)
    {
        var externalLoginInfo = await base.GetExternalLoginInfoAsync(expectedXsrf);
        return externalLoginInfo;
    }

    public override async Task<IdentityResult> UpdateExternalAuthenticationTokensAsync(ExternalLoginInfo externalLogin)
    {
        var identityResult = await base.UpdateExternalAuthenticationTokensAsync(externalLogin);
        return identityResult;
    }

    public override AuthenticationProperties ConfigureExternalAuthenticationProperties(
        string provider,
        string redirectUrl,
        string userId = null
    )
    {
        var authenticationProperties = base.ConfigureExternalAuthenticationProperties(provider, redirectUrl, userId);
        return authenticationProperties;
    }

    protected override Task<SignInResult> SignInOrTwoFactorAsync(
        User user,
        bool isPersistent,
        string loginProvider = null,
        bool bypassTwoFactor = false
    )
    {
        return base.SignInOrTwoFactorAsync(user, isPersistent, loginProvider, bypassTwoFactor);
    }

    protected override Task<bool> IsLockedOut(User user)
    {
        return base.IsLockedOut(user);
    }

    protected override Task<SignInResult> LockedOut(User user)
    {
        return base.LockedOut(user);
    }

    protected override async Task<SignInResult> PreSignInCheck(User user)
    {
        var signInResult = await base.PreSignInCheck(user);
        return signInResult;
    }

    protected override Task ResetLockout(User user)
    {
        return base.ResetLockout(user);
    }
}