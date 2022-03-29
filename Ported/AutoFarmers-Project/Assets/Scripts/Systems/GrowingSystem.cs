using Unity.Entities;
using Unity.Transforms;

namespace AutoFarmers
{
    public partial class GrowingSystem : SystemBase
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
            var farmBuffer = GetBuffer<TileBufferElement>(farmEntity);
            var ecb = CommandBufferSystem.CreateCommandBuffer();

            Entities
                .WithAll<PlantAgeData>()
                .WithNone<DoneGrowingTag>()
                .ForEach((Entity e, ref NonUniformScale scale, ref PlantAgeData age, in Translation translation) =>
                {
                    if (age.Value >= 1f)
                    {
                        ecb.AddComponent<DoneGrowingTag>(e);
                        var currentPosition = translation.Value.ToTileIndex();
                        var index = Utilities.FlatIndex(currentPosition.x, currentPosition.y, farmData.FarmSize.y);
                        TileBufferElement tile = farmBuffer[index];
                        tile.TileState = TileState.Harvestable;
                        farmBuffer[index] = tile;
                    }
                    else
                    {
                        age.Value += farmData.PlantGrowthRate * time;
                        scale.Value = age.Value;
                    }

                }).Run();
        }
    }
}