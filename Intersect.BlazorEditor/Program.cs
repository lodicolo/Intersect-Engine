using Intersect.BlazorEditor;
using Intersect.BlazorEditor.Interop;
using Material.Blazor;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

var runtimeInteropEnvironment = Environment.GetEnvironmentVariable("RUNTIME_INTEROP_ENVIRONMENT");

Console.WriteLine($"test: '{runtimeInteropEnvironment}'");

switch (runtimeInteropEnvironment)
{
    case "tauri":
        builder.Services.AddSingleton<INativeInterop, TauriNativeInterop>();
        break;

    default:
        builder.Services.AddSingleton<INativeInterop, ChromiumNativeInterop>();
        break;
}

builder.Services.AddMBServices();

var app = builder.Build();

await app.RunAsync();