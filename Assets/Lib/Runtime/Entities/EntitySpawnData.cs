using UnityEngine;

[System.Serializable]
public class TrapSpawn
{
    public TrapData TrapData;
    public Vector2Int Coords;
}

[System.Serializable]
public class BlockSpawn
{
    public BlockingData BlockData;
    public Vector2Int Coords;
}