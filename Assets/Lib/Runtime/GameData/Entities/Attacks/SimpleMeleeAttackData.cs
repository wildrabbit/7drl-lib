using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New MeleeAttack", menuName = "7DRL_Lib/Data/Attacks/Melee")]
public class SimpleMeleeAttackData : BaseAttackData
{
    public bool SingleTarget;

    public override BaseAttack SpawnRuntime()
    {
        return new SimpleMeleeAttack(this);
    }
}
