using System;
using System.Collections.Generic;
using UnityEngine;

public enum GeneratorType
{
    Fixed,
    BSP,
    Room7DRL
}

public abstract class BaseMapGeneratorData : ScriptableObject
{
    public abstract GeneratorType GeneratorType { get; }
    public abstract Vector2Int  MapSize { get; }

    public int WallTile;
    public int GroundTile;
    public int NoTile;

    public bool OriginIsTopLeft;
}