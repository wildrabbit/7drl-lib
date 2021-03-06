using UnityEngine;
using System.Collections;
using System;

public enum DistanceStrategy
{ 
    Manhattan,
    Chebyshev
}

public class RectGridMap : IMapHelper
{
    DistanceStrategy _strategy;
    bool IncludeDiagonals;
    public RectGridMap(DistanceStrategy strategy)
    {
        _strategy = strategy;
    }

    MoveDirection[] _diagonalDirections = new MoveDirection[]
    {
        MoveDirection.None,
        MoveDirection.N, MoveDirection.NE, MoveDirection.SE,
        MoveDirection.S, MoveDirection.SW, MoveDirection.NW,
        MoveDirection.E, MoveDirection.E
    };

    MoveDirection[] _noDiagonalDirections = new MoveDirection[]
    {
        MoveDirection.None, MoveDirection.N, MoveDirection.E, MoveDirection.S, MoveDirection.W
    };

    Vector2Int[] _neighbourOffsetsDiagonals = new Vector2Int[]
{
        new Vector2Int(0, 0),
        new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(-1,1),
        new Vector2Int(-1, 0), new Vector2Int(-1, -1), new Vector2Int(1, -1),
        new Vector2Int(0,1), new Vector2Int(0,-1)
};

    Vector2Int[] _neighbourOffsetsNoDiagonals = new Vector2Int[]
    {
        new Vector2Int(0, 0),
        new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(-1, 0), new Vector2Int(0,-1)
    };

    public int Distance(Vector2Int c1, Vector2Int c2)
    {
        int distance = 0;
        switch(_strategy)
        {
            case DistanceStrategy.Chebyshev:
            {
                distance  = Mathf.Max(Mathf.Abs(c2.x - c1.x), Mathf.Abs(Mathf.Abs(c2.y - c1.y)));
                break;
            }
            case DistanceStrategy.Manhattan:
            {
                distance = Mathf.Abs(c2.x - c1.x) + Mathf.Abs(c2.y - c1.y);
                break;
            }
        }
        return distance;
    }
    public Vector2Int GetDirectionOffset(MoveDirection moveDirection, Vector2Int srcCoords, bool diagonals)
    {
        MoveDirection[] dirList = diagonals ? _diagonalDirections : _noDiagonalDirections;
        int idx = System.Array.IndexOf(dirList, moveDirection);
        if (idx >= 0)
        {
            return GetOffsets(srcCoords, diagonals)[idx];
        }
        return Vector2Int.zero;
        
    }

    public Vector2Int[] GetOffsets(Vector2Int coords, bool diagonals)
    {
        return diagonals? _neighbourOffsetsDiagonals : _neighbourOffsetsNoDiagonals;
    }
}
