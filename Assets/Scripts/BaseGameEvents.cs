using UnityEngine;
using System;
using System.Collections.Generic;

public class BaseGameEvents
{
    public class PlayerEvents
    {
        public event Action PlayerDied; // TODO: Action<PlayerDeathEvent>
        public event Action<Vector2Int, Vector3> Moved;

        public void SendPlayerDied()
        {
            PlayerDied?.Invoke(); // PlayerDied?.Invoke(new PlayerDeathEvent(....));
        }

        public void SendPlayerMoved(Vector2Int coords, Vector3 pos)
        {
            Moved?.Invoke(coords, pos);
        }
        //.....
    }

    public class TimeEvents
    {
        public event Action<int> NewTurn;

        public void SendNewTurn(int newTurn)
        {
            NewTurn?.Invoke(newTurn);
        }
    }

    public class MonsterEvents
    {
        public event Action<Monster, Vector2Int> Spawned;

        public void SendMonsterSpawned(Monster m, Vector2Int coords)
        {
            Spawned?.Invoke(m, coords);
        }
    }

    public class EntityEvents
    {
        public event Action<List<BaseEntity>> EntitiesAdded;
        public event Action<List<BaseEntity>> EntitiesRemoved;

        public void SendEntitiesAdded(List<BaseEntity> newEntities)
        {
            EntitiesAdded?.Invoke(newEntities);
        }

        public void SendEntitiesRemoved(List<BaseEntity> removedEntities)
        {
            EntitiesRemoved?.Invoke(removedEntities);
        }
    }

    public TimeEvents Time;
    public PlayerEvents Player;
    public MonsterEvents Monsters;
    public EntityEvents Entities;

    public BaseGameEvents()
    {
        Player = new PlayerEvents();
        Time = new TimeEvents();
        Monsters = new MonsterEvents();
        Entities = new EntityEvents();
    }
}
