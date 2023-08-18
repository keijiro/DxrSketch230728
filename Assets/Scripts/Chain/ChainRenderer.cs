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
    bool _isTimeControlled;

    void UpdateXforms()
      => new ChainUpdateJob()
           { Config = Config, Spline = new NativeSpline(Spline.Spline, Allocator.TempJob), Time = Time }
           .Schedule(_pool.Xforms);

    void OnSplineModified(Spline spline)
      => UpdateXforms();

    #endregion

    #region ITimeControl implementation

    public void OnControlTimeStart() => _isTimeControlled = true;
    public void OnControlTimeStop() => _isTimeControlled = false;
    public void SetTime(double time) => Time = (float)time;

    public void GatherProperties
      (PlayableDirector director, IPropertyCollector driver)
      => driver.AddFromName<StageRenderer>
           (gameObject, "<Time>k__BackingField");

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

    void Update()
    {
        if (_pool == null) _pool = new InstancePool();
        _pool.Capacity = Config.InstanceCount;
        _pool.Meshes = Meshes;
        _pool.Material = Material;
        _pool.RandomSeed = Config.Seed;
        UpdateXforms();
    }

    void LateUpdate()
    {
        if (Application.isPlaying && !_isTimeControlled)
            Time += UnityEngine.Time.deltaTime;
    }

    #endregion
}

} // namespace Sketch
