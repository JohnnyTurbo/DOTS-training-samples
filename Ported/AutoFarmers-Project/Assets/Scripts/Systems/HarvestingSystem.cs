using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace AutoFarmers
{
    [UpdateAfter(typeof(FarmerTaskSearchSystem))]
    public partial class HarvestingSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem CommandBufferSystem;

        protected override void OnCreate()
        {
            CommandBufferSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var ecb = CommandBufferSystem.CreateCommandBuffer();

            var farmEntity = GetSingletonEntity<FarmData>();
            var farmData = GetSingleton<FarmData>();
            var farmBuffer = GetBuffer<TileBufferElement>(farmEntity);

            Entities
                .WithAll<HarvestingTag>()
                .WithAny<FarmerTag, DroneTag>()
                .WithNone<TargetData>()
                .ForEach((Entity e, ref DynamicBuffer<PathBufferElement> pathBuffer, in Translation translation) =>
                {
                   
                    var gridPos = translation.Value.ToTileIndex();
                    var tileIndex = Utilities.FlatIndex(gridPos.x, gridPos.y, farmData.FarmSize.y);
                    TileBufferElement tile = farmBuffer[tileIndex];

                    tile.TileState = TileState.Tilled;
                    tile.IsTargeted = false;
                    var plant = tile.OccupiedObject;
                    tile.OccupiedObject = Entity.Null;
                    farmBuffer[tileIndex] = tile;
                    ecb.AddComponent(e, new CarryData() { carriedEntity = plant });

                    int2 startingPos = translation.Value.ToTileIndex();
                    var siloPos = farmBuffer[tileIndex].ClosestSiloLocation;

                    if (HasComponent<FarmerTag>(e))
                    { 
                        Utilities.AStarPathfinding aStar = new Utilities.AStarPathfinding(farmBuffer, farmData.FarmSize);
                        var path = aStar.FindPath(startingPos, siloPos);
                        foreach (var tilePath in path)
                        {
                            pathBuffer.Add(new PathBufferElement { Value = tilePath });
                        }
                    }
                    else
                    {
                         pathBuffer.Add(new PathBufferElement { Value = siloPos });
                    }

                    ecb.AddComponent<TargetData>(e);
                    ecb.RemoveComponent<HarvestingTag>(e);
                    ecb.AddComponent<DepositingTag>(e);
                }).Run();
        }
    }
}