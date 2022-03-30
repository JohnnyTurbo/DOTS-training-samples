using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace AutoFarmers
{
    public partial class DroneTaskSearchSystem : SystemBase
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
                .WithNone<TargetData, HarvestingTag, DepositingTag>()
                .WithNone<Timeout>()
                .WithAll<DroneTag>()
                .ForEach((Entity e, ref SearchRadiusData radius, ref DynamicBuffer<PathBufferElement> pathBuffer, ref RandomData random, in Translation translation) =>
                {
                    var invalidPos = new int2(-1, -1);
                    var bestTilePos = invalidPos;
                    var startingPos = translation.Value.ToTileIndex();

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
                                var index = Utilities.FlatIndex(curPosition.x, curPosition.y, farmSize.y);
                                var tile = farmBuffer[index];
                                if (!tile.IsTargeted && tile.TileState == TileState.Harvestable)
                                {
                                    bestTilePos = curPosition;
                                    break;
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

                    if (bestTilePos.Equals(invalidPos))
                    {
                        var newTimeout = new Timeout { Value = farmData.DroneTimeout };
                        ecb.AddComponent<Timeout>(e);
                        ecb.SetComponent(e, newTimeout);
                        radius.Value = math.min(radius.Value * 2, farmData.MaxFarmSize);
                    }
                    else
                    {
                        ecb.AddComponent<HarvestingTag>(e);
                        var tileIndex = Utilities.FlatIndex(bestTilePos.x, bestTilePos.y, farmSize.y);
                        var destinationTile = farmBuffer[tileIndex];
                        destinationTile.IsTargeted = true;
                        farmBuffer[tileIndex] = destinationTile;
                        radius.Value = farmData.DefaultDroneSearchRadius;

                        pathBuffer.Add(new PathBufferElement { Value = bestTilePos });

                        ecb.AddComponent<TargetData>(e);
                    }
                }).Run();
        }
    }
}