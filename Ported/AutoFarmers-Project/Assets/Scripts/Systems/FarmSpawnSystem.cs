﻿using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace AutoFarmers
{
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
    public partial class FarmSpawnSystem : SystemBase
    {
        private Random _random;

        protected override void OnUpdate()
        {
            // Run code on first update only
            this.Enabled = false;
            _random.InitState((uint)System.DateTime.Now.Millisecond);

            var farmEntity = GetSingletonEntity<FarmData>();
            var farmData = GetSingleton<FarmData>();
            var farmStats = GetComponent<StatsData>(farmEntity);
            var farmSize = farmData.FarmSize;

            var farmBuffer = EntityManager.AddBuffer<TileBufferElement>(farmEntity);

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

                    farmBuffer = EntityManager.GetBuffer<TileBufferElement>(farmEntity);
                    farmBuffer.Add(newTileBufferElement);
                }
            }

            int farmerCount = 1;
            for (int i = 0; i < farmerCount; i++)
            {
                farmBuffer = EntityManager.GetBuffer<TileBufferElement>(farmEntity);
                int arrayIndex;
                int2 spawnPosition;
                do
                {
                    spawnPosition = _random.NextInt2(int2.zero, farmSize);
                    arrayIndex = Utilities.FlatIndex(spawnPosition.x, spawnPosition.y, farmSize.y);

                    farmStats.FarmerCount += 1;

                } while (farmBuffer[arrayIndex].TileState != TileState.Empty);

                var farmerEntity = EntityManager.Instantiate(farmData.FarmerPrefab);
                var farmerPosition = new Translation
                {
                    Value = new float3(spawnPosition.x, 0, spawnPosition.y)
                };
                EntityManager.SetComponentData(farmerEntity, farmerPosition);
            }

            EntityManager.SetComponentData(farmEntity, farmStats);
        }
    }
}