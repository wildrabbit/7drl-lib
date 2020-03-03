using System;
using System.Collections.Generic;
using AI;
using UnityEngine;


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


    public bool IsMelee => _monsterData.IsMelee;
    
    public int TurnsInSameState => _turnsInSameState;
    public int TurnLimit => _turnLimit;
    public float EscapeHPRatio => _monsterData.EscapeHPRatio;
    public int EscapeSafeDistance => _monsterData.EscapeSafeDistance;
    public int VisibilityRange => _monsterData.VisibilityRange;
    public List<Vector2Int> Path => _path;
    public int CurrentPathIdx => _currentPathIdx;
    public float PathDelay => _monsterData.PathUpdateDelay;
    public float PathElapsed => _elapsedPathUpdate;

    int IBattleEntity.HP => HP;

    int IBattleEntity.Damage => _monsterData.MeleeDamage;

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

    public int AttackDistance()
    {
        if (_monsterData.IsMelee)
        {
            return 1;
        }
        return 0; // attack distance == 0? 
    }

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

        _currentState = _monsterData.InitialState;
        _currentStateTimeUnitsElapsed = 0.0f;
    }

    public void SetAIController(AIController aiController)
    {
        _aiController = aiController;
    }

    public override void AddTime(float timeUnits, ref int playContext)
    {
        _elapsedNextAction += timeUnits;

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
        _currentStateTimeUnitsElapsed = 0.0f;
    }


    public override void OnDestroyed()
    {
        _entityController.NotifyMonsterKilled(this);
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

    public int PlayerCollided(Player p)
    {
        if(_monsterData.PlayerCollisionDmg > 0)
        {
            TakeDamage(_monsterData.PlayerCollisionDmg);
        }
        return _monsterData.PlayerCollisionDmg;
    }

    // TODO: Do we use this during the AI step??
    public override bool TryResolveMoveIntoCoords(Vector2Int coords)
    {
        if (!_movingTrait.EvaluateTile(_mapController.GetTileAt(coords)))
        {
            return false;
        }

        List<BaseEntity> otherEntities = _entityController.GetEntitiesAt(coords);
        foreach (var other in otherEntities)
        {
            // Handle collision + entity actions!
        }

        return true;
    }

    public void WanderStep()
    {
        var neighbour = _mapController.RandomNeighbour(Coords, x => !IsTileValidMoveTarget(x));
        Coords = neighbour;  
        // TODO: Handle collisions?
    }

    public bool IsTileValidMoveTarget(Vector2Int coords)
    {
        bool matchesTile = _movingTrait.EvaluateTile(_mapController.GetTileAt(coords));
        if (!matchesTile) return false;

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
        PathUtils.FindPath(_mapController, Coords, _entityController.Player.Coords, ref _path);

        _currentPathIdx = 0;
        _elapsedPathUpdate = 0.0f;
    }

    public void TickPathElapsed(float units)
    {
        _elapsedPathUpdate += units;
        _currentPathIdx++;
    }

    public void FollowPath()
    {
        Coords = _path[_currentPathIdx];
    }
}