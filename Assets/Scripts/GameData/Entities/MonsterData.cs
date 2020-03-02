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
    public AI.State InitialState;
    public AI.State NoState;

    public string[] userTags; // TODO: Resolve tags from properties!
    HashSet<string> _tags;
    public HashSet<string> MonsterTags
    {
        get
        {
            if(_tags == null)
            {
                ResolveTags();
            }
            return _tags;
        }
    }

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

    public void ResolveTags()
    {
        _tags = new HashSet<string>(userTags);
        // TODO: Resolve tags depending on data properties (i.e: difficulty, etc, etc)
    }

    public bool MatchesTagSet(IEnumerable<string> reference)
    {
        return _tags.IsSupersetOf(reference);
    }

    public bool CheckTag(string tag)
    {
        return _tags.Contains(tag);
    }

}
