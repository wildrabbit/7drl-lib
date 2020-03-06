using System;
using System.Collections.Generic;
using UnityEngine;

public enum BattleRole
{
    Attacker,
    Defender
}

public interface IBattleEntity: IEntity
{
    HPTrait HPTrait { get; }
    BattleTrait BattleTrait { get;  }
    
    bool IsHostileTo(IBattleEntity other);         

    void ApplyBattleResults(BattleActionResult result, BattleRole role);
}

public class BattleTrait
{
    public BaseAttack FirstAttack => (_attacks == null || _attacks.Count == 0) ? null : _attacks[0];
    BaseAttack CurrentAttack => _attacks[_currentAttackIdx];

    public int Damage => UnityEngine.Random.Range(CurrentAttack.Data.MinDamage, CurrentAttack.Data.MaxDamage);
    IBattleEntity Owner => _owner;
    IBattleEntity _owner;

    BattleTraitData _data;
    
    List<BaseAttack> _attacks;
    int _currentAttackIdx;

    List<IBattleEntity> _targets;
    MoveDirection _attackDirection;

    public void GetReachableStateForCoords(List<Vector2Int> coords, out List<bool> state)
    {
        state = CurrentAttack.GetReachableStateForCoords(_owner, coords);
    }

    IEntityController _entityController;
    BaseGameEvents _gameEvents;
    BaseGameEvents.BattleEvents _battleEvents;

    public void Init(IEntityController entityController, IMapController mapController, BattleTraitData data, IBattleEntity owner, BaseGameEvents gameEvents)
    {
        _targets = new List<IBattleEntity>();
        _data = data;
        _entityController = entityController;
        _owner = owner;
        _attacks = new List<BaseAttack>();
        _currentAttackIdx = 0;
        _gameEvents = gameEvents;
        _battleEvents = gameEvents.Battle;
        foreach(var attackData in data.Attacks)
        {
            var attack = attackData.SpawnRuntime();
            attack.Init(entityController, mapController);
            _attacks.Add(attack);
        }
    }

    
    public bool TryGetAvailableAttack()
    {
        int bestIdx = -1;
        int bestAverageDamage = 0;
        List<IBattleEntity> bestTargets = new List<IBattleEntity>();
       
        for (int i = 0; i < _attacks.Count; ++i)
        {
            if (!_attacks[i].Ready) continue;

            var targets = _attacks[i].FindAllReachableTargets(_owner);
            if (targets.Count == 0) continue;
            
            int totalDamage = targets.Count * (_attacks[i].Data.MinDamage + _attacks[i].Data.MaxDamage) / 2;
            if (totalDamage >= bestAverageDamage)
            {
                bestIdx = i;
                bestAverageDamage = totalDamage;
                bestTargets = targets;
            }                        
        }
            
        if (bestIdx >= 0)
        {
            _currentAttackIdx = bestIdx;
            PrepareAttack(bestTargets);
            return true;
        }

        _attackDirection = MoveDirection.None;
        _targets?.Clear();
        return false;
    }

    public bool TryAttackCoords(Vector2Int newPlayerCoords)
    {
        if (!CurrentAttack.Ready)
        {
            _battleEvents.SendAttemptedAttackOnCooldown(_owner);
            return false;
        }
            
        var targets = CurrentAttack.FindTargetsAtCoords(_owner, newPlayerCoords);
        if(targets.Count > 0)
        {
            PrepareAttack(targets);
            var defeated = StartAttack();
            return defeated.IsSupersetOf(targets);
        }
        return false;
    }

    public bool CanAttackEntity(IBattleEntity other)
    {
        return CurrentAttack.CanTargetBeReached(_owner, other);
    }

    public bool TryAttackEntity(IBattleEntity other)
    {
        bool canAttack = _owner.IsHostileTo(other) && CurrentAttack.CanTargetBeReached(_owner, other);
        if(canAttack)
        {
            PrepareAttack(CurrentAttack.FindAllReachableTargets(_owner));
        }
        var defeated = StartAttack();
        return defeated.Contains(other);
    }

    public void PrepareAttack(List<IBattleEntity> targets)
    {
        _targets = targets;
    }

  
    public HashSet<IBattleEntity> StartAttack()
    {
        // Use direction for the view
        HashSet<IBattleEntity> defeated = new HashSet<IBattleEntity>();

        foreach(var target in _targets)
        {
            BattleUtils.SolveAttack(_owner, target, out var result);
            _battleEvents.SendAttack(_owner, target, result);
            if(result.DefenderDefeated)
            {
                defeated.Add(target);
            }
        }
       
        if (_targets.Count > 0)
        {
            _attacks[_currentAttackIdx].Elapsed = 0;
        }
        return defeated;
    }
    
     

    string Name { get; }
    public bool RangedAttack => (CurrentAttack is RangeAttack);

    public void TickCooldowns(float timeUnits)
    {
        foreach (var attack in _attacks)
        {
            if (attack.Elapsed >= 0)
            {
                attack.Elapsed += timeUnits;
            }
            if (attack.Elapsed >= attack.Data.Cooldown)
            {
                attack.Elapsed = -1;
            }
        }
    }
}
