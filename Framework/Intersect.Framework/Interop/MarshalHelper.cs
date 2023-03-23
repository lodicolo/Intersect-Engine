using System.Runtime.InteropServices;

namespace Intersect.Framework.Interop;

/// <summary>
/// Marshal
/// </summary>
public static class MarshalHelper
{
    public static IntPtr GetFunctionPointerForDelegate<TDelegate>(TDelegate @delegate)
        where TDelegate : MulticastDelegate => Marshal.GetFunctionPointerForDelegate(@delegate);
}
