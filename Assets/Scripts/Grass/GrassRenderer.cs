using UnityEngine;

namespace Sketch {

[ExecuteInEditMode]
public sealed class GrassRenderer : MonoBehaviour
{
    #region Editable attributes

    [field:SerializeField]
    public GrassConfig Config { get; set; } = GrassConfig.Default();

    #endregion

    #region MonoBehaviour implementation

    void OnDestroy()
      => _mesh.Destroy();

    void Update()
      => ConstructMesh();

    #endregion

    #region Private members

    TempMesh _mesh = new TempMesh();

    void ConstructMesh()
    {
        _mesh.Clear();
        GrassBuilder.Build(Time.time, Config, _mesh.Attach(this));
    }

    #endregion
}

} // namespace Sketch
