using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

using Random = Unity.Mathematics.Random;

namespace Sketch {

// Configuration struct
[System.Serializable]
public struct StarConfig
{
    [Tooltip("The total number of the instances")]
    public int InstanceCount;

    [Tooltip("The duration of the animation")]
    public float Lifetime;

    [Tooltip("The fading duration")]
    public float Fade;

    [Tooltip("The angular speed (min, max)")]
    public float2 Spin;

    [Tooltip("The random displacement")]
    public float Displacement;

    [Tooltip("The instance scale (min, max, exp)")]
    public float3 Scale;

    [Tooltip("The random number seed")]
    public uint Seed;

    // Default configuration
    public static StarConfig Default()
      => new StarConfig()
        { InstanceCount = 16,
          Lifetime = 3,
          Fade = 0.5f,
          Spin = 0.5f,
          Displacement = 0.1f,
          Scale = math.float3(0.2f, 1, 1.5f),
          Seed = 1 };
}

[BurstCompile]
struct StarUpdateJob : IJobParallelForTransform
{
    public float4x4 Root;
    public float Time;
    public StarConfig Config;

    [BurstCompile]
    public void Execute(int index, TransformAccess transform)
    {
        // Animation period
        var period = Config.Lifetime + Config.Fade * 2;

        // Normalized time parameter
        var ntime = (float)index / Config.InstanceCount;
        ntime += Time / period;

        // 0-1 time paramter
        var time01 = math.frac(ntime);

        // Instance index and random number generator
        index += (int)ntime * Config.InstanceCount;
        var rand = Random.CreateFromIndex(Config.Seed ^ (uint)index);

        // Fade in/out
        var ifade = Config.Fade / period;
        var fade1 = math.smoothstep(0, ifade, time01);
        var fade2 = math.smoothstep(1 - ifade, 1, time01);

        // Rotation
        var rot = rand.NextQuaternionRotation();
        var raxis = rand.NextFloat3Direction();
        var rvel = rand.NextFloat(Config.Spin.x, Config.Spin.y) * period;
        rot = math.mul(rot, quaternion.AxisAngle(raxis, rvel * time01));
        rot = math.mul(math.quaternion(Root), rot);

        // Position
        var pos = math.transform(Root, 0);
        var disp = math.float3(0, 0, rand.NextFloat(Config.Displacement));
        pos += math.mul(rot, disp);

        // Transform components
        var scale = math.pow(rand.NextFloat(), Config.Scale.z);
        scale = math.lerp(Config.Scale.x, Config.Scale.y, scale);
        scale *= (fade1 - fade2);
        scale = math.length(math.mul(Root, math.float4(scale, 0, 0, 0)));

        // Output
        transform.localPosition = pos;
        transform.localRotation = rot;
        transform.localScale = (float3)scale;
    }
}

} // namespace Sketch
