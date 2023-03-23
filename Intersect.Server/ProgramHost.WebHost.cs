using Swashbuckle.AspNetCore.SwaggerGen.ConventionalRouting;

namespace Intersect.Server;

internal partial class ProgramHost
{
    private void ConfigureWebHostDefaults(IWebHostBuilder webHostBuilder)
    {
        webHostBuilder.ConfigureServices(ConfigureWebHostBuilderServices);

        webHostBuilder.ConfigureKestrel(ConfigureKestrel);

        webHostBuilder.Configure(ConfigureWebHostApplicationBuilder);
    }

    private void ConfigureWebHostApplicationBuilder(
        WebHostBuilderContext webHostBuilderContext,
        IApplicationBuilder applicationBuilder
    )
    {
        if (webHostBuilderContext.HostingEnvironment.IsDevelopment())
        {
            applicationBuilder.UseDeveloperExceptionPage();
            applicationBuilder.UseSwagger();
            applicationBuilder.UseSwaggerUI();
        }
        else
        {
            applicationBuilder.UseExceptionHandler("/Error");
            applicationBuilder.UseHsts();
            applicationBuilder.UseHttpsRedirection();
        }

        applicationBuilder.UseStaticFiles();
        applicationBuilder.UseCookiePolicy();

        applicationBuilder.UseCors("AllowAll");

        applicationBuilder.UseRouting();

        applicationBuilder.UseAuthentication();
        applicationBuilder.UseAuthorization();

        applicationBuilder.UseEndpoints(
            endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
                ConventionalRoutingSwaggerGen.UseRoutes(endpoints);
            }
        );
    }
}