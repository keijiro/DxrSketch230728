using UnityEngine;
using UnityEngine.Splines;
using System;

namespace Sketch {

[ExecuteInEditMode]
sealed class SceneRenderer : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] SceneConfig _config = SceneConfig.Default();
    [SerializeField] Mesh[] _meshes = null;
    [SerializeField] uint _modelCapacity = 10000;

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
      => ConstructMesh(true);

    void OnSplineChanged(Spline spline, int knot, SplineModification mod)
      => OnValidate();

    void OnEnable()
      => Spline.Changed += OnSplineChanged;

    void OnDisable()
      => Spline.Changed -= OnSplineChanged;

    #endregion

    #region Private members

    Mesh _mesh;
    Modeler[] _sceneBuffer;

    void ConstructMesh(bool forceUpdate = false)
    {
        if (_mesh == null) return;

        // Scene buffer (modeler array) allocation
        if ((_sceneBuffer?.Length ?? 0) != _modelCapacity)
            _sceneBuffer = new Modeler[_modelCapacity];

        // Geometry cache (should live until mesh building)
        var caches = new GeometryCache[_meshes.Length];
        var shapes = new GeometryCacheRef[_meshes.Length];
        for (var i = 0; i < _meshes.Length; i++)
         {
             caches[i] = new GeometryCache(_meshes[i]);
             shapes[i] = caches[i];
         }

        // Model-level scene building
        var scene = SceneBuilder.Build(_config, shapes, _sceneBuffer);

        // Mesh building from the model array
        MeshBuilder.Build(scene, _mesh);

        foreach (var cache in caches) cache.Dispose();
    }

    #endregion
}

} // namespace Sketch
