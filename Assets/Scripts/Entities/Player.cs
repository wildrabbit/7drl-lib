using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Tilemaps;

public enum BombWalkabilityType
{
    Block,
    CrossOwnBombs,
    CrossAny
}

public enum BombImmunityType
{
    AnyBombs,
    OwnBombs,
    NoBombs
}


public class Player : BaseEntity, IHealthTrackingEntity, IBattleEntity
{
    public int HP => _hpTrait.HP;
    public int MaxHP => _hpTrait.MaxHP;
    public float Speed => _speed;

    public List<string> _attributes = new List<string>();

    public override Vector2Int Coords
    {
        get
        {
            return base.Coords;
        }
        set
        {
            base.Coords = value;
            _playerEvents.SendPlayerMoved(value, transform.position);
        }
    }

    public bool CanMoveIntoMonsterCoords => _playerData.CanMoveIntoMonsterCoords;
    public int DmgFromMonsterCollision => _playerData.MonsterCollisionDmg;

    public HPTrait HPTrait => _hpTrait;
    public BaseMovingTrait MovingTrait => _movingTrait;

    public BattleTrait BattleTrait => _battleTrait;

    protected BattleTrait _battleTrait;

    protected float _oldSpeed;
    protected float _speed;

    PlayerData _playerData;
    protected HPTrait _hpTrait;
    protected BaseMovingTrait _movingTrait;
    protected BaseGameEvents.PlayerEvents _playerEvents;
    protected BaseGameEvents.HPEvents _healthEvents;


    protected override void DoInit(BaseEntityDependencies deps)
    {
        _playerData = ((PlayerData)_entityData);

        name = "Player";
        _hpTrait = new HPTrait();
        _hpTrait.Init(this, _playerData.HPData, deps.GameEvents.Health);

        _speed = _playerData.Speed;

        _movingTrait = _playerData.MovingTraitData.CreateRuntimeTrait();

        _playerEvents = deps.GameEvents.Player;
        _healthEvents = deps.GameEvents.Health;
        _healthEvents.HealthExhausted += OnDied;

        _battleTrait = new BattleTrait();
        _battleTrait.Init(_entityController, _mapController, _playerData.BattleData, this, deps.GameEvents.Battle);
    }

    public override void AddTime(float timeUnits, ref int playState)
    {
        if (_hpTrait.Regen)
        {
            _hpTrait.UpdateRegen(timeUnits);
        }
        _battleTrait.TickCooldowns(timeUnits);
    }

    internal bool ExistsHostilesAt(Vector2Int newPlayerCoords)
    {
        var battleEntities = _entityController.GetEntitiesAt(newPlayerCoords).FindAll(x => typeof(IBattleEntity).IsAssignableFrom(x.GetType())).ConvertAll(x => (IBattleEntity)x);

        return  battleEntities.Exists(x => x.IsHostileTo(this));
    }

    internal bool AttackCoords(Vector2Int newPlayerCoords)
    {
        var allDefeated = BattleTrait.TryAttackCoords(newPlayerCoords);
        return allDefeated;
    }

    public void OnDied(IHealthTrackingEntity e)
    {
        if(e == this)
        {
            _playerEvents.SendPlayerDied();
            _entityController.DestroyEntity(this);
        }
    }

    public bool TakeDamage(int damage)
    {
        _hpTrait.Decrease(damage);
        if (HP == 0)
        {            
            return true;
        }
        return false;
    }

    public override void Cleanup()
    {
        base.Cleanup();
    }

    public override void OnAdded()
    {
        base.OnAdded();
    }

    public override void OnDestroyed()
    {
        base.OnDestroyed();
        _healthEvents.HealthExhausted -= OnDied;
        _entityController.PlayerDestroyed();
    }

    public void ApplyBattleResults(BattleActionResult results, BattleRole role)
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

    public override void SetSpeedRate(float speedRate)
    {
        _oldSpeed = _speed;
        _speed *= (1 + speedRate / 100);
    }

    public override void ResetSpeedRate()
    {
        _speed = _oldSpeed;
    }

    public int MonsterCollided(Monster monster)
    {
        if (_playerData.MonsterCollisionDmg > 0)
        {
            TakeDamage(_playerData.MonsterCollisionDmg);
        }
        return _playerData.MonsterCollisionDmg;
    }

    public bool ValidMapCoords(Vector2Int testCoords)
    {
        var tile = _mapController.GetTileAt(testCoords);

        return (tile != null && _movingTrait.EvaluateTile(tile));        
    }

    public bool SeesHostilesAtCoords(Vector2Int testCoords)
    {
        var nearby = _entityController.GetEntitiesAt(testCoords);
        List<IBattleEntity> hostiles = CollectionUtils.GetImplementors<BaseEntity, IBattleEntity>(nearby).FindAll(x => x.IsHostileTo(this));

        return hostiles.Count > 0;
    }

    public override float DistanceFromPlayer()
    {
        return 0; // :D
    }

    public List<IBattleEntity> FindHostileTargetsInMaxRange(int radius)
    {
        var nearby = _entityController.GetNearbyEntities(Coords, radius);
        List<IBattleEntity> result = nearby.FindAll(x => x is Monster).ConvertAll(x => (IBattleEntity)x);
        return result;
    }

    public bool IsHostileTo(IBattleEntity other)
    {
        return other is Monster;
    }

    public override string[] Attributes
    {
        get
        {
            return _battleTrait.FirstAttack.Attributes;
        }
    }

}
