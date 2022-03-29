using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace AutoFarmers
{
    [UpdateAfter(typeof(FarmerTaskSearchSystem))]
    public partial class MiningSystem : SystemBase
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
                .WithAll<MiningTaskTag>()
                .WithNone<TargetData>()
                .ForEach((Entity e, in Translation translation) =>
                {

                    var currentPosition = translation.Value.ToTileIndex();
                    var index = Utilities.FlatIndex(currentPosition.x, currentPosition.y, farmData.FarmSize.y);
                    var tile = farmBuffer[index];
                    tile.TileState = TileState.Empty;
                    tile.IsTargeted = false;
                    ecb.RemoveComponent<MiningTaskTag>(e);
                    var rockEntity = tile.OccupiedObject;
                    ecb.DestroyEntity(rockEntity);
                    tile.OccupiedObject = Entity.Null;
                    farmBuffer[index] = tile;
                }).Run();
        }
    }
}