using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Intersect.Server;

internal static partial class ProgramConfiguration
{
    private static partial bool IgnoreDefaultsForConfigureAppBuilder();

    internal static void ConfigureAppBuilder(WebHostBuilderContext context, IApplicationBuilder app)
    {
        if (IgnoreDefaultsForConfigureAppBuilder())
        {
            ConfigureAppBuilderForEnvironment(context, app);
            ConfigureAppBuilderCommon(context, app);
            return;
        }

        app.UseStaticFiles();

        app.UseSerilogRequestLogging();

        if (context.HostingEnvironment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else if (context.HostingEnvironment.IsStaging())
        {
            app.UseHsts();
        }
        else if (context.HostingEnvironment.IsProduction())
        {
            app.UseHsts();
        }

        ConfigureAppBuilderForEnvironment(context, app);

        app.UseHttpsRedirection();

        ConfigureAppBuilderCommon(context, app);
    }

    static partial void ConfigureAppBuilderForEnvironment(WebHostBuilderContext context, IApplicationBuilder app);

    static partial void ConfigureAppBuilderCommon(WebHostBuilderContext context, IApplicationBuilder app);
}
