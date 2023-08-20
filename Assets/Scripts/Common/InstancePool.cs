using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Jobs;

using Object = UnityEngine.Object;
using Random = Unity.Mathematics.Random;

namespace Sketch {

public class InstancePool : IDisposable
{
    #region Public properties

    public int Capacity
      { get => _instances.Count; set => ChangeCapacity(value); }

    public ReadOnlySpan<Mesh> Meshes
      { get => _meshes; set => ResetMeshes(value); }

    public Material Material
      { get => _material; set => ResetMaterial(value); }

    public uint RandomSeed
      { get => _randomSeed; set => ResetRandomSeed(value); }

    public TransformAccessArray Xforms => UpdateXforms();

    #endregion

    #region Public methods

    public InstancePool()
      => _mpblock = new MaterialPropertyBlock();

    public void Dispose()
      => ChangeCapacity(0);

    #endregion

    #region Private members

    static readonly Type[] InstanceComponents =
      { typeof(MeshFilter), typeof(MeshRenderer) };

    List<GameObject> _instances = new List<GameObject>();

    Mesh[] _meshes = new Mesh[] { null };
    Material _material;
    uint _randomSeed = 1;

    TransformAccessArray _xforms;
    MaterialPropertyBlock _mpblock;

    #endregion

    #region Allocation / deallocation

    void AddNewInstance()
    {
        var i = _instances.Count;

        var go = new GameObject("Instance", InstanceComponents);
        go.hideFlags = HideFlags.HideAndDontSave;

        go.GetComponent<MeshFilter>().sharedMesh = GetMeshForIndex(i);

        var rend = go.GetComponent<MeshRenderer>();
        rend.sharedMaterial = _material;
        rend.SetPropertyBlock(_mpblock);

        _instances.Add(go);
        InvalidateXforms();
    }

    void RemoveLastInstance()
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
        while (_instances.Count < capacity) AddNewInstance();
        while (_instances.Count > capacity) RemoveLastInstance();
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

    #region Mesh / material methods

    bool CompareMeshes(ReadOnlySpan<Mesh> meshes)
    {
        if (_meshes.Length != meshes.Length) return false;
        for (var i = 0; i < _meshes.Length; i++)
            if (_meshes[i] != meshes[i]) return false;
        return true;
    }

    Mesh GetMeshForIndex(int i)
    {
        var rand = Random.CreateFromIndex(RandomSeed ^ (uint)i);
        return _meshes[rand.NextInt(_meshes.Length)];
    }

    void ResetMeshes(ReadOnlySpan<Mesh> meshes)
    {
        if (CompareMeshes(meshes)) return;
        _meshes = meshes.ToArray();
        ResetRandomSeed();
    }

    void ResetRandomSeed(uint? seed = null)
    {
        if (_randomSeed == seed) return;
        if (seed != null) _randomSeed = (uint)seed;
        for (var i = 0; i < _instances.Count; i++)
            _instances[i].GetComponent<MeshFilter>().sharedMesh
              = GetMeshForIndex(i);
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
