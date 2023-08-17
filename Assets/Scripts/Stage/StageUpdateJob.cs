using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace Sketch {

// Configuration struct
[System.Serializable]
public struct StageConfig
{
    public uint2 CellCounts;
    public float CellSize;
    public float InstanceSize;

    [Tooltip("The random number seed")]
    public uint Seed;

    public int TotalInstanceCount
      => (int)(CellCounts.x * CellCounts.y);

    // Default configuration
    public static StageConfig Default()
      => new StageConfig()
        { CellCounts = 10,
          CellSize = 0.5f,
          InstanceSize = 0.5f,
          Seed = 1 };
}

[BurstCompile]
struct StageUpdateJob : IJobParallelForTransform
{
    public float Time;
    public StageConfig Config;
    public float4x4 Parent;

    [BurstCompile]
    public void Execute(int index, TransformAccess transform)
    {
        var i = index % Config.CellCounts.x;
        var j = index / Config.CellCounts.x;

        var x = (i - (Config.CellCounts.x - 1) * 0.5f) * Config.CellSize;
        var z = (j - (Config.CellCounts.y - 1) * 0.5f) * Config.CellSize;

        var o1 = math.float2(Time * 0.2f, 0);
        var np = math.float2(x, z) * 0.8f;
        var y = noise.snoise(np + o1) * 0.2f;
        y = math.max(0, y);

        var y2 = noise.snoise(np * 4);
        y *= y2 * y2 * y2 * y2 * 6;

        var rot = quaternion.RotateZ(0.4f);
        var scale = Config.InstanceSize;

        var vy = math.mul(rot, math.float3(0, y, 0));

        var pos = math.float3(x, 0, z) + vy;

        transform.localPosition = math.mul(Parent, math.float4(pos, 1)).xyz;
        transform.localRotation = rot;
        transform.localScale = (float3)0.1f;
    }
}

} // namespace Sketch
