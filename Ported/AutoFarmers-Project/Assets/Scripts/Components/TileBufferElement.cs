using System;
using Unity.Entities;
using Unity.Mathematics;

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

    public static class TileStateExtensions
    {
        public static TaskTypes ToTaskType(this TileState tileState)
        {
            switch (tileState)
            {
                case TileState.Empty:
                    return TaskTypes.Tilling;
                case TileState.Tilled:
                    return TaskTypes.Planting;
                case TileState.Rock:
                    return TaskTypes.Mining;
                case TileState.Harvestable:
                    return TaskTypes.Harvesting;
                default:
                    return TaskTypes.None;
            }
        }
    }
    
    [GenerateAuthoringComponent]
    public struct TileBufferElement : IBufferElementData
    {
        public Entity TileRenderEntity;
        public Entity OccupiedObject;
        public TileState TileState;
        public int2 ClosestSiloLocation;
        public bool IsTargeted;
    }
}