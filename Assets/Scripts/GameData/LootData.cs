using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LootItemData
{
    [SerializeField] int _lootID;
    public int LootID => _lootID;
}

public class LootData : ScriptableObject
{
    public List<LootItemData> Loot;
}