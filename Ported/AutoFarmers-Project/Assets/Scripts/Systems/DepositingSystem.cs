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
            var farmBuffer = GetBuffer<TileBufferElement>(farmEntity);
            var ecb = CommandBufferSystem.CreateCommandBuffer().AsParallelWriter();

            Entities
                .WithAll<DepositingTag>()
                .WithAny<FarmerTag, DroneTag>()
                .WithNone<TargetData>()
                .WithReadOnly(farmBuffer)
                .ForEach((Entity e, int entityInQueryIndex, ref CarriedEntity carried, ref CurrentTask currentTask, in Translation translation) =>
                {
                    var currentPosition = translation.Value.ToTileIndex();
                    var index = Utilities.FlatIndex(currentPosition.x, currentPosition.y, farmData.FarmSize.y);

                    TileBufferElement tile = farmBuffer[index];
                    var silo = tile.OccupiedObject;

                    var plant = carried.Value;
                    ecb.DestroyEntity(entityInQueryIndex, plant);
                    ecb.RemoveComponent<CarriedEntity>(entityInQueryIndex, e);

                    ecb.RemoveComponent<DepositingTag>(entityInQueryIndex, e);

                    // Update stats
                    var siloStats = GetComponent<StatsData>(silo);

                    siloStats.HarvestCount += 1;
                    farmStats.HarvestCount += 1;

                    if ((siloStats.HarvestCount % farmData.HarvestThreshold) == 0)
                    {
                        farmStats.WorkerCount += 1;
                        siloStats.WorkerCount += 1;

                        if (siloStats.WorkerCount % farmData.DroneThreshold == 0)
                        {
                            for (var i = 0; i < farmData.DronesToSpawn; i++)
                            {
                                farmStats.DroneCount += 1;
                                siloStats.DroneCount += 1;

                                var droneEntity = ecb.Instantiate(entityInQueryIndex, farmData.DronePrefab);
                                var dronePosition = new Translation
                                {
                                    Value = new float3(currentPosition.x, 2f, currentPosition.y)
                                };
                                ecb.SetComponent(entityInQueryIndex, droneEntity, dronePosition);
                            }
                        }

                        else
                        {
                            farmStats.FarmerCount += 1;
                            siloStats.FarmerCount += 1;

                            // Spawn farmers
                            var farmerEntity = ecb.Instantiate(entityInQueryIndex, farmData.FarmerPrefab);
                            var farmerPosition = new Translation
                            {
                                Value = new float3(currentPosition.x, 0, currentPosition.y)
                            };
                            ecb.SetComponent(entityInQueryIndex, farmerEntity, farmerPosition);
                        }
                    }

                    ecb.SetComponent<StatsData>(entityInQueryIndex, silo, siloStats);
                    ecb.SetComponent<StatsData>(entityInQueryIndex, farmEntity, farmStats);
                    currentTask.Value = TaskTypes.None;
                }).ScheduleParallel();

            CommandBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}