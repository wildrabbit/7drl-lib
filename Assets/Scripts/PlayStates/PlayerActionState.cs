using System.Collections.Generic;
using UnityEngine;

public class PlayerActionStateContext : PlayStateContext
{
    public IEntityController EntityController;
    public IMapController Map;
    public bool BumpingWallsWillSpendTurn;
    // TODO: GameEvents

    public override void Refresh(GameController gameController)
    {
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
        
        
        timeWillPass = false;

        if(input.IdleTurn)
        {
            timeWillPass = true;
            //contextData.GameEvents.PlayPhase.SendIdle();
            //PlayerActionEvent evt = new PlayerActionEvent(actionData.Turns, actionData.TimeUnits);
            //evt.SetIdle();
            //log.AddEvent(evt);
            return GameController.PlayStates.Action;
        }

        Vector2Int playerCoords = map.CoordsFromWorld(player.transform.position);


        // GAME SPECIFIC
        if(HandleExtendedActions(out timeWillPass, out var nextPlayState))
        {
            return nextPlayState;
        }

        Vector2Int offset = map.CalculateMoveOffset(input.MoveDir, playerCoords);
        if (offset != Vector2Int.zero)
        {
            var newPlayerCoords = playerCoords + offset;

            if (!player.TryResolveMoveIntoCoords(newPlayerCoords))
            {
                timeWillPass = actionData.BumpingWallsWillSpendTurn;
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

    protected virtual bool HandleExtendedActions(out bool timeWillPass, out int nextPlayContext)
    {
        nextPlayContext = GameController.PlayStates.Action;
        timeWillPass = false;
        bool willForceExit = false;

        return willForceExit;
    }
}
