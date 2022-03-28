using Unity.Entities;
using Unity.Mathematics;

namespace AutoFarmers
{
    [GenerateAuthoringComponent]
    public struct SizeData : IComponentData
    {
        public int2 Value;
    }
}