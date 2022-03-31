using Unity.Entities;
using Unity.Transforms;

namespace AutoFarmers
{
    public struct CarryData : IComponentData
    {
        public Entity carriedEntity;
        public Translation carriedTranslation; 
    }
}