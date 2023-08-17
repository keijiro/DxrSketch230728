using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Sketch {

[ExecuteInEditMode]
public sealed class StageRenderer
  : MonoBehaviour, ITimeControl, IPropertyPreview
{
    #region Editable attributes

    [field:SerializeField]
    public StageConfig Config { get; set; } = StageConfig.Default();

    [field:SerializeField]
    public Mesh[] Meshes { get; set; }

    [field:SerializeField]
    public Material Material { get; set; }

    [field:SerializeField]
    public float Time { get; set; }

    #endregion

    #region Private members

    NodePool _pool;

    uint TotalInstanceCount
      => Config.CellCounts.x * Config.CellCounts.y;

    void UpdateXforms()
      => new StageXformJob()
           { Config = Config, Time = Time,
             Parent = transform.localToWorldMatrix }.Schedule(_pool.Xforms);

    #endregion

    #region ITimeControl implementation

    bool _isTimeControlled;

    public void OnControlTimeStart() => _isTimeControlled = true;
    public void OnControlTimeStop() => _isTimeControlled = false;
    public void SetTime(double time) => Time = (float)time;

    public void GatherProperties
      (PlayableDirector director, IPropertyCollector driver)
      => driver.AddFromName<StageRenderer>
           (gameObject, "<Time>k__BackingField");

    #endregion

    #region MonoBehaviour implementation

    void Update()
    {
        if (_pool == null) _pool = new NodePool();
        _pool.Capacity = (int)TotalInstanceCount;
        _pool.Shapes = Meshes;
        _pool.Material = Material;
        _pool.RandomSeed = Config.Seed;
        UpdateXforms();
    }

    void LateUpdate()
    {
        if (Application.isPlaying && !_isTimeControlled)
            Time += UnityEngine.Time.deltaTime;
    }

    void OnDisable()
    {
        _pool?.Dispose();
        _pool = null;
    }

    #endregion
}

} // namespace Sketch
