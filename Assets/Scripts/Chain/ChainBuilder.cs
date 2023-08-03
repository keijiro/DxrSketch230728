using Klak.Math;
using Sketch.Common;
using Sketch.MeshKit;
using System;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine.Splines;
using Random = Unity.Mathematics.Random;

namespace Sketch {

// Configuration struct
[Serializable]
public struct ChainConfig
{
    public int InstanceCount;
    public float Displacement;
    public uint Seed;

    // Default configuration
    public static ChainConfig Default()
      => new ChainConfig()
        { InstanceCount = 100,
          Displacement = 0.1f,
          Seed = 1 };
}

// Builder implementation
[BurstCompile]
static class ChainBuilder
{
    // Public entry point
    public static void Build
      (in ChainConfig config,
       Spline spline,
       ReadOnlySpan<ShapeRef> shapes,
       Span<ShapeInstance> output)
    {
        using var tempSpline = new NativeSpline(spline);
        unsafe { Build_burst(config, &tempSpline, shapes, output); }
    }

    // Bursted entry point
    [BurstCompile]
    unsafe public static void Build_burst
      (in ChainConfig cfg,
       in NativeSpline* ptr_spline,
       in ReadOnlyRawSpan<ShapeRef> raw_shapes,
       in RawSpan<ShapeInstance> raw_output)
    {
        var spline = *ptr_spline;
        var shapes = raw_shapes.AsReadOnlySpan();
        var output = raw_output.AsSpan();

        var rand = new Random(cfg.Seed);

        for (var i = 0; i < cfg.InstanceCount; i++)
        {
            var t = (float)i / cfg.InstanceCount;

            // Spline point sample
            float3 pos, tan, up;
            spline.Evaluate(t, out pos, out tan, out up);

            // Random rotation
            var rz = rand.NextFloat(math.PI * 2);
            var rx = rand.NextFloat(0.2f, 0.4f) * math.PI;

            var rot = quaternion.LookRotation(tan, up);
            rot = math.mul(rot, quaternion.RotateZ(rz));
            rot = math.mul(rot, quaternion.RotateX(rx));

            var dis = math.mul(rot, math.float3(0, 0, 1));
            pos += dis * rand.NextFloat(cfg.Displacement);

            // Random scale
            var scale = math.pow(rand.NextFloat(), 1.5f);
            scale = math.lerp(0.2f, 1, scale);

            // Random shape
            var shape = shapes[rand.NextInt(shapes.Length)];

            output[i] = new MeshKit.ShapeInstance(position: pos,
                                                  rotation: rot,
                                                  scale: scale,
                                                  color: 1,
                                                  shape: shape);
        }
    }
}

} // namespace Sketch
