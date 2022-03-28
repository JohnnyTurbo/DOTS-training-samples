using Unity.Entities;

namespace AutoFarmers
{
    public enum TileState
    {
        Empty,
        Tilled,
        Planted,
        Rock,
        Silo,
        Harvestable
    }
    
    [GenerateAuthoringComponent]
    public struct TileBufferElement : IBufferElementData
    {
        public Entity TileRenderEntity;
        public TileState TileState;
        public bool IsTargeted;
    }
}