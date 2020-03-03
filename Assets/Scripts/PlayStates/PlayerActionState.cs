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

    public override void Init(GameController gameController)
    {
        Refresh(gameController);
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
        if(HandleExtendedActions(actionData, out timeWillPass, out var nextPlayState))
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
                if(player.CanAttackCoords(newPlayerCoords))
                {
                    bool allDefeated = player.AttackCoords(newPlayerCoords);
                    canMove = allDefeated;
                }
                // ...others

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

        // REMOVE
        if(Input.GetKeyDown(KeyCode.P))
        {
            foreach(Monster m in entityController.Monsters)
            {
                m.ToggleDebug();
            }
        }

        return GameController.PlayStates.Action;
    }

    protected virtual bool HandleExtendedActions(PlayerActionStateContext contextData, out bool timeWillPass, out int nextPlayContext)
    {
        nextPlayContext = GameController.PlayStates.Action;
        timeWillPass = false;
        bool willForceExit = false;

        return willForceExit;
    }
}
