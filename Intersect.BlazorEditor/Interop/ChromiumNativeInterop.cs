using Microsoft.JSInterop;

namespace Intersect.BlazorEditor.Interop;

public sealed class ChromiumNativeInterop : NativeInterop
{
    public ChromiumNativeInterop(IJSRuntime jsRuntime) : base(jsRuntime, "ChromiumInterop")
    {
    }
}