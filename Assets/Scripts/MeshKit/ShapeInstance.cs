using System;
using Unity.Mathematics;

namespace Sketch.MeshKit {

// Unmanaged shape instance descriptor
readonly struct ShapeInstance
{
    #region Private fields

    readonly float3 _position;
    readonly quaternion _rotation;
    readonly float _scale;
    readonly float4 _color;
    readonly ShapeRef _shape;

    #endregion

    #region Utility properties

    public int VertexCount => _shape.Vertices.Length;
    public int IndexCount => _shape.Indices.Length;

    #endregion

    #region Public methods

    public ShapeInstance(float3 position,
                         quaternion rotation,
                         float scale,
                         float4 color,
                         ShapeRef shape)
    {
        _position = position;
        _rotation = rotation;
        _scale = scale;
        _color = color;
        _shape = shape;
    }

    public void Bake(Span<float3> vertices,
                     Span<float4> uvs,
                     Span<uint> indices,
                     uint indexOffset)
    {
        CopyVertices(vertices);
        FillUVs(uvs);
        CopyIndices(indices, indexOffset);
    }

    #endregion

    #region Builder methods

    void CopyVertices(Span<float3> dest)
    {
        var mtx = float4x4.TRS(_position, _rotation, _scale);
        for (var i = 0; i < _shape.Vertices.Length; i++)
            dest[i] = math.transform(mtx, _shape.Vertices[i]);
    }

    void FillUVs(Span<float4> dest)
    {
        for (var i = 0; i < _shape.Vertices.Length; i++)
            dest[i] = _color;
    }

    void CopyIndices(Span<uint> dest, uint offs)
    {
        for (var i = 0; i < _shape.Indices.Length; i++)
            dest[i] = _shape.Indices[i] + offs;
    }

    #endregion
}

} // namespace Sketch.MeshKit
