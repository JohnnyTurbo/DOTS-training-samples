using Unity.Entities;

namespace AutoFarmers
{
    [GenerateAuthoringComponent]
    public struct PlantAgeData : IComponentData
    {
        public float Value;
    }
}