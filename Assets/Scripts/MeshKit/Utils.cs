using System;
using Unity.Collections;

using Object = UnityEngine.Object;
using UnsafeUtility = Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility;

namespace MeshKit {

// Used to pass Span<T> to Burst functions
unsafe readonly struct UntypedSpan
{
    public readonly void* Pointer;
    public readonly int Length;

    public Span<T> GetTyped<T>(int ext = 0)
      => new Span<T>(Pointer, Length + ext);

    public UntypedSpan(void* ptr, int len)
    {
        Pointer = ptr;
        Length = len;
    }
}

// Used to pass ReadonlySpan<T> to Burst functions
unsafe readonly struct UntypedReadOnlySpan
{
    public readonly void* Pointer;
    public readonly int Length;

    public ReadOnlySpan<T> GetTyped<T>(int ext = 0)
      => new ReadOnlySpan<T>(Pointer, Length + ext);

    public UntypedReadOnlySpan(void* ptr, int len)
    {
        Pointer = ptr;
        Length = len;
    }
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

    // Span -> UntypedSpan
    public unsafe static UntypedSpan
      GetUntyped<T>(this Span<T> span) where T : unmanaged
    {
        fixed (T* p = span) return new UntypedSpan(p, span.Length);
    }

    // ReadonlySpan -> UntypedReadOnlySpan
    public unsafe static UntypedReadOnlySpan
      GetUntyped<T>(this ReadOnlySpan<T> span) where T : unmanaged
    {
        fixed (T* p = span) return new UntypedReadOnlySpan(p, span.Length);
    }

    // NativeArray -> UntypedSpan
    public unsafe static UntypedSpan
      GetUntypedSpan<T>(this NativeArray<T> array) where T : unmanaged
        => new UntypedSpan(GetUnsafePtr(array), array.Length);

    // NativeArray -> UntypedReadOnlySpan
    public unsafe static UntypedReadOnlySpan
      GetUntypedReadOnlySpan<T>(this NativeArray<T> array) where T : unmanaged
        => new UntypedReadOnlySpan(GetUnsafePtr(array), array.Length);
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
