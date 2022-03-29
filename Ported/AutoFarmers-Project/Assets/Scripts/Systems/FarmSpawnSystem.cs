﻿using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace AutoFarmers
{
    public partial class FarmSpawnSystem : SystemBase
    {
        private Random _random;

        protected override void OnUpdate()
        {
            // Run code on first update only
            this.Enabled = false;
            _random.InitState(1234);

            var farmEntity = GetSingletonEntity<FarmData>();
            var farmData = GetSingleton<FarmData>();
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

                    if (_random.NextFloat() <= farmData.PercentSilos)
                    {
                        var newSilo = EntityManager.Instantiate(farmData.SiloPrefab);
                        EntityManager.SetComponentData(newSilo, fieldPosition);
                        tileState = TileState.Silo;
                    }
                    else if (_random.NextFloat() <= farmData.PercentRocks)
                    {
                        var newRock = EntityManager.Instantiate(farmData.RockPrefab);
                        EntityManager.SetComponentData(newRock, fieldPosition);
                        tileState = TileState.Rock;
                    }

                    var newTileBufferElement = new TileBufferElement
                    {
                        TileRenderEntity = newFieldTile,
                        TileState = tileState,
                        IsTargeted = false
                    };

                    farmBuffer = EntityManager.GetBuffer<TileBufferElement>(farmEntity);
                    farmBuffer.Add(newTileBufferElement);
                }
            }

            farmBuffer = EntityManager.GetBuffer<TileBufferElement>(farmEntity);
            int arrayIndex;
            int2 spawnPosition;
            do
            {
                spawnPosition = _random.NextInt2(int2.zero, farmSize);
                arrayIndex = Utilities.FlatIndex(spawnPosition.x, spawnPosition.y, farmSize.y);
            } while (farmBuffer[arrayIndex].TileState != TileState.Empty);

            var farmerEntity = EntityManager.Instantiate(farmData.FarmerPrefab);
            var farmerPosition = new Translation
            {
                Value = new float3(spawnPosition.x, 0, spawnPosition.y)
            };
            EntityManager.SetComponentData(farmerEntity, farmerPosition);
        }
    }
}