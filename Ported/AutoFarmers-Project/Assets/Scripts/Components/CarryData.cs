using Unity.Entities;

namespace AutoFarmers
{
    public struct CarryData : IComponentData
    {
        public Entity carriedEntity;
    }
}