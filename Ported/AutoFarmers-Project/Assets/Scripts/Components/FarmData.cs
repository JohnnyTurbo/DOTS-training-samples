using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace AutoFarmers
{
    [GenerateAuthoringComponent]
    public struct FarmData : IComponentData
    {
        public Entity FarmerPrefab;
        public Entity DronePrefab;
        public Entity RockPrefab;
        public Entity FieldPrefab;
        public Entity SiloPrefab;
        public Entity PlantPrefab;

        public float PlantGrowthRate;
        public float PercentSilos;
        public float PercentRocks;
        public float DroneTimeout;
        
        public int HarvestThreshold; // number of harvests before spawning a farmer
        public int DroneThreshold;
        public int DronesToSpawn;
        public int DefaultFarmerSearchRadius;
        public int DefaultDroneSearchRadius;
        
        public int2 FarmSize;
        public Color TilledColor;

        public int MaxFarmSize => FarmSize.x * FarmSize.y * 4;
    }
}