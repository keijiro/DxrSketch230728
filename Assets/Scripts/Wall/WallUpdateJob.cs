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
    public int InstanceCount;

    public float2 Height;
    public float2 Radius;
    public float4 Size;
    public float2 Speed;

    [Tooltip("The random number seed")]
    public uint Seed;

    // Default configuration
    public static WallConfig Default()
      => new WallConfig()
        { InstanceCount = 100,
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
        var s = rand.NextFloat2(Config.Size.xy, Config.Size.zw);
        var vr = rand.NextFloat(Config.Speed.x, Config.Speed.y);

        var pos = math.float3(0, y, l);
        var rot = quaternion.RotateY(vr * (Time + 1000));

        transform.localPosition = math.mul(rot, pos);
        transform.localRotation = rot;
        transform.localScale = math.float3(s, 1);
    }
}

} // namespace Sketch
