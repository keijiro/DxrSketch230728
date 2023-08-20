using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Sketch {

[ExecuteInEditMode]
public sealed class StageRenderer
  : MonoBehaviour, ITimeControl, IPropertyPreview
{
    #region Editable properties

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

    InstancePool _pool;

    void UpdateXforms()
      => new StageUpdateJob() { Config = Config, Time = Time,
                                Parent = transform.localToWorldMatrix }
           .Schedule(_pool.Xforms).Complete();

    #endregion

    #region ITimeControl / IPropertyPreview implementation

    public void OnControlTimeStart() {}
    public void OnControlTimeStop() {}
    public void SetTime(double time) => Time = (float)time;
    public void GatherProperties(PlayableDirector dir, IPropertyCollector drv)
      => drv.AddFromName<StageRenderer>(gameObject, "<Time>k__BackingField");

    #endregion

    #region MonoBehaviour implementation

    void OnDisable()
    {
        _pool?.Dispose();
        _pool = null;
    }

    void LateUpdate()
    {
        if (_pool == null) _pool = new InstancePool();
        _pool.Capacity = Config.TotalInstanceCount;
        _pool.Meshes = Meshes;
        _pool.Material = Material;
        _pool.RandomSeed = Config.Seed;
        UpdateXforms();
    }

    #endregion
}

} // namespace Sketch
