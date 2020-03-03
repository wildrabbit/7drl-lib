using UnityEngine;
using System;
using System.Collections.Generic;

public class BaseGameEvents
{
    public class PlayerEvents
    {
        public event Action Died; // TODO: Action<PlayerDeathEvent>
        public event Action<Vector2Int, Vector3> Moved;

        public void SendPlayerDied()
        {
            Died?.Invoke(); // PlayerDied?.Invoke(new PlayerDeathEvent(....));
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
        public event Action<Monster> Destroyed;

        public void SendSpawned(Monster m, Vector2Int coords)
        {
            Spawned?.Invoke(m, coords);
        }

        public void SendDestroyed(Monster m)
        {
            Destroyed?.Invoke(m);
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

    public class FlowEvents
    {
        public event Action Started;
        public event Action<GameResult> GameOver;
        public event Action<int> StateChange;

        public void SendStarted()
        {
            Started?.Invoke();
        }

        public void SendGameOver(GameResult result)
        {
            GameOver?.Invoke(result);
        }

        public void SendStateChange(int nextState)
        {
            StateChange?.Invoke(nextState);
        }
    }

    public TimeEvents Time;
    public PlayerEvents Player;
    public MonsterEvents Monsters;
    public EntityEvents Entities;
    public FlowEvents Flow;

    public BaseGameEvents()
    {
        Player = new PlayerEvents();
        Time = new TimeEvents();
        Monsters = new MonsterEvents();
        Entities = new EntityEvents();
        Flow = new FlowEvents();
    }
}
