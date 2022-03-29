using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace AutoFarmers
{
    public partial class FarmerMovementSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem CommandBufferSystem;
        
        protected override void OnCreate()
        {
            CommandBufferSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
        }
        
        protected override void OnUpdate()
        {
            var time = Time.DeltaTime;
            var ecb = CommandBufferSystem.CreateCommandBuffer();

            Entities
                .WithAll<TargetData, FarmerTag>()
                .ForEach((Entity e, ref Translation translation, ref DynamicBuffer<PathBufferElement> pathBuffer, in SpeedData speed) =>
                {
                    if (pathBuffer.Length == 0)
                    {
                        //reached target
                        ecb.RemoveComponent<TargetData>(e);
                        return;
                    }
                    
                    var destination = new float3(pathBuffer[0].Value.x, 0 , pathBuffer[0].Value.y);
                    
                    var distance = math.distance(destination, translation.Value);

                    var threshold = 0.05f;
                    
                    if (distance > threshold)
                    {
                        var direction = destination - translation.Value;
                        var directionNormalized = math.normalize((direction));
                        
                        translation.Value += time * speed.Value * directionNormalized;
                    }
                    else // waypoint reached
                    {
                        Debug.Log("waypoint reached");
                        pathBuffer.RemoveAt((0));
                    }

                }).Run();
        }
    }
}