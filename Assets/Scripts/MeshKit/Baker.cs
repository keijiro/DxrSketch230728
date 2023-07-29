using Unity.Mathematics;
using UnityEngine.Rendering;
using IndexFormat = UnityEngine.Rendering.IndexFormat;
using Mesh = UnityEngine.Mesh;
using MeshTopology = UnityEngine.MeshTopology;
using BurstCompileAttribute = Unity.Burst.BurstCompileAttribute;

namespace MeshKit {

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
        BakeDataBursted(instances.GetUntyped(),
                        vbuf.GetUntypedSpan(),
                        cbuf.GetUntypedSpan(),
                        ibuf.GetUntypedSpan());

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
    static void BakeDataBursted(in UntypedSpan u_instances,
                                in UntypedSpan u_vspan,
                                in UntypedSpan u_cspan,
                                in UntypedSpan u_ispan)
    {
        var instances = u_instances.GetTyped<ShapeInstance>();

        // Warning: Not sure but this "1" extension is needed.
        var vspan = u_vspan.GetTyped<float3>(1);
        var cspan = u_cspan.GetTyped<float4>(1);
        var ispan = u_ispan.GetTyped<uint>(1);

        var (voffs, ioffs) = (0, 0);

        foreach (var i in instances)
        {
            var (vc, ic) = (i.VertexCount, i.IndexCount);

            var vslice = vspan.Slice(voffs, vc);
            var cslice = cspan.Slice(voffs, vc);
            var islice = ispan.Slice(ioffs, ic);

            i.Bake(vslice, cslice, islice, (uint)voffs);

            voffs += vc;
            ioffs += ic;
        }
    }
}

} // namespace MeshKit
