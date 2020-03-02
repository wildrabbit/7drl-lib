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
    public RectGridMap(DistanceStrategy strategy)
    {
        _strategy = strategy;
    }

    Vector2Int[] _neighbourOffsets = new Vector2Int[]
    {
        new Vector2Int(0, 0),
        new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(-1,1),
        new Vector2Int(-1, 0), new Vector2Int(-1, -1), new Vector2Int(1, -1),
        new Vector2Int(0,1), new Vector2Int(0,-1)
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
    public Vector2Int GetDirectionOffset(MoveDirection moveDirection, Vector2Int srcCoords)
    {
        return _neighbourOffsets[(int)moveDirection];
    }

    public Vector2Int[] GetOffsets(Vector2Int coords)
    {
        return _neighbourOffsets;
    }
}
