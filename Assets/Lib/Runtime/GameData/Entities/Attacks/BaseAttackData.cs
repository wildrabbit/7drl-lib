using UnityEngine;
using System.Collections.Generic;


public abstract class BaseAttackData : ScriptableObject
{
    public int Cooldown;
    public int MinDamage;
    public int MaxDamage;

    public string[] Attributes;



    public abstract BaseAttack SpawnRuntime();
}
