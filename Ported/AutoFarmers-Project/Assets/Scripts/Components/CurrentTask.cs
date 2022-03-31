using Unity.Entities;

namespace AutoFarmers
{
    [GenerateAuthoringComponent]
    public struct CurrentTask : IComponentData
    {
        public TaskTypes Value;
    }
}