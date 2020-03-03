using UnityEngine;
using System.Collections.Generic;

public abstract class BaseAttack
{
    public bool Ready => Data.Cooldown == 0 || Elapsed < 0 || Elapsed >= (float)Data.Cooldown;
    public BaseAttackData Data;
    public float Elapsed;

    public abstract bool CanTargetBeReached(Vector2Int source, Vector2Int target, out MoveDirection dir);

    public void Rotate90CC(ref Vector2Int coords)
    {
        int aux = coords.x;
        coords.x = -coords.y;
        coords.y = aux;
    }
}
