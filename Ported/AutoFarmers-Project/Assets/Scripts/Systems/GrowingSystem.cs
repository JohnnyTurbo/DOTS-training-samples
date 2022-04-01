using Unity.Entities;
using Unity.Transforms;

namespace AutoFarmers
{
    public partial class GrowingSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem _ecbSystem;

        protected override void OnCreate()
        {
            _ecbSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var time = Time.DeltaTime;
            var farmData = GetSingleton<FarmData>();
            var farmEntity = GetSingletonEntity<FarmData>();
            
            var parallelECB = _ecbSystem.CreateCommandBuffer().AsParallelWriter();
            
            Entities
                .WithAll<PlantAgeData>()
                .ForEach((Entity e, int entityInQueryIndex, ref NonUniformScale scale, ref PlantAgeData age, in Translation translation) =>
                {
                    if (age.Value < 1f)
                    {
                        age.Value += farmData.PlantGrowthRate * time;
                        scale.Value = age.Value;
                    }
                    else 
                    {
                        age.Value = int.MaxValue;
                    }

                }).ScheduleParallel();

            var ecb = _ecbSystem.CreateCommandBuffer();
            
            Entities
                .WithAll<PlantAgeData>()
                .ForEach((Entity e, ref PlantAgeData age, in Translation translation) =>
                {
                    if (age.Value == int.MaxValue)
                    {
                        var currentPosition = translation.Value.ToTileIndex();
                        var index = Utilities.FlatIndex(currentPosition.x, currentPosition.y, farmData.FarmSize.y);

                        var farmBuffer = GetBuffer<TileBufferElement>(farmEntity);
                        var tile = farmBuffer[index];
                        tile.TileState = TileState.Harvestable;
                        farmBuffer[index] = tile;

                        ecb.RemoveComponent<PlantAgeData>(e);
                    }
                }).Run();
        }
    }
}