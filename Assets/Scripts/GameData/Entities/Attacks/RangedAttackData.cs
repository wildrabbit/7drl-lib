using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New RangedAttack", menuName = "7DRL_Lib/Data/Attacks/Ranged")]
public class RangedAttackData : BaseAttackData
{
    public bool Piercing;
    public List<TileBase> AllowedTiles;
    public int MinRange;
    public int MaxRange;

    public override BaseAttack SpawnRuntime()
    {
        return new RangeAttack(this);
    }
}
