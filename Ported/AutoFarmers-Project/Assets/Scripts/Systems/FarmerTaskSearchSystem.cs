using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

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
            var farmSize = GetComponent<FarmData>(farmEntity).FarmSize;
            var ecb = _ecbSystem.CreateCommandBuffer();

            Entities
                .WithNone<TargetData, MiningTaskTag, DepositingTag>()
                .WithNone<HarvestingTag, PlantingTag, TillingTag>()
                .WithAll<FarmerTag>()
                .ForEach((Entity e, ref SearchRadiusData radius, ref DynamicBuffer<PathBufferElement> pathBuffer, ref RandomData random, in Translation translation) =>
                {
                    var curBestTask = TaskTypes.None;
                    var bestTilePos = new int2(-1, -1);
                    var startingPos = translation.Value.ToTileIndex();
                    var lastValidPos = new int2(-1, -1);

                    int X = farmSize.x;
                    int Y = farmSize.y;

                    int x, y, dx, dy;
                    x = y = 0;                    
                    do
                    {
                        dy = random.Value.NextInt(-1, 2);
                        dx = random.Value.NextInt(-1, 2);
                    } while (dx == 0 && dy == 0);
                    

                    for (var i = 0; i < radius.Value; i++)
                    {
                        if ((-X / 2 <= x) && (x <= X / 2) && (-Y / 2 <= y) && (y <= Y / 2))
                        {
                            int2 curPosition = startingPos + new int2(x, y);
                            if (!(curPosition.x < 0 || curPosition.x >= X || curPosition.y < 0 || curPosition.y >= Y))
                            {
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
                                }
                            }
                        }

                        if ((x == y) || ((x < 0) && (x == -y)) || ((x > 0) && (x == 1 - y)))
                        {
                            var t = dx;
                            dx = -dy;
                            dy = t;
                        }

                        x += dx;
                        y += dy;
                    }

                 
                    
                    if (curBestTask == TaskTypes.None)
                    {
                        bestTilePos = lastValidPos;
                    }
                    else
                    {
                        ecb.AddComponent(e, (curBestTask.TaskType()));
                        var tileIndex = Utilities.FlatIndex(bestTilePos.x, bestTilePos.y, farmSize.y);
                        var destinationTile = farmBuffer[tileIndex];
                        destinationTile.IsTargeted = true;
                        farmBuffer[tileIndex] = destinationTile;
                    }

                    //Debug.Log($"Cur Best Task: {curBestTask} at pos: {bestTilePos}");                    
                    if (startingPos.x != bestTilePos.x)
                    {
                        pathBuffer.Add(new PathBufferElement { Value = new int2(bestTilePos.x, startingPos.y) });
                    }

                    pathBuffer.Add(new PathBufferElement { Value = bestTilePos });

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