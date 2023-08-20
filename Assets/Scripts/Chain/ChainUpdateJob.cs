using Klak.Math;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Splines;

using Random = Unity.Mathematics.Random;

namespace Sketch {

// Configuration struct
[System.Serializable]
public struct ChainConfig
{
    [Tooltip("The duration of the animation")]
    public float Lifetime;

    [Tooltip("The delay time between the head/tail")]
    public float Delay;

    [Tooltip("The fading duration (min, max)")]
    public float2 Fade;

    [Tooltip("The angular speed (in, mid, out)")]
    public float3 Spin;

    [Tooltip("The total number of the instances")]
    public int InstanceCount;

    [Tooltip("The random displacement from the base curve")]
    public float Displacement;

    [Tooltip("The bloom angle (min, max)")]
    public float2 Bloom;

    [Tooltip("The instance scale (min, max, exp)")]
    public float3 Scale;

    [Tooltip("The random number seed")]
    public uint Seed;

    // Default configuration
    public static ChainConfig Default()
      => new ChainConfig()
        { Lifetime = 5,
          Delay = 1,
          Fade = math.float2(0.5f, 1),
          Spin = 1,
          InstanceCount = 100,
          Displacement = 0.1f,
          Bloom = math.float2(0.1f, 0.2f),
          Scale = math.float3(0.2f, 1, 1.5f),
          Seed = 1 };
}

[BurstCompile]
struct ChainUpdateJob : IJobParallelForTransform
{
    public ChainConfig Config;
    public NativeSpline Spline;
    public float Time;

    [BurstCompile]
    public void Execute(int index, TransformAccess transform)
    {
        var cfg = Config;
        var rand = Random.CreateFromIndex(cfg.Seed ^ (uint)index);

        var param = (float)index / cfg.InstanceCount;

        // Spline point sample
        float3 pos, tan, up;
        Spline.Evaluate(param, out pos, out tan, out up);

        // Random rotation
        var rz = rand.NextFloat(math.PI * 2);
        var rx = rand.NextFloat(cfg.Bloom.x, cfg.Bloom.y) * math.PI / 2;

        var t = Time - param * cfg.Delay;
        var fade_dur = rand.NextFloat(cfg.Fade.x, cfg.Fade.y);
        var fade1 = math.smoothstep(0, fade_dur, t);
        var fade2 = math.smoothstep(cfg.Lifetime - fade_dur, cfg.Lifetime, t);
        rz += math.dot(cfg.Spin, math.float3(fade1, t, fade2));

        var rot = quaternion.LookRotation(tan, up);
        rot = math.mul(rot, quaternion.RotateZ(rz));
        rot = math.mul(rot, quaternion.RotateX(rx));

        // Random scale
        var scale = math.pow(rand.NextFloat(), cfg.Scale.z);
        scale = math.lerp(cfg.Scale.x, cfg.Scale.y, scale);
        scale *= (fade1 - fade2);

        var dis = math.mul(rot, math.float3(0, 0, 1));
        pos += dis * rand.NextFloat(cfg.Displacement);

        transform.localPosition = pos;
        transform.localRotation = rot;
        transform.localScale = (float3)scale;
    }
}

} // namespace Sketch
