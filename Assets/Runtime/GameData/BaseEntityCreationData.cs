using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BaseEntityCreationData : ScriptableObject
{
    public PlayerData PlayerData;
    public Player PlayerPrefab;

    public List<MonsterData> MonsterData;
    public Monster MonsterPrefab;

    public BlockingEntity BlockPrefab;
    public Trap TrapPrefab;

}
