using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum MonsterState
{
    Idle = 0,
    Wandering,
    Chasing,
    Escaping,
    BattleAction,
    BombPlacement
}


public class Monster : BaseEntity,  IHealthTrackingEntity, IBattleEntity
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

    string IBattleEntity.Name => name;

    public HPTrait HPTrait => _hpTrait;
    public BaseMovingTrait MovingTrait => _movingTrait;
    public BattleTrait BattleTrait => _battleTrait;

    BattleTrait _battleTrait;

    public bool ValidPath
    {
        get
        {
            return _path != null && _path.Count > 0 && _currentPathIdx >= 0 && _currentPathIdx < (_path.Count - 1) && _elapsedPathUpdate < PathDelay;
        }
    }

    public bool UselessPath => _path != null && _path.Count < 2;

    public Vector3[] PathWorld => _path?.ConvertAll(x => _mapController.WorldFromCoords(x)).ToArray();

    public event System.Action PathChanged;

    protected MonsterData _monsterData; // Should we expose it?

    protected HPTrait _hpTrait;
    protected BaseMovingTrait _movingTrait;

    float _elapsedNextAction;

    float _decisionDelay;

    float _elapsedPathUpdate;
    int _turnLimit;
    int _turnsInSameState;

    protected List<Vector2Int> _path;
    protected int _currentPathIdx;

    protected BaseGameEvents.MonsterEvents _monsterEvents;
    
    protected override void DoInit(BaseEntityDependencies deps)
    {
        _monsterData = ((MonsterData)_entityData);
        name = _monsterData.name;
        _hpTrait = new HPTrait();
        _hpTrait.Init(this, _monsterData.HPData, deps.GameEvents.Health);

        _decisionDelay = _monsterData.ThinkingDelay;
        _elapsedNextAction = 0.0f;
        _elapsedPathUpdate = 0.0f;

        _movingTrait = _monsterData.MovingTraitData.CreateRuntimeTrait();
        
        _monsterEvents = deps.GameEvents.Monsters;

        _currentState = _monsterData.InitialState;
        _currentStateTimeUnitsElapsed = 0.0f;

        _battleTrait = new BattleTrait();
        _battleTrait.Init(_entityController, _monsterData.BattleData, this, deps.GameEvents.Battle);
    }

    public override void AddTime(float timeUnits, ref int playContext)
    {
        _elapsedNextAction += timeUnits;
        _battleTrait.TickCooldowns(timeUnits);

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

    public void WanderStep()
    {
        var neighbour = _mapController.RandomNeighbour(Coords, x => !IsTileValidMoveTarget(x));
        if(neighbour != Coords)
        {
            Coords = neighbour;
        }
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
        if(ValidNavigationCoords(_path[_currentPathIdx]))
        {
            var entitiesAt = _entityController.GetEntitiesAt(_path[_currentPathIdx], new BaseEntity[] { this });
            if (entitiesAt.Count == 0 || !entitiesAt.Exists(x => x.IsHostileTo(this)))
            {
                Coords = _path[_currentPathIdx];
                _currentPathIdx++;
            }
            else
            {
                Debug.Log("Player in front. Wait");
            }
        }
    }

    public void LaunchAttack()
    {
        _battleTrait.StartAttack();
    }

    public bool DebugPaths = false;
    public Material DebugMaterial;
    
    public void ToggleDebug()
    {
        DebugPaths = !DebugPaths;
    }


    public List<IBattleEntity> FindHostileTargetsInMaxRange(int radius)
    {
        List<IBattleEntity> result = new List<IBattleEntity>();
        if(_mapController.Distance(_entityController.Player.Coords, Coords) <= radius)
        {
            result.Add(_entityController.Player); // easy :D
        }
        return result;
    }

    public bool TryFindAttack(BaseAttack attack, out MoveDirection direction, out List<IBattleEntity> targets)
    {
        throw new NotImplementedException();
    }

    public void ApplyBattleResults(BattleActionResult result, BattleRole role)
    {
        throw new NotImplementedException();
    }

    public override bool IsHostileTo(IBattleEntity other)
    {
        return other is Player;
    }
}