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


public class Player : BaseEntity, IHealthTrackingEntity, IBattleEntity2
{
    public int HP => _hpTrait.HP;
    public int MaxHP => _hpTrait.MaxHP;
    public float Speed => _speed;

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
    BattleTrait _battleTrait;
    
    float _oldSpeed;
    float _speed;

    PlayerData _playerData;
    HPTrait _hpTrait;
    BaseMovingTrait _movingTrait;
    BaseGameEvents.PlayerEvents _playerEvents;


    protected override void DoInit(BaseEntityDependencies deps)
    {
        _playerData = ((PlayerData)_entityData);

        name = "Player";
        _hpTrait = new HPTrait();
        _hpTrait.Init(this, _playerData.HPData);

        _speed = _playerData.Speed;

        _movingTrait = _playerData.MovingTraitData.CreateRuntimeTrait();
        _movingTrait.Init(_playerData.MovingTraitData);

        _playerEvents = deps.GameEvents.Player;

        _battleTrait = new BattleTrait();
        _battleTrait.Init(_entityController, _playerData.BattleData, this);
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
        return _entityController.GetEntitiesAt(newPlayerCoords).Exists(x => x.IsHostileTo(this));
    }

    internal bool AttackCoords(Vector2Int newPlayerCoords)
    {
        var allDefeated = BattleTrait.TryAttackCoords(newPlayerCoords);
        return allDefeated;
    }

    public bool TakeDamage(int damage)
    {
        _hpTrait.Decrease(damage);
        Debug.Log($"Player took {damage} damage!. Current HP: {HP}");
        if (HP == 0)
        {
            _playerEvents.SendPlayerDied();
            _entityController.DestroyEntity(this);
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

    public bool CanAttackCoords(Vector2Int testCoords)
    {
        List<BaseEntity> otherEntities = _entityController.GetEntitiesAt(testCoords).FindAll(x => x.IsHostileTo(this));
        return otherEntities.Count > 0;
    }

    public override float DistanceFromPlayer()
    {
        return 0; // :D
    }

    public List<IBattleEntity2> FindHostileTargetsInMaxRange(int radius)
    {
        var nearby = _entityController.GetNearbyEntities(Coords, radius);
        List<IBattleEntity2> result = nearby.FindAll(x => x is Monster).ConvertAll(x => (IBattleEntity2)x);
        return result;
    }

    public bool TryFindAttack(BaseAttack attack, out MoveDirection direction, out List<IBattleEntity2> targets)
    {
        throw new NotImplementedException();
    }

    public override bool IsHostileTo(IBattleEntity2 other)
    {
        return other is Monster;
    }
}
