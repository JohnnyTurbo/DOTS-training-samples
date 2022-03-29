﻿using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace AutoFarmers
{
    public partial class FarmerTaskSearchSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var farmEntity = GetSingletonEntity<FarmData>();
            var farmBuffer = GetBuffer<TileBufferElement>(farmEntity);
            var farmSize = GetComponent<FarmData>(farmEntity).FarmSize;
            
            Entities
                .WithNone<TargetData>()
                .ForEach((Entity e, ref SearchRadiusData radius, in Translation translation) =>
                {
                    var curBestTask = TaskTypes.None;
                    var bestTilePos = new int2(-1, -1);
                    var startingPos = new int2((int)translation.Value.x, (int)translation.Value.z);
                    int X = farmSize.x;
                    int Y = farmSize.y;
                
                    int x, y, dx, dy;
                    x = y = dx = 0;
                    dy = -1;
                    for (var i = 0; i < radius.Value; i++)
                    {
                        if ((-X / 2 <= x) && (x <= X / 2) && (-Y / 2 <= y) && (y <= Y / 2))
                        {
                            int2 curPosition = startingPos + new int2(x, y);
                            if (!(curPosition.x < 0 || curPosition.x >= X || curPosition.y < 0 || curPosition.y >= Y))
                            {
                                var index = FlatIndex(curPosition.x, curPosition.y, farmSize.y);
                                var tile = farmBuffer[index];
                                if (!tile.IsTargeted)
                                {
                                    var tileTaskType = tile.TileState.ToTaskType();
                                    if (tileTaskType < curBestTask)
                                    {
                                        curBestTask = tileTaskType;
                                        bestTilePos = curPosition;
                                    }
                                }
                            }
                        }

                        if ((x == y) || ((x < 0) && (x == -y)) || ((x > 0) && (x == 1 - y)))
                        {
                            var t = dx;
                            dx = -dy;
                            dy = t;
                        }

                        x += dx;
                        y += dy;
                    }
                    Debug.Log($"Cur Best Task: {curBestTask} at pos: {bestTilePos}");
                    EntityManager.AddComponent<TargetData>(e);
                }).WithStructuralChanges().Run();
        }
        
        private static int FlatIndex(int x, int y, int h) => h * x + y;
    }
}

/*
void Spiral( int X, int Y){
    int x,y,dx,dy;
    x = y = dx =0;
    dy = -1;
    int t = std::max(X,Y);
    int maxI = t*t;
    for(int i =0; i < maxI; i++){
        if ((-X/2 <= x) && (x <= X/2) && (-Y/2 <= y) && (y <= Y/2)){
            // DO STUFF...
        }
        if( (x == y) || ((x < 0) && (x == -y)) || ((x > 0) && (x == 1-y))){
            t = dx;
            dx = -dy;
            dy = t;
        }
        x += dx;
        y += dy;
    }
}*/