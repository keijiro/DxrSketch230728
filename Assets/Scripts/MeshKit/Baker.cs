using Sketch.Common;
using System;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine.Rendering;
using UnityEngine;

namespace Sketch.MeshKit {

// Baker: Shape instances -> Combined single mesh
[BurstCompile]
static class Baker
{
    // Public method
    public static void Bake(ReadOnlySpan<ShapeInstance> instances, Mesh mesh)
    {
        // Total vertex / index count
        var (vcount, icount) = (0, 0);
        foreach (var i in instances)
        {
            vcount += i.VertexCount;
            icount += i.IndexCount;
        }

        // Mesh baking
        using var vbuf = Util.NewTempArray<float3>(vcount);
        using var cbuf = Util.NewTempArray<float4>(vcount);
        using var ibuf = Util.NewTempArray<uint>(icount);
        BakeBursted(instances, vbuf, cbuf, ibuf);

        // Mesh object construction
        mesh.Clear();
        mesh.indexFormat = IndexFormat.UInt32;
        mesh.SetVertices(vbuf);
        mesh.SetUVs(0, cbuf);
        mesh.SetIndices(ibuf, MeshTopology.Triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
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
