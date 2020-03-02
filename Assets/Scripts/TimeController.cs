using System;
using System.Collections.Generic;
using UnityEngine;

public class TimeController: IDisposable
{
    public int Turns => _turnCount;
    public float TimeUnits => _elapsedUnits;

    int _turnCount;
    float _elapsedUnits;
    float _timeScale;

    List<IScheduledEntity> _scheduledEntities;

    IEntityController _entityController;
    BaseGameEvents _gameEvents;    

    public float GameSpeed => _entityController.Player.Speed;

    public void Init(IEntityController entityController, BaseGameEvents gameEvents, float defaultTimescale)
    {
        _timeScale = defaultTimescale;
        _scheduledEntities = new List<IScheduledEntity>();
        _gameEvents = gameEvents;

        _entityController = entityController;
    }

    public void Start()
    {
        _gameEvents.Entities.EntitiesAdded += RegisterScheduledEntities;
        _gameEvents.Entities.EntitiesRemoved += UnregisterScheduledEntities;

        _elapsedUnits = 0;
        _turnCount = 0;
    }

    public void Update(ref int playState)
    {
        float units = _timeScale * (1 / GameSpeed);
        foreach (var scheduled in _scheduledEntities)
        {
            scheduled.AddTime(units, ref playState);
        }
        _elapsedUnits += units;
        _turnCount++;

        _gameEvents.Time.SendNewTurn(_turnCount);
    }

    public void AddScheduledEntity(IScheduledEntity entity)
    {
        _scheduledEntities.Add(entity);
    }

    public void RemoveScheduledEntity(IScheduledEntity entity)
    {
        _scheduledEntities.Remove(entity);
    }

    private void UnregisterScheduledEntities(List<BaseEntity> entities)
    {
        foreach (var e in entities)
        {
            _scheduledEntities.Remove(e);
        }
    }

    private void RegisterScheduledEntities(List<BaseEntity> entities)
    {
        foreach (var e in entities)
        {
            _scheduledEntities.Add(e);
        }
    }

    public void Cleanup()
    {
        _gameEvents.Entities.EntitiesAdded -= RegisterScheduledEntities;
        _gameEvents.Entities.EntitiesRemoved -= UnregisterScheduledEntities;

        _scheduledEntities.Clear();
    }

    public void Dispose()
    {
        Cleanup();
    }
}
