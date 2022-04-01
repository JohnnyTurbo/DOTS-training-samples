using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using static AutoFarmers.Utilities;

namespace AutoFarmers
{
    public partial class FarmerTaskSearchSystem : SystemBase
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
            var farmSize = farmData.FarmSize;
            var ecb = _ecbSystem.CreateCommandBuffer();

            Entities
                .WithNone<TargetData, MiningTaskTag, DepositingTag>()
                .WithNone<HarvestingTag, PlantingTag, TillingTag>()
                .WithAll<FarmerTag>()
                .ForEach((Entity e, ref SearchRadiusData radius, ref DynamicBuffer<PathBufferElement> pathBuffer, in Translation translation) =>
                {  
                    AStarPathfinding aStar = new AStarPathfinding(farmBuffer, farmSize, radius.Value);
                    var curBestTask = TaskTypes.None;
                    var bestTilePos = new int2(-1, -1);
                    var startingPos = translation.Value.ToTileIndex();
                    var lastValidPos = new int2(-1, -1);

                    NativeList<int2> bestPath = new NativeList<int2>(Allocator.Temp);
                        
                    int X = farmSize.x;
                    int Y = farmSize.y;

                    var startX = math.clamp(startingPos.x - radius.Value / 2, 0, farmSize.x);
                    var endX = math.clamp(startingPos.x + radius.Value / 2, 0, farmSize.x);
                    var startY = math.clamp(startingPos.y - radius.Value / 2, 0, farmSize.y);
                    var endY = math.clamp(startingPos.y + radius.Value / 2, 0, farmSize.y);

                    for (var i = startX; i < endX; ++i)
                    {
                        for (var j = startY; j < endY; ++j)
                        {
                            int2 curPosition = new int2(i, j);
                            lastValidPos = curPosition;

                            var index = Utilities.FlatIndex(curPosition.x, curPosition.y, farmSize.y);
                            var tile = farmBuffer[index];
                            if (!tile.IsTargeted)
                            {
                                var tileTaskType = tile.TileState.ToTaskType();
                                if(tileTaskType == TaskTypes.Mining)
                                {
                                    aStar.SetValidOverride(true);
                                }
                                else
                                {
                                    aStar.SetValidOverride(false);
                                }
                                var pathTemp = aStar.FindPath(startingPos, curPosition);
                                if (tileTaskType < curBestTask)
                                {
                                    if (!pathTemp.IsEmpty)
                                    {
                                        if (tileTaskType == TaskTypes.Harvesting)
                                        {
                                            var siloPath = aStar.FindPath(curPosition, tile.ClosestSiloLocation);
                                            if (siloPath.Length > 0)
                                            {
                                                curBestTask = tileTaskType;
                                                bestTilePos = curPosition;
                                                bestPath = pathTemp;
                                            }
                                        }
                                        else
                                        {
                                            curBestTask = tileTaskType;
                                            bestTilePos = curPosition;
                                            bestPath = pathTemp;
                                        }
                                    }
                                }
                                else if (tileTaskType == curBestTask)
                                {
                                    if (bestPath.Length > pathTemp.Length && pathTemp.Length > 0)
                                    {
                                        if (tileTaskType == TaskTypes.Harvesting)
                                        {
                                            var siloPath = aStar.FindPath(curPosition, tile.ClosestSiloLocation);
                                            if (siloPath.Length > 0)
                                            {
                                                curBestTask = tileTaskType;
                                                bestTilePos = curPosition;
                                                bestPath = pathTemp;
                                            }
                                        }
                                        else
                                        {
                                            bestPath = pathTemp;
                                            bestTilePos = curPosition;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (bestPath.Length == 0)
                    {
                        //farmer stuck
                        radius.Value = math.min(radius.Value + farmData.SearchRadiusIncrement, farmData.MaxFarmSize);
                    }
                    else
                    {
                        ecb.SetComponent(e, new CurrentTask{Value = curBestTask});
                        
                        var tileIndex = Utilities.FlatIndex(bestTilePos.x, bestTilePos.y, farmSize.y);
                        var destinationTile = farmBuffer[tileIndex];
                        destinationTile.IsTargeted = true;
                        farmBuffer[tileIndex] = destinationTile;
                        radius.Value = farmData.DefaultFarmerSearchRadius;
                    }

                    foreach (var tile in bestPath)
                    {
                        pathBuffer.Add(new PathBufferElement { Value = tile });
                    }

                    ecb.AddComponent<TargetData>(e);

                }).Run();
        }
    }
}

/*
void Spiral( int X, int Y){
    int x,y,dx,dy;
    x = y = dx =0;
    dy = -1;
    int t = std::max(X,Y);
    int maxI = t*t;
    for(int i =0; i < maxI; i++){
        if ((-X/2 <= x) && (x <= X/2) && (-Y/2 <= y) && (y <= Y/2)){
            // DO STUFF...
        }
        if( (x == y) || ((x < 0) && (x == -y)) || ((x > 0) && (x == 1-y))){
            t = dx;
            dx = -dy;
            dy = t;
        }
        x += dx;
        y += dy;
    }
}*/