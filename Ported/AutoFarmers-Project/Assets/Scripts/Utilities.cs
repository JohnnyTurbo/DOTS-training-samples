using Unity.Mathematics;
using Unity.Transforms;

namespace AutoFarmers
{
    public static class Utilities
    {
        public static int FlatIndex(int x, int y, int h) => h * x + y;

        public static int2 ToTileIndex(this float3 position)
        {
            return new int2((int)math.round(position.x), (int)math.round(position.z));
        }
    }
}