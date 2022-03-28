using Unity.Entities;

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
    }
}