using System;
using Unity.Entities;

namespace AutoFarmers
{
    public enum TaskTypes
    {
        Harvesting = 3,
        Planting = 1,
        Tilling = 2,
        Mining = 0,
        None = 4
    }

    public static class TaskTypesExtensions
    {
        public static ComponentType TaskType(this TaskTypes taskType)
        {
            switch (taskType)
            {
                case TaskTypes.Harvesting:
                    return ComponentType.ReadOnly<HarvestingTag>();
                case TaskTypes.Planting:
                    return ComponentType.ReadOnly<PlantingTag>();
                case TaskTypes.Tilling:
                    return ComponentType.ReadOnly<TillingTag>();
                case TaskTypes.Mining:
                    return ComponentType.ReadOnly<MiningTaskTag>();
                default:
                    throw new ArgumentOutOfRangeException(nameof(taskType), taskType, null);
            }
        }
    }

}