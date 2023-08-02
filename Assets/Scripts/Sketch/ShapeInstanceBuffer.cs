using Sketch.MeshKit;
using System;

namespace Sketch {

// Shared shape instance array for temporary use
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
