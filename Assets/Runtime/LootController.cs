using System;
using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
public class LootSpawn
{
    public int lootID;
    public int amount;
    public Vector2Int coords;
}

[System.Serializable]
public class LootInfo
{
    public float LootChance;
    //public List<BombData> Items;

    public int MinItems; // amount of same item
    public int MaxItems;

    public int MinRolls; // number of items
    public int MaxRolls;
}

public class LootController
{
    IMapController _map;
    IEntityController _entityController;
    LootData _lootData;

    public void Init(LootData _itemData, IMapController map, IEntityController entityController)
    {
        _map = map;
        _entityController = entityController;
        
        _entityController.OnMonsterKilled += OnMonsterKilled;
    }

    private void OnMonsterKilled(Monster monster)
    {
        //GenerateLootAt(monster.LootInfo, monster.Coords);
    }

    public void LoadLootSpawns(List<LootSpawn> lootSpawns)
    {
        foreach(var spawn in lootSpawns)
        {
            // _entityController.CreatePickable(_data, spawn.item, spawn.coords, spawn.amount, false);
        }
    }

    public bool GenerateLootAt(LootInfo info, Vector2Int coords)
    {
        if(UnityEngine.Random.value < info.LootChance)
        {
            int numRolls = UnityEngine.Random.Range(info.MinRolls, info.MaxRolls + 1);
            for(int i = 0; i < numRolls; ++i)
            {
                int amount = UnityEngine.Random.Range(info.MinItems, info.MaxItems + 1);
                // select Data
                // _entityController.CreatePickable(_data, item, coords, amount, false);
            }
            return true;
        }
        return false;
    }

    public void Cleanup()
    {
        _entityController.OnMonsterKilled -= OnMonsterKilled;
    }
}