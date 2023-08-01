using Unity.Mathematics;
using UnityEngine.Rendering;
using Sketch.Common;

using BurstCompileAttribute = Unity.Burst.BurstCompileAttribute;
using IndexFormat = UnityEngine.Rendering.IndexFormat;
using Mesh = UnityEngine.Mesh;
using MeshTopology = UnityEngine.MeshTopology;

namespace Sketch.MeshKit {

// Baker: Shape instances -> Combined single mesh
[BurstCompile]
static class Baker
{
    // Public method
    public static void Bake(System.Span<ShapeInstance> instances, Mesh mesh)
    {
        // Total vertex / index count
        var (vcount, icount) = (0, 0);
        foreach (var i in instances)
        {
            vcount += i.VertexCount;
            icount += i.IndexCount;
        }

        // Native arrays for vertex / color / index data
        using var vbuf = Util.NewNativeArray<float3>(vcount);
        using var cbuf = Util.NewNativeArray<float4>(vcount);
        using var ibuf = Util.NewNativeArray<uint>(icount);

        // Data construction
        BakeDataBursted(instances, vbuf, cbuf, ibuf);

        // Mesh object construction
        mesh.Clear();
        mesh.indexFormat = IndexFormat.UInt32;
        mesh.SetVertices(vbuf);
        mesh.SetUVs(0, cbuf);
        mesh.SetIndices(ibuf, MeshTopology.Triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    // Burst accelerated vertex data construction
    [BurstCompile]
    static void BakeDataBursted(in RawSpan<ShapeInstance> instances,
                                in RawSpan<float3> vspan,
                                in RawSpan<float4> cspan,
                                in RawSpan<uint> ispan)
    {
        var (voffs, ioffs) = (0, 0);

        foreach (var i in instances.GetTyped())
        {
            var (vc, ic) = (i.VertexCount, i.IndexCount);

            // Warning: Not sure but this "1" extension is needed.
            var vslice = vspan.GetTyped(1).Slice(voffs, vc);
            var cslice = cspan.GetTyped(1).Slice(voffs, vc);
            var islice = ispan.GetTyped(1).Slice(ioffs, ic);

            i.Bake(vslice, cslice, islice, (uint)voffs);

            voffs += vc;
            ioffs += ic;
        }
    }
}

} // namespace Sketch.MeshKit
