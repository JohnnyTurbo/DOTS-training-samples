using Unity.Entities;

namespace AutoFarmers
{
    [GenerateAuthoringComponent]
    public struct SpawnedTileBufferElement : IBufferElementData
    {
        public int index;
        public Entity tileRenderer;
    }
}