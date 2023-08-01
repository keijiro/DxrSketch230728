using System;
using Unity.Collections;

namespace Sketch.Common {

// RawSpan - Unmanaged blittable version of Span<T>
public unsafe readonly ref struct RawSpan<T> where T : unmanaged
{
    public readonly void* Pointer;
    public readonly int Length;

    public Span<T> GetTyped(int ext = 0)
      => new Span<T>(Pointer, Length + ext);

    public RawSpan(void* ptr, int len)
      { Pointer = ptr; Length = len; }

    public static implicit operator RawSpan<T>(Span<T> span)
      { fixed (T* p = span) return new RawSpan<T>(p, span.Length); }

    public static implicit operator RawSpan<T>(NativeArray<T> array)
      => new RawSpan<T>(RawSpanUtil.GetPtr(array), array.Length);
}

// ReadOnly version
public unsafe readonly ref struct ReadOnlyRawSpan<T> where T : unmanaged
{
    public readonly void* Pointer;
    public readonly int Length;

    public ReadOnlySpan<T> GetTyped(int ext = 0)
      => new ReadOnlySpan<T>(Pointer, Length + ext);

    public ReadOnlyRawSpan(void* ptr, int len)
      { Pointer = ptr; Length = len; }

    public static implicit operator ReadOnlyRawSpan<T>(ReadOnlySpan<T> span)
      { fixed (T* p = span) return new ReadOnlyRawSpan<T>(p, span.Length); }

    public static implicit operator ReadOnlyRawSpan<T>(NativeArray<T> array)
      => new ReadOnlyRawSpan<T>(RawSpanUtil.GetPtr(array), array.Length);
}

// Extension method
public static class SpanExtensions
{
    // NativeArray -> Span
    public unsafe static Span<T>
      GetSpan<T>(this NativeArray<T> array) where T : unmanaged
      => new Span<T>(RawSpanUtil.GetPtr(array), array.Length);

    // NativeArray -> ReadOnlySpan
    public unsafe static ReadOnlySpan<T>
      GetReadOnlySpan<T>(this NativeArray<T> array) where T : unmanaged
      => new ReadOnlySpan<T>(RawSpanUtil.GetPtr(array), array.Length);

    // Span -> RawSpan
    public unsafe static RawSpan<T>
      GetRaw<T>(this Span<T> span) where T : unmanaged
      { fixed (T* p = span) return new RawSpan<T>(p, span.Length); }

    // ReadonlySpan -> ReadOnlyRawSpan
    public unsafe static ReadOnlyRawSpan<T>
      GetRaw<T>(this ReadOnlySpan<T> span) where T : unmanaged
      { fixed (T* p = span) return new ReadOnlyRawSpan<T>(p, span.Length); }

    // NativeArray -> RawSpan
    public unsafe static RawSpan<T>
      GetRawSpan<T>(this NativeArray<T> array) where T : unmanaged
      => new RawSpan<T>(RawSpanUtil.GetPtr(array), array.Length);

    // NativeArray -> ReadOnlyRawSpan
    public unsafe static ReadOnlyRawSpan<T>
      GetReadOnlyRawSpan<T>(this NativeArray<T> array) where T : unmanaged
      => new ReadOnlyRawSpan<T>(RawSpanUtil.GetPtr(array), array.Length);
}

// Utility methods for internal-use
static class RawSpanUtil
{
    // NativeArray -> raw pointer
    public unsafe static void*
      GetPtr<T>(this NativeArray<T> array) where T : unmanaged
      => Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.
         GetUnsafePtr(array);
}

} // namespace Sketch.Common
