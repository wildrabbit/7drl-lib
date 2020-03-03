using System;
using System.Collections.Generic;
using AI;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;

public class BaseMonsterAction
{
    public Vector2Int NextCoords;
    
}

public class ChaseMonsterAction: BaseMonsterAction
{
    public BaseEntity Target;

    public bool RefreshPath;
    public List<Vector2Int> Path;
    public float PathElapsed;
    public int PathIdx;
}

public class MeleeAttackAction: BaseMonsterAction
{
    public IBattleEntity Target;
    // TODO: Melee weapon, stuff
}

public class MonsterDependencies: BaseEntityDependencies
{
    public AIController AIController;
}

public enum MonsterState
{
    Idle = 0,
    Wandering,
    Chasing,
    Escaping,
    BattleAction,
    BombPlacement
}


public class Monster : BaseEntity, IBattleEntity, IHealthTrackingEntity
{

    public SpriteRenderer ViewPrefab;

    public int HP => _hpTrait.HP;
    public int MaxHP => _hpTrait.MaxHP;

    AI.State _currentState;
    AI.State _noState => _monsterData.NoState;
    float _currentStateTimeUnitsElapsed;

    public List<Vector2Int> Path => _path;
    public int CurrentPathIdx => _currentPathIdx;
    public float PathDelay => _monsterData.PathUpdateDelay;
    public float PathElapsed => _elapsedPathUpdate;

    public List<BaseAttack> _attacks;
    public int _currentAttackIdx;

    int IBattleEntity.HP => HP;
    string IBattleEntity.Name => name;

    public HPTrait HPTrait => _hpTrait;
    public BaseMovingTrait MovingTrait => _movingTrait;

    public bool ValidPath
    {
        get
        {
            return _path != null && _path.Count > 0 && _currentPathIdx >= 0 && _currentPathIdx < (_path.Count - 1) && _elapsedPathUpdate < PathDelay;
        }
    }

    public bool UselessPath => _path != null && _path.Count < 2;

    public int Damage => _attacks[_currentAttackIdx].Data.AddedDamage /* +CharacterDamage */;

    public Vector3[] PathWorld => _path?.ConvertAll(x => _mapController.WorldFromCoords(x)).ToArray();
    public event System.Action PathChanged;

    MonsterData _monsterData; // Should we expose it?

    HPTrait _hpTrait;
    BaseMovingTrait _movingTrait;

    float _elapsedNextAction;

    float _decisionDelay;

    float _elapsedPathUpdate;
    int _turnLimit;
    int _turnsInSameState;

    List<Vector2Int> _path;
    int _currentPathIdx;

    BaseGameEvents.MonsterEvents _monsterEvents;

    AIController _aiController;
    
    protected override void DoInit(BaseEntityDependencies deps)
    {
        MonsterDependencies monsterDeps = ((MonsterDependencies)deps);
        _aiController = monsterDeps.AIController;

        _monsterData = ((MonsterData)_entityData);
        name = _monsterData.name;
        _hpTrait = new HPTrait();
        _hpTrait.Init(this, _monsterData.HPData);

        _decisionDelay = _monsterData.ThinkingDelay;
        _elapsedNextAction = 0.0f;
        _elapsedPathUpdate = 0.0f;

        _movingTrait = _monsterData.MovingTraitData.CreateRuntimeTrait();
        _movingTrait.Init(_monsterData.MovingTraitData);

        _monsterEvents = deps.GameEvents.Monsters;

        _currentState = _monsterData.InitialState;
        _currentStateTimeUnitsElapsed = 0.0f;

        _attacks = new List<BaseAttack>();
        foreach(var attackData in _monsterData.Attacks)
        {
            _attacks.Add(attackData.SpawnRuntime());
        }
    }

    public void SetAIController(AIController aiController)
    {
        _aiController = aiController;
    }

    public override void AddTime(float timeUnits, ref int playContext)
    {
        _elapsedNextAction += timeUnits;
        foreach(var attack in _attacks)
        {
            if(attack.Elapsed >= 0)
            {
                attack.Elapsed += timeUnits;
            }
            if (attack.Elapsed >= attack.Data.Cooldown)
            {
                attack.Elapsed = -1;
            }
        }

        while (_elapsedNextAction >= _decisionDelay)
        {
            _currentStateTimeUnitsElapsed += _decisionDelay;
            _currentState.Process(this, _decisionDelay);
            _elapsedNextAction = Mathf.Max(_elapsedNextAction - _decisionDelay, 0.0f);
        }
        
    }

    public void TransitionToNextState(AI.State next)
    {
        if (next != _noState)
        {
            Debug.Log($"<color=purple>FSM:</color>Monster {_monsterData.DisplayName} at {Coords} changes State from {_currentState.name} to {next.name}");
            _currentState = next;
            OnExitState();
        }
    }

    public bool CheckTurnTimeElapsed(float duration)
    {
        return _currentStateTimeUnitsElapsed >= duration; 
    }

    public void OnExitState()
    {
        _path?.Clear();
        PathChanged?.Invoke();
        _currentPathIdx = 0;
        _elapsedPathUpdate = 0.0f;

        _currentStateTimeUnitsElapsed = 0.0f;
    }
    public override void OnCreated()
    {
        _monsterEvents.SendSpawned(this, Coords);
    }

    public override void OnDestroyed()
    {
        _monsterEvents.SendDestroyed(this);
    }

    public override void Cleanup()
    {
        base.Cleanup();
    }

    void IBattleEntity.ApplyBattleResults(BattleActionResult results, BattleRole role)
    {
        if(role == BattleRole.Defender)
        {
            TakeDamage(results.DefenderDmgTaken);
        }
        else
        {
            Debug.Log($"{name} attacked {results.DefenderName} and caused {results.AttackerDmgInflicted} dmg");
        }
    }

    public bool TakeDamage(int amount)
    {
        _hpTrait.Decrease(amount);
        Debug.Log($"{name} took {amount} damage!. Current HP: {HP}");
        if (HP == 0)
        {
            _entityController.DestroyEntity(this);
            return true;
        }
        return false;
    }

    public override void SetSpeedRate(float speedRate)
    {
        _decisionDelay *= (1 - speedRate/100);
        Debug.Log($"Monster speed rate changed by {speedRate}% to a value of {_decisionDelay}");
    }

    public override void ResetSpeedRate()
    {
        _decisionDelay = _monsterData.ThinkingDelay;
        Debug.Log($"Monster speed rate restored to {_decisionDelay}");
    }

    // TODO: Do we use this during the AI step??
    public override bool TryResolveMoveIntoCoords(Vector2Int coords)
    {
        if (!ValidNavigationCoords(coords))
        {
            return false;
        }

        List<BaseEntity> otherEntities = _entityController.GetEntitiesAt(coords);
        foreach (var other in otherEntities)
        {
            if (other is Player p)
            {
                return false;
            }
        }

        Coords = coords;
        return true;
    }

    public void WanderStep()
    {
        var neighbour = _mapController.RandomNeighbour(Coords, x => !IsTileValidMoveTarget(x));
        TryResolveMoveIntoCoords(neighbour);
        // TODO: Handle collisions?
    }

    public bool ValidNavigationCoords(Vector2Int coords)
    {
        TileBase tile = _mapController.GetTileAt(coords);
        return _movingTrait.EvaluateTile(tile);
    }

    public bool IsTileValidMoveTarget(Vector2Int coords)
    {
        if (!ValidNavigationCoords(coords)) return false;

        bool occupied = _entityController.ExistsEntitiesAt(coords);
        return !occupied || !(_entityController.GetEntitiesAt(coords, new BaseEntity[] { this }).Contains(_entityController.Player));
    }

    public override float DistanceFromPlayer()
    {
        return _mapController.Distance(Coords, _entityController.Player.Coords);
    }

    public void RecalculatePath()
    {
        if(_path == null)
        {
            _path = new List<Vector2Int>();
        }
        PathUtils.FindPath(_mapController, Coords, _entityController.Player.Coords, ref _path, ValidNavigationCoords);
        PathChanged?.Invoke();
        _currentPathIdx = 0;
        _elapsedPathUpdate = 0.0f;
    }

    public void TickPathElapsed(float units)
    {
        _elapsedPathUpdate += units;
    }

    public void FollowPath()
    {
        if(TryResolveMoveIntoCoords(_path[_currentPathIdx]))
        {
            _currentPathIdx++;
        }
    }

    public bool TrySelectAvailableAttack()
    {
        int bestIdx = -1;
        int bestDmg = 0;
        for(int i = 0; i < _attacks.Count; ++i)
        {
            if (!_attacks[i].Ready) continue;
            if(_attacks[i].CanTargetBeReached(Coords, _entityController.Player.Coords, out var dir))
            {
                if(_attacks[i].Data.AddedDamage >= bestDmg)
                {
                    bestIdx = i;
                    bestDmg = _attacks[i].Data.AddedDamage;
                }
            }
        }

        if(bestIdx >= 0)
        {
            _currentAttackIdx = bestIdx;
            return true;
        }

        return false;
    }

    public void LaunchAttack()
    {
        BattleUtils.SolveAttack(this, _entityController.Player, out var result);
        _attacks[_currentAttackIdx].Elapsed = 0;
    }

    public bool DebugPaths = false;
    public Material DebugMaterial;
    
    public void ToggleDebug()
    {
        DebugPaths = !DebugPaths;
    }

}