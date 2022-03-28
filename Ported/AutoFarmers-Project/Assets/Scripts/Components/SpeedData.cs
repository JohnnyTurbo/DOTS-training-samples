using Unity.Entities;

namespace AutoFarmers
{
    [GenerateAuthoringComponent]
    public struct SpeedData : IComponentData
    {
        public float Value;
    }
}