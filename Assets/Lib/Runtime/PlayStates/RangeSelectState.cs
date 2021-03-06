using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RangeSelectStateContext : PlayStateContext
{
    public IEntityController EntityController;
    public IMapController MapController;
    public RangeDisplay ViewPrefab;

    RangeDisplay _viewInstance; // TODO: Add a proper class when implemented
    public Vector2Int Target;
    
    public void SetupTargetView()
    {
        Target = EntityController.Player.Coords;
        _viewInstance = GameObject.Instantiate(ViewPrefab);
        var rootPos = MapController.WorldFromCoords(Target);
        _viewInstance.transform.position = rootPos;
        _viewInstance.RefreshLine(new List<Vector3>() { rootPos }, new List<bool>() { false });
    }

    public void RefreshTargetView(MoveDirection dir)
    {
        var offset = MapController.CalculateMoveOffset(dir, Target);

        Target += offset;

        if (MapController.ValidCoords(new Vector3Int(Target.x, Target.y, 0)))
        {
            var targets = BresenhamUtils.CalculateLine(EntityController.Player.Coords, Target);
            EntityController.Player.BattleTrait.GetReachableStateForCoords(targets, out List<bool> targetOccluded);
            var targetPositions = targets.ConvertAll(x => MapController.WorldFromCoords(x));

            _viewInstance.RefreshLine(targetPositions, targetOccluded);
        }
        else Target -= offset;
    }

    public void ClearTargetView()
    {
        if(_viewInstance)
        {
            GameObject.Destroy(_viewInstance.gameObject);
            _viewInstance = null;
        }
            
    }

}


public class RangeSelectState : IPlayState
{
    public void Enter(PlayStateContext context)
    {
        RangeSelectStateContext rangeContext = ((RangeSelectStateContext)context);
        rangeContext.SetupTargetView();
    }

    public void Exit(PlayStateContext context)
    {
        RangeSelectStateContext rangeContext = ((RangeSelectStateContext)context);
        rangeContext.ClearTargetView();
    }

    public int Update(PlayStateContext context, out bool willSpendTime)
    {
        willSpendTime = false;

        RangeSelectStateContext ctxt = context as RangeSelectStateContext;

        if(ctxt.Input.MoveDir != MoveDirection.None)
        {
            ctxt.RefreshTargetView(ctxt.Input.MoveDir);
        }

        if(ctxt.Input.ActionConfirm || ctxt.Input.RangeStart)
        {
            ctxt.ClearTargetView();
            willSpendTime = ctxt.EntityController.Player.BattleTrait.TryAttackCoords(ctxt.Target);
            return GameController.PlayStates.Action;
        }
        
        if(ctxt.Input.ActionCancel)
        {
            ctxt.ClearTargetView();
            return GameController.PlayStates.Action;
        }
        
        return GameController.PlayStates.RangePrepare;
    }
}

