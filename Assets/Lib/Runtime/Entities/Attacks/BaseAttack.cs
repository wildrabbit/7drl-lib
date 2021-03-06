using UnityEngine;
using System.Collections.Generic;
using System;

public abstract class BaseAttack
{

    public bool Ready => Data.Cooldown == 0 || Elapsed < 0 || Elapsed >= (float)Data.Cooldown;
    public BaseAttackData Data;
    public float Elapsed;
    public string[] Attributes => Data.Attributes;

    protected IEntityController _entityController;
    protected IMapController _mapController;

    public virtual void Init(IEntityController entityController, IMapController mapController)
    {
        _entityController = entityController;
        _mapController = mapController;
    }
    
    public abstract bool CanTargetBeReached(IBattleEntity attacker, IBattleEntity target);

    public abstract List<IBattleEntity> FindAllReachableTargets(IBattleEntity source);
    public abstract List<IBattleEntity> FindTargetsAtCoords(IBattleEntity source, Vector2Int refCoords);
    public abstract List<bool> GetReachableStateForCoords(IBattleEntity source, List<Vector2Int> coords);


    public BaseAttack(BaseAttackData data)
    {
        Data = data;
        Elapsed = -1;
    }
}
