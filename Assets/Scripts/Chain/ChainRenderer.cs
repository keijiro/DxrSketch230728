using MeshKit;
using UnityEngine;
using UnityEngine.Splines;

namespace Sketch {

[ExecuteInEditMode]
public sealed class ChainRenderer : MonoBehaviour
{
    #region Editable attributes

    [field:SerializeField] public SplineContainer Spline
      { get; set; }

    [field:SerializeField] public ChainConfig Config
      { get; set; } = ChainConfig.Default();

    [field:SerializeField] public Mesh[] Shapes
      { get; set; }

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        // Temporary mesh object
        _mesh = new Mesh();
        _mesh.hideFlags = HideFlags.DontSave;
        GetComponent<MeshFilter>().sharedMesh = _mesh;
    }

    void Update()
      => ConstructMesh();

    void OnDestroy()
      => Util.DestroyObject(_mesh);

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

    void ConstructMesh()
    {
        if (_mesh == null) return;
        if (Shapes == null || Shapes.Length == 0) return;

        // Instance buffer allocation
        if ((_instances?.Length ?? 0) != Config.InstanceCount)
            _instances = new ShapeInstance[Config.InstanceCount];

        // Shape array (should live until mesh building)
        var caches = new Shape[Shapes.Length];
        var shapes = new ShapeRef[Shapes.Length];
        for (var i = 0; i < Shapes.Length; i++)
        {
            caches[i] = new Shape(Shapes[i]);
            shapes[i] = caches[i];
        }

        // Mesh building
        ChainBuilder.Build(Config, Spline.Spline, shapes, _instances);
        Baker.Bake(_instances, _mesh);

        foreach (var cache in caches) cache.Dispose();
    }

    #endregion
}

} // namespace Sketch
