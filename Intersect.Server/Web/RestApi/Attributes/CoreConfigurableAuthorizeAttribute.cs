using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace Intersect.Server.Web.RestApi.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
internal class CoreConfigurableAuthorizeAttribute : AuthorizeAttribute
{
    public CoreConfigurableAuthorizeAttribute()
    {
        var roles = Roles?.Split(',').Where(role => !string.IsNullOrWhiteSpace(role)).Select(role => role.Trim()) ??
                        Array.Empty<string>();

        Policy = $"{CoreConfigurableAuthorizationPolicyProvider.CoreConfigurablePolicyName}/{string.Join(',', roles)}";
    }
}