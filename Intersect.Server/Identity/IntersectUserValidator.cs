using System.Diagnostics;
using Intersect.Server.Database.PlayerData;
using Microsoft.AspNetCore.Identity;

namespace Intersect.Server.Identity;

public sealed class IntersectUserValidator : IUserValidator<User>
{
    public async Task<IdentityResult> ValidateAsync(UserManager<User> manager, User user)
    {
        var identityResult = await Task.FromResult(IdentityResult.Success);
        return identityResult;
    }
    
    // public virtual async Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user)
    // {
    //     if (manager == null)
    //         throw new ArgumentNullException(nameof (manager));
    //     if ((object) user == null)
    //         throw new ArgumentNullException(nameof (user));
    //     List<IdentityError> errors = new List<IdentityError>();
    //     await this.ValidateUserName(manager, user, (ICollection<IdentityError>) errors).ConfigureAwait(false);
    //     if (manager.Options.User.RequireUniqueEmail)
    //         await this.ValidateEmail(manager, user, errors).ConfigureAwait(false);
    //     IdentityResult identityResult = errors.Count > 0 ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success;
    //     errors = (List<IdentityError>) null;
    //     return identityResult;
    // }
}