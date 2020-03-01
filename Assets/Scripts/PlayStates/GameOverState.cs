using UnityEngine;
using System.Collections;

public class GameOverStateContext : PlayStateContext
{
    public GameController Controller; 
    // Replace with IGameFlowController logic so we only care about the restart bit.
    
    public override void Refresh(GameController gameController)
    {
        Controller = gameController;
    }
}


public class GameOverState : IPlayState
{
    public int Update(PlayStateContext context, out bool willSpendTime)
    {
        if(context.Input.Any)
        {
            (context as GameOverStateContext).Controller.Restart();   
        }
        willSpendTime = false;
        return GameController.PlayStates.GameOver;
    }
}

