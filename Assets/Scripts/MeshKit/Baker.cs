using Sketch.Common;
using System;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine.Rendering;
using UnityEngine;

using UnityEngine.Profiling;

namespace Sketch.MeshKit {

// Baker: Shape instances -> Combined single mesh
[BurstCompile]
static class Baker
{
    static VertexAttributeDescriptor[] VertexAttribs =
      { new VertexAttributeDescriptor(VertexAttribute.Position,  VertexAttributeFormat.Float32, 3),
        new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 4) };

    // Public method
    public static void Bake(ReadOnlySpan<ShapeInstance> instances, Mesh mesh)
    {
        Profiler.BeginSample("Vertex Counting");

        // Total vertex / index count
        var (vcount, icount) = (0, 0);
        foreach (var i in instances)
        {
            vcount += i.VertexCount;
            icount += i.IndexCount;
        }

        Profiler.EndSample();
        Profiler.BeginSample("Vertex Baking");

        // Mesh baking
        using var vbuf = Util.NewTempArray<float3>(vcount);
        using var cbuf = Util.NewTempArray<float4>(vcount);
        using var ibuf = Util.NewTempArray<uint>(icount);
        BakeBursted(instances, vbuf, cbuf, ibuf);

        Profiler.EndSample();
        Profiler.BeginSample("Mesh Update");

        // Mesh object construction
        mesh.Clear();
        mesh.indexFormat = IndexFormat.UInt32;
        mesh.SetVertices(vbuf);
        mesh.SetUVs(0, cbuf);
        mesh.SetIndices(ibuf, MeshTopology.Triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        Profiler.EndSample();
    }

    // Burst accelerated mesh baking method
    [BurstCompile]
    static void BakeBursted(in ReadOnlyRawSpan<ShapeInstance> instances,
                            in RawSpan<float3> vspan,
                            in RawSpan<float4> cspan,
                            in RawSpan<uint> ispan)
    {
        var (voffs, ioffs) = (0, 0);
        foreach (var i in instances.AsReadOnlySpan())
        {
            var (vc, ic) = (i.VertexCount, i.IndexCount);
            // Warning: Not sure but this "1" extension is needed.
            i.Bake(vspan.AsSpan(1).Slice(voffs, vc),
                   cspan.AsSpan(1).Slice(voffs, vc),
                   ispan.AsSpan(1).Slice(ioffs, ic), (uint)voffs);
            voffs += vc;
            ioffs += ic;
        }
    }
}

} // namespace Sketch.MeshKit
