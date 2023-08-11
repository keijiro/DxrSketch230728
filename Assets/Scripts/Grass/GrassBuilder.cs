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
    public int BandsPerInstance;
    public int PointsPerRing;
    public float RingRadius;
    public float Height;
    public uint Seed;

    // Default configuration
    public static GrassConfig Default()
      => new GrassConfig()
        { InstanceCount = 100,
          SpawnRadius = 10,
          BandsPerInstance = 10,
          PointsPerRing = 4,
          RingRadius = 0.1f,
          Height = 1,
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
        var fcount = cfg.InstanceCount *
          cfg.BandsPerInstance * cfg.PointsPerRing;

        using var vbuf = Util.NewTempArray<float3>(fcount * 4);
        using var ibuf = Util.NewTempArray<uint>(fcount * 6);

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
        var bandHeight = cfg.Height / (cfg.BandsPerInstance * 2);

        for (var i = 0; i < cfg.InstanceCount; i++)
        {
            var p0 = rand.NextFloat2OnDisk() * cfg.SpawnRadius;

            for (var j = 0; j < cfg.BandsPerInstance; j++)
            {
                for (var k = 0; k < cfg.PointsPerRing; k++)
                {
                    var rad = cfg.RingRadius *
                      (1 - math.smoothstep(0, cfg.BandsPerInstance * 2, j));
                    var phi = math.PI * 2 * k / cfg.PointsPerRing;
                    var x = math.cos(phi) * rad + p0.x;
                    var z = math.sin(phi) * rad + p0.y;
                    var y1 = bandHeight * j * 2;
                    var y2 = y1 + bandHeight;
                    var v1 = math.float3(x, y1, z);
                    var v2 = math.float3(x, y2, z);
                    vspan[offs + 0] = vspan[offs + 2] = v1;
                    vspan[offs + 1] = vspan[offs + 3] = v2;
                    offs += 4;
                }
            }
        }
    }

    [BurstCompile]
    static void BakeIndices(in GrassConfig cfg, in RawSpan<uint> raw_ispan)
    {
        var ispan = raw_ispan.AsSpan();
        var (offs, idx) = (0, 0u);
        var wrap = (uint)(cfg.PointsPerRing * 4 - 2);

        for (var i = 0; i < cfg.InstanceCount; i++)
        {
            for (var j = 0; j < cfg.BandsPerInstance; j++)
            {
                ispan[offs++] = idx + wrap;
                ispan[offs++] = idx + wrap + 1;
                ispan[offs++] = idx;

                ispan[offs++] = idx;
                ispan[offs++] = idx + wrap + 1;
                ispan[offs++] = idx + 1;

                idx += 2;

                for (var k = 1; k < cfg.PointsPerRing; k++)
                {
                    ispan[offs++] = idx;
                    ispan[offs++] = idx + 1;
                    ispan[offs++] = idx + 2;

                    ispan[offs++] = idx + 2;
                    ispan[offs++] = idx + 1;
                    ispan[offs++] = idx + 3;

                    idx += 4;
                }

                idx += 2;
            }
        }
    }
}

} // namespace Sketch
