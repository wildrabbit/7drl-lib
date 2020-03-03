using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "New MonsterData", menuName = "7DRL_Lib/Monster")]
public class MonsterData: BaseEntityData
{
    public float ThinkingDelay;
    public HPTraitData HPData;
    public BaseMovingTraitData MovingTraitData;
    public BattleTraitData BattleData;

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
