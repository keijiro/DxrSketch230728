using Sketch.MeshKit;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine;

namespace Sketch {

[ExecuteInEditMode]
public sealed class StageRenderer
  : MonoBehaviour, ITimeControl, IPropertyPreview
{
    #region Editable attributes

    [field:SerializeField]
    public StageConfig Config { get; set; } = StageConfig.Default();

    [field:SerializeField]
    public Mesh[] Shapes { get; set; }

    [field:SerializeField]
    public float Time { get; set; }

    #endregion

    #region MonoBehaviour implementation

    void OnDisable()
    {
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
      => driver.AddFromName<StageRenderer>(gameObject, "<Time>k__BackingField");

    #endregion

    #region Private members

    ShapeCache _shapeCache = new ShapeCache();
    TempMesh _mesh = new TempMesh();

    void ConstructMesh(bool forceUpdate)
    {
        if (!forceUpdate) return;

        _mesh.Clear();

        if (Shapes == null || Shapes.Length == 0) return;
        _shapeCache.Update(Shapes);

        var instances = ShapeInstanceBuffer.Get(Config.TotalInstanceCount);
        StageBuilder.Build(Time, Config, _shapeCache, instances);
        Baker.Bake(instances, _mesh.Attach(this));
    }

    #endregion
}

} // namespace Sketch
