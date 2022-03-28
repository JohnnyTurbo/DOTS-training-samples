using Unity.Entities;
using Unity.Mathematics;

namespace AutoFarmers
{
    [GenerateAuthoringComponent]
    public struct TargetData : IComponentData
    {
        public int2 Value;
    }
}