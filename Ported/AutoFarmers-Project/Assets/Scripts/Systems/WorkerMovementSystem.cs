using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace AutoFarmers
{
    public partial class WorkerMovementSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem _ecbSystem;
        
        protected override void OnCreate()
        {
            _ecbSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
        }
        
        protected override void OnUpdate()
        {
            var time = Time.DeltaTime;
            var distanceThreshold = GetSingleton<FarmData>().DistanceThreshold;
            var ecb = _ecbSystem.CreateCommandBuffer().AsParallelWriter();
            
            Entities
                .WithAll<TargetData>()
                .WithAny<FarmerTag, DroneTag>()
                .ForEach((Entity workerEntity, int entityInQueryIndex, ref Translation translation, ref DynamicBuffer<PathBufferElement> pathBuffer, in SpeedData speed) =>
                {
                    if (pathBuffer.Length == 0)
                    {
                        //reached target
                        ecb.RemoveComponent<TargetData>(entityInQueryIndex, workerEntity);
                        return;
                    }
                    
                    var destination = new float3(pathBuffer[0].Value.x, translation.Value.y, pathBuffer[0].Value.y);
                    
                    var distance = math.distance(destination, translation.Value);

                    if (distance > distanceThreshold)
                    {
                        var direction = destination - translation.Value;
                        var directionNormalized = math.normalize((direction));
                        
                        translation.Value += time * speed.Value * directionNormalized;
                    }
                    else // waypoint reached
                    {
                        pathBuffer.RemoveAt((0));
                    }
                }).ScheduleParallel();
        }
    }
}