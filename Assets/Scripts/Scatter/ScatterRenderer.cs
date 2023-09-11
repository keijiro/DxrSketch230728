using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Sketch {

[ExecuteInEditMode]
public sealed class ScatterRenderer
  : MonoBehaviour, ITimeControl, IPropertyPreview
{
    #region Editable properties

    [field:SerializeField]
    public ScatterConfig Config { get; set; } = ScatterConfig.Default();

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
      => new ScatterUpdateJob()
           { Config = Config, Time = Time, Root = transform.localToWorldMatrix }
           .Schedule(_pool.Xforms).Complete();

    #endregion

    #region ITimeControl / IPropertyPreview implementation

    public void OnControlTimeStart() {}
    public void OnControlTimeStop() {}
    public void SetTime(double time) => Time = (float)time;
    public void GatherProperties(PlayableDirector dir, IPropertyCollector drv)
      => drv.AddFromName<ScatterRenderer>(gameObject, "<Time>k__BackingField");

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
        _pool.Capacity = Config.InstanceCount;
        _pool.Meshes = Meshes;
        _pool.Material = Material;
        _pool.RandomSeed = Config.Seed;
        UpdateXforms();
    }

    #endregion
}

} // namespace Sketch
