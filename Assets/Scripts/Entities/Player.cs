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


public class Player : BaseEntity, IBattleEntity, IHealthTrackingEntity
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

    int IBattleEntity.HP => HP;
    int IBattleEntity.Damage => 0;
    string IBattleEntity.Name => name;

    public HPTrait HPTrait => _hpTrait;
    public BaseMovingTrait MovingTrait => _movingTrait;

    BombImmunityType _bombImmunity;
    BombWalkabilityType _walkOverBombs;

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
    }

    public override void AddTime(float timeUnits, ref int playContext)
    {
        if (_hpTrait.Regen)
        {
            _hpTrait.UpdateRegen(timeUnits);
        }
    }



    public bool TakeDamage(int damage)
    {
        _hpTrait.Decrease(damage);
        Debug.Log($"Player took {damage} damage!. Current HP: {HP}");
        if (HP == 0)
        {
            _entityController.PlayerKilled();
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

    public override bool TryResolveMoveIntoCoords(Vector2Int testCoords)
    {
        var tile = _mapController.GetTileAt(testCoords);

        if(tile == null || !_movingTrait.EvaluateTile(tile))
        {
            return false;
        }

        List<BaseEntity> otherEntities = _entityController.GetEntitiesAt(testCoords);
        foreach(var other in otherEntities)
        {
            // Handle collision + entity actions!
        }

        Coords = testCoords;
        //_playerEvents.SendPlayerMoved(testCoords, transform.position);
        return true;
    }

    public override float DistanceFromPlayer()
    {
        return 0; // :D
    }
}
