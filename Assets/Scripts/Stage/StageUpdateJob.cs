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
    public float InstanceScale;

    [Tooltip("The random number seed")]
    public uint Seed;

    public int TotalInstanceCount
      => (int)(CellCounts.x * CellCounts.y);

    // Default configuration
    public static StageConfig Default()
      => new StageConfig()
        { CellCounts = 10,
          CellSize = 0.1f,
          InstanceScale = 0.1f,
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

        var o1 = math.float2(Time * 0.1f, 0);
        var np = math.float2(x, z);
        var y = noise.snoise(np * 0.5f + o1) * 0.2f;
        y = math.max(0, y * y * y);

        var y2 = noise.snoise(np * 3);
        y *= math.max(0, y2 * y2 * y2) * 300;

        var rot = quaternion.RotateZ(0.4f);
        var scale = Config.InstanceScale;

        var vy = math.mul(rot, math.float3(0, y, 0));

        var pos = math.float3(x, 0, z) + vy;

        transform.localPosition = math.mul(Parent, math.float4(pos, 1)).xyz;
        transform.localRotation = math.mul(math.quaternion(Parent), rot);
        transform.localScale = (float3)(math.length(Parent.c0.xyz) * scale);
    }
}

} // namespace Sketch
