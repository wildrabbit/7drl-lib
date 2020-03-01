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

    public void Init(IEntityController entityController, float defaultTimescale)
    {
        _timeScale = defaultTimescale;
        _scheduledEntities = new List<IScheduledEntity>();

        _entityController = entityController;
    }

    public void Start()
    {
        _entityController.OnEntitiesAdded += RegisterScheduledEntities;
        _entityController.OnEntitiesRemoved += UnregisterScheduledEntities;

        _elapsedUnits = 0;
        _turnCount = 0;
    }

    public void Update(ref PlayContext playContext)
    {
        float units = _timeScale * (1 / _entityController.Player.Speed);
        foreach (var scheduled in _scheduledEntities)
        {
            scheduled.AddTime(units, ref playContext);
        }
        _elapsedUnits += units;
        _turnCount++;
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
