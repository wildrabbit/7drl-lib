using System;
using System.Collections.Generic;
using UnityEngine;

public enum BattleRole
{
    Attacker,
    Defender
}

public interface IBattleEntity2
{
    string Name { get; }
    HPTrait HPTrait { get; }
    BattleTrait BattleTrait { get;  }
    Vector2Int Coords { get; }

    bool IsHostileTo(IBattleEntity2 other);         

    void ApplyBattleResults(BattleActionResult result, BattleRole role);
}

public class BattleTrait
{
    BaseAttack CurrentAttack => _attacks[_currentAttackIdx];
    IBattleEntity2 Owner => _owner;
    IBattleEntity2 _owner;

    BattleTraitData _data;
    
    List<BaseAttack> _attacks;
    int _currentAttackIdx;

    List<IBattleEntity2> _targets;
    MoveDirection _attackDirection;

    IEntityController _entityController;

    public void Init(IEntityController entityController,  BattleTraitData data, IBattleEntity2 owner)
    {
        _data = data;
        _entityController = entityController;
        _owner = owner;
        _attacks = new List<BaseAttack>();
        _currentAttackIdx = 0;
        foreach(var attackData in data.Attacks)
        {
            var attack = attackData.SpawnRuntime();
            attack.Init();
            _attacks.Add(attack);
        }
    }

    public bool TryGetAvailableAttack()
    {
        int bestIdx = -1;
        int bestTotalDamage = 0;
        MoveDirection moveDir = MoveDirection.None;
        List<IBattleEntity2> bestTargets = new List<IBattleEntity2>();

        MoveDirection[] rotations = new MoveDirection[]
        {
            MoveDirection.N, MoveDirection.E, MoveDirection.S, MoveDirection.W
        };
       
        for (int i = 0; i < _attacks.Count; ++i)
        {
            if (!_attacks[i].Ready) continue;

            foreach (var rot in rotations)
            {
                var targets = FindAttackTargets(_attacks[i], rot);
                if (targets.Count > 0)
                {
                    int totalDamage = targets.Count * _attacks[i].Data.AddedDamage;
                    if (totalDamage >= bestTotalDamage)
                    {
                        bestIdx = i;
                        bestTotalDamage = _attacks[i].Data.AddedDamage;
                        moveDir = rot;
                        bestTargets = targets;
                    }
                }
            }
        }

        if (bestIdx >= 0)
        {
            _currentAttackIdx = bestIdx;
            PrepareAttack(bestTargets, moveDir);
            return true;
        }

        _attackDirection = MoveDirection.None;
        _targets?.Clear();
        return false;
    }

    public bool TryAttackCoords(Vector2Int newPlayerCoords)
    {
        MoveDirection direction = ResolveDirectionFromCoords(_owner.Coords, newPlayerCoords);
        var targets = FindAttackTargets(_attacks[_currentAttackIdx], direction);
        var targetsAtCoords = targets.FindAll(x => x.Coords == newPlayerCoords);
        PrepareAttack(targets, direction);
        var defeated = StartAttack();
        return defeated.IsSupersetOf(targetsAtCoords);
    }

    public bool CanAttackEntity(IBattleEntity2 other)
    {
        MoveDirection direction = ResolveDirectionFromCoords(_owner.Coords, other.Coords);
        return ExistsTargets(_attacks[_currentAttackIdx], direction);
    }

    public bool TryAttackEntity(IBattleEntity2 other)
    {
        MoveDirection direction = ResolveDirectionFromCoords(_owner.Coords, other.Coords);
        var targets = FindAttackTargets(_attacks[_currentAttackIdx], direction);
        PrepareAttack(targets, direction);
        var defeated = StartAttack();
        return defeated.Contains(other);
    }

    public void PrepareAttack(List<IBattleEntity2> targets, MoveDirection attackDirection)
    {
        _targets = targets;
        _attackDirection = attackDirection;
    }

    public List<IBattleEntity2> FindAttackTargets(BaseAttack attack, MoveDirection direction)
    {
        List<IBattleEntity2> results = new List<IBattleEntity2>();
        var offsets = attack.GetRotatedOffsets(direction);
        foreach(var offset in offsets)
        {
            var entities = _entityController.GetEntitiesAt(offset + _owner.Coords);
            var filteredHostiles = entities.FindAll(x => typeof(IBattleEntity2).IsAssignableFrom(x.GetType())).ConvertAll(x =>(IBattleEntity2)x);
            filteredHostiles.RemoveAll(x => !x.IsHostileTo(_owner));
            results.AddRange(filteredHostiles);
        }
        return results;
    }

    public bool ExistsTargets(BaseAttack attack, MoveDirection direction)
    {
        var offsets = attack.GetRotatedOffsets(direction);
        foreach (var offset in offsets)
        {
            var entities = _entityController.GetEntitiesAt(offset + _owner.Coords);
            var filteredHostiles = entities.FindAll(x => typeof(IBattleEntity2).IsAssignableFrom(x.GetType())).ConvertAll(x => (IBattleEntity2)x);
            filteredHostiles.RemoveAll(x => !x.IsHostileTo(_owner));
            if(filteredHostiles.Count > 0)
            {
                return true;
            }
        }
        return false;
    }

    public MoveDirection ResolveDirectionFromCoords(Vector2Int src, Vector2Int tgt)
    {
        Vector2 offset = tgt - src;
        if (offset.Equals(Vector2Int.zero))
        {
            return MoveDirection.None;
        }

        if (Mathf.Abs(offset.x) >= Mathf.Abs(offset.y))
        {
            return offset.x >= 0 ? MoveDirection.N : MoveDirection.S;
        }
        else return offset.y >= 0 ? MoveDirection.E : MoveDirection.W;

    }

    public HashSet<IBattleEntity2> StartAttack()
    {
        // Use direction for the view
        HashSet<IBattleEntity2> defeated = new HashSet<IBattleEntity2>();
        foreach(var target in _targets)
        {
            BattleUtils.SolveAttack(_owner, target, out var result);
            if(result.DefenderDefeated)
            {
                defeated.Add(target);
            }
        }
        _attacks[_currentAttackIdx].Elapsed = 0;
        return defeated;
    }
    
     public int Damage => _attacks[_currentAttackIdx].Data.AddedDamage /* +CharacterDamage */;


    string Name { get; }

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
