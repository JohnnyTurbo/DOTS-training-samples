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
            var farmData = GetSingleton<FarmData>();
            var ecb = CommandBufferSystem.CreateCommandBuffer();

            Entities
                .WithAll<PlantAgeData>()
                .WithNone<DoneGrowingTag>()
                .ForEach((Entity e, ref NonUniformScale scale, ref PlantAgeData age) =>
                {
                    if (age.Value >= 1f)
                    {
                        ecb.AddComponent<DoneGrowingTag>(e);
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