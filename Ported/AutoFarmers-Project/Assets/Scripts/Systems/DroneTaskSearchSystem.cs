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
                .ForEach((Entity e, ref SearchRadiusData radius, ref DynamicBuffer<PathBufferElement> pathBuffer, in Translation translation) =>
                {
                    var invalidPos = new int2(-1, -1);
                    var bestTilePos = invalidPos;
                    var startingPos = translation.Value.ToTileIndex();

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
                            var index = Utilities.FlatIndex(curPosition.x, curPosition.y, farmSize.y);
                            var tile = farmBuffer[index];
                            if (!tile.IsTargeted && tile.TileState == TileState.Harvestable)
                            {
                                bestTilePos = curPosition;
                                break;
                            }
                        }
                    }

                    if (bestTilePos.Equals(invalidPos))
                    {
                        var newTimeout = new Timeout { Value = farmData.DroneTimeout };
                        ecb.AddComponent<Timeout>(e);
                        ecb.SetComponent(e, newTimeout);
                        radius.Value = math.min(radius.Value + farmData.SearchRadiusIncrement, farmData.MaxFarmSize);
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