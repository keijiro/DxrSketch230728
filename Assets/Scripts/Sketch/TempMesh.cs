using UnityEngine;

namespace Sketch {

// Utility for managing a "Don't Save" mesh object
sealed class TempMesh
{
    Mesh _mesh;

    public void Clear()
      => _mesh?.Clear();

    public void Destroy()
    {
        Common.Util.DestroyObject(_mesh);
        _mesh = null;
    }

    public Mesh Attach(Component parent)
    {
        if (_mesh == null)
        {
            _mesh = new Mesh();
            _mesh.hideFlags = HideFlags.DontSave;
            parent.GetComponent<MeshFilter>().sharedMesh = _mesh;
        }
        return _mesh;
    }
}

} // namespace Sketch
