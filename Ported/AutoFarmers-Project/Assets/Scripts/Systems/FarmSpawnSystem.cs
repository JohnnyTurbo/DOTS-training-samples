﻿using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace AutoFarmers
{
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
    public partial class FarmSpawnSystem : SystemBase
    {
        private Random _random;
        public Entity FarmEntity;

        protected override void OnUpdate()
        {
            // Run code on first update only
            this.Enabled = false;
            _random.InitState((uint)System.DateTime.Now.Millisecond);

            FarmEntity = GetSingletonEntity<FarmData>();
            var farmData = GetSingleton<FarmData>();
            var farmStats = GetComponent<StatsData>(FarmEntity);
            var farmSize = farmData.FarmSize;

            var farmBuffer = EntityManager.AddBuffer<TileBufferElement>(FarmEntity);

            for (var x = 0; x < farmSize.x; x++)
            {
                for (var y = 0; y < farmSize.y; y++)
                {
                    var newFieldTile = EntityManager.Instantiate(farmData.FieldPrefab);
                    var fieldPosition = new Translation { Value = new float3(x, 0, y) };
                    EntityManager.SetComponentData(newFieldTile, fieldPosition);

                    var tileState = TileState.Empty;
                    var occupiedObject = Entity.Null;

                    if (_random.NextFloat() <= farmData.PercentSilos)
                    {
                        var newSilo = EntityManager.Instantiate(farmData.SiloPrefab);
                        EntityManager.SetComponentData(newSilo, fieldPosition);
                        tileState = TileState.Silo;
                        occupiedObject = newSilo;
                    }
                    else if (_random.NextFloat() <= farmData.PercentRocks)
                    {
                        var newRock = EntityManager.Instantiate(farmData.RockPrefab);
                        EntityManager.SetComponentData(newRock, fieldPosition);
                        tileState = TileState.Rock;
                        occupiedObject = newRock;
                    }

                    var newTileBufferElement = new TileBufferElement
                    {
                        TileRenderEntity = newFieldTile,
                        OccupiedObject = occupiedObject,
                        TileState = tileState,
                        IsTargeted = false
                    };

                    farmBuffer = EntityManager.GetBuffer<TileBufferElement>(FarmEntity);
                    farmBuffer.Add(newTileBufferElement);
                }
            }

            // Pre-cache the location to the destination silo
            for (var startX = 0; startX < farmSize.x; startX++)
            {
                for (var startY = 0; startY < farmSize.y; startY++)
                {
                    // Find a silo location
                    int x, y, dx, dy;
                    x = y = dx = 0;
                    dy = -1;

                    int X = farmData.FarmSize.x;
                    int Y = farmData.FarmSize.y;
                    int maxTiles = (int)(math.pow(math.max(X, Y), 2)) * 4;
                    int2 startingPos = new int2(startX, startY);
                    int2 siloPos = new int2();

                    for (var i = 0; i < maxTiles; i++)
                    {
                        if ((-X / 2 <= x) && (x <= X / 2) && (-Y / 2 <= y) && (y <= Y / 2))
                        {
                            int2 curPosition = startingPos + new int2(x, y);

                            if (!(curPosition.x < 0 || curPosition.x >= X || curPosition.y < 0 || curPosition.y >= Y))
                            {
                                var spiralIndex = Utilities.FlatIndex(curPosition.x, curPosition.y, farmData.FarmSize.y);
                                var curTile = farmBuffer[spiralIndex];
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

                    var index = Utilities.FlatIndex(startX, startY, farmSize.y);
                    var tile = farmBuffer[index];
                    tile.ClosestSiloLocation = siloPos;
                    farmBuffer[index] = tile;
                }
            }

            int farmerCount = 1;
            for (int i = 0; i < farmerCount; i++)
            {
                farmBuffer = EntityManager.GetBuffer<TileBufferElement>(FarmEntity);
                int arrayIndex;
                int2 spawnPosition;
                do
                {
                    spawnPosition = _random.NextInt2(int2.zero, farmSize);
                    arrayIndex = Utilities.FlatIndex(spawnPosition.x, spawnPosition.y, farmSize.y);
                } while (farmBuffer[arrayIndex].TileState != TileState.Empty);
                
                farmStats.FarmerCount += 1;
                var farmerEntity = EntityManager.Instantiate(farmData.FarmerPrefab);
                var farmerPosition = new Translation
                {
                    Value = new float3(spawnPosition.x, 0, spawnPosition.y)
                };
                var farmerRandom = EntityManager.GetComponentData<RandomData>(farmerEntity);
                farmerRandom.Value.InitState((uint)farmerEntity.Index);
                EntityManager.SetComponentData(farmerEntity, farmerRandom);
                EntityManager.SetComponentData(farmerEntity, farmerPosition);
            }

            EntityManager.SetComponentData(FarmEntity, farmStats);
        }
    }
}