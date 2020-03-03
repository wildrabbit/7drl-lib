﻿using System;
using System.Collections.Generic;
using UnityEngine;

using URandom = UnityEngine.Random;

public class BattleActionResult
{
    // Stage 1: Free for all
    public int AttackerDmgInflicted;
    public int DefenderDmgTaken;

    // Stage 2: Hit chance
    public bool AttackerFlopped;
    public bool DefenderAvoided;
    public bool Critical;
    public bool DefenderDefeated;

    // Stage 3: Ripostes, counters, etc
    public bool DefenderCountered;
    public BattleActionResult CounterResult;

    public bool WillDefeatDefender;

    public string AttackerName;
    public string DefenderName;
    public int DefenderStartHP;
}

public class BattleUtils
{
    public static void SolveAttack(IBattleEntity2 attacker, IBattleEntity2 defender, out BattleActionResult results)
    {
        results = new BattleActionResult();
        results.AttackerName = attacker.Name;
        results.DefenderName = defender.Name;

        results.DefenderStartHP = defender.HPTrait.HP;
        
        // 1. Solve chance to hit
        float attackerHitChance = 1.0f;
        if (URandom.value > attackerHitChance)
        {
            results.AttackerFlopped = true;
            return;
        }

        // 2. Solve defender's deflection
        float defenderAvoidChance = 0.0f;
        if (URandom.value <= defenderAvoidChance)
        {
            results.DefenderAvoided = true;
            return;
        }

        // 3. Crit check?
        float attackCriticalBonus = 0.0f;

        float attackerCritChance = 0.0f;
        if (URandom.value <= attackerCritChance)
        {
            attackCriticalBonus = 0.0f;
            results.Critical = true;
        }

        // 4. Attack
        float attack = attacker.BattleTrait.Damage;
        attack *= (1 + attackCriticalBonus);
        float defense = 0.0f;

        int damageInflicted = (int)((attack * attack) / (attack + defense));
        results.AttackerDmgInflicted = damageInflicted;
        results.DefenderDmgTaken = Mathf.Min(damageInflicted, defender.HPTrait.HP);
        results.DefenderDefeated = damageInflicted >= defender.HPTrait.HP;

        attacker.ApplyBattleResults(results, BattleRole.Attacker);
        defender.ApplyBattleResults(results, BattleRole.Defender);
    }
}