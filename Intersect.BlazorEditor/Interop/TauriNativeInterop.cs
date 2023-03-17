using Microsoft.JSInterop;

namespace Intersect.BlazorEditor.Interop;

public sealed class TauriNativeInterop : NativeInterop
{
    public TauriNativeInterop(IJSRuntime jsRuntime) : base(jsRuntime, "TauriInterop")
    {
    }
}