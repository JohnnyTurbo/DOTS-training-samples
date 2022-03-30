using Unity.Entities;

namespace AutoFarmers
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class AssignSpawnedEntitiesSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var farmEntity = GetSingletonEntity<FarmData>();
            var farmBuffer = GetBuffer<TileBufferElement>(farmEntity);
            var plantBuffer = GetBuffer<SpawnedPlantBufferElement>(farmEntity);
            var tileBuffer = GetBuffer<SpawnedTileBufferElement>(farmEntity);

            foreach (SpawnedPlantBufferElement element in plantBuffer)
            {
                TileBufferElement tile = farmBuffer[element.index];
                tile.OccupiedObject = element.plant;
                farmBuffer[element.index] = tile;
            }

            foreach (SpawnedTileBufferElement element in tileBuffer)
            {
                TileBufferElement tile = farmBuffer[element.index];
                tile.TileRenderEntity = element.tileRenderer;
                farmBuffer[element.index] = tile;
            }

            plantBuffer.Clear();
            tileBuffer.Clear();
        }
    }
}
