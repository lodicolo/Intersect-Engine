using Microsoft.JSInterop;

namespace Intersect.BlazorEditor.Interop;

public abstract class NativeInterop : INativeInterop
{
    private readonly Task _initializerTask;

    private readonly string _interopClassName;

    private readonly IJSRuntime _jsRuntime;

    protected NativeInterop(IJSRuntime jsRuntime, string interopClassName)
    {
        _jsRuntime = jsRuntime;
        _interopClassName = interopClassName;
        _initializerTask = jsRuntime.InvokeVoidAsync("initializeInterop", interopClassName).AsTask();
    }

    public virtual Task<string> RuntimeVersion =>
        _initializerTask.ContinueWith(_ => _jsRuntime.InvokeAsync<string>("window.interop.getRuntimeVersion").AsTask())
            .Unwrap();

    public virtual async Task Close() => await _jsRuntime.InvokeVoidAsync("window.interop.close");

    public virtual async Task Open(string url) => await _jsRuntime.InvokeVoidAsync("window.interop.open", url);

    public virtual async Task SendNotification(string message) =>
        await _jsRuntime.InvokeVoidAsync("window.interop.sendNotification", message);
}