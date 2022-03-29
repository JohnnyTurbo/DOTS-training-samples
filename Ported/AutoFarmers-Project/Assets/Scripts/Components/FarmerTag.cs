using Unity.Entities;

namespace AutoFarmers 
{
    [GenerateAuthoringComponent]
    public struct FarmerTag : IComponentData {}
    
    public struct MiningTaskTag : IComponentData {}
    
    public struct HarvestingTag : IComponentData {}
    
    public struct TillingTag : IComponentData {}
    
    public struct PlantingTag : IComponentData {}
    
}