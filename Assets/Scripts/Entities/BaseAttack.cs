using UnityEngine;
using System.Collections.Generic;

public abstract class BaseAttack
{
    public bool Ready => Data.Cooldown == 0 || Elapsed < 0 || Elapsed >= (float)Data.Cooldown;
    public BaseAttackData Data;
    public float Elapsed;

    public abstract bool CanTargetBeReached(Vector2Int source, Vector2Int target, out MoveDirection dir);

    public Vector2Int Rotate90CC(Vector2Int coords)
    {
        return new Vector2Int(-coords.y, coords.x);
    }
}
