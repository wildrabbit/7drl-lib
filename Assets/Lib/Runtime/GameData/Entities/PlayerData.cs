﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName ="New PlayerData", menuName = "7DRL_Lib/Player")]
public class PlayerData: BaseEntityData
{
    public bool CanMoveIntoMonsterCoords;
    public int MonsterCollisionDmg;

    public HPTraitData HPData;
    public BaseMovingTraitData MovingTraitData;
    public BattleTraitData BattleData;
    public float Speed;

}