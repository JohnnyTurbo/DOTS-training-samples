using Unity.Entities;

namespace AutoFarmers
{
    [GenerateAuthoringComponent]
    public struct HealthData : IComponentData
    {
        public int Value;
    }
}