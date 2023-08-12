using Sketch.Common;
using System;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using Random = Unity.Mathematics.Random;
using Klak.Math;

namespace Sketch {

// Configuration struct
[Serializable]
public struct GrassConfig
{
    public int InstanceCount;
    public float SpawnRadius;
    public float BladeWidth;
    public float BladeHeight;
    public int Subdivision;
    public uint Seed;

    // Default configuration
    public static GrassConfig Default()
      => new GrassConfig()
        { InstanceCount = 100,
          SpawnRadius = 10,
          BladeWidth = 0.01f,
          BladeHeight = 0.5f,
          Subdivision = 10,
          Seed = 1 };
}

// Builder implementation
[BurstCompile]
static class GrassBuilder
{
    // Public entry point
    public static void Build
      (float time,
       in GrassConfig cfg,
       Mesh mesh)
    {
        var vcount = cfg.InstanceCount * (cfg.Subdivision + 1) * 2;
        var icount = cfg.InstanceCount * cfg.Subdivision * 6;

        using var vbuf = Util.NewTempArray<float3>(vcount);
        using var ibuf = Util.NewTempArray<uint>(icount);

        BakeVertices(cfg, vbuf);
        BakeIndices(cfg, ibuf);

        // Mesh object construction
        mesh.Clear();
        mesh.indexFormat = IndexFormat.UInt32;
        mesh.SetVertices(vbuf);
        mesh.SetIndices(ibuf, MeshTopology.Triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    [BurstCompile]
    static void BakeVertices(in GrassConfig cfg, in RawSpan<float3> raw_vspan)
    {
        var vspan = raw_vspan.AsSpan();
        var (offs, idx) = (0, 0u);

        var rand = new Random(cfg.Seed);

        for (var i = 0; i < cfg.InstanceCount; i++)
        {
            var p0 = rand.NextFloat2OnDisk() * cfg.SpawnRadius;

            var str = math.float2(0, 0);

            var rot = rand.NextFloat(0.03f, 0.15f);
            var mrot = math.float2x2(math.cos(rot), -math.sin(rot), math.sin(rot), math.cos(rot));
            var sss = math.float2(cfg.BladeHeight / cfg.Subdivision * rand.NextFloat(0.75f, 1.4f), 0);

            var tr = float4x4.TRS(math.float3(p0, 0).xzy,
                                  quaternion.RotateY(rand.NextFloat(-0.2f, 0.2f)), 1);

            for (var j = 0; j < cfg.Subdivision + 1; j++)
            {
                var p = (float)j / cfg.Subdivision;

                //var p2 = p * p * p * p * p;
                var ext = cfg.BladeWidth * 0.5f * (1 - 0.4f * p);

                var x0 = p0.x*0 - ext;
                var x1 = p0.x*0 + ext;

                str += sss;
                sss = math.mul(mrot, sss);

                //vspan[offs++] = math.float3(x0, str.x, p0.y + str.y);
                //vspan[offs++] = math.float3(x1, str.x, p0.y + str.y);
                vspan[offs++] = math.mul(tr, math.float4(-ext, str, 1)).xyz;
                vspan[offs++] = math.mul(tr, math.float4(+ext, str, 1)).xyz;
            }
        }
    }

    [BurstCompile]
    static void BakeIndices(in GrassConfig cfg, in RawSpan<uint> raw_ispan)
    {
        var ispan = raw_ispan.AsSpan();
        var (offs, idx) = (0, 0u);

        for (var i = 0; i < cfg.InstanceCount; i++)
        {
            for (var j = 0; j < cfg.Subdivision; j++)
            {
                ispan[offs++] = idx;
                ispan[offs++] = idx + 1;
                ispan[offs++] = idx + 2;

                ispan[offs++] = idx + 2;
                ispan[offs++] = idx + 1;
                ispan[offs++] = idx + 3;

                idx += 2;
            }

            idx += 2;
        }
    }
}

} // namespace Sketch
