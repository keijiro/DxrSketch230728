using MeshKit;
using UnityEngine;
using UnityEngine.Splines;

namespace Sketch {

[ExecuteInEditMode]
public sealed class ChainRenderer : MonoBehaviour
{
    #region Editable attributes

    [field:SerializeField]
    public SplineContainer Spline { get; set; }

    [field:SerializeField]
    public ChainConfig Config { get; set; } = ChainConfig.Default();

    [field:SerializeField]
    public Mesh[] Shapes { get; set; }

    #endregion

    #region MonoBehaviour implementation

    void OnEnable()
      => UnityEngine.Splines.Spline.Changed += OnSplineChanged;

    void OnDisable()
    {
        UnityEngine.Splines.Spline.Changed -= OnSplineChanged;
        _shapeCache.Destroy();
        _mesh.Destroy();
    }

    void Update()
      => ConstructMesh();

    #endregion

    #region Private members

    ShapeCache _shapeCache = new ShapeCache();
    TempMesh _mesh = new TempMesh();

    void OnSplineChanged(Spline spline, int knot, SplineModification mod)
      => ConstructMesh();

    void ConstructMesh()
    {
        _mesh.Clear();

        if (Shapes == null || Shapes.Length == 0) return;
        _shapeCache.Update(Shapes);

        var instances = ShapeInstanceBuffer.Get(Config.InstanceCount);
        ChainBuilder.Build(Config, Spline.Spline, _shapeCache, instances);
        Baker.Bake(instances, _mesh.Attach(this));
    }

    #endregion
}

} // namespace Sketch
