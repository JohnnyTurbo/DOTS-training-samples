using Unity.Entities;

namespace AutoFarmers
{
    public partial class TimeoutSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem _ecbSystem;

        protected override void OnStartRunning()
        {
            _ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var deltaTime = Time.DeltaTime;
            var ecb = _ecbSystem.CreateCommandBuffer();
            
            Entities.ForEach((Entity e, ref Timeout timeout) =>
            {
                timeout.Value -= deltaTime;
                if (timeout.Value <= 0)
                {
                    ecb.RemoveComponent<Timeout>(e);
                }
            }).Run();
        }
    }
}