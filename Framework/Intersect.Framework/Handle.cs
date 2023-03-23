using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Intersect.Framework.Converters.Json;

using NewtonsoftJsonConverter = Newtonsoft.Json.JsonConverterAttribute;
using SystemJsonConverter = System.Text.Json.Serialization.JsonConverterAttribute;

namespace Intersect.Framework;

/// <summary>
/// Represents a type-bound handle (pointer).
/// </summary>
/// <param name="IntPtr">the generic pointer</param>
/// <typeparam name="T">the type this Handle is for</typeparam>
[DataContract]
[SystemJsonConverter(typeof(HandleSystemJsonConverterFactory))]
[NewtonsoftJsonConverter(typeof(HandleNewtonsoftJsonConverter))]
[Serializable]
public record struct Handle<T>(IntPtr IntPtr)
{
    /// <summary>
    /// Empty handle with zeroed internal <see cref="System.IntPtr"/>.
    /// </summary>
    public static readonly Handle<T> None = default;

    /// <summary>
    /// Converts an <see cref="Handle{T}"/> to the contained <see cref="IntPtr"/>.
    /// </summary>
    /// <param name="handle">the <see cref="Handle{T}"/> to convert</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator IntPtr(Handle<T> handle) => handle.IntPtr;
}
