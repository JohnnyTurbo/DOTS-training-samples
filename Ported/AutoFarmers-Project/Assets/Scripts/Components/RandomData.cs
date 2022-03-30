using Unity.Entities;
using Unity.Mathematics;

namespace AutoFarmers
{
    [GenerateAuthoringComponent]
    public struct RandomData : IComponentData
    {
        public Random Value;
    }
}