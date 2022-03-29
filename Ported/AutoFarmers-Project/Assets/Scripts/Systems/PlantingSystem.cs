using Unity.Entities;
using Unity.Transforms;

namespace AutoFarmers
{
    [UpdateAfter(typeof(FarmerTaskSearchSystem))]
    public partial class PlantingSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem CommandBufferSystem;

        protected override void OnCreate()
        {
            CommandBufferSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var ecb = CommandBufferSystem.CreateCommandBuffer();

            var farmEntity = GetSingletonEntity<FarmData>();
            var farmData = GetSingleton<FarmData>();
            var farmBuffer = GetBuffer<TileBufferElement>(farmEntity);

            Entities
                .WithAll<PlantingTag, FarmerTag>()
                .WithNone<TargetData>()
                .ForEach((Entity e, in Translation translation) =>
                {
                    var newPlant = ecb.Instantiate(farmData.PlantPrefab);
                    ecb.SetComponent(newPlant, translation);
                    ecb.SetComponent(newPlant, new NonUniformScale() {Value = 0f });

                    var gridPos = translation.Value.ToTileIndex();
                    var tileIndex = Utilities.FlatIndex(gridPos.x, gridPos.y, farmData.FarmSize.y);
                    TileBufferElement tile = farmBuffer[tileIndex];
                    tile.TileState = TileState.Planted;
                    tile.IsTargeted = false;
                    farmBuffer[tileIndex] = tile;
                    
                    ecb.RemoveComponent<PlantingTag>(e);
                }).Run();
        }
    }
}