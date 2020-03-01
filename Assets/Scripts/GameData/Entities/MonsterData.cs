using System.Collections.Generic;
using UnityEngine;

public enum MonsterAITemplate
{
    Chaser,
    Avoider,
    Wanderer,
    Smartass
}

// TODO: Put ai template params in ai template data??


[CreateAssetMenu(fileName = "New MonsterData", menuName = "7DRL_Lib/Monster")]
public class MonsterData: BaseEntityData
{
    public float ThinkingDelay;
    public HPTraitData HPData;
    public BaseMovingTraitData MovingTraitData;
    public MonsterState InitialState;

    public int WanderToIdleMinTurns;
    public int WanderToIdleMaxTurns;

    public int MinIdleTurns;
    public int MaxIdleTurns;

    public bool IsMelee;
    public int MeleeDamage;

    public int VisibilityRange; // Wander -> Chase
    public float EscapeHPRatio;
    public int EscapeSafeDistance;

    public int PlayerCollisionDmg;

    public float PathUpdateDelay;
    
    [Header("loot stuff")]
    public LootInfo LootInfoOnDeath;
    public int XPOnDeath;
}
