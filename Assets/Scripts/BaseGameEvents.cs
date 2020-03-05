using UnityEngine;
using System;
using System.Collections.Generic;

public class BaseGameEvents
{
    public class PlayerEvents
    {
        public event Action Died; // TODO: Action<PlayerDeathEvent>
        public event Action<Vector2Int, Vector3> Moved;
        public event Action IdleTurn;

        public void SendPlayerDied()
        {
            Died?.Invoke(); // PlayerDied?.Invoke(new PlayerDeathEvent(....));
        }

        public void SendPlayerMoved(Vector2Int coords, Vector3 pos)
        {
            Moved?.Invoke(coords, pos);
        }
        
        public void SendIdleTurn()
        {
            IdleTurn?.Invoke();
        }
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

    public class HPEvents
    {
        public event Action<IHealthTrackingEntity, int , bool, bool, bool> HealthEvent;
        public event Action<IHealthTrackingEntity> HealthExhausted;
        public event Action<IHealthTrackingEntity> MaxHealthChanged;

        public void SendHealthEvent(IHealthTrackingEntity entity, int delta, bool healed, bool attacked, bool regen)
        {
            HealthEvent?.Invoke(entity, delta, healed, attacked, regen);
        }
        public void SendHealthExhausted(IHealthTrackingEntity entity)
        {
            HealthExhausted?.Invoke(entity);
        }

        public void SendMaxHPChanged(IHealthTrackingEntity owner)
        {
            MaxHealthChanged?.Invoke(owner);
        }
    }

    public class BattleEvents
    {
        public event Action<IBattleEntity, IBattleEntity, BattleActionResult> Attack;

        public void SendAttack(IBattleEntity attacker, IBattleEntity defender, BattleActionResult result)
        {
            Attack?.Invoke(attacker, defender, result);
        }
    }

    public class TrapEvents
    {
        public event Action<Trap, BaseEntity> EntityFellIntoTrap;

        public void SentEntityIntoTrap(Trap leTrap, BaseEntity leVictim)
        {
            EntityFellIntoTrap?.Invoke(leTrap, leVictim);
        }
    }

    public  class BlockingEvents
    {
        public event Action<IBlockingEntity, BaseEntity, bool> EntityBlockInteraction;

        public void SentEntityBlockInteraction(IBlockingEntity leBlock, BaseEntity leEntity, bool unlocked)
        {
            EntityBlockInteraction?.Invoke(leBlock, leEntity, unlocked);
        }
    }

    public TimeEvents Time;
    public PlayerEvents Player;
    public MonsterEvents Monsters;
    public EntityEvents Entities;
    public FlowEvents Flow;
    public HPEvents Health;
    public BattleEvents Battle;
    public TrapEvents Traps;
    public BlockingEvents Blocks;

    public BaseGameEvents()
    {
        Player = new PlayerEvents();
        Time = new TimeEvents();
        Monsters = new MonsterEvents();
        Entities = new EntityEvents();
        Flow = new FlowEvents();
        Health = new HPEvents();
        Battle = new BattleEvents();
        Traps = new TrapEvents();
        Blocks = new BlockingEvents();
    }
}
