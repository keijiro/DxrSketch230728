using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Jobs;

using Object = UnityEngine.Object;
using Random = Unity.Mathematics.Random;

namespace Sketch {

public class NodePool : IDisposable
{
    #region Public properties

    public int Capacity
      { get => _instances.Count; set => ChangeCapacity(value); }

    public ReadOnlySpan<Mesh> Shapes
      { get => _shapes; set => ResetShapes(value); }

    public Material Material
      { get => _material; set => ResetMaterial(value); }

    public uint RandomSeed
      { get => _randomSeed; set => ResetRandomSeed(value); }

    public TransformAccessArray Xforms => UpdateXforms();

    #endregion

    #region Public methods

    public NodePool()
      => _mpblock = new MaterialPropertyBlock();

    public void Dispose()
      => ChangeCapacity(0);

    #endregion

    #region Private members

    static readonly Type[] NodeComponents =
      { typeof(MeshFilter), typeof(MeshRenderer) };

    List<GameObject> _instances = new List<GameObject>();

    Mesh[] _shapes = new Mesh[] { null };
    Material _material;
    uint _randomSeed = 1;

    TransformAccessArray _xforms;
    MaterialPropertyBlock _mpblock;

    #endregion

    #region Allocation / deallocation

    void AddNewNode()
    {
        var i = _instances.Count;

        var go = new GameObject("Node", NodeComponents);
        go.hideFlags = HideFlags.HideAndDontSave;

        go.GetComponent<MeshFilter>().sharedMesh = GetShapeForIndex(i);

        var rend = go.GetComponent<MeshRenderer>();
        rend.sharedMaterial = _material;
        rend.SetPropertyBlock(_mpblock);

        _instances.Add(go);
        InvalidateXforms();
    }

    void RemoveLastNode()
    {
        var i = _instances.Count - 1;

        if (Application.isPlaying)
            Object.Destroy(_instances[i]);
        else
            Object.DestroyImmediate(_instances[i]);

        _instances.RemoveAt(i);
        InvalidateXforms();
    }

    void ChangeCapacity(int capacity)
    {
        capacity = Mathf.Clamp(capacity, 0, 0x20000);
        while (_instances.Count < capacity) AddNewNode();
        while (_instances.Count > capacity) RemoveLastNode();
    }

    #endregion

    #region Transform access array

    void InvalidateXforms()
    {
        if (_xforms.isCreated) _xforms.Dispose();
    }

    TransformAccessArray UpdateXforms()
    {
        if (!_xforms.isCreated)
            _xforms = new TransformAccessArray
              (_instances.Select(go => go.transform).ToArray());
        return _xforms;
    }

    #endregion

    #region Shape / material methods

    bool CompareShapes(ReadOnlySpan<Mesh> shapes)
    {
        if (_shapes.Length != shapes.Length) return false;
        for (var i = 0; i < _shapes.Length; i++)
            if (_shapes[i] != shapes[i]) return false;
        return true;
    }

    Mesh GetShapeForIndex(int i)
    {
        var rand = Random.CreateFromIndex(RandomSeed ^ (uint)i);
        return _shapes[rand.NextInt(_shapes.Length)];
    }

    void ResetShapes(ReadOnlySpan<Mesh> shapes)
    {
        if (CompareShapes(shapes)) return;
        _shapes = shapes.ToArray();
        ResetRandomSeed();
    }

    void ResetRandomSeed(uint? seed = null)
    {
        if (_randomSeed == seed) return;
        for (var i = 0; i < _instances.Count; i++)
            _instances[i].GetComponent<MeshFilter>().sharedMesh
              = GetShapeForIndex(i);
    }

    void ResetMaterial(Material m)
    {
        if (_material == m) return;
        _material = m;
        foreach (var go in _instances)
            go.GetComponent<MeshRenderer>().sharedMaterial = m;
    }

    #endregion
}

} // namespace Sketch
