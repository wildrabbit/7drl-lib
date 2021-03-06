using System;
using System.Collections.Generic;
using UnityEngine;

public interface ITriggerEntity: IEntity
{
    TriggerTrait Trigger { get; }
    void OnEntityEntered(BaseEntity e);
    void OnEntityLeft(BaseEntity e);
    void OnEntityRemained(BaseEntity e);

    bool IsAffectedByTrigger(BaseEntity e);
}

public class TriggerTrait
{
    ITriggerEntity _owner;

    HashSet<BaseEntity> _entitiesAtTrigger;
    IEntityController _entityController;

    public void Init(ITriggerEntity owner, IEntityController entityController)
    {
        _owner = owner;
        _entitiesAtTrigger = new HashSet<BaseEntity>();
        _entityController = entityController;
    }

    public void CheckEntitiesExited()
    {
        var entitiesAt = _entityController.GetEntitiesAt(_owner.Coords);
        entitiesAt.RemoveAll(FilterEntityAtTrigger);

        var remove = new List<BaseEntity>();
        foreach (var toRemove in _entitiesAtTrigger)
        {
            if (!entitiesAt.Contains(toRemove))
            {
                remove.Add(toRemove);
                _owner.OnEntityLeft(toRemove);
            }
        }
        foreach (var purged in remove)
        {
            _entitiesAtTrigger.Remove(purged);
        }

    }

    public void CheckEntitiesStaying()
    {
        var entitiesAt = _entityController.GetEntitiesAt(_owner.Coords);
        entitiesAt.RemoveAll(FilterEntityAtTrigger);

        foreach (var e in entitiesAt)
        {
            if(_entitiesAtTrigger.Contains(e))
            {
               _owner.OnEntityRemained(e);
            }
        }
    }

    public void CheckEntitiesEntered()
    {
        var entitiesAt = _entityController.GetEntitiesAt(_owner.Coords);
        entitiesAt.RemoveAll(FilterEntityAtTrigger);

        foreach (var e in entitiesAt)
        {
            if (!_entitiesAtTrigger.Contains(e))
            {
                _entitiesAtTrigger.Add(e);
                _owner.OnEntityEntered(e);
            }
        }
    }

    bool FilterEntityAtTrigger(BaseEntity e)
    {
        bool isOwner = typeof(ITriggerEntity).IsAssignableFrom(e.GetType()) && ((ITriggerEntity)e) == _owner;
        return isOwner || !_owner.IsAffectedByTrigger(e);
    }
}
