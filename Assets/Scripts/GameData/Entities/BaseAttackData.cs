using UnityEngine;
using System.Collections.Generic;


public abstract class BaseAttackData : ScriptableObject
{
    public int Cooldown;
    public int AddedDamage;

    public List<Vector2Int> TargetOffsetsNorth;

    public BaseEntityData SpawnData; // Projectiles, volumes, etc

    public abstract BaseAttack SpawnRuntime();
}
