using UnityEngine;

namespace Sketch {

sealed class TempMesh
{
    Mesh _mesh;

    public void Clear()
      => _mesh?.Clear();

    public void Destroy()
    {
        if (_mesh != null)
        {
            if (UnityEngine.Application.isPlaying)
                Object.Destroy(_mesh);
            else
                Object.DestroyImmediate(_mesh);
            _mesh = null;
        }
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
