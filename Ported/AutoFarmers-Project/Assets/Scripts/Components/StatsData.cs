using Unity.Entities;

namespace AutoFarmers
{
    [GenerateAuthoringComponent]
    public struct StatsData : IComponentData
    {
        public int HarvestCount;
        public int WorkerCount;
        public int FarmerCount;
        public int DroneCount;
    }
}