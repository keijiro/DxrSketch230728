using System;
using Unity.Collections;

using Object = UnityEngine.Object;
using UnsafeUtility = Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility;

namespace MeshKit {

// Used to pass Span<T> to Burst functions
unsafe readonly ref struct RawSpan<T> where T : unmanaged
{
    public readonly void* Pointer;
    public readonly int Length;

    public Span<T> GetTyped(int ext = 0)
      => new Span<T>(Pointer, Length + ext);

    public RawSpan(void* ptr, int len)
    {
        Pointer = ptr;
        Length = len;
    }

    public static implicit operator RawSpan<T>
      (in Span<T> span)
    {
        fixed (T* p = span) return new RawSpan<T>(p, span.Length);
    }

    public static implicit operator RawSpan<T>
      (in NativeArray<T> array)
        => new RawSpan<T>(UnsafeExtensions.GetUnsafePtr(array), array.Length);
}

// Used to pass ReadonlySpan<T> to Burst functions
unsafe readonly ref struct ReadOnlyRawSpan<T> where T : unmanaged
{
    public readonly void* Pointer;
    public readonly int Length;

    public ReadOnlySpan<T> GetTyped(int ext = 0)
      => new ReadOnlySpan<T>(Pointer, Length + ext);

    public ReadOnlyRawSpan(void* ptr, int len)
    {
        Pointer = ptr;
        Length = len;
    }

    public static implicit operator ReadOnlyRawSpan<T>
      (in ReadOnlySpan<T> span)
    {
        fixed (T* p = span) return new ReadOnlyRawSpan<T>(p, span.Length);
    }

    public static implicit operator ReadOnlyRawSpan<T>
      (in NativeArray<T> array)
        => new ReadOnlyRawSpan<T>(UnsafeExtensions.GetUnsafePtr(array), array.Length);
}

static class UnsafeExtensions
{
    // NativeArray -> raw pointer
    public unsafe static void*
      GetUnsafePtr<T>(this NativeArray<T> array) where T : unmanaged
        => UnsafeUtility.GetUnsafePtr(array);

    // NativeArray -> Span
    public unsafe static Span<T>
      GetSpan<T>(this NativeArray<T> array) where T : unmanaged
        => new Span<T>(GetUnsafePtr(array), array.Length);

    // Span -> RawSpan
    public unsafe static RawSpan<T>
      GetRaw<T>(this Span<T> span) where T : unmanaged
    {
        fixed (T* p = span) return new RawSpan<T>(p, span.Length);
    }

    // ReadonlySpan -> ReadOnlyRawSpan
    public unsafe static ReadOnlyRawSpan<T>
      GetRaw<T>(this ReadOnlySpan<T> span) where T : unmanaged
    {
        fixed (T* p = span) return new ReadOnlyRawSpan<T>(p, span.Length);
    }

    // NativeArray -> RawSpan
    public unsafe static RawSpan<T>
      GetRawSpan<T>(this NativeArray<T> array) where T : unmanaged
        => new RawSpan<T>(GetUnsafePtr(array), array.Length);

    // NativeArray -> ReadOnlyRawSpan
    public unsafe static ReadOnlyRawSpan<T>
      GetReadOnlyRawSpan<T>(this NativeArray<T> array) where T : unmanaged
        => new ReadOnlyRawSpan<T>(GetUnsafePtr(array), array.Length);
}

static class Util
{
    public static NativeArray<T>
      NewNativeArray<T>(int length) where T : unmanaged
        => new NativeArray<T>(length, Allocator.Persistent,
                              NativeArrayOptions.UninitializedMemory);

    public static void DestroyObject(Object o)
    {
        if (o == null) return;
        if (UnityEngine.Application.isPlaying)
            Object.Destroy(o);
        else
            Object.DestroyImmediate(o);
    }
}

} // namespace MeshKit
