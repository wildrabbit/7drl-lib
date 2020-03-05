﻿using System;

public interface IBlockingEntity
{
    BlockingTrait BlockingTrait { get; }
    void Unlock(BaseEntity unlockingEntity);
}


public class BlockingTrait
{
    BlockingTraitData _traitData;
    IBlockingEntity _owner;
    BaseGameEvents.BlockingEvents _blockEvents;

    public void Init(IBlockingEntity owner, BlockingTraitData data, BaseGameEvents.BlockingEvents blockEvents)
    {
        _owner = owner;
        _traitData = data;
        _blockEvents = blockEvents;
    }

    public bool CanBeUnlockedByEntity(BaseEntity entity)
    {
        // No distance checks here, this will be called depending on specific interactions (i.e, player bumping, projectile colliding, etc)
        return Array.Exists(_traitData.UnlockAttributes, unlockingAttr => Array.IndexOf(entity.Attributes, unlockingAttr) >= 0);
    }

    public bool CanBeTrespassedByEntity(BaseEntity entity)
    {
        return false; // You shall not pass until further notice (what to do w/ ghosts?)
    }

    public bool TryUnlock(BaseEntity entity)
    {
        if(CanBeUnlockedByEntity(entity))
        {
            _blockEvents.SentEntityBlockInteraction(_owner, entity, unlocked: true);
            _owner.Unlock(entity);
            return true;
        }
        _blockEvents.SentEntityBlockInteraction(_owner, entity, unlocked: false);
        return false;
    }
}
