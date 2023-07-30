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

    void Start()
    {
        _mesh = new Mesh();
        _mesh.hideFlags = HideFlags.DontSave;
        GetComponent<MeshFilter>().sharedMesh = _mesh;
    }

    void Update()
      => ConstructMesh();

    void OnDestroy()
    {
        _shapeCache?.Dispose();
        _shapeCache = null;

        Util.DestroyObject(_mesh);
        _mesh = null;
    }

    void OnValidate()
      => ConstructMesh();

    void OnSplineChanged(Spline spline, int knot, SplineModification mod)
      => OnValidate();

    void OnEnable()
      => UnityEngine.Splines.Spline.Changed += OnSplineChanged;

    void OnDisable()
      => UnityEngine.Splines.Spline.Changed -= OnSplineChanged;

    #endregion

    #region Private members

    Mesh _mesh;
    ShapeInstance[] _instances;
    ShapeCache _shapeCache;

    void ConstructMesh()
    {
        if (_mesh == null) return;
        if (Shapes == null || Shapes.Length == 0) return;

        if ((_instances?.Length ?? 0) != Config.InstanceCount)
            _instances = new ShapeInstance[Config.InstanceCount];

        if (_shapeCache == null)
            _shapeCache = new ShapeCache(Shapes);
        else
            _shapeCache.Update(Shapes);

        ChainBuilder.Build
          (Config, Spline.Spline, _shapeCache.ShapeRefs, _instances);

        Baker.Bake(_instances, _mesh);

    }

    #endregion
}

} // namespace Sketch
