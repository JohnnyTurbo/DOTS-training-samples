using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

namespace AutoFarmers
{
    [UpdateAfter(typeof(FarmerTaskSearchSystem))]
    public partial class PerformWorkSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem _ecbSystem;

        protected override void OnStartRunning()
        {
            _ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var farmEntity = GetSingletonEntity<FarmData>();
            var farmBuffer = GetBuffer<TileBufferElement>(farmEntity);
            var farmData = GetComponent<FarmData>(farmEntity);
            var ecb = _ecbSystem.CreateCommandBuffer();
            
            Entities
                //.WithAll<MiningTaskTag>()
                //.WithNone<TargetData>()
                .ForEach((Entity e, ref CurrentTask currentTask, ref DynamicBuffer<PathBufferElement> pathBuffer, in Translation translation, in SearchRadiusData radius) =>
                {
                    if (pathBuffer.Length != 0 || currentTask.Value == TaskTypes.None)
                    {
                        return;
                    }
                    var currentPosition = translation.Value.ToTileIndex();
                    var index = Utilities.FlatIndex(currentPosition.x, currentPosition.y, farmData.FarmSize.y);
                    var tile = farmBuffer[index];

                    switch (currentTask.Value)
                    {
                        case TaskTypes.Harvesting:
                            tile.TileState = TileState.Tilled;
                            tile.IsTargeted = false;
                            var plant = tile.OccupiedObject;
                            ecb.AddComponent(e, new CarriedEntity() { Value = plant });
                            tile.OccupiedObject = Entity.Null;
                            var siloPos = farmBuffer[index].ClosestSiloLocation;
                            
                            if (HasComponent<FarmerTag>(e))
                            { 
                                AStarPathfinding aStar = new AStarPathfinding(farmBuffer, farmData.FarmSize, radius.Value);
                                var path = aStar.FindPath(currentPosition, siloPos);
                                foreach (var tilePath in path)
                                {
                                    pathBuffer.Add(new PathBufferElement { Value = tilePath });
                                }
                            }
                            else
                            {
                                pathBuffer.Add(new PathBufferElement { Value = siloPos });
                            }
                            ecb.AddComponent<DepositingTag>(e); break;
                        
                        case TaskTypes.Planting:
                            var newPlant = ecb.Instantiate(farmData.PlantPrefab);
                            ecb.SetComponent(newPlant, translation);
                            ecb.SetComponent(newPlant, new NonUniformScale() { Value = 0f });
                            ecb.AppendToBuffer(farmEntity, new SpawnedPlantBufferElement() { index = index, plant = newPlant });
                            tile.TileState = TileState.Planted;
                            tile.IsTargeted = false;
                            break;
                        
                        case TaskTypes.Tilling:
                            tile.TileState = TileState.Tilled;
                            tile.IsTargeted = false;
                            var tileRenderEntity = tile.TileRenderEntity;
                            var newFieldTile = ecb.Instantiate(farmData.TilledPrefab);
                            var fieldPosition = GetComponent<Translation>(tileRenderEntity);
                            ecb.SetComponent(newFieldTile, fieldPosition);
                            ecb.DestroyEntity(tileRenderEntity);
                            ecb.AppendToBuffer(farmEntity,
                                new SpawnedTileBufferElement() { index = index, tileRenderer = newFieldTile });
                            break;
                        
                        case TaskTypes.Mining:
                            tile.TileState = TileState.Empty;
                            tile.IsTargeted = false;
                            var rockEntity = tile.OccupiedObject;
                            ecb.DestroyEntity(rockEntity);
                            tile.OccupiedObject = Entity.Null;
                            break;
                        
                        case TaskTypes.None:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    currentTask.Value = TaskTypes.None;
                    
                    farmBuffer[index] = tile;
                }).Run();
        }
    }
}