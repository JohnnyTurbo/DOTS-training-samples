using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

/// <summary>
/// Intentionally spawning the particles in the floor.
/// In the demo, the cubes spawn all over the place,
/// but still, these particles will be attracted to the tornado. 
/// </summary>
public partial class TornadoSpanwerSystem : SystemBase
{
    const int padding = 4;
    protected override void OnUpdate()
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        var random = new Random((uint)DateTime.Now.Millisecond+1);
        var floor = GetSingletonEntity<Floor>();
        var scale = GetComponent<NonUniformScale>(floor).Value*padding;
        var maxHeight = GetSingleton<TornadoMovement>().MaxHeight;
        Entities
            .ForEach((Entity entity, in TornadoSpawner spawner) =>
            {
                ecb.DestroyEntity(entity);
                
                float3 randomXZPos;
                Translation translation;
                TornadoParticle particleProps;
                
                int particleCount = spawner.particleCount;
                for (int i = 0; i < particleCount; i++)
                {
                    var particleInstance = ecb.Instantiate(spawner.particlePrefab);

                    randomXZPos = random.NextFloat3(scale*-1 ,scale);
                    //translation = new Translation {Value = new float3(randomXZPos.x, random.NextFloat(maxHeight), randomXZPos.z)};
                    translation = new Translation {Value = new float3(randomXZPos.x, 0, randomXZPos.z)};
                    particleProps = new TornadoParticle
                        {SpinRate = random.NextFloat(1, 60), UpwardSpeed = random.NextFloat(1, 25), RadiusMult = random.NextFloat(1)};
                    ecb.SetComponent(particleInstance, translation);
                    ecb.SetComponent(particleInstance, particleProps);
                }
            }).Run();

        ecb.Playback(EntityManager);
        ecb.Dispose();
    }
}
