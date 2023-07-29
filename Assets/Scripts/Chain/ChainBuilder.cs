using MeshKit;
using System;
using UnityEngine.Splines;
using Unity.Burst;
using Unity.Mathematics;

using Random = Unity.Mathematics.Random;

namespace Sketch {

// Configuration struct
[Serializable]
public struct ChainConfig
{
    public int InstanceCount;
    public uint Seed;

    // Default configuration
    public static ChainConfig Default()
      => new ChainConfig()
        { InstanceCount = 100,
          Seed = 1 };
}

// Builder implementation
[BurstCompile]
static class ChainBuilder
{
    // Public entry point
    public static void Build(in ChainConfig config,
                             Spline spline,
                             ReadOnlySpan<ShapeRef> shapes,
                             Span<ShapeInstance> output)
    {
        using var tempSpline = new NativeSpline(spline);
        unsafe { Build_burst(config,
                             &tempSpline,
                             shapes.GetUntyped(),
                             output.GetUntyped()); }
    }

    // Bursted entry point
    [BurstCompile]
    unsafe public static void Build_burst(in ChainConfig cfg,
                                          in NativeSpline* p_spline,
                                          in UntypedReadOnlySpan u_shapes,
                                          in UntypedSpan u_output)
    {
        var spline = *p_spline;
        var shapes = u_shapes.GetTyped<ShapeRef>();
        var output = u_output.GetTyped<ShapeInstance>();

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
