﻿using System;
using System.Collections.Generic;
using UnityEngine;


public delegate void EntitiesAddedDelegate(List<BaseEntity> entities);
public delegate void EntitiesRemovedDelegate(List<BaseEntity> entities);

public delegate void PlayerDestroyedDelegate();
public delegate void MonsterDestroyedDelegate(Monster monster);

public delegate void PlayerMonsterCollision(Player p, Monster m, int playerDmg, int monsterDmg);
public delegate void EntityHealthDelegate(BaseEntity e, int dmg, bool explosion, bool poison, bool heal, bool collision);

public interface IEntityController
{
    event PlayerDestroyedDelegate OnPlayerKilled;
    event MonsterDestroyedDelegate OnMonsterKilled;

    event PlayerMonsterCollision OnPlayerMonsterCollision;

    event EntityHealthDelegate OnEntityHealth;

    Player Player { get; }
    List<BaseEntity> AllEntities { get; }

    void Init(IMapController mapController, BaseEntityCreationData creationData, BaseGameEvents gameEvents);
    void StartGame();

    Player CreatePlayer(PlayerData data, Vector2Int coords);
    Monster CreateMonster(MonsterData data, Vector2Int coords);
    Trap CreateTrap(TrapData data, Vector2Int coords);
    BlockingEntity CreateBlockingEntity(BlockingData data, Vector2Int coords);

    List<Monster> CreateMonsters(List<(MonsterData, Vector2Int)> list);
    T Create<T>(BaseEntity prefab, BaseEntityData data, BaseEntityDependencies deps) where T : BaseEntity;

    List<T> GetFilteredEntitiesAt<T>(Vector2Int coords);

    bool ExistsNearbyEntity(Vector2Int coords, int radius, BaseEntity[] excluded = null);
    bool ExistsEntitiesAt(Vector2Int coords, BaseEntity[] excluded = null); // We could use the former with radius == 0, but with this we skip the distance calculations
    List<BaseEntity> GetEntitiesAt(Vector2Int actionTargetCoords, BaseEntity[] excluded = null);
    List<BaseEntity> GetNearbyEntities(Vector2Int refCoords, int radius, BaseEntity[] excluded = null);
    void AddNewEntities();
    void DestroyEntity(BaseEntity entity);
    void PurgeEntities();
    void RemovePendingEntities();
    void Cleanup();

    void PlayerDestroyed();
    

    void CollisionMonsterPlayer(Player p, Monster m, int playerDmg, int monsterDmg);

    void EntityHealthEvent(BaseEntity entity, int healthDelta, bool isExplosion, bool isHeal, bool isPoison, bool isCollision);

    IEnumerable<Monster> Monsters { get;  }
}
