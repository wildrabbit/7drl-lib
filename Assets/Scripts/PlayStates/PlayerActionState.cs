using System.Collections.Generic;
using UnityEngine;

public class PlayerActionStateContext : PlayStateContext
{
    public IEntityController EntityController;
    public IMapController Map;
    public bool BumpingWallsWillSpendTurn;
    public BaseGameEvents Events;

    public override void Refresh(GameController gameController)
    {
    }

    public override void Init(GameController gameController)
    {
        Refresh(gameController);
        Events = gameController.GameEvents;
    }
}


public class PlayerActionState : IPlayState
{
    public int Update(PlayStateContext contextData, out bool timeWillPass)
    {
        PlayerActionStateContext actionData = contextData as PlayerActionStateContext;
        BaseInputController input = actionData.Input;
        IMapController map = actionData.Map;
        IEntityController entityController = actionData.EntityController;
        Player player = entityController.Player;

        int nextPlayState = GameController.PlayStates.Action;
        
        timeWillPass = false;

        if(input.IdleTurn)
        {
            timeWillPass = true;
            actionData.Events.Player.SendIdleTurn();
            return GameController.PlayStates.Action;
        }

        if(input.RangeStart)
        {
            timeWillPass = false;
            return GameController.PlayStates.RangePrepare;
        }

        Vector2Int playerCoords = map.CoordsFromWorld(player.transform.position);


        // GAME SPECIFIC
        if(HandleExtendedActions(actionData, out timeWillPass, out nextPlayState))
        {
            return nextPlayState;
        }

        Vector2Int offset = map.CalculateMoveOffset(input.MoveDir, playerCoords);
        if (offset != Vector2Int.zero)
        {
            var newPlayerCoords = playerCoords + offset;

            if (!player.ValidMapCoords(newPlayerCoords))
            {
                timeWillPass = actionData.BumpingWallsWillSpendTurn;
            }
            else
            {
                timeWillPass = true;

                // Check interactions
                bool canMove = true;

                // Blockers:
                var blocker = FindBlockingEntityAt(entityController, newPlayerCoords);
                if(blocker != null)
                {
                    if (blocker.BlockingTrait.TryUnlock(player))
                    {
                        player.Coords = newPlayerCoords;
                    }
                    return nextPlayState;
                }

                bool exit = HandleAdditionalMoveInteractions(actionData, newPlayerCoords, ref nextPlayState, ref canMove);
                if(exit)
                {
                    if(canMove)
                    {
                        player.Coords = newPlayerCoords;
                    }
                    return nextPlayState;
                }

                if (player.SeesHostilesAtCoords(newPlayerCoords))
                {
                    bool allDefeated = player.AttackCoords(newPlayerCoords);
                    canMove = allDefeated;
                }

                if(canMove)
                {
                    player.Coords = newPlayerCoords;
                }
            }
        }

        // Quick Inventory actions:
        int inventoryIdx = System.Array.FindIndex(input.NumbersPressed, x => x);
        if(inventoryIdx != -1)
        {
            bool dropModifier = input.ShiftPressed;          
        }

        return GameController.PlayStates.Action;
    }

    public IBlockingEntity FindBlockingEntityAt(IEntityController entityController, Vector2Int coords)
    {
        var entities = entityController.GetEntitiesAt(coords);
        var blocking = CollectionUtils.GetImplementors<BaseEntity, IBlockingEntity>(entities);
        return blocking.Count > 0 ? blocking[0] : null;
    }


    protected virtual bool HandleAdditionalMoveInteractions(PlayerActionStateContext contextData, Vector2Int newPlayerCoords, ref int nextPlayState, ref bool canMove)
    {
        bool immediateExit = false;
        return immediateExit;
    }

    protected virtual bool HandleExtendedActions(PlayerActionStateContext contextData, out bool timeWillPass, out int nextPlayContext)
    {
        nextPlayContext = GameController.PlayStates.Action;
        timeWillPass = false;
        bool willForceExit = false;

        return willForceExit;
    }

    public virtual void Enter(PlayStateContext context)
    {

    }


    public virtual void Exit(PlayStateContext context)
    {

    }
}
