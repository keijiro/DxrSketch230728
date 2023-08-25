using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Playables;
using UnityEngine.Splines;
using UnityEngine.Timeline;

namespace Sketch {

[ExecuteInEditMode]
public sealed class ChainRenderer
  : MonoBehaviour, ITimeControl, IPropertyPreview
{
    #region Editable properties

    [field:SerializeField]
    public SplineContainer Spline { get; set; }

    [field:SerializeField]
    public ChainConfig Config { get; set; } = ChainConfig.Default();

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
    {
        using var temp = new NativeSpline(Spline.Spline, Allocator.TempJob);
        new ChainUpdateJob() { Config = Config, Spline = temp, Time = Time }
          .Schedule(_pool.Xforms).Complete();
    }

    void OnSplineModified(Spline spline)
      => UpdateXforms();

    #endregion

    #region ITimeControl / IPropertyPreview implementation

    public void OnControlTimeStart() {}
    public void OnControlTimeStop() {}
    public void SetTime(double time) => Time = (float)time;
    public void GatherProperties(PlayableDirector dir, IPropertyCollector drv)
      => drv.AddFromName<ChainRenderer>(gameObject, "<Time>k__BackingField");

    #endregion

    #region MonoBehaviour implementation

    void OnEnable()
      => SplineObserver.OnModified += OnSplineModified;

    void OnDisable()
    {
        SplineObserver.OnModified -= OnSplineModified;
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
