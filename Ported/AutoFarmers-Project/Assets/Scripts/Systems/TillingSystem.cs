using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace AutoFarmers
{
    [UpdateAfter(typeof(FarmerTaskSearchSystem))]
    public partial class TillingSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem _ecbSystem;

        protected override void OnStartRunning()
        {
            _ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var farmEntity = GetSingletonEntity<FarmData>();
            var farmBuffer = GetBuffer<TileBufferElement>(farmEntity);
            var farmData = GetComponent<FarmData>(farmEntity);
            var ecb = _ecbSystem.CreateCommandBuffer();
            
            Entities
                .WithAll<TillingTag>()
                .WithNone<TargetData>()
                .ForEach((Entity e, in Translation translation) =>
                {
                    var currentPosition = new int2((int)translation.Value.x, (int)translation.Value.z);
                    var index = Utilities.FlatIndex(currentPosition.x, currentPosition.y, farmData.FarmSize.y);
                    var tile = farmBuffer[index];
                    tile.TileState = TileState.Tilled;
                    tile.IsTargeted = false;
                    var tilledColor = new URPMaterialPropertyBaseColor { Value = new float4
                    {
                        x = farmData.TilledColor.r,
                        y = farmData.TilledColor.g,
                        z = farmData.TilledColor.b,
                        w = farmData.TilledColor.a
                    }};
                    ecb.SetComponent(tile.TileRenderEntity, tilledColor);
                    farmBuffer[index] = tile;
                    ecb.RemoveComponent<TillingTag>(e);
                }).Run();
        }
    }
}