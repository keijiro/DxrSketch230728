using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

using Random = Unity.Mathematics.Random;

namespace Sketch {

// Configuration struct
[System.Serializable]
public struct WallConfig
{
    [Tooltip("The total number of the instances")]
    public int InstanceCount;

    [Tooltip("The instance height (min, max)")]
    public float2 Height;

    [Tooltip("The radius of the circle (min, max)")]
    public float2 Radius;

    [Tooltip("The instance scale (min x-y, max x-y)")]
    public float4 Scale;

    [Tooltip("The angular speed (min, max)")]
    public float2 Speed;

    [Tooltip("The random number seed")]
    public uint Seed;

    // Default configuration
    public static WallConfig Default()
      => new WallConfig()
        { InstanceCount = 100,
          Height = math.float2(1, 2),
          Radius = math.float2(10, 11),
          Scale = math.float4(1, 1, 2, 2),
          Speed = math.float2(0.1f, 0.2f),
          Seed = 1 };
}

[BurstCompile]
struct WallUpdateJob : IJobParallelForTransform
{
    public float Time;
    public WallConfig Config;

    [BurstCompile]
    public void Execute(int index, TransformAccess transform)
    {
        var rand = Random.CreateFromIndex(Config.Seed ^ (uint)index);

        var y = rand.NextFloat(Config.Height.x, Config.Height.y);
        var l = rand.NextFloat(Config.Radius.x, Config.Radius.y);
        var s = rand.NextFloat2(Config.Scale.xy, Config.Scale.zw);
        var vr = rand.NextFloat(Config.Speed.x, Config.Speed.y);

        var pos = math.float3(0, y, l);
        var rot = quaternion.RotateY(vr * (Time + 1000));

        transform.localPosition = math.mul(rot, pos);
        transform.localRotation = rot;
        transform.localScale = math.float3(s, 1);
    }
}

} // namespace Sketch
