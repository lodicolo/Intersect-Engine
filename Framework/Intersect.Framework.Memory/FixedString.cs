using System.Runtime.InteropServices;
using System.Text;

namespace Intersect.Framework.Memory;

public unsafe record struct FixedString : IDisposable
{
    private IntPtr _dataPointer;

    public FixedString(Encoding encoding, char[] chars) : this(encoding, chars.AsSpan())
    {
    }

    public FixedString(Encoding encoding, ReadOnlySpan<char> chars)
    {
        var byteCount = encoding.GetByteCount(chars);
        _dataPointer = Marshal.AllocHGlobal(byteCount + 1);
        fixed (char* charsPointer = chars)
        {
            encoding.GetBytes(
                charsPointer,
                chars.Length,
                (byte*)_dataPointer,
                byteCount
            );
        }
    }

    public FixedString(Encoding encoding, string text) : this(encoding, text.AsSpan())
    {
    }

    public void Dispose()
    {
        if (_dataPointer == default)
        {
            throw new ObjectDisposedException("This FixedString has already been disposed.");
        }

        Marshal.FreeHGlobal(_dataPointer);
        _dataPointer = default;
    }

    public static implicit operator IntPtr(FixedString fixedString) => fixedString._dataPointer;
}
