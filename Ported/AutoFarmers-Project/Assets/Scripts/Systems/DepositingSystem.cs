using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace AutoFarmers
{
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    [UpdateAfter(typeof(FarmerTaskSearchSystem))]
    public partial class DepositingSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem CommandBufferSystem;

        protected override void OnCreate()
        {
            CommandBufferSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var time = Time.DeltaTime;
            var farmEntity = GetSingletonEntity<FarmData>();
            var farmData = GetSingleton<FarmData>();
            var farmStats = GetComponent<StatsData>(farmEntity);
            var ecb = CommandBufferSystem.CreateCommandBuffer();

            Entities
                .WithAll<DepositingTag>()
                .WithNone<TargetData>()
                .ForEach((Entity e, in DynamicBuffer<Child> childBuffer, in Translation translation) =>
                {
                    var currentPosition = translation.Value.ToTileIndex();
                    var index = Utilities.FlatIndex(currentPosition.x, currentPosition.y, farmData.FarmSize.y);

                    var farmBuffer = GetBuffer<TileBufferElement>(farmEntity);
                    TileBufferElement tile = farmBuffer[index];
                    var silo = tile.OccupiedObject;

                    var plant = childBuffer[0].Value;
                    ecb.DestroyEntity(plant);

                    ecb.RemoveComponent<DepositingTag>(e);

                    // Update stats
                    var siloStats = GetComponent<StatsData>(silo);

                    siloStats.HarvestCount += 1;
                    farmStats.HarvestCount += 1;

                    if ((siloStats.HarvestCount % farmData.HarvestThreshold) == 0)
                    {
                        farmStats.FarmerCount += 1;
                        siloStats.FarmerCount += 1;

                        // Spawn farmers
                        var farmerEntity = EntityManager.Instantiate(farmData.FarmerPrefab);
                        var farmerPosition = new Translation
                        {
                            Value = new float3(currentPosition.x, 0, currentPosition.y)
                        };
                        ecb.SetComponent(farmerEntity, farmerPosition);
                    }

                    ecb.SetComponent<StatsData>(silo, siloStats);
                    ecb.SetComponent<StatsData>(farmEntity, farmStats);

                }).WithStructuralChanges().Run();
        }
    }
}