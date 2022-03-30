using Unity.Entities;

namespace AutoFarmers
{
    [GenerateAuthoringComponent]
    public struct SpawnedPlantBufferElement : IBufferElementData
    {
        public int index;
        public Entity plant;
    }
}