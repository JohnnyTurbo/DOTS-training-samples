using Unity.Entities;
using Unity.Transforms;

namespace AutoFarmers
{
    [UpdateAfter(typeof(FarmerTaskSearchSystem))]
    public partial class TillingSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem _ecbSystem;

        protected override void OnStartRunning()
        {
            _ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var farmEntity = GetSingletonEntity<FarmData>();
            var farmBuffer = GetBuffer<TileBufferElement>(farmEntity);
            var farmData = GetComponent<FarmData>(farmEntity);
            var ecb = _ecbSystem.CreateCommandBuffer();
            var newTileBuffer = GetBuffer<SpawnedTileBufferElement>(farmEntity);

            Entities
                .WithAll<TillingTag>()
                .WithNone<TargetData>()
                .ForEach((Entity e, in Translation translation) =>
                {
                    var currentPosition = translation.Value.ToTileIndex();
                    var index = Utilities.FlatIndex(currentPosition.x, currentPosition.y, farmData.FarmSize.y);
                    var tile = farmBuffer[index];
                    tile.TileState = TileState.Tilled;
                    tile.IsTargeted = false;

                    var renderer = tile.TileRenderEntity;
                    var newFieldTile = ecb.Instantiate(farmData.TilledPrefab);
                    var fieldPosition = GetComponent<Translation>(renderer);
                    ecb.SetComponent(newFieldTile, fieldPosition);
                    ecb.DestroyEntity(renderer);
                    ecb.AppendToBuffer(farmEntity, new SpawnedTileBufferElement() { index = index, tileRenderer = newFieldTile });

                    farmBuffer[index] = tile;
                    ecb.RemoveComponent<TillingTag>(e);
                }).Run();
        }
    }
}