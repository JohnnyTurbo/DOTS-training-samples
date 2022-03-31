using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace AutoFarmers
{
    public static class Utilities
    {
        public static int FlatIndex(int x, int y, int h) => h * x + y;

        public static int2 ToTileIndex(this float3 position)
        {
            return new int2((int)math.round(position.x), (int)math.round(position.z));
        }

        public struct PathfindingNode
        {
            public int2 Pos;
            public int2 CameFrom;
            public float F => G + H;
            public float G;
            public float H;
            public bool IsValid;

            public PathfindingNode(int2 startPos, bool isValid)
            {
                Pos = startPos;
                CameFrom = new int2(-1, -1);
                G = 0.0f;
                H = 0.0f;
                IsValid = isValid;
            }
        }

        public struct AStarPathfinding
        {
            NativeList<PathfindingNode> Map; // We could cache the map with the rocks and only update it if there's a change to rocks (e.g. destroyed)
            int2 MapSize;
            int MapCapacity => MapSize.x * MapSize.y;

            public AStarPathfinding(in DynamicBuffer<TileBufferElement> farmBuffer, int2 farmSize)
            {
                MapSize = farmSize;
                int farmCapacity = MapSize.x * MapSize.y;

                Map = new NativeList<PathfindingNode>(farmCapacity, Allocator.Temp);

                for (var i = 0; i < MapSize.x; ++i)
                {
                    for (var j = 0; j < MapSize.y; ++j)
                    {
                        Map.Add(new PathfindingNode());
                    }
                }

                for (var i = 0; i < MapSize.x; ++i)
                {
                    for (var j = 0; j < MapSize.y; ++j)
                    {
                        var curPos = new int2(i, j);
                        var mapIndex = Utilities.FlatIndex(i, j, MapSize.y);
                        var isValid = farmBuffer[mapIndex].TileState != TileState.Rock;

                        ref var tile = ref Map.ElementAt(mapIndex);
                        tile.Pos = curPos;
                        tile.CameFrom = new int2(-1, -1);
                        tile.G = 0.0f;
                        tile.H = 0.0f;
                        tile.IsValid = isValid;
                    }
                }
            }

            public NativeList<int2> FindPath(int2 startPos, int2 endPos)
            {
                NativeList<int2> openList = new NativeList<int2>(MapCapacity, Allocator.Temp);
                NativeList<int2> closeList = new NativeList<int2>(MapCapacity, Allocator.Temp);
                NativeList<int2> result = new NativeList<int2>(MapCapacity, Allocator.Temp);

                var index = FlatIndex(startPos);
                ref var startNode = ref Map.ElementAt(index);

                startNode.H = EstimatedCost(startPos, endPos);
                startNode.G = 0.0f;

                openList.Add(startPos);
                while (!openList.IsEmpty)
                {
                    var curNodePos = GetAndRemoveSmallestFNode(ref openList);
                    var curNodeTile = GetTileAt(curNodePos);
                    closeList.Add(curNodePos);

                    if (curNodePos.Equals(endPos))
                    {
                        result = GetBacktrackedPath(endPos); // success!
                        break;
                    }

                    var neighbours = GetAccessibleNodesFrom(curNodePos);
                    for (var i = 0; i < neighbours.Length; ++i)
                    {
                        var neighbour = neighbours[i];

                        if (Contains(closeList, neighbour))
                        {
                            continue;
                        }

                        var possibleG = curNodeTile.G + EstimatedCost(curNodePos, neighbour);
                        var isPossibleGBetter = false;

                        var neighbourIndex = FlatIndex(neighbour);
                        ref var neighbourTile = ref Map.ElementAt(neighbourIndex);

                        if (!Contains(openList, neighbour))
                        {
                            neighbourTile.H = EstimatedCost(neighbour, endPos);
                            openList.Add(neighbour);

                            isPossibleGBetter = true;
                        }
                        else if (possibleG < neighbourTile.G)
                        {
                            isPossibleGBetter = true;
                        }

                        // not in the if-elseif
                        if (isPossibleGBetter)
                        {
                            neighbourTile.CameFrom = curNodeTile.Pos;
                            neighbourTile.G = possibleG;
                        }
                    }
                }

                // "result" might be empty
                return result;
            }

            public NativeList<int2> GetBacktrackedPath(int2 endPos)
            {
                NativeList<int2> reversedResult = new NativeList<int2>(MapCapacity, Allocator.Temp);
                reversedResult.Add(endPos);

                var cameFrom = GetTileAt(endPos).CameFrom;
                while (IsValidPosition(cameFrom))
                {
                    reversedResult.Add(cameFrom);
                    cameFrom = GetTileAt(cameFrom).CameFrom;
                }

                NativeList<int2> result = new NativeList<int2>(MapCapacity, Allocator.Temp);
                for (var i = reversedResult.Length - 1; i >= 0; --i)
                {
                    result.Add(reversedResult[i]);
                }

                return result;
            }

            public float EstimatedCost(int2 startPos, int2 endPos)
            {
                return math.distance(startPos, endPos);
            }

            public int2 GetAndRemoveSmallestFNode(ref NativeList<int2> list)
            {
                // we are assuming list is never empty
                var smallestListIndex = -1;
                var smallestF = float.MaxValue;
                var result = new int2(-1, -1);

                for (var i = 0; i < list.Length; ++i)
                {
                    var node = GetTileAt(list[i]);
                    if (node.F < smallestF)
                    {
                        smallestListIndex = i;
                        smallestF = node.F;
                        result = node.Pos;
                    }
                }

                list.RemoveAt(smallestListIndex);
                return result;
            }

            public NativeList<int2> GetAccessibleNodesFrom(int2 pos)
            {
                NativeList<int2> result = new NativeList<int2>(4, Allocator.Temp);

                int2 up = pos + new int2(0, 1);
                int2 down = pos + new int2(0, -1);
                int2 left = pos + new int2(-1, 0);
                int2 right = pos + new int2(1, 0);

                if (IsValidPosition(up))
                {
                    result.Add(up);
                }
                if (IsValidPosition(down))
                {
                    result.Add(down);
                }
                if (IsValidPosition(left))
                {
                    result.Add(left);
                }
                if (IsValidPosition(right))
                {
                    result.Add(right);
                }

                return result;
            }

            public PathfindingNode GetTileAt(int2 pos)
            {
                return GetTileAt(FlatIndex(pos.x, pos.y));
            }

            public PathfindingNode GetTileAt(int index)
            {
                if (IsValidIndex(index))
                    return Map[index];
                return InvalidNode;
            }

            public bool IsValidIndex(int index)
            {
                return index >= 0 && index < MapCapacity;
            }

            public bool IsValidPosition(int2 pos)
            {
                var index = FlatIndex(pos.x, pos.y);

                var isValidIndex = IsValidIndex(index);
                if (!isValidIndex)
                    return false;

                var isValid = Map[index].IsValid;
                return isValid;
            }

            public bool Contains(NativeArray<int2> list, int2 elem)
            {
                foreach (var e in list)
                {
                    if (e.Equals(elem))
                        return true;
                }
                return false;
            }

            public int FlatIndex(int2 pos) => FlatIndex(pos.x, pos.y);
            public int FlatIndex(int i, int j) => Utilities.FlatIndex(i, j, MapSize.y);
            private PathfindingNode InvalidNode => new PathfindingNode(new int2(-1, -1), false);
        }
    }
}
