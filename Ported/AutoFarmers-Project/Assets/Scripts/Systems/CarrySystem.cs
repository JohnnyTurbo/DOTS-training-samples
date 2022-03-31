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

            Entities.ForEach((Entity e, ref CarriedEntity carry, in Translation translation) =>
                {
                    var carryPos = new float3(translation.Value.x, translation.Value.y + CarryYOffset, translation.Value.z);
                    ecb.SetComponent(carry.Value, new Translation() { Value = carryPos });
                }).Run();
        }
    }
}