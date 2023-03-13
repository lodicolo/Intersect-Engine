using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Intersect.Logging;
using Intersect.Security.Claims;
using Intersect.Server.Database;
using Intersect.Server.Database.PlayerData;
using Intersect.Server.Identity;
using Intersect.Server.Web.RestApi.Configuration;
using Intersect.Server.Web.RestApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

        var controllerRelativeTypeName =
            controller.ControllerType.FullName?.Replace(RootNamespace, string.Empty) ?? string.Empty;
        var controllerRelativeTypeNameParts = controllerRelativeTypeName.Split('.');
        var namespaceParts = controllerRelativeTypeNameParts.Take(controllerRelativeTypeNameParts.Length - 1);
        var normalizedNamespaceParts = namespaceParts.Select(namespacePart => namespacePart.ToLowerInvariant());
        var resolvedTemplate = string.Join(
            '/',
            normalizedNamespaceParts.Prepend("api").Append(attributeRouteModel.Template)
        );
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

        if (policyNameParts.Length < 2 ||
            !string.Equals(policyNameParts.First(), CoreConfigurablePolicyName, StringComparison.Ordinal))
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

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CoreConfigurableAuthorizationRequirement requirement
    )
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

                if (!claimsPrincipal.HasClaim(
                        claim => IntersectClaimTypes.Role.Equals(claim.Type, StringComparison.OrdinalIgnoreCase) &&
                                 role.Equals(claim.Value, StringComparison.OrdinalIgnoreCase)
                    ))
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

public class AesDataProtector : IDataProtector, IDisposable
{
    private const int HeaderSize = (sizeof(byte) * 2) + (sizeof(int) * 2);

    private const int NonceSize = 12;

    private const int TagSize = 16;

    private readonly AesGcm _aesGcm;

    private readonly byte[] _keyData;

    private readonly byte[] _purpose;

    public AesDataProtector(string purpose, string dataProtectionKey)
    {
        _keyData = Convert.FromHexString(dataProtectionKey);
        _aesGcm = new AesGcm(_keyData);
        _purpose = Encoding.UTF8.GetBytes(purpose);
    }

    private AesDataProtector(string purpose, byte[] keyData)
    {
        _keyData = keyData;
        _aesGcm = new AesGcm(_keyData);
        _purpose = Encoding.UTF8.GetBytes(purpose);
    }

    public IDataProtector CreateProtector(string purpose) => new AesDataProtector(purpose, _keyData);

    public byte[] Protect(byte[] plaintext)
    {
        try
        {
            var cipherLength = HeaderSize + NonceSize + TagSize + plaintext.Length;
            var heap = new byte[cipherLength];
            var data = cipherLength > 1024 ? heap : stackalloc byte[cipherLength];

            var offset = 0;

            data[offset] = NonceSize & 0xff;
            offset += sizeof(byte);

            data[offset] = TagSize & 0xff;
            offset += sizeof(byte);

            var nonce = data.Slice(offset, NonceSize);
            offset += NonceSize;

            var ciphertext = data[offset..(offset + plaintext.Length)];
            offset += ciphertext.Length;

            var tag = data.Slice(offset, TagSize);

            RandomNumberGenerator.Fill(nonce);

            _aesGcm.Encrypt(
                nonce,
                plaintext,
                ciphertext,
                tag,
                _purpose
            );

            if (cipherLength <= 1024)
            {
                data.CopyTo(heap);
            }

            return heap;
        }
        catch (Exception exception)
        {
            Log.Error(exception, "Error during encryption.");
            throw;
        }
    }

    public byte[] Unprotect(byte[] protectedData)
    {
        try
        {
            var plaintextLength = protectedData.Length - (HeaderSize + NonceSize + TagSize);
            var protectedSpan = protectedData.AsSpan();
            var heap = new byte[plaintextLength];

            var offset = 0;

            var nonceSize = protectedSpan[offset];
            offset += sizeof(byte);

            var tagSize = protectedSpan[offset];
            offset += sizeof(byte);

            var nonce = protectedSpan.Slice(offset, nonceSize);
            offset += NonceSize;

            var ciphertext = protectedSpan[offset..(offset + plaintextLength)];
            offset += ciphertext.Length;

            var tag = protectedSpan.Slice(offset, tagSize);

            RandomNumberGenerator.Fill(nonce);

            var plaintext = plaintextLength > 1024 ? heap : stackalloc byte[plaintextLength];

            _aesGcm.Decrypt(
                nonce,
                ciphertext,
                tag,
                plaintext,
                _purpose
            );

            if (plaintextLength <= 1024)
            {
                plaintext.CopyTo(heap);
            }

            return heap;
        }
        catch (Exception exception)
        {
            Log.Error(exception, "Error during encryption.");
            throw;
        }
    }

    public void Dispose()
    {
        Array.Fill(_keyData, default);
        _aesGcm?.Dispose();
    }

    ~AesDataProtector()
    {
        Dispose();
    }
}

public class AesDataProtectionProvider : IDataProtectionProvider
{
    private readonly string _dataProtectionKey;

    public AesDataProtectionProvider(string dataProtectionKey)
    {
        _dataProtectionKey = dataProtectionKey;
    }

    public IDataProtector CreateProtector(string purpose) => new AesDataProtector(purpose, _dataProtectionKey);
}

public sealed class PlayerContextFactory : IDbContextFactory<PlayerContext>
{
    public PlayerContext CreateDbContext() => DbInterface.CreatePlayerContext(false);
}

public partial class KebabParameterTransformer : IOutboundParameterTransformer
{
    public string TransformOutbound(object value) =>
        // Slugify value
        value == null ? null : GetCaseChangePattern().Replace(value.ToString(), "$1-$2").ToLower();

    [GeneratedRegex("([a-z])([A-Z])")]
    private static partial Regex GetCaseChangePattern();
}

public class AspNetCoreWebApp
{
    public void Start()
    {
        var builder = WebApplication.CreateBuilder();

        builder.Configuration.AddJsonFile("resources/config/api.config.json");
        var apiConfiguration = new ApiConfiguration();
        builder.Configuration.Bind(apiConfiguration);

        builder.Services.AddDbContextFactory<PlayerContext, PlayerContextFactory>(
            options => { },
            ServiceLifetime.Scoped
        );

        builder.Services.AddSingleton(ApiConfiguration.Create());
        builder.Services.AddSingleton<IAuthorizedRoutesService, AuthorizedRoutesService>();

        builder.Services.AddAuthentication(
                options =>
                {
                    // options.DefaultScheme = "Intersect.Application";
                    // options.DefaultSignInScheme = "Intersect.External";
                    options.DefaultScheme = IdentityConstants.ApplicationScheme;
                    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
                }
            )
            .AddIdentityCookies(o => { });

        builder.Services.AddOptions().AddLogging();

        builder.Services.TryAddScoped<IdentityErrorDescriber>();
        builder.Services.TryAddScoped<ILookupNormalizer, IntersectLookupNormalizer>();
        builder.Services.TryAddScoped<IPasswordHasher<User>, IntersectPasswordHasher>();
        builder.Services.TryAddScoped<IPasswordValidator<User>, IntersectPasswordValidator>();
        builder.Services.TryAddScoped<IUserClaimsPrincipalFactory<User>, UserClaimsPrincipalFactory<User>>();
        builder.Services.TryAddScoped<IUserConfirmation<User>, DefaultUserConfirmation<User>>();
        builder.Services.TryAddScoped<IUserValidator<User>, IntersectUserValidator>();
        builder.Services.TryAddScoped<UserManager<User>, IntersectUserManager>();

        builder.Services.Configure<IdentityOptions>(
            identityOptions =>
            {
                identityOptions.Stores.MaxLengthForKeys = 128;
                identityOptions.SignIn.RequireConfirmedAccount = true;
#if DEBUG
                identityOptions.Password.RequireDigit = false;
                identityOptions.Password.RequiredLength = 4;
                identityOptions.Password.RequireLowercase = false;
                identityOptions.Password.RequireNonAlphanumeric = false;
                identityOptions.Password.RequireUppercase = false;
                identityOptions.Password.RequiredUniqueChars = 0;
#endif
            }
        );

        var identityBuilder = new IdentityBuilder(typeof(User), builder.Services);

        identityBuilder.Services.AddHttpContextAccessor();
        identityBuilder.Services.TryAddScoped<ISecurityStampValidator, IntersectSecurityStampValidator>();
        identityBuilder.Services
            .TryAddScoped<ITwoFactorSecurityStampValidator, IntersectTwoFactorSecurirtyStampValidator>();
        identityBuilder.Services.TryAddScoped<SignInManager<User>, IntersectSignInManager>();

        identityBuilder.AddDefaultUI().AddDefaultTokenProviders();

        builder.Services.TryAddScoped<IUserStore<User>, IntersectUserStore>();

        builder.Services.Configure<CookiePolicyOptions>(
            options =>
            {
                // This lambda determines whether user consent for non-essential 
                // cookies is needed for a given request.
                options.CheckConsentNeeded = _ => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            }
        );

        builder.Services.AddRazorPages(
            razorPagesOptions =>
                razorPagesOptions.Conventions.Add(new PageRouteTransformerConvention(new KebabParameterTransformer()))
        );

        // builder.Services.AddAuthentication(options =>
        //     {
        //         options.DefaultScheme = "Intersect.Application";
        //         options.DefaultSignInScheme = "Intersect.External";
        //     })
        //     .AddCookie(builder =>
        //     {
        //     })
        //     .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        //     {
        //         Debugger.Break();
        //     })
        //     .AddOAuth("IntersectLegacy", oauthOptions =>
        //     {
        //         oauthOptions.AuthorizationEndpoint = "/authorization";
        //         oauthOptions.CallbackPath = "/callback";
        //         oauthOptions.ClientId = default(Guid).ToString();
        //         oauthOptions.ClientSecret = "secret";
        //         oauthOptions.DataProtectionProvider = new AesDataProtectionProvider(apiConfiguration.DataProtectionKey);
        //         oauthOptions.TokenEndpoint = "/api/oauth/token";
        //         oauthOptions.Events = new OAuthEvents
        //         {
        //             OnAccessDenied = async context => Debugger.Break(),
        //             OnCreatingTicket = async context => Debugger.Break(),
        //             OnRedirectToAuthorizationEndpoint = async context => Debugger.Break(),
        //             OnRemoteFailure = async context => Debugger.Break(),
        //             OnTicketReceived = async context => Debugger.Break()
        //         };
        //         oauthOptions.Validate();
        //     });
        builder.Services.AddSingleton<IAuthorizationHandler, CoreConfigurableAuthorizationHandler>();
        builder.Services.AddSingleton<IAuthorizationPolicyProvider, CoreConfigurableAuthorizationPolicyProvider>();

        builder.Services.AddControllers(
            mvcOptions => mvcOptions.Conventions.Add(new VersionedControllerModelConvention())
        );

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.WebHost.UseStaticWebAssets();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseCookiePolicy();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapRazorPages();

        app.Urls.Add("http://localhost:5400");

        app.Run();
    }
}