using Sketch.MeshKit;
using UnityEngine.Playables;
using UnityEngine.Splines;
using UnityEngine.Timeline;
using UnityEngine;

namespace Sketch {

[ExecuteInEditMode]
public sealed class ChainRenderer
  : MonoBehaviour, ITimeControl, IPropertyPreview
{
    #region Editable attributes

    [field:SerializeField]
    public SplineContainer Spline { get; set; }

    [field:SerializeField]
    public ChainConfig Config { get; set; } = ChainConfig.Default();

    [field:SerializeField]
    public Mesh[] Shapes { get; set; }

    [field:SerializeField]
    public float Time { get; set; }

    #endregion

    #region MonoBehaviour implementation

    void OnEnable()
      => SplineObserver.OnModified += OnSplineModified;

    void OnDisable()
    {
        SplineObserver.OnModified -= OnSplineModified;
        _shapeCache.Destroy();
        _mesh.Destroy();
    }

    void Update()
      => ConstructMesh(true);

    #endregion

    #region ITimeControl implementation

    public void OnControlTimeStart() {}
    public void OnControlTimeStop() {}
    public void SetTime(double time) => Time = (float)time;

    public void GatherProperties(PlayableDirector director, IPropertyCollector driver)
      => driver.AddFromName<ChainRenderer>(gameObject, "<Time>k__BackingField");

    #endregion

    #region Private members

    ShapeCache _shapeCache = new ShapeCache();
    TempMesh _mesh = new TempMesh();

    void OnSplineModified(Spline spline)
      => ConstructMesh(spline == Spline.Spline);

    void ConstructMesh(bool forceUpdate)
    {
        if (!forceUpdate) return;

        _mesh.Clear();

        if (Shapes == null || Shapes.Length == 0) return;
        _shapeCache.Update(Shapes);

        var instances = ShapeInstanceBuffer.Get(Config.InstanceCount);
        ChainBuilder.Build(Time, Config, Spline.Spline, _shapeCache, instances);
        Baker.Bake(instances, _mesh.Attach(this));
    }

    #endregion
}

} // namespace Sketch
