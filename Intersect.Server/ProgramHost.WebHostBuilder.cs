using System.Data.Common;
using Intersect.Enums;
using Intersect.Server.Web.RestApi;
using Intersect.Server.Web.RestApi.Constraints;
using Intersect.Server.Web.RestApi.Payloads;
using Swashbuckle.AspNetCore.SwaggerGen.ConventionalRouting;

namespace Intersect.Server;

internal partial class ProgramHost
{
    private void ConfigureWebHostBuilderServices(
        WebHostBuilderContext webHostBuilderContext,
        IServiceCollection services
    )
    {
        services.AddHealthChecks();

        services.AddCors(
            corsOptions => corsOptions.AddPolicy(
                "AllowAll",
                corsPolicyBuilder => corsPolicyBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
            )
        );

        services.AddControllers(mvcOptions => mvcOptions.Conventions.Add(new VersionedControllerModelConvention()));

        services.AddRouting(
            routeOptions =>
            {
                routeOptions.ConstraintMap.Add(nameof(AdminAction), typeof(AdminActionsConstraint));
                routeOptions.ConstraintMap.Add(nameof(LookupKey), typeof(LookupKey.Constraint));
                routeOptions.ConstraintMap.Add(nameof(ChatMessage), typeof(ChatMessage.Constraint));
            }
        );

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(
            swaggerGenOptions =>
            {
                // g.DocInclusionPredicate((path, description) => true);
                swaggerGenOptions.ResolveConflictingActions(
                    descriptions =>
                    {
                        var descriptionArray = descriptions.ToArray();
                        // Debugger.Break();
                        return descriptionArray.First();
                    }
                );
            }
        );

        services.AddSwaggerGenWithConventionalRoutes();

        services.AddRazorPages().AddRazorRuntimeCompilation();

        services.AddSIDIdentityServer()
            .UseEFStore(
                dbContextOptionsBuilder =>
                {
                    var rawConnectionString = webHostBuilderContext.Configuration.GetConnectionString("Identity");
                    DbConnectionStringBuilder connectionStringBuilder = new();
                    connectionStringBuilder.ConnectionString = rawConnectionString;
                    var dataSource = connectionStringBuilder["DataSource"];
                }
            )
            .AddDeveloperSigningCredentials()
            .AddWsFederationSigningCredentials()
            .AddBackChannelAuthentication()
            .AddEmailAuthentication()
            .AddAuthentication(
                authBuilder =>
                {
                    authBuilder.AddOIDCAuthentication(
                        oidcOptions =>
                        {
                            oidcOptions.Authority = "http://localhost:5001";
                            oidcOptions.ClientId = "website";
                            oidcOptions.ClientSecret = "password";
                            oidcOptions.ResponseType = "code";
                            oidcOptions.ResponseMode = "query";
                            oidcOptions.SaveTokens = true;
                            oidcOptions.GetClaimsFromUserInfoEndpoint = true;
                            oidcOptions.RequireHttpsMetadata = false;
                            oidcOptions.TokenValidationParameters = new()
                            {
                                NameClaimType = "name"
                            };
                            oidcOptions.Scope.Add("profile");
                        }
                    );
                }
            );
    }
}