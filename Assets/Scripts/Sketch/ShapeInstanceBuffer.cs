using System;
using MeshKit;

namespace Sketch {

static class ShapeInstanceBuffer
{
    static ShapeInstance[] _buffer = new ShapeInstance[1024];

    public static Span<ShapeInstance> Get(int capacity)
    {
        if (_buffer.Length < capacity) _buffer = new ShapeInstance[capacity];
        return new Span<ShapeInstance>(_buffer).Slice(0, capacity);
    }
}

} // namespace Sketch
