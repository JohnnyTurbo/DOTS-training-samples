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
                    var curBestTask = TaskTypes.None;
                    var bestTilePos = new int2(-1, -1);
                    var startingPos = translation.Value.ToTileIndex();
                    var lastValidPos = new int2(-1, -1);

                    int X = farmSize.x;
                    int Y = farmSize.y;

                    var startX = math.clamp(startingPos.x - radius.Value / 2, 0, farmSize.x);
                    var endX = math.clamp(startingPos.x + radius.Value / 2, 0, farmSize.x);
                    var startY = math.clamp(startingPos.y - radius.Value / 2, 0, farmSize.y);
                    var endY = math.clamp(startingPos.y + radius.Value / 2, 0, farmSize.y);

                    var bestDistance = float.MaxValue;

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
                                if (tileTaskType < curBestTask)
                                {
                                    curBestTask = tileTaskType;
                                    bestTilePos = curPosition;
                                }
                                else if (tileTaskType == curBestTask)
                                {
                                    var distance = math.distancesq(curPosition, startingPos);
                                    if (distance < bestDistance)
                                    {
                                        bestDistance = distance;
                                        bestTilePos = curPosition;
                                    }
                                }
                            }
                        }
                    }

                    if (curBestTask == TaskTypes.None)
                    {
                        bestTilePos = lastValidPos;
                        radius.Value = math.min(radius.Value + farmData.SearchRadiusIncrement, farmData.MaxFarmSize);
                    }
                    else
                    {
                        ecb.AddComponent(e, (curBestTask.TaskType()));
                        var tileIndex = Utilities.FlatIndex(bestTilePos.x, bestTilePos.y, farmSize.y);
                        var destinationTile = farmBuffer[tileIndex];
                        destinationTile.IsTargeted = true;
                        farmBuffer[tileIndex] = destinationTile;
                        radius.Value = farmData.DefaultFarmerSearchRadius;
                    }

                    //AStarPathfinding aStar = new AStarPathfinding(farmBuffer, farmSize);
                    //var path = aStar.FindPath(startingPos, bestTilePos);

                    //if (path.IsEmpty)
                    //{
                    //    Debug.Log("No path found");
                    //}
                    //else
                    {
                        Debug.Log($"Cur Best Task: {curBestTask} at pos: {bestTilePos}");
                        if (startingPos.x != bestTilePos.x)
                        {
                            pathBuffer.Add(new PathBufferElement { Value = new int2(bestTilePos.x, startingPos.y) });
                        }

                        pathBuffer.Add(new PathBufferElement { Value = bestTilePos });

                        //foreach (var tilePos in path)
                        //{
                        //    pathBuffer.Add(new PathBufferElement { Value = tilePos });
                        //}
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