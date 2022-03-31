using Unity.Entities;

namespace AutoFarmers
{
    [GenerateAuthoringComponent]
    public struct CarriedEntity : IComponentData
    {
        public Entity Value;
    }
}