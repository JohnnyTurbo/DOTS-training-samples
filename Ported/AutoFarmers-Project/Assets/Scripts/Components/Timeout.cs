using Unity.Entities;

namespace AutoFarmers
{
    [GenerateAuthoringComponent]
    public struct Timeout : IComponentData
    {
        public float Value;
    }
}