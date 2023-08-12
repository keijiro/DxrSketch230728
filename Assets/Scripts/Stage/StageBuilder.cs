using Klak.Math;
using Sketch.Common;
using Sketch.MeshKit;
using System;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Sketch {

// Configuration struct
[Serializable]
public struct StageConfig
{
    public int2 CellCounts;
    public float CellSize;
    public float InstanceSize;

    [Tooltip("The random number seed")]
    public uint Seed;

    public int TotalInstanceCount
      => CellCounts.x * CellCounts.y;

    // Default configuration
    public static StageConfig Default()
      => new StageConfig()
        { CellCounts = 10,
          CellSize = 0.5f,
          InstanceSize = 0.5f,
          Seed = 1 };
}

// Builder implementation
[BurstCompile]
static class StageBuilder
{
    // Public entry point
    public static void Build
      (float time,
       in StageConfig config,
       ReadOnlySpan<ShapeRef> shapes,
       Span<ShapeInstance> output)
    {
        unsafe { Build_burst(time, config, shapes, output); }
    }

    // Bursted entry point
    [BurstCompile]
    unsafe public static void Build_burst
      (float time,
       in StageConfig cfg,
       in ReadOnlyRawSpan<ShapeRef> raw_shapes,
       in RawSpan<ShapeInstance> raw_output)
    {
        var shapes = raw_shapes.AsReadOnlySpan();
        var output = raw_output.AsSpan();
        var rand = new Random(cfg.Seed);

        var idx = 0;
        for (var i = 0; i < cfg.CellCounts.y; i++)
        {
            for (var j = 0; j < cfg.CellCounts.x; j++)
            {
                var x = (i - (cfg.CellCounts.x - 0.5f) * 0.5f) * cfg.CellSize;
                var z = (j - (cfg.CellCounts.y - 0.5f) * 0.5f) * cfg.CellSize;

                var pos = math.float3(x, 0, z);
                var rot = quaternion.identity;
                var scale = cfg.InstanceSize;

                // Random shape
                var shape = shapes[rand.NextInt(shapes.Length)];

                output[idx++] = new MeshKit.ShapeInstance(position: pos,
                                                          rotation: rot,
                                                          scale: scale,
                                                          color: 1,
                                                          shape: shape);
            }
        }
    }
}

} // namespace Sketch
