using Unity.Entities;
using Unity.Transforms;

namespace AutoFarmers
{
    public partial class DepositingSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem CommandBufferSystem;

        protected override void OnCreate()
        {
            CommandBufferSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var time = Time.DeltaTime;
            var farmEntity = GetSingletonEntity<FarmData>();
            var farmData = GetSingleton<FarmData>();
            var farmBuffer = GetBuffer<TileBufferElement>(farmEntity);
            var ecb = CommandBufferSystem.CreateCommandBuffer();

            Entities
                .WithAll<DepositingTag>()
                .WithNone<TargetData>()
                .ForEach((Entity e, in DynamicBuffer<Child> childBuffer, in Translation translation) =>
                {
                    var currentPosition = translation.Value.ToTileIndex();
                    var index = Utilities.FlatIndex(currentPosition.x, currentPosition.y, farmData.FarmSize.y);
                    TileBufferElement tile = farmBuffer[index];
                    var silo = tile.OccupiedObject;

                    var plant = childBuffer[0].Value;
                    ecb.DestroyEntity(plant);

                    ecb.RemoveComponent<DepositingTag>(e);

                }).Run();
        }
    }
}