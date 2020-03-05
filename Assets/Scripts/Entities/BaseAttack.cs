using UnityEngine;
using System.Collections.Generic;
using System;

public abstract class BaseAttack
{
    public abstract int MaxRadius { get; }
    public bool Ready => Data.Cooldown == 0 || Elapsed < 0 || Elapsed >= (float)Data.Cooldown;
    public BaseAttackData Data;
    public float Elapsed;
    public string[] Attributes => Data.Attributes;

    public abstract void Init();
    
    public abstract bool CanTargetBeReached(Vector2Int source, Vector2Int target, out List<MoveDirection> dir);

    public abstract bool CanTargetBeReachedAtDir(Vector2Int coords1, Vector2Int coords2, MoveDirection direction);

    public abstract List<Vector2Int> GetRotatedOffsets(MoveDirection moveDir);
}
