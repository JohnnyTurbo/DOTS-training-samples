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
                    ecb.AddComponent<Parent>(plant);
                    ecb.AddComponent<LocalToParent>(plant);
                    ecb.SetComponent(plant, new Translation() { Value = new float3(0,1,0)});
                    ecb.SetComponent(plant, new Parent() { Value = e});

                    int x, y, dx, dy;
                    x = y = dx = 0;
                    dy = -1;

                    int X = farmData.FarmSize.x;
                    int Y = farmData.FarmSize.y;
                    int maxTiles = (int)(math.pow(math.max(X, Y), 2)) * 4;
                    int2 startingPos = translation.Value.ToTileIndex();
                    int2 siloPos = new int2();

                    for (var i = 0; i < maxTiles; i++)
                    {
                        if ((-X / 2 <= x) && (x <= X / 2) && (-Y / 2 <= y) && (y <= Y / 2))
                        {
                            int2 curPosition = startingPos + new int2(x, y);
                            if (!(curPosition.x < 0 || curPosition.x >= X || curPosition.y < 0 || curPosition.y >= Y))
                            {
                                var index = Utilities.FlatIndex(curPosition.x, curPosition.y, farmData.FarmSize.y);
                                var curTile = farmBuffer[index];
                                if (curTile.TileState == TileState.Silo)
                                {
                                    siloPos = curPosition;
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
                    
                    if (HasComponent<FarmerTag>(e) && startingPos.x != siloPos.x)
                    {
                        pathBuffer.Add(new PathBufferElement { Value = new int2(siloPos.x, startingPos.y) });
                    }

                    pathBuffer.Add(new PathBufferElement { Value = siloPos });

                    ecb.AddComponent<TargetData>(e);
                    ecb.RemoveComponent<HarvestingTag>(e);
                    ecb.AddComponent<DepositingTag>(e);
                }).Run();
        }
    }
}