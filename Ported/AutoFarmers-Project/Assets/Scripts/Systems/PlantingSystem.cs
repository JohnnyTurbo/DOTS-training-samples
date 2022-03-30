using Unity.Entities;
using Unity.Transforms;

namespace AutoFarmers
{
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    [UpdateAfter(typeof(FarmerTaskSearchSystem))]
    public partial class PlantingSystem : SystemBase
    {
        //private EndSimulationEntityCommandBufferSystem CommandBufferSystem;

        //protected override void OnCreate()
        //{
        //    CommandBufferSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
        //}

        protected override void OnUpdate()
        {
            //var ecb = CommandBufferSystem.CreateCommandBuffer();

            var farmEntity = GetSingletonEntity<FarmData>();
            var farmData = GetSingleton<FarmData>();
            //var farmBuffer = GetBuffer<TileBufferElement>(farmEntity);

            Entities
                .WithAll<PlantingTag, FarmerTag>()
                .WithNone<TargetData>()
                .ForEach((Entity e, in Translation translation) =>
                {
                    //var newPlant = ecb.Instantiate(farmData.PlantPrefab);
                    //ecb.SetComponent(newPlant, translation);
                    //ecb.SetComponent(newPlant, new NonUniformScale() {Value = 0f });
                    var newPlant = EntityManager.Instantiate(farmData.PlantPrefab);
                    EntityManager.SetComponentData(newPlant, translation);
                    EntityManager.SetComponentData(newPlant, new NonUniformScale() { Value = 0f });

                    var gridPos = translation.Value.ToTileIndex();
                    var tileIndex = Utilities.FlatIndex(gridPos.x, gridPos.y, farmData.FarmSize.y);

                    var farmBuffer = GetBuffer<TileBufferElement>(farmEntity);
                    TileBufferElement tile = farmBuffer[tileIndex];
                    tile.TileState = TileState.Planted;
                    tile.IsTargeted = false;
                    tile.OccupiedObject = newPlant;
                    farmBuffer[tileIndex] = tile;

                    EntityManager.RemoveComponent<PlantingTag>(e);
                    //ecb.RemoveComponent<PlantingTag>(e);
                }).WithStructuralChanges().Run();
        }
    }
}