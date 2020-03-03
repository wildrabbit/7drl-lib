using System;
using System.Collections.Generic;
using UnityEngine;

public class EntityController : IEntityController
{
    protected BaseEntityCreationData _entityCreationData;

    Player _player;
    List<BaseEntity> _allEntities;

    List<BaseEntity> _entitiesToRemove;
    List<BaseEntity> _entitiesToAdd;

    public Player Player => _player;
    public IEnumerable<Monster> Monsters => _allEntities.FindAll(x => x is Monster).ConvertAll(baseE => (Monster)baseE);

    protected IMapController _mapController;
    protected BaseGameEvents _gameEvents;
    
    public event PlayerDestroyedDelegate OnPlayerKilled;
    public event MonsterDestroyedDelegate OnMonsterKilled;
    public event PlayerMonsterCollision OnPlayerMonsterCollision;
    public event EntityHealthDelegate OnEntityHealth;

    public void Init(IMapController mapController, BaseEntityCreationData entityCreationData, BaseGameEvents gameEvents)
    {
        _mapController = mapController;
        _entityCreationData = entityCreationData;
        _allEntities = new List<BaseEntity>();
        _entitiesToAdd = new List<BaseEntity>();
        _entitiesToRemove = new List<BaseEntity>();

        _gameEvents = gameEvents;

    }

    public void StartGame()
    {
        CreatePlayer(_entityCreationData.PlayerData, _mapController.PlayerStart);
    }

    public virtual Player CreatePlayer(PlayerData data, Vector2Int coords)
    {
        BaseEntityDependencies deps = new BaseEntityDependencies()
        {
            ParentNode = null,
            EntityController = this,
            MapController = _mapController,
            GameEvents = _gameEvents,
            Coords = coords
        };
        _player = Create<Player>(_entityCreationData.PlayerPrefab, data, deps);
        return _player;
    }

    public Monster CreateMonster(MonsterData data, Vector2Int coords)
    {
        BaseEntityDependencies deps = new BaseEntityDependencies()
        {
            ParentNode = null,
            EntityController = this,
            Coords = coords,
            MapController = _mapController
        };
        var monster = Create<Monster>(_entityCreationData.MonsterPrefab, data, deps);
        return monster;
    }

    public List<Monster> CreateMonsters(List<(MonsterData, Vector2Int)> list)
    {
        List<Monster> addedMonsters = new List<Monster>();
        foreach(var (data, coords) in list)
        {
            BaseEntityDependencies deps = new BaseEntityDependencies()
            {
                ParentNode = null,
                EntityController = this,
                Coords = coords,
                MapController = _mapController,
                GameEvents = _gameEvents
            };
            var monster = Create<Monster>(_entityCreationData.MonsterPrefab, data, deps);
            addedMonsters.Add(monster);
        }
        return addedMonsters;
    }

    public T Create<T>(T prefab, BaseEntityData data, BaseEntityDependencies deps) where T : BaseEntity
    {
        T entity = GameObject.Instantiate<T>(prefab);
        entity.Init(data, deps);
        _entitiesToAdd.Add(entity);
        entity.OnCreated();
        return entity;
    }

    public bool ExistsNearbyEntity(Vector2Int coords, int radius, BaseEntity[] excluded = null)
    {
        var filteredEntities = excluded != null ? GetFilteredEntities(excluded) : _allEntities;
        foreach (var e in filteredEntities)
        {
            if (_mapController.Distance(e.Coords, coords) <= radius)
            {
                return true;
            }
        }
        return false;
    }

    public List<BaseEntity> GetNearbyEntities(Vector2Int refCoords, int radius, BaseEntity[] excluded = null)
    {
        var filteredEntities = excluded != null ? GetFilteredEntities(excluded) : new List<BaseEntity>(_allEntities);
        for(int i = filteredEntities.Count - 1; i>=0; --i)
        {
            if (_mapController.Distance(filteredEntities[i].Coords, refCoords) > radius)
            {
                filteredEntities.RemoveAt(i);
            }
        }
        return filteredEntities;
    }

    public List<BaseEntity> GetEntitiesAt(Vector2Int actionTargetCoords, BaseEntity[] excluded = null)
    {
        List<BaseEntity> resultEntities = new List<BaseEntity>();
        List<BaseEntity> candidates = excluded != null ? GetFilteredEntities(excluded) : _allEntities;

        foreach (BaseEntity entity in candidates)
        {
            if (entity.Coords == actionTargetCoords)
            {
                resultEntities.Add(entity);
            }
        }

        return resultEntities;
    }

    public List<BaseEntity> GetFilteredEntities(BaseEntity[] excluded)
    {
        List<BaseEntity> filtered = new List<BaseEntity>(_allEntities);
        foreach (var excludedEntity in excluded)
        {
            filtered.Remove(excludedEntity);
        }
        return filtered;
    }

    public void DestroyEntity(BaseEntity entity)
    {
        entity.Active = false;
        _entitiesToRemove.Add(entity);
    }

    public void AddNewEntities()
    {
        foreach (var e in _entitiesToAdd)
        {
            e.OnAdded();
            _allEntities.Add(e);
            // TODO: Check specific lists
        }
        _gameEvents.Entities.SendEntitiesAdded(_entitiesToAdd);
        _entitiesToAdd.Clear();
    }

    public void PurgeEntities()
    {
        foreach (var e in _entitiesToAdd)
        {
            e.OnDestroyed();
            GameObject.Destroy(e.gameObject);
        }
        foreach (var e in _allEntities)
        {
            e.OnDestroyed();
            GameObject.Destroy(e.gameObject);
            // Check specific lists
        }
        _entitiesToAdd.Clear();
        _entitiesToRemove.Clear();
        _allEntities.Clear();
    }

    public void RemovePendingEntities()
    {
        if (_entitiesToRemove.Count == 0)
        {
            return;
        }

        _gameEvents.Entities.SendEntitiesRemoved(_entitiesToRemove);

        foreach (var e in _entitiesToRemove)
        {
            e.OnDestroyed();
            GameObject.Destroy(e.gameObject);
            _allEntities.Remove(e);
            // Check specific lists
        }
        _entitiesToRemove.Clear();
    }

    public void PlayerDestroyed()
    {
        _player = null;
    }

    public void Cleanup()
    {
        PurgeEntities();
    }

    public bool ExistsEntitiesAt(Vector2Int coords, BaseEntity[] excluded = null)
    {
        var filteredEntities = excluded != null ? GetFilteredEntities(excluded) : _allEntities;
        foreach (var e in filteredEntities)
        {
            if (e.Coords == coords)
            {
                return true;
            }
        }
        return false;
    }



    public void CollisionMonsterPlayer(Player p, Monster m, int playerDmg, int monsterDmg)
    {
        OnPlayerMonsterCollision(p, m, playerDmg, monsterDmg);
    }

    public void PlayerDamaged(int dmg, bool isExplosion, bool isPoison, bool isCollision)
    {
        EntityHealthEvent(_player, dmg, isExplosion, false, isPoison, isCollision);
    }

    public void PlayerHealed(int restoredhp)
    {
        EntityHealthEvent(_player, restoredhp, false, true, false, false);
    }

    public void MonsterDamaged(Monster m, int dmg, bool isExplosion, bool isPoison, bool isCollision)
    {
        EntityHealthEvent(m, dmg, isExplosion, false, isPoison, isCollision);
    }

    public void MonsterHealed(Monster m, int restoredhp)
    {
        EntityHealthEvent(m, restoredhp, false, true, false, false);
    }

    public void EntityHealthEvent(BaseEntity entity, int healthDelta, bool isExplosion, bool isHeal, bool isPoison, bool isCollision)
    {
        OnEntityHealth?.Invoke(entity, healthDelta, isExplosion, isPoison, isHeal, isCollision);
    }

}
