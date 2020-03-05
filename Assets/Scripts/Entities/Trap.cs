using System;
using System.Collections.Generic;
using UnityEngine;

public class Trap : BaseEntity, ITriggerEntity
{
    public override bool BlocksRanged(bool piercing) => false;
    public TriggerTrait Trigger => _triggerTrait;
    TriggerTrait _triggerTrait;
    TrapData _trapData;
    public TrapData Data => _trapData;

    Dictionary<BaseEntity, float> _entitiesAtTrap;
    BaseGameEvents.EntityEvents _entityEvents;

    protected override void DoInit(BaseEntityDependencies dependencies)
    {
        _trapData = (TrapData)_entityData;
        _triggerTrait = new TriggerTrait();
        _triggerTrait.Init(this, _entityController);
        _entitiesAtTrap = new Dictionary<BaseEntity, float>();
        _entityEvents = dependencies.GameEvents.Entities;
        _entityEvents.EntitiesRemoved += EntitiesRemoved;
    }

    private void EntitiesRemoved(List<BaseEntity> removedEntities)
    {
        foreach(var entity in removedEntities)
        {
            _entitiesAtTrap.Remove(entity);
        }
    }

    public override void OnDestroyed()
    {
        _entityEvents.EntitiesRemoved -= EntitiesRemoved;
    }


    // TODO: Don't force BaseEntity to be a scheduled entity :/
    public override void AddTime(float timeUnits, ref int playContext)
    {
        _triggerTrait.CheckEntitiesExited();
        float dmgRate = _trapData.Rate;
        if(dmgRate > 0)
        {
            var entitiesToApplyEffect = new List<BaseEntity>();
            var keys = new List<BaseEntity>(_entitiesAtTrap.Keys);
            foreach (var key in keys)
            {
                var elapsed = _entitiesAtTrap[key];
                elapsed += timeUnits;
                while (elapsed >= dmgRate)
                {
                    entitiesToApplyEffect.Add(key);
                    elapsed = Mathf.Max(elapsed - dmgRate, 0.0f);
                }
                _entitiesAtTrap[key] = elapsed;
            }

            foreach(var entity in entitiesToApplyEffect)
            {
                ApplyEffect(entity);
            }
        }
        _triggerTrait.CheckEntitiesStaying();
        _triggerTrait.CheckEntitiesEntered();
    }

    public void ApplyEffect(BaseEntity e)
    {
        if (typeof(IHealthTrackingEntity).IsAssignableFrom(e.GetType()))
        {
            ((IHealthTrackingEntity)e).HPTrait.Decrease(_trapData.Damage);
        }
    }

    public void OnEntityEntered(BaseEntity e)
    {
        ApplyEffect(e);
        _entitiesAtTrap.Add(e, 0.0f);
    }

    public void OnEntityLeft(BaseEntity e)
    {
        _entitiesAtTrap.Remove(e);        
    }

    public void OnEntityRemained(BaseEntity e)
    {
    }

    public bool IsAffectedByTrigger(BaseEntity e)
    {
        return e is Player;
    }
}
