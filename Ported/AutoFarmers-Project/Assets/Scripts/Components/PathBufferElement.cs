using Unity.Entities;
using Unity.Mathematics;

namespace AutoFarmers
{
    [InternalBufferCapacity(8)]
    [GenerateAuthoringComponent]
    public struct PathBufferElement : IBufferElementData
    {
        public int2 Value;

        public static implicit operator PathBufferElement(int2 value)
        {
            return new PathBufferElement { Value = value };
        }

        public static implicit operator int2(PathBufferElement element)
        {
            return element.Value;
        }
    }
}