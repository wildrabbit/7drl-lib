using UnityEngine;
using System.Collections;

public interface IMapHelper
{
    int Distance(Vector2Int c1, Vector2Int c2);
    Vector2Int GetDirectionOffset(MoveDirection moveDirection, Vector2Int srcCoords, bool diagonals);
    Vector2Int[] GetOffsets(Vector2Int srcCoords, bool diagonals);
}
