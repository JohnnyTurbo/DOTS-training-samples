using Unity.Entities;

namespace AutoFarmers
{
    [GenerateAuthoringComponent]
    public struct SearchRadiusData : IComponentData
    {
        public int Value;
    }
}