using System.Net;
using System.Text.Json.Serialization;
using Intersect.Server.Database.PlayerData.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Intersect.Server.Web.RestApi.Routes;

[ApiController]
[Route("oauth")]
[Authorize]
public sealed class OAuthController : IntersectControllerBase
{
    [HttpDelete]
    [Route("tokens/{tokenId:guid}")]
    public async Task<IActionResult> DeleteTokenById(Guid tokenId)
    {
        var actor = IntersectUser;
        if (actor == default)
        {
            return Unauthorized();
        }

        if (!RefreshToken.TryFind(tokenId, out var refreshToken))
        {
            return Unauthorized();
        }

        if (refreshToken.Id != tokenId)
        {
            return Problem();
        }

        if (refreshToken.UserId != actor.Id && !actor.Power.ApiRoles.UserManage)
        {
            return Unauthorized();
        }

        if (!RefreshToken.Remove(refreshToken))
        {
            return Problem();
        }

        return Ok(
            new UsernameAndTokenResponse
            {
                TokenId = tokenId
            }
        );
    }

    [Authorize]
    [HttpDelete]
    [Route("tokens/{username}")]
    public async Task<IActionResult> DeleteTokensForUsername(string username, CancellationToken cancellationToken)
    {
        var user = Database.PlayerData.User.Find(username);

        if (user == null)
        {
            return Unauthorized();
        }

        var actor = IntersectUser;
        if (actor == default)
        {
            return Unauthorized();
        }

        if (!actor.Power.ApiRoles.UserManage && actor.Id != user.Id)
        {
            return Unauthorized();
        }

        if (!RefreshToken.HasTokens(user))
        {
            return StatusCode((int)HttpStatusCode.Gone);
        }

        var success = await RefreshToken.RemoveForUserAsync(user.Id, cancellationToken).ConfigureAwait(false);
        return success ? Ok(
            new
            {
                Username = username
            }
        ) : Unauthorized();
    }

    [HttpDelete]
    [Route("tokens/{username}/{tokenId:guid}")]
    public async Task<IActionResult> DeleteTokenForUsernameById(string username, Guid tokenId)
    {
        var user = Database.PlayerData.User.Find(username);

        if (user == null)
        {
            return Unauthorized();
        }

        if (IntersectUser?.Id != user.Id && !IntersectUser.Power.ApiRoles.UserManage)
        {
            return Unauthorized();
        }

        if (!RefreshToken.TryFind(tokenId, out _))
        {
            return StatusCode((int)HttpStatusCode.Gone);
        }

        if (!RefreshToken.Remove(tokenId))
        {
            return Problem();
        }

        return Ok(
            new UsernameAndTokenResponse
            {
                TokenId = tokenId,
                Username = username
            }
        );
    }

    private class UsernameAndTokenResponse
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Guid TokenId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string Username { get; set; }
    }
}