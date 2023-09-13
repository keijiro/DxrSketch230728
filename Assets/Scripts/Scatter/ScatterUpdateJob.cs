using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

using Random = Unity.Mathematics.Random;

namespace Sketch {

// Configuration struct
[System.Serializable]
public struct ScatterConfig
{
    [Tooltip("The total number of the instances")]
    public int InstanceCount;

    [Tooltip("The extent of the spawn box")]
    public float3 Extent;

    [Tooltip("The duration of the animation")]
    public float Lifetime;

    [Tooltip("The fading duration")]
    public float Fade;

    [Tooltip("Initial random rotation")]
    public bool RandomRotation;

    [Tooltip("The angular speed (min, max)")]
    public float2 Spin;

    [Tooltip("The instance scale (min, max, exp)")]
    public float3 Scale;

    [Tooltip("The speed parameters (min, max, spread)")]
    public float3 Speed;

    [Tooltip("The turbulence parameters (freq, amount)")]
    public float2 Noise;

    [Tooltip("The random number seed")]
    public uint Seed;

    // Default configuration
    public static ScatterConfig Default()
      => new ScatterConfig()
        { InstanceCount = 16,
          Extent = 1,
          Lifetime = 2,
          Fade = 0.5f,
          RandomRotation = true,
          Spin = 0.5f,
          Scale = math.float3(0.2f, 1, 1.5f),
          Speed = math.float3(0.1f, 1, 0.1f),
          Noise = math.float2(0.1f, 0.1f),
          Seed = 1 };
}

[BurstCompile]
struct ScatterUpdateJob : IJobParallelForTransform
{
    public float4x4 Root;
    public float Time;
    public ScatterConfig Config;

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
        var rot = Config.RandomRotation ?
          rand.NextQuaternionRotation() : quaternion.identity;
        var raxis = rand.NextFloat3Direction();
        var rvel = rand.NextFloat(Config.Spin.x, Config.Spin.y) * period;
        rot = math.mul(rot, quaternion.AxisAngle(raxis, rvel * time01));
        rot = math.mul(math.quaternion(Root), rot);

        // Velocity
        var vel = math.float3(0, 0, 1);
        vel = math.lerp(vel, rand.NextFloat3Direction(), Config.Speed.z);
        vel = math.normalize(vel);
        vel *= rand.NextFloat(Config.Speed.x, Config.Speed.y);

        // Position without turbulence
        var pos = math.transform(Root, 0);
        pos += rand.NextFloat3(-0.5f, 0.5f) * Config.Extent;
        pos += vel * time01 * period;

        // Tubulence
        float3 grad1, grad2;
        noise.snoise(pos.xyz *  Config.Noise.x, out grad1);
        noise.snoise(pos.zyx * -Config.Noise.x, out grad2);
        pos += math.cross(grad1, grad2) * Config.Noise.y;

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
