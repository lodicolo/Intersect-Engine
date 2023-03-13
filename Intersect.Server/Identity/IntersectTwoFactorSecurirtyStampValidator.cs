using Intersect.Server.Database.PlayerData;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Intersect.Server.Identity;

public sealed class IntersectTwoFactorSecurirtyStampValidator : TwoFactorSecurityStampValidator<User>
{
    public IntersectTwoFactorSecurirtyStampValidator(
        IOptions<SecurityStampValidatorOptions> options,
        SignInManager<User> signInManager,
        ISystemClock clock,
        ILoggerFactory logger
    ) : base(options, signInManager, clock, logger)
    {
    }
}