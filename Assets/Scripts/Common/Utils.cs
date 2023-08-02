using Unity.Collections;
using Object = UnityEngine.Object;

namespace Sketch.Common {

// Miscellaneous utility methods
static class Util
{
    public static NativeArray<T>
      NewNativeArray<T>(int length) where T : unmanaged
        => new NativeArray<T>(length, Allocator.Persistent,
                              NativeArrayOptions.UninitializedMemory);

    public static NativeArray<T>
      NewTempArray<T>(int length) where T : unmanaged
        => new NativeArray<T>(length, Allocator.Temp,
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

} // namespace Sketch.Common
