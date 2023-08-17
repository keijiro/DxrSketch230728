using Sketch.MeshKit;
using System.Linq;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine.Jobs;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine;

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

public sealed class StageRenderer
  : MonoBehaviour, ITimeControl, IPropertyPreview
{
    #region Editable attributes

    [field:SerializeField]
    public StageConfig Config { get; set; } = StageConfig.Default();

    [field:SerializeField]
    public GameObject Prefab { get; set; }

    [field:SerializeField]
    public float Time { get; set; }

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        var sheet = new MaterialPropertyBlock();
        _instances = new GameObject[TotalInstanceCount];

        for (var i = 0; i < TotalInstanceCount; i++)
        {
            var go = Instantiate(Prefab);
            go.hideFlags = HideFlags.HideAndDontSave;
            go.GetComponent<Renderer>().SetPropertyBlock(sheet);
            go.transform.parent = transform;
            _instances[i] = go;
        }

        _xforms = new TransformAccessArray
          (_instances.Select(go => go.transform).ToArray());
        UpdateXforms();
    }

    void Update()
      => UpdateXforms();

    void LateUpdate()
    {
        if (Application.isPlaying && !_isTimeControlled)
            Time += UnityEngine.Time.deltaTime;
    }

    void OnDestroy()
    {
        foreach (var go in _instances) Destroy(go);
        _instances = null;
        _xforms.Dispose();
    }

    #endregion

    #region ITimeControl implementation

    bool _isTimeControlled;

    public void OnControlTimeStart() => _isTimeControlled = true;
    public void OnControlTimeStop() => _isTimeControlled = false;
    public void SetTime(double time) => Time = (float)time;

    public void GatherProperties(PlayableDirector director, IPropertyCollector driver)
      => driver.AddFromName<StageRenderer>(gameObject, "<Time>k__BackingField");

    #endregion

    #region Private members

    GameObject[] _instances;
    TransformAccessArray _xforms;

    uint TotalInstanceCount
      => Config.CellCounts.x * Config.CellCounts.y;

    void UpdateXforms()
      => new StageXformJob(){ Time = Time, Config = Config }.Schedule(_xforms);

    #endregion
}

[BurstCompile]
struct StageXformJob : IJobParallelForTransform
{
    public float Time;
    public StageConfig Config;

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

        transform.localPosition = pos;
        transform.localRotation = rot;
        transform.localScale = (float3)0.1f;
    }
}

} // namespace Sketch
