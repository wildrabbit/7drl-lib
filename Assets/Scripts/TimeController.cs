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
    BaseGameEvents.TimeEvents _timeEvents;    

    public float GameSpeed => _entityController.Player.Speed;

    public void Init(IEntityController entityController, BaseGameEvents.TimeEvents timeEvents, float defaultTimescale)
    {
        _timeScale = defaultTimescale;
        _scheduledEntities = new List<IScheduledEntity>();
        _timeEvents = timeEvents;

        _entityController = entityController;
    }

    public void Start()
    {
        _entityController.OnEntitiesAdded += RegisterScheduledEntities;
        _entityController.OnEntitiesRemoved += UnregisterScheduledEntities;

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

        _timeEvents.SendNewTurn(_turnCount);
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
        _entityController.OnEntitiesAdded -= RegisterScheduledEntities;
        _entityController.OnEntitiesRemoved -= UnregisterScheduledEntities;

        _scheduledEntities.Clear();
    }

    public void Dispose()
    {
        Cleanup();
    }
}
