using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Intersect.Security.Claims;
using Intersect.Server.Web.RestApi.Configuration;
using Intersect.Server.Web.RestApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Intersect.Server.Web.RestApi;

public class VersionedControllerModelConvention : IControllerModelConvention
{
    private const string RootNamespace = "Intersect.Server.Web.RestApi.Routes.";

    public void Apply(ControllerModel controller)
    {
        var attributeRouteModel = controller.Selectors.FirstOrDefault()?.AttributeRouteModel;
        if (attributeRouteModel == default)
        {
            return;
        }

        var controllerRelativeTypeName = controller.ControllerType.FullName?.Replace(RootNamespace, string.Empty) ?? string.Empty;
        var controllerRelativeTypeNameParts = controllerRelativeTypeName.Split('.');
        var namespaceParts = controllerRelativeTypeNameParts.Take(controllerRelativeTypeNameParts.Length - 1);
        var normalizedNamespaceParts = namespaceParts.Select(namespacePart => namespacePart.ToLowerInvariant());
        var resolvedTemplate = string.Join('/', normalizedNamespaceParts.Prepend("api").Append(attributeRouteModel.Template));
        attributeRouteModel.Template = resolvedTemplate;
    }
}

public class CoreConfigurableAuthorizationRequirement : IAuthorizationRequirement
{
    public readonly string[] Roles;

    public CoreConfigurableAuthorizationRequirement(IEnumerable<string> roles)
    {
        Roles = roles.Where(role => !string.IsNullOrWhiteSpace(role)).ToArray();
    }
}

public class CoreConfigurableAuthorizationPolicyProvider : IAuthorizationPolicyProvider
{
    internal const string CoreConfigurablePolicyName = "CoreConfigurable";
    
    private readonly IAuthorizationPolicyProvider _fallback;

    public CoreConfigurableAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallback = new DefaultAuthorizationPolicyProvider(options);
    }
    
    public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
    {
        var policyNameParts = policyName.Split('/', 2);

        if (policyNameParts.Length < 2 || !string.Equals(policyNameParts.First(), CoreConfigurablePolicyName, StringComparison.Ordinal))
        {
            return _fallback.GetPolicyAsync(policyName);
        }

        var builder = new AuthorizationPolicyBuilder();
        builder.AddRequirements(new CoreConfigurableAuthorizationRequirement(policyNameParts.Last().Split(',')));
        var policy = builder.Build();
        return Task.FromResult(policy);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallback.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy> GetFallbackPolicyAsync() => _fallback.GetFallbackPolicyAsync();
}

public class CoreConfigurableAuthorizationHandler : AuthorizationHandler<CoreConfigurableAuthorizationRequirement>
{
    private readonly ApiConfiguration _apiConfiguration;

    public CoreConfigurableAuthorizationHandler(ApiConfiguration apiConfiguration)
    {
        _apiConfiguration = apiConfiguration;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CoreConfigurableAuthorizationRequirement requirement)
    {
        var whitelistedRoles = requirement.Roles;

        if (whitelistedRoles.Length > 0)
        {
            if (context.User is not ClaimsPrincipal claimsPrincipal)
            {
                context.Fail(new AuthorizationFailureReason(this, "No authorization token"));
                return;
            }

            foreach (var role in whitelistedRoles)
            {
                if (string.IsNullOrWhiteSpace(role))
                {
                    continue;
                }

                if (!claimsPrincipal.HasClaim(claim =>
                        IntersectClaimTypes.Role.Equals(claim.Type, StringComparison.OrdinalIgnoreCase) &&
                        role.Equals(claim.Value, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                context.Succeed(requirement);
                return;
            }
            
            context.Fail(new AuthorizationFailureReason(this, "Not authorized"));
            return;
        }

        if (context.Resource is not HttpContext httpContext)
        {
            context.Fail(new AuthorizationFailureReason(this, "Invalid context"));
            return;
        }

        var route = httpContext.Request.Path.Value;
        var method = httpContext.Request.Method;
        var authorizedRoutesService = httpContext.RequestServices.GetService<IAuthorizedRoutesService>();
        if (authorizedRoutesService == default)
        {
            return;
        }

        if (!authorizedRoutesService.RequiresAuthorization(route, method))
        {
            context.Succeed(requirement);
        }
    }
}

public class AspNetCoreWebApp
{
    public void Start()
    {
        var builder = WebApplication.CreateBuilder();

        builder.Services.AddSingleton(ApiConfiguration.Create());
        builder.Services.AddSingleton<IAuthorizedRoutesService, AuthorizedRoutesService>();

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme,
            options =>
            {
            });
        builder.Services.AddSingleton<IAuthorizationHandler, CoreConfigurableAuthorizationHandler>();
        builder.Services.AddSingleton<IAuthorizationPolicyProvider, CoreConfigurableAuthorizationPolicyProvider>();
        
        builder.Services.AddControllers(mvcOptions => mvcOptions.Conventions.Add(new VersionedControllerModelConvention()));

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.Urls.Add("http://localhost:5400");

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}