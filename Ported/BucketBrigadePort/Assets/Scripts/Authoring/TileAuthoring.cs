﻿using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct Tile : IComponentData
{
    public int2 Id;
}
