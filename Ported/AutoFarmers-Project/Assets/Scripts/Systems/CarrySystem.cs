using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace AutoFarmers
{
    public partial class CarrySystem : SystemBase
    {
        private const float CarryYOffset = 1f;

        private EndSimulationEntityCommandBufferSystem CommandBufferSystem;

        protected override void OnCreate()
        {
            CommandBufferSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var ecb = CommandBufferSystem.CreateCommandBuffer();

            Entities
                .WithAll<CarryData>()
                .ForEach((Entity e, ref CarryData carry, in Translation translation) =>
                {
                    float3 carryPos = new float3(translation.Value.x, translation.Value.y + CarryYOffset, translation.Value.z);
                    ecb.SetComponent(carry.carriedEntity, new Translation() { Value = carryPos });
                }).Run();
        }
    }
}