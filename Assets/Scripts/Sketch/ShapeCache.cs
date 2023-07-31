using System;
using MeshKit;
using Mesh = UnityEngine.Mesh;

namespace Sketch {

sealed class ShapeCache
{
    #region Public members

    public static implicit operator ReadOnlySpan<ShapeRef>(ShapeCache self)
      => self.ShapeRefs;

    public ReadOnlySpan<ShapeRef> ShapeRefs => _refs;

    public void Destroy()
      => Release();

    public void Update(ReadOnlySpan<Mesh> meshes)
    {
        if (CheckCached(meshes)) return;

        Release();

        var count = meshes.Length;
        _guids = new int[count];
        _shapes = new Shape[count];
        _refs = new ShapeRef[count];

        for (var i = 0; i < count; i++)
        {
            _guids[i] = meshes[i].GetInstanceID();
            _shapes[i] = new Shape(meshes[i]);
            _refs[i] = _shapes[i];
        }
    }

    #endregion

    #region Private members

    int[] _guids;
    Shape[] _shapes;
    ShapeRef[] _refs;

    bool CheckCached(ReadOnlySpan<Mesh> meshes)
    {
        if (_shapes == null) return false;
        if (_shapes.Length != meshes.Length) return false;
        for (var i = 0; i < _shapes.Length; i++)
            if (_guids[i] != meshes[i].GetInstanceID()) return false;
        return true;
    }

    void Release()
    {
        if (_shapes == null) return;
        foreach (var shape in _shapes) shape.Dispose(); 
        (_guids, _shapes, _refs) = (null, null, null);
    }

    #endregion
}

} // namespace Sketch
