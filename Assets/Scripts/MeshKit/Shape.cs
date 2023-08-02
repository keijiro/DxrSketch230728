using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Sketch.MeshKit {

// Managed shape container
public sealed class Shape : System.IDisposable
{
    public NativeArray<float3> Vertices;
    public NativeArray<uint> Indices;

    public Shape(Mesh src)
    {
        var v = new NativeArray<Vector3>(src.vertices, Allocator.Persistent);
        var i = new NativeArray<int>(src.triangles, Allocator.Persistent);
        Vertices = v.Reinterpret<float3>();
        Indices = i.Reinterpret<uint>();
    }

    public void Dispose()
    {
        if (Vertices.IsCreated) Vertices.Dispose();
        if (Indices.IsCreated) Indices.Dispose();
    }
}

// Unmanaged weak reference to shape contents
public readonly struct ShapeRef
{
    public readonly NativeSlice<float3> Vertices;
    public readonly NativeSlice<uint> Indices;

    public ShapeRef(Shape shape)
    {
        Vertices = new NativeSlice<float3>(shape.Vertices);
        Indices = new NativeSlice<uint>(shape.Indices);
    }

    // Implicit conversion operator
    public static implicit operator ShapeRef(Shape shape)
      => new ShapeRef(shape);
}

} // namespace Sketch.MeshKit
